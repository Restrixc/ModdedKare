using System.Collections;
using System.Collections.Generic;
using System.IO;
using KoboldKare;
using Photon.Pun;
using UnityEngine;
using UnityEngine.VFX;

public class WeatherManager : MonoBehaviourPun, IPunObservable, ISavable
{
	[Range(0f, 1f)]
	public float rainAmount = 0f;

	public float fogRangeWhenRaining;

	public Color fogColorWhenRaining;

	private float origFogRange;

	private Color origFogColor;

	public Material skyboxMaterial;

	public VisualEffect dust;

	public VisualEffect rain;

	public ParticleSystem rainBackup;

	private VisualEffect spawnedEffect;

	public AudioSource rainSounds;

	public AudioSource thunderSounds;

	public GameEventGeneric midnightEvent;

	private Camera cachedCamera;

	[SerializeField]
	private ReagentContents rainContents = new ReagentContents();

	public Material splashMaterial;

	private HashSet<PhotonView> views = new HashSet<PhotonView>();

	public Coroutine rainVFXUpdate;

	private Camera cam
	{
		get
		{
			if (cachedCamera == null || !cachedCamera.isActiveAndEnabled)
			{
				cachedCamera = Camera.current;
			}
			if (cachedCamera == null || !cachedCamera.isActiveAndEnabled)
			{
				cachedCamera = Camera.main;
			}
			return cachedCamera;
		}
	}

	public IEnumerator WaitAndThenClear()
	{
		yield return new WaitForSeconds(5f);
		views.Clear();
	}

	private void Awake()
	{
		origFogRange = RenderSettings.fogDensity;
		origFogColor = RenderSettings.fogColor;
		midnightEvent.AddListener(OnMidnight);
	}

	private void OnMidnight(object ignore)
	{
		StopRain();
		RandomlyRain(0.15f);
	}

	private void OnDestroy()
	{
		skyboxMaterial.SetFloat("_CloudDensityOffset", -0.3f);
		skyboxMaterial.SetFloat("_Brightness", 0.5f);
		midnightEvent.RemoveListener(OnMidnight);
	}

	public IEnumerator Rain()
	{
		rainSounds.enabled = true;
		thunderSounds.enabled = true;
		while (rainAmount < 1f)
		{
			rainAmount = Mathf.MoveTowards(rainAmount, 1f, Time.deltaTime * 0.1f);
			RenderSettings.fogDensity = Mathf.MoveTowards(RenderSettings.fogDensity, fogRangeWhenRaining, Time.deltaTime * 0.1f);
			RenderSettings.fogColor = Color.Lerp(origFogColor, fogColorWhenRaining, Time.deltaTime * 0.1f);
			rainSounds.volume = rainAmount * 0.7f;
			yield return null;
		}
		while (rainAmount >= 1f)
		{
			yield return new WaitForSeconds(100f);
			FillRaincatchers("Water");
			thunderSounds.Play();
		}
	}

	public IEnumerator StopRainRoutine()
	{
		while (rainAmount > 0f)
		{
			rainAmount = Mathf.MoveTowards(rainAmount, 0f, Time.fixedDeltaTime * 0.1f);
			RenderSettings.fogDensity = Mathf.MoveTowards(RenderSettings.fogDensity, origFogRange, Time.fixedDeltaTime * 0.1f);
			RenderSettings.fogColor = Color.Lerp(fogColorWhenRaining, origFogColor, Time.fixedDeltaTime * 0.1f);
			rainSounds.volume = rainAmount * 0.7f;
			rain.Stop();
			yield return new WaitForFixedUpdate();
		}
		rainSounds.enabled = false;
		thunderSounds.enabled = false;
	}

	public void StopRain()
	{
		if (base.photonView.IsMine)
		{
			StopCoroutine("Rain");
			StartCoroutine("StopRainRoutine");
		}
	}

	public void RandomlyRain(float chance)
	{
		if (base.photonView.IsMine)
		{
			if (Random.Range(0f, 1f) < chance)
			{
				StopCoroutine("StopRainRoutine");
				StopCoroutine("Rain");
				StartCoroutine("Rain");
				rain.SendEvent("Fire");
			}
			else
			{
				StopRain();
			}
		}
	}

	public void FillRaincatchers(string Reagant)
	{
	}

	private void Update()
	{
		if (Mathf.Approximately(rainAmount, 0f) && !dust.gameObject.activeInHierarchy)
		{
			dust.gameObject.SetActive(value: true);
		}
		if (!Mathf.Approximately(rainAmount, 0f) && dust.gameObject.activeInHierarchy)
		{
			dust.gameObject.SetActive(value: false);
		}
		skyboxMaterial.SetFloat("_CloudDensityOffset", Mathf.Lerp(-0.75f, 0.1f, rainAmount));
		skyboxMaterial.SetFloat("_Brightness", Mathf.Lerp(0.5f, 0.25f, rainAmount));
	}

	public void LateUpdate()
	{
		if (cam != null)
		{
			base.transform.position = cam.transform.position + Vector3.up * 10f;
		}
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.IsWriting)
		{
			stream.SendNext(rainAmount);
		}
		else
		{
			rainAmount = (float)stream.ReceiveNext();
		}
	}

	public void Save(BinaryWriter writer)
	{
		writer.Write(rainAmount);
	}

	public void Load(BinaryReader reader)
	{
		rainAmount = reader.ReadSingle();
	}
}
