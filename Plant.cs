using System.Collections;
using System.IO;
using Photon.Pun;
using UnityEngine;
using UnityEngine.VFX;

[RequireComponent(typeof(GenericReagentContainer))]
public class Plant : GeneHolder, IPunInstantiateMagicCallback, IPunObservable, ISavable
{
	public delegate void SwitchAction();

	public ScriptablePlant plant;

	[SerializeField]
	private GenericReagentContainer container;

	[SerializeField]
	public Color darkenedColor;

	[SerializeField]
	private VisualEffect effect;

	[SerializeField]
	private VisualEffect wateredEffect;

	[SerializeField]
	private GameObject display;

	[SerializeField]
	public AudioSource audioSource;

	private static readonly int BrightnessContrastSaturation = Shader.PropertyToID("_HueBrightnessContrastSaturation");

	private bool growing;

	public event SwitchAction switched;

	private void Start()
	{
		container.OnFilled.AddListener(OnFilled);
	}

	private void OnDestroy()
	{
		container.OnFilled.RemoveListener(OnFilled);
	}

	private IEnumerator GrowRoutine()
	{
		growing = true;
		yield return new WaitForSeconds(30f);
		if (base.photonView.IsMine)
		{
			if (plant.possibleNextGenerations == null || (float)plant.possibleNextGenerations.Length == 0f)
			{
				PhotonNetwork.Destroy(base.gameObject);
				yield break;
			}
			base.photonView.RPC("Spill", RpcTarget.All, container.volume);
			base.photonView.RPC("SwitchToRPC", RpcTarget.AllBufferedViaServer, PlantDatabase.GetID(plant.possibleNextGenerations[Random.Range(0, plant.possibleNextGenerations.Length)]));
			growing = false;
		}
	}

	private void OnFilled(ReagentContents contents, GenericReagentContainer.InjectType injectType)
	{
		if (plant.possibleNextGenerations != null && plant.possibleNextGenerations.Length != 0)
		{
			Renderer[] componentsInChildren = display.GetComponentsInChildren<Renderer>();
			foreach (Renderer renderer in componentsInChildren)
			{
				renderer.material.SetFloat("_BounceAmount", 1f);
				StartCoroutine(DarkenMaterial(renderer.material));
			}
			wateredEffect.SendEvent("Play");
			audioSource.Play();
			effect.gameObject.SetActive(value: false);
			effect.gameObject.SetActive(value: true);
			StopCoroutine("GrowRoutine");
			StartCoroutine("GrowRoutine");
		}
	}

	[PunRPC]
	private void SwitchToRPC(short newPlantID)
	{
		ScriptablePlant checkPlant = PlantDatabase.GetPlant(newPlantID);
		if (!(checkPlant == plant))
		{
			SwitchTo(checkPlant);
		}
	}

	public override void SetGenes(KoboldGenes newGenes)
	{
		if (display != null)
		{
			Vector4 hbcs = new Vector4((float)(int)newGenes.hue / 255f, (float)(int)newGenes.brightness / 255f, 0.5f, (float)(int)newGenes.saturation / 255f);
			Renderer[] componentsInChildren = display.GetComponentsInChildren<Renderer>();
			foreach (Renderer r in componentsInChildren)
			{
				Material[] materials = r.materials;
				foreach (Material material in materials)
				{
					material.SetColor(BrightnessContrastSaturation, hbcs);
				}
			}
		}
		base.SetGenes(newGenes);
	}

	private void SwitchTo(ScriptablePlant newPlant)
	{
		if (plant == newPlant)
		{
			return;
		}
		plant = newPlant;
		UndarkenMaterials();
		wateredEffect.Stop();
		if (display != null)
		{
			Object.Destroy(display);
		}
		if (newPlant.display != null)
		{
			display = Object.Instantiate(newPlant.display, base.transform);
			if (GetGenes() != null)
			{
				SetGenes(GetGenes());
			}
		}
		if (PhotonNetwork.IsMasterClient)
		{
			ScriptablePlant.Produce[] produces = newPlant.produces;
			foreach (ScriptablePlant.Produce produce in produces)
			{
				int spawnCount = Random.Range(produce.minProduce, produce.maxProduce);
				for (int i = 0; i < spawnCount; i++)
				{
					PhotonNetwork.InstantiateRoomObject(produce.prefab.photonName, base.transform.position + Vector3.up + Random.insideUnitSphere * 0.5f, Quaternion.identity, 0, new object[2]
					{
						GetGenes(),
						false
					});
				}
			}
		}
		this.switched?.Invoke();
		if (plant.possibleNextGenerations == null || plant.possibleNextGenerations.Length == 0)
		{
			StartCoroutine(GrowRoutine());
		}
	}

	public void OnPhotonInstantiate(PhotonMessageInfo info)
	{
		if (info.photonView.InstantiationData != null && info.photonView.InstantiationData[0] is short)
		{
			SwitchTo(PlantDatabase.GetPlant((short)info.photonView.InstantiationData[0]));
		}
		if (info.photonView.InstantiationData != null && info.photonView.InstantiationData[1] is KoboldGenes)
		{
			SetGenes((KoboldGenes)info.photonView.InstantiationData[1]);
		}
		else
		{
			SetGenes(new KoboldGenes().Randomize());
		}
		PlantSpawnEventHandler.TriggerPlantSpawnEvent(base.photonView.gameObject, plant);
	}

	private void UndarkenMaterials()
	{
		if (display == null)
		{
			return;
		}
		Renderer[] componentsInChildren = display.GetComponentsInChildren<Renderer>();
		foreach (Renderer renderer in componentsInChildren)
		{
			if (renderer.material.HasProperty("_Color"))
			{
				renderer.material.SetColor("_Color", Color.white);
			}
			if (renderer.material.HasProperty("_BaseColor"))
			{
				renderer.material.SetColor("_BaseColor", Color.white);
			}
		}
	}

	private IEnumerator DarkenMaterial(Material tgtMat)
	{
		float startTime = Time.time;
		float duration = 1f;
		while (Time.time < startTime + duration)
		{
			float t = (Time.time - startTime) / duration;
			if (tgtMat.HasProperty("_Color"))
			{
				tgtMat.SetColor("_Color", Color.Lerp(tgtMat.GetColor("_Color"), darkenedColor, t));
			}
			if (tgtMat.HasProperty("_BaseColor"))
			{
				tgtMat.SetColor("_BaseColor", Color.Lerp(tgtMat.GetColor("_BaseColor"), darkenedColor, t));
			}
			yield return null;
		}
	}

	public void Save(BinaryWriter writer)
	{
		writer.Write(PlantDatabase.GetID(plant));
		writer.Write(base.transform.position.x);
		writer.Write(base.transform.position.y);
		writer.Write(base.transform.position.z);
		GetGenes().Save(writer);
		writer.Write(growing);
	}

	public void Load(BinaryReader reader)
	{
		SwitchTo(PlantDatabase.GetPlant(reader.ReadInt16()));
		float x = reader.ReadSingle();
		float y = reader.ReadSingle();
		float z = reader.ReadSingle();
		base.transform.position = new Vector3(x, y, z);
		KoboldGenes loadedGenes = new KoboldGenes();
		loadedGenes.Load(reader);
		SetGenes(loadedGenes);
		if (reader.ReadBoolean())
		{
			Renderer[] componentsInChildren = display.GetComponentsInChildren<Renderer>();
			foreach (Renderer renderer in componentsInChildren)
			{
				renderer.material.SetFloat("_BounceAmount", 1f);
				StartCoroutine(DarkenMaterial(renderer.material));
			}
			wateredEffect.SendEvent("Play");
			audioSource.Play();
			effect.gameObject.SetActive(value: false);
			effect.gameObject.SetActive(value: true);
			StopCoroutine("GrowRoutine");
			StartCoroutine("GrowRoutine");
		}
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
	}
}
