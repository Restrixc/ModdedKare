using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using ExitGames.Client.Photon;
using UnityEngine;

public class ReagentContents : IEnumerable<Reagent>, IEnumerable, ISavable
{
	public delegate void ReagentContentsChangedAction(ReagentContents contents);

	public ReagentContentsChangedAction changed;

	private float maxVolume = float.MaxValue;

	private const float metabolizationVolumeEpsilon = 0.1f;

	[SerializeField]
	private Dictionary<short, Reagent> contents = new Dictionary<short, Reagent>();

	public float volume
	{
		get
		{
			float v = 0f;
			foreach (KeyValuePair<short, Reagent> content in contents)
			{
				v += content.Value.volume;
			}
			return v;
		}
	}

	public int Count => contents.Count;

	public ReagentContents(ReagentContents other)
	{
		Copy(other);
	}

	public ReagentContents(float maxVolume = float.MaxValue)
	{
		this.maxVolume = maxVolume;
	}

	public void Copy(ReagentContents other)
	{
		contents = new Dictionary<short, Reagent>(other.contents);
		maxVolume = other.maxVolume;
	}

	public float GetMaxVolume()
	{
		return maxVolume;
	}

	public void SetMaxVolume(float newMaxVolume)
	{
		if (!(Math.Abs(maxVolume - newMaxVolume) < 0.001f))
		{
			maxVolume = newMaxVolume;
			if (contents != null && maxVolume < volume)
			{
				Spill(volume - maxVolume);
			}
			changed?.Invoke(this);
		}
	}

	public void OverrideReagent(short id, float volume)
	{
		if (contents.ContainsKey(id))
		{
			contents[id].volume = volume;
			return;
		}
		contents.Add(id, new Reagent
		{
			id = id,
			volume = volume
		});
		changed?.Invoke(this);
	}

	public void AddMix(short id, float addVolume, GenericReagentContainer worldContainer = null)
	{
		if (contents.ContainsKey(id))
		{
			contents[id].volume = Mathf.Max(0f, contents[id].volume + addVolume);
		}
		else
		{
			contents.Add(id, new Reagent
			{
				id = id,
				volume = addVolume
			});
			if (!(worldContainer != null))
			{
			}
		}
		if (worldContainer != null)
		{
			ReagentDatabase.DoReactions(worldContainer, id);
		}
		if (volume > maxVolume)
		{
			Spill(volume - maxVolume);
		}
		changed?.Invoke(this);
	}

	public void AddMix(Reagent reagent, GenericReagentContainer worldContainer = null)
	{
		AddMix(reagent.id, reagent.volume, worldContainer);
	}

	public void AddMix(ReagentContents container, GenericReagentContainer worldContainer = null)
	{
		foreach (KeyValuePair<short, Reagent> content in container.contents)
		{
			AddMix(content.Value, worldContainer);
		}
	}

	public ReagentContents Spill(float spillVolume)
	{
		float v = volume;
		ReagentContents spillContents = new ReagentContents();
		if (v <= 0f)
		{
			return spillContents;
		}
		float spillRatio = Mathf.Clamp01(spillVolume / v);
		foreach (KeyValuePair<short, Reagent> pair in contents)
		{
			spillContents.AddMix(pair.Key, pair.Value.volume * spillRatio);
			contents[pair.Key].volume = pair.Value.volume * (1f - spillRatio);
		}
		changed?.Invoke(this);
		return spillContents;
	}

	public void Clear()
	{
		contents.Clear();
		changed?.Invoke(this);
	}

	public ReagentContents Metabolize(float deltaTime)
	{
		float v = volume;
		ReagentContents metabolizeContents = new ReagentContents();
		if (v <= 0f)
		{
			return metabolizeContents;
		}
		foreach (KeyValuePair<short, Reagent> pair in contents)
		{
			float metabolizationHalfLife = ReagentDatabase.GetReagent(pair.Key).GetMetabolizationHalfLife();
			float metaHalfLife = ((metabolizationHalfLife == 0f) ? pair.Value.volume : (pair.Value.volume * Mathf.Pow(0.5f, deltaTime / metabolizationHalfLife)));
			if (pair.Value.volume <= 0.1f)
			{
				metabolizeContents.AddMix(pair.Value);
				contents[pair.Key].volume = 0f;
			}
			else
			{
				float loss = Mathf.Max(pair.Value.volume - metaHalfLife, 0f);
				contents[pair.Key].volume = Mathf.Max(metaHalfLife, 0f);
				metabolizeContents.AddMix(pair.Key, loss);
			}
		}
		changed?.Invoke(this);
		return metabolizeContents;
	}

	public float GetVolumeOf(short id)
	{
		if (contents.ContainsKey(id))
		{
			return contents[id].volume;
		}
		return 0f;
	}

	public float GetVolumeOf(ScriptableReagent reagent)
	{
		short id = ReagentDatabase.GetID(reagent);
		return GetVolumeOf(id);
	}

	public bool IsCleaningAgent()
	{
		float totalCleanerVolume = 0f;
		foreach (KeyValuePair<short, Reagent> pair in contents)
		{
			if (ReagentDatabase.GetReagent(pair.Key).IsCleaningAgent())
			{
				totalCleanerVolume += pair.Value.volume;
			}
		}
		return totalCleanerVolume > volume * 0.5f;
	}

	public float GetCalories()
	{
		float totalCalories = 0f;
		foreach (KeyValuePair<short, Reagent> pair in contents)
		{
			totalCalories += pair.Value.volume * ReagentDatabase.GetReagent(pair.Key).GetCalories();
		}
		return totalCalories;
	}

	public Color GetColor()
	{
		float v = volume;
		if (v <= 0f)
		{
			return Color.white;
		}
		Color totalColor = Color.black;
		foreach (KeyValuePair<short, Reagent> pair in contents)
		{
			if (!(pair.Value.volume <= 0f))
			{
				totalColor += ReagentDatabase.GetReagent(pair.Key).GetColor() * (pair.Value.volume / v);
			}
		}
		return totalColor;
	}

	public float GetValue()
	{
		float totalValue = 0f;
		foreach (KeyValuePair<short, Reagent> pair in contents)
		{
			totalValue += ReagentDatabase.GetReagent(pair.Key).GetValue() * pair.Value.volume;
		}
		return totalValue;
	}

	public static short SerializeReagentContents(StreamBuffer outStream, object customObject)
	{
		ReagentContents reagentContents = (ReagentContents)customObject;
		short size = (short)(4 + 6 * reagentContents.contents.Count);
		byte[] bytes = new byte[size];
		int index = 0;
		Protocol.Serialize(reagentContents.maxVolume, bytes, ref index);
		foreach (KeyValuePair<short, Reagent> pair in reagentContents.contents)
		{
			if (!(pair.Value.volume <= 0f))
			{
				Protocol.Serialize(pair.Key, bytes, ref index);
				Protocol.Serialize(pair.Value.volume, bytes, ref index);
			}
		}
		outStream.Write(bytes, 0, size);
		return size;
	}

	public static object DeserializeReagentContents(StreamBuffer inStream, short length)
	{
		ReagentContents reagentContents = new ReagentContents();
		byte[] bytes = new byte[length];
		inStream.Read(bytes, 0, length);
		int index = 0;
		Protocol.Deserialize(out float maxVolume, bytes, ref index);
		reagentContents.maxVolume = maxVolume;
		while (index < length)
		{
			short id = 0;
			float volume = 0f;
			Protocol.Deserialize(out id, bytes, ref index);
			Protocol.Deserialize(out volume, bytes, ref index);
			reagentContents.OverrideReagent(id, volume);
		}
		return reagentContents;
	}

	public void Save(BinaryWriter outStream)
	{
		int count = 0;
		foreach (KeyValuePair<short, Reagent> content in contents)
		{
			if (!(content.Value.volume <= 0f))
			{
				count++;
			}
		}
		outStream.Write(maxVolume);
		outStream.Write(count);
		foreach (KeyValuePair<short, Reagent> pair in contents)
		{
			if (!(pair.Value.volume <= 0f))
			{
				outStream.Write(pair.Key);
				outStream.Write(pair.Value.volume);
			}
		}
	}

	public void Load(BinaryReader inStream)
	{
		Clear();
		maxVolume = inStream.ReadSingle();
		int count = inStream.ReadInt32();
		for (int i = 0; i < count; i++)
		{
			short id = inStream.ReadInt16();
			float volume = inStream.ReadSingle();
			OverrideReagent(id, volume);
		}
	}

	public IEnumerator<Reagent> GetEnumerator()
	{
		foreach (KeyValuePair<short, Reagent> content in contents)
		{
			yield return content.Value;
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
