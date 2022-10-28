using System.Collections;
using System.IO;
using Photon.Pun;
using UnityEngine;

public class RotateSelectorUsable : UsableMachine
{
	public delegate void RotatedAction(int newValue);

	[SerializeField]
	private Sprite useSprite;

	[SerializeField]
	private Transform spinnyWheel;

	[SerializeField]
	private AudioPack selectPack;

	private AudioSource source;

	private int selectedMode;

	private const int maxSelections = 4;

	private Quaternion startRotation;

	public event RotatedAction rotated;

	private void Awake()
	{
		startRotation = spinnyWheel.localRotation;
		if (source == null)
		{
			source = base.gameObject.AddComponent<AudioSource>();
			source.playOnAwake = false;
			source.maxDistance = 10f;
			source.minDistance = 0.2f;
			source.rolloffMode = AudioRolloffMode.Linear;
			source.spatialBlend = 1f;
			source.loop = false;
			source.enabled = false;
		}
	}

	public override bool CanUse(Kobold k)
	{
		return constructed;
	}

	public override Sprite GetSprite(Kobold k)
	{
		return useSprite;
	}

	private void SetSelected(int select)
	{
		int newValue = select % 4;
		if (newValue != selectedMode)
		{
			selectedMode = select % 4;
			spinnyWheel.localRotation = Quaternion.AngleAxis((float)selectedMode * 360f / 4f, -Vector3.right) * startRotation;
			this.rotated?.Invoke(select);
		}
	}

	public int GetSelected()
	{
		return selectedMode;
	}

	public override void Use()
	{
		SetSelected(selectedMode + 1);
		StopAllCoroutines();
		source.enabled = true;
		selectPack.Play(source);
		StartCoroutine(DisableAfterTime());
	}

	public override void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		base.OnPhotonSerializeView(stream, info);
		if (stream.IsWriting)
		{
			stream.SendNext(GetSelected());
		}
		else
		{
			SetSelected((int)stream.ReceiveNext());
		}
	}

	public override void Save(BinaryWriter writer)
	{
		base.Save(writer);
		writer.Write(GetSelected());
	}

	public override void Load(BinaryReader reader)
	{
		base.Load(reader);
		SetSelected(reader.ReadInt32());
	}

	private IEnumerator DisableAfterTime()
	{
		yield return new WaitForSeconds(source.clip.length + 0.1f);
		source.enabled = false;
	}
}
