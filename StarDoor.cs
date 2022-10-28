using System.Collections;
using System.IO;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.VFX;

public class StarDoor : GenericUsable
{
	[SerializeField]
	private Sprite useSprite;

	[SerializeField]
	private AudioPack starDoorBreak;

	[SerializeField]
	private int starRequirement = 1;

	[SerializeField]
	private TMP_Text text;

	[SerializeField]
	private Renderer starRenderer;

	[SerializeField]
	private VisualEffect starDissolveVFX;

	private Material starMaterial;

	private AudioSource starDoorBreakSource;

	private static readonly int Progress1 = Shader.PropertyToID("_DissolveProgress");

	public override Sprite GetSprite(Kobold k)
	{
		return useSprite;
	}

	public override bool CanUse(Kobold k)
	{
		return ObjectiveManager.GetStars() >= starRequirement;
	}

	public override void Use()
	{
		StartCoroutine(DissolveRoutine());
	}

	private void Start()
	{
		text.text = starRequirement.ToString();
		starMaterial = starRenderer.material;
		if (starDoorBreakSource == null)
		{
			starDoorBreakSource = base.gameObject.AddComponent<AudioSource>();
			starDoorBreakSource.playOnAwake = false;
			starDoorBreakSource.maxDistance = 10f;
			starDoorBreakSource.minDistance = 0.2f;
			starDoorBreakSource.rolloffMode = AudioRolloffMode.Linear;
			starDoorBreakSource.spatialBlend = 1f;
			starDoorBreakSource.loop = true;
		}
	}

	private IEnumerator DissolveRoutine()
	{
		starDissolveVFX.gameObject.SetActive(value: true);
		starDoorBreakSource.enabled = true;
		starDoorBreak.Play(starDoorBreakSource);
		float startTime = Time.time;
		float duration = 3f;
		while (Time.time < startTime + duration)
		{
			float t = (Time.time - startTime) / duration;
			starMaterial.SetFloat(Progress1, t);
			yield return null;
		}
		starDoorBreakSource.enabled = false;
		if (base.photonView.IsMine)
		{
			PhotonNetwork.Destroy(base.photonView.gameObject);
		}
	}

	public override void Save(BinaryWriter writer)
	{
		base.Save(writer);
		writer.Write(base.transform.position.x);
		writer.Write(base.transform.position.y);
		writer.Write(base.transform.position.z);
		writer.Write(base.transform.rotation.x);
		writer.Write(base.transform.rotation.y);
		writer.Write(base.transform.rotation.z);
		writer.Write(base.transform.rotation.w);
	}

	public override void Load(BinaryReader reader)
	{
		base.Load(reader);
		float x = reader.ReadSingle();
		float y = reader.ReadSingle();
		float z = reader.ReadSingle();
		base.transform.position = new Vector3(x, y, z);
		x = reader.ReadSingle();
		y = reader.ReadSingle();
		z = reader.ReadSingle();
		float w = reader.ReadSingle();
		base.transform.rotation = new Quaternion(x, y, z, w);
	}
}
