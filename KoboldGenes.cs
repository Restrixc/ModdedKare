using System;
using System.Collections.Generic;
using System.IO;
using ExitGames.Client.Photon;
using UnityEngine;

[Serializable]
public class KoboldGenes : ISavable
{
	public string testingillegalshit = "";

	public float maxEnergy = 5f;

	public float baseSize = 20f;

	public float fatSize;

	public float ballSize;

	public float dickSize;

	public float breastSize;

	public float bellySize = 20f;

	public float metabolizeCapacitySize = 20f;

	public byte hue;

	public byte brightness = 128;

	public byte saturation = 128;

	public byte dickEquip = byte.MaxValue;

	public float dickThickness;

	public byte grabCount = 1;

	private const short byteCount = 41;

	private static float RandomGaussian(float minValue = 0f, float maxValue = 1f)
	{
		float u;
		float S;
		do
		{
			u = 2f * UnityEngine.Random.value - 1f;
			float v = 2f * UnityEngine.Random.value - 1f;
			S = u * u + v * v;
		}
		while (S >= 1f);
		float std = u * Mathf.Sqrt(-2f * Mathf.Log(S) / S);
		float mean = (minValue + maxValue) / 2f;
		float sigma = (maxValue - mean) / 3f;
		return Mathf.Clamp(std * sigma + mean, minValue, maxValue);
	}

	public KoboldGenes With(float? maxEnergy = null, float? baseSize = null, float? fatSize = null, float? ballSize = null, float? dickSize = null, float? breastSize = null, float? bellySize = null, float? metabolizeCapacitySize = null, byte? hue = null, byte? brightness = null, byte? saturation = null, byte? dickEquip = null, float? dickThickness = null, byte? grabCount = null)
	{
		return new KoboldGenes
		{
			maxEnergy = (maxEnergy ?? this.maxEnergy),
			baseSize = (baseSize ?? this.baseSize),
			fatSize = (fatSize ?? this.fatSize),
			ballSize = (ballSize ?? this.ballSize),
			dickSize = (dickSize ?? this.dickSize),
			breastSize = (breastSize ?? this.breastSize),
			bellySize = (bellySize ?? this.bellySize),
			metabolizeCapacitySize = (metabolizeCapacitySize ?? this.metabolizeCapacitySize),
			hue = (hue ?? this.hue),
			brightness = (brightness ?? this.brightness),
			saturation = (saturation ?? this.saturation),
			dickEquip = (dickEquip ?? this.dickEquip),
			dickThickness = (dickThickness ?? this.dickThickness),
			grabCount = (grabCount ?? this.grabCount)
		};
	}

	private byte GetRandomDick()
	{
		List<Equipment> equipments = EquipmentDatabase.GetEquipments();
		float totalDicks = 0f;
		foreach (Equipment equipment2 in equipments)
		{
			if (equipment2 is DickEquipment)
			{
				totalDicks += 1f;
			}
		}
		float randomSelection = UnityEngine.Random.Range(0f, totalDicks);
		float selection = 0f;
		foreach (Equipment equipment in equipments)
		{
			if (equipment is DickEquipment)
			{
				selection += 1f;
				if (selection >= randomSelection)
				{
					return (byte)equipments.IndexOf(equipment);
				}
			}
		}
		return byte.MaxValue;
	}

	public KoboldGenes Randomize(float multiplier = 1f)
	{
		if (UnityEngine.Random.Range(0f, 1f) > 0.4f)
		{
			breastSize = UnityEngine.Random.Range(0f, 10f) * multiplier;
			ballSize = UnityEngine.Random.Range(10f, 20f) * multiplier;
			dickSize = UnityEngine.Random.Range(0f, 20f) * multiplier;
			dickEquip = GetRandomDick();
		}
		else
		{
			breastSize = UnityEngine.Random.Range(10f, 40f) * multiplier;
			ballSize = UnityEngine.Random.Range(5f, 25f) * multiplier;
			dickSize = UnityEngine.Random.Range(0f, 20f) * multiplier;
			dickEquip = byte.MaxValue;
		}
		fatSize = UnityEngine.Random.Range(0f, 3f);
		dickThickness = RandomGaussian() * multiplier;
		baseSize = UnityEngine.Random.Range(14f, 24f) * multiplier;
		hue = (byte)UnityEngine.Random.Range(0, 255);
		brightness = (byte)Mathf.RoundToInt(RandomGaussian(0f, 255f));
		saturation = (byte)Mathf.RoundToInt(RandomGaussian(0f, 255f));
		return this;
	}

	public static KoboldGenes Mix(KoboldGenes a, KoboldGenes b)
	{
		if (a == null && b == null)
		{
			Debug.LogError("Tried to mix two null gene pools, how does this happen?");
			return new KoboldGenes().Randomize();
		}
		if (a == null)
		{
			return b;
		}
		if (b == null)
		{
			return a;
		}
		KoboldGenes c = ((!(UnityEngine.Random.Range(0f, 1f) > 0.5f)) ? ((KoboldGenes)b.MemberwiseClone()) : ((KoboldGenes)a.MemberwiseClone()));
		float hueAngA = (float)(int)a.hue / 255f;
		float hueAngB = (float)(int)b.hue / 255f;
		c.hue = (byte)Mathf.RoundToInt(FloatExtensions.CircularLerp(hueAngA, hueAngB, 0.5f) * 255f);
		c.brightness = (byte)Mathf.RoundToInt(Mathf.Lerp((float)(int)a.brightness / 255f, (float)(int)b.brightness / 255f, 0.5f) * 255f);
		c.saturation = (byte)Mathf.RoundToInt(Mathf.Lerp((float)(int)a.saturation / 255f, (float)(int)b.saturation / 255f, 0.5f) * 255f);
		c.bellySize = Mathf.Lerp(a.bellySize * 0.85f, b.bellySize * 1.15f, UnityEngine.Random.Range(0f, 1f));
		c.metabolizeCapacitySize = Mathf.Lerp(a.metabolizeCapacitySize, b.metabolizeCapacitySize, UnityEngine.Random.Range(0f, 1f));
		c.dickSize = Mathf.Lerp(a.dickSize * 0.85f, b.dickSize * 1.15f, UnityEngine.Random.Range(0f, 1f));
		c.ballSize = Mathf.Lerp(a.ballSize * 0.85f, b.ballSize * 1.15f, UnityEngine.Random.Range(0f, 1f));
		c.fatSize = Mathf.Lerp(a.fatSize * 0.85f, b.fatSize * 1.15f, UnityEngine.Random.Range(0f, 1f));
		c.baseSize = Mathf.Lerp(a.baseSize * 0.85f, b.baseSize * 1.15f, UnityEngine.Random.Range(0f, 1f));
		c.maxEnergy = Mathf.Lerp(a.maxEnergy * 0.85f, b.maxEnergy * 1.15f, UnityEngine.Random.Range(0f, 1f));
		c.dickThickness = Mathf.Lerp(a.dickThickness * 0.85f, b.dickThickness * 1.15f, UnityEngine.Random.Range(0f,1f));
		c.grabCount = (byte)Mathf.Max(Mathf.RoundToInt(Mathf.Lerp((int)a.grabCount, (int)b.grabCount, 0.5f)), 1);
		return c;
	}

	public static short Serialize(StreamBuffer outStream, object customObject)
	{
		KoboldGenes genes = (KoboldGenes)customObject;
		byte[] bytes = new byte[41];
		int index = 0;
		Protocol.Serialize(genes.maxEnergy, bytes, ref index);
		Protocol.Serialize(genes.baseSize, bytes, ref index);
		Protocol.Serialize(genes.fatSize, bytes, ref index);
		Protocol.Serialize(genes.ballSize, bytes, ref index);
		Protocol.Serialize(genes.dickSize, bytes, ref index);
		Protocol.Serialize(genes.breastSize, bytes, ref index);
		Protocol.Serialize(genes.bellySize, bytes, ref index);
		Protocol.Serialize(genes.metabolizeCapacitySize, bytes, ref index);
		bytes[index++] = genes.hue;
		bytes[index++] = genes.brightness;
		bytes[index++] = genes.saturation;
		bytes[index++] = genes.dickEquip;
		bytes[index++] = genes.grabCount;
		Protocol.Serialize(genes.dickThickness, bytes, ref index);
		outStream.Write(bytes, 0, 41);
		return 41;
	}

	public static object Deserialize(StreamBuffer inStream, short length)
	{
		KoboldGenes genes = new KoboldGenes();
		byte[] bytes = new byte[length];
		inStream.Read(bytes, 0, length);
		int index = 0;
		while (index < length)
		{
			Protocol.Deserialize(out genes.maxEnergy, bytes, ref index);
			Protocol.Deserialize(out genes.baseSize, bytes, ref index);
			Protocol.Deserialize(out genes.fatSize, bytes, ref index);
			Protocol.Deserialize(out genes.ballSize, bytes, ref index);
			Protocol.Deserialize(out genes.dickSize, bytes, ref index);
			Protocol.Deserialize(out genes.breastSize, bytes, ref index);
			Protocol.Deserialize(out genes.bellySize, bytes, ref index);
			Protocol.Deserialize(out genes.metabolizeCapacitySize, bytes, ref index);
			genes.hue = bytes[index++];
			genes.brightness = bytes[index++];
			genes.saturation = bytes[index++];
			genes.dickEquip = bytes[index++];
			genes.grabCount = bytes[index++];
			Protocol.Deserialize(out genes.dickThickness, bytes, ref index);
		}
		return genes;
	}

	public void Save(BinaryWriter writer)
	{
		writer.Write(maxEnergy);
		writer.Write(baseSize);
		writer.Write(fatSize);
		writer.Write(ballSize);
		writer.Write(dickSize);
		writer.Write(breastSize);
		writer.Write(bellySize);
		writer.Write(metabolizeCapacitySize);
		writer.Write(hue);
		writer.Write(brightness);
		writer.Write(saturation);
		writer.Write(dickEquip);
		writer.Write(grabCount);
		writer.Write(dickThickness);
	}

	public void Load(BinaryReader reader)
	{
		maxEnergy = reader.ReadSingle();
		baseSize = reader.ReadSingle();
		fatSize = reader.ReadSingle();
		ballSize = reader.ReadSingle();
		dickSize = reader.ReadSingle();
		breastSize = reader.ReadSingle();
		bellySize = reader.ReadSingle();
		metabolizeCapacitySize = reader.ReadSingle();
		hue = reader.ReadByte();
		brightness = reader.ReadByte();
		saturation = reader.ReadByte();
		dickEquip = reader.ReadByte();
		grabCount = reader.ReadByte();
		dickThickness = reader.ReadSingle();
	}

	public override string ToString()
	{
		return $"Kobold Genes: \n           maxEnergy: {maxEnergy}\n           baseSize: {baseSize}\n           fatSize: {fatSize}\n           ballSize: {ballSize}\n           dickSize: {dickSize}\n           breastSize: {breastSize}\n           bellySize: {bellySize}\n           metabolizeCapacitySize: {metabolizeCapacitySize}\n           hue: {hue}\n           brightness: {brightness}\n           saturation: {saturation}\n           dickEquip: {dickEquip}\n           grabCount: {grabCount}\n           dickThickness: {dickThickness}";
	}
}
