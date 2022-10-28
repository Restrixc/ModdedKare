using System.Collections;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityScriptableSettings;

public class GameManager : MonoBehaviour
{
	public static GameManager instance;

	[SerializeField]
	private GameObject mainCanvas;

	public AudioMixerGroup soundEffectGroup;

	public AudioMixerGroup soundEffectLoudGroup;

	public LayerMask precisionGrabMask;

	public LayerMask walkableGroundMask;

	public LayerMask waterSprayHitMask;

	public LayerMask plantHitMask;

	public LayerMask decalHitMask;

	public LayerMask usableHitMask;

	public AnimationCurve volumeCurve;

	public GameObject selectOnPause;

	public AudioClip buttonHoveredMenu;

	public AudioClip buttonHoveredSubmenu;

	public AudioClip buttonClickedMenu;

	public AudioClip buttonClickedSubmenu;

	public LoadingListener loadListener;

	[SerializeField]
	private GameObject MultiplayerTab;

	[SerializeField]
	private GameObject OptionsTab;

	[SerializeField]
	private GameObject MainViewTab;

	[SerializeField]
	private GameObject CreditsTab;

	[SerializeField]
	private GameObject SaveTab;

	[HideInInspector]
	public bool isPaused = false;

	public static void SetUIVisible(bool visible)
	{
		instance.UIVisible(visible);
	}

	public static Coroutine StartCoroutineStatic(IEnumerator routine)
	{
		if (instance == null)
		{
			return null;
		}
		return instance.StartCoroutine(routine);
	}

	public void Pause(bool pause)
	{
		SingletonScriptableObject<PopupHandler>.instance.ClearAllPopups();
		if (!pause)
		{
			InputOptions.SaveControls();
			SettingsManager.Save();
			MultiplayerTab.gameObject.SetActive(value: false);
			OptionsTab.gameObject.SetActive(value: false);
			CreditsTab.gameObject.SetActive(value: false);
			SaveTab.gameObject.SetActive(value: false);
			MainViewTab.gameObject.SetActive(value: true);
		}
		if (pause)
		{
		}
		isPaused = pause;
		if (!isPaused && SceneManager.GetActiveScene().name != "MainMenu")
		{
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
		}
		else
		{
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		}
		mainCanvas.SetActive(isPaused || SceneManager.GetActiveScene().name == "MainMenu");
		if (!PhotonNetwork.OfflineMode || SceneManager.GetActiveScene().name == "MainMenu")
		{
			Time.timeScale = 1f;
			return;
		}
		Time.timeScale = (isPaused ? 0f : 1f);
		if (selectOnPause != null)
		{
			selectOnPause.GetComponent<Selectable>().Select();
		}
		else
		{
			Debug.LogError("[GameManager] selectOnPause is not bound to the resume button! Button was not selected for controller support.");
		}
	}

	public void Quit()
	{
		Application.Quit();
	}

	private void Awake()
	{
		if (instance == null)
		{
			instance = this;
		}
		else if (instance != this)
		{
			Object.Destroy(base.gameObject);
			return;
		}
		PhotonNetwork.AddCallbackTarget(SingletonScriptableObject<NetworkManager>.instance);
		Object.DontDestroyOnLoad(base.gameObject);
	}

	private void Start()
	{
		if (Application.isEditor && SceneManager.GetActiveScene().name != "MainMenu")
		{
			SingletonScriptableObject<NetworkManager>.instance.StartSinglePlayer();
			instance.Pause(pause: false);
		}
		SaveManager.Init();
	}

	private void UIVisible(bool visible)
	{
		Canvas[] componentsInChildren = GetComponentsInChildren<Canvas>();
		foreach (Canvas c in componentsInChildren)
		{
			c.enabled = visible;
		}
		if (Camera.main != null && Camera.main.gameObject.GetComponentInChildren<Canvas>(includeInactive: true) != null)
		{
			Camera.main.gameObject.GetComponentInChildren<Canvas>(includeInactive: true).enabled = visible;
		}
	}

	public AudioClip SpawnAudioClipInWorld(AudioPack pack, Vector3 position)
	{
		GameObject g = new GameObject("One shot Audio");
		g.transform.position = position;
		AudioSource source = g.AddComponent<AudioSource>();
		source.rolloffMode = AudioRolloffMode.Custom;
		source.SetCustomCurve(AudioSourceCurveType.CustomRolloff, volumeCurve);
		source.minDistance = 0f;
		source.maxDistance = 25f;
		source.outputAudioMixerGroup = soundEffectGroup;
		source.spatialBlend = 1f;
		source.pitch = Random.Range(0.85f, 1.15f);
		pack.Play(source);
		Object.Destroy(g, source.clip.length);
		return source.clip;
	}

	public void SpawnAudioClipInWorld(AudioClip clip, Vector3 position, float volume = 1f, AudioMixerGroup group = null)
	{
		if (group == null)
		{
			group = soundEffectGroup;
		}
		GameObject g = new GameObject("One shot Audio");
		g.transform.position = position;
		AudioSource source = g.AddComponent<AudioSource>();
		source.rolloffMode = AudioRolloffMode.Custom;
		source.SetCustomCurve(AudioSourceCurveType.CustomRolloff, volumeCurve);
		source.minDistance = 0f;
		source.maxDistance = 25f;
		source.outputAudioMixerGroup = soundEffectGroup;
		source.clip = clip;
		source.spatialBlend = 1f;
		source.volume = volume;
		source.pitch = Random.Range(0.85f, 1.15f);
		source.Play();
		Object.Destroy(g, clip.length);
	}

	public void PlayUISFX(ButtonMouseOver btn, ButtonMouseOver.EventType evtType)
	{
		if (btn.buttonType == ButtonMouseOver.ButtonTypes.Default)
		{
			if (evtType == ButtonMouseOver.EventType.Hover)
			{
				SpawnAudioClipInWorld(buttonHoveredMenu, Vector3.zero);
			}
			else
			{
				SpawnAudioClipInWorld(buttonClickedMenu, Vector3.zero);
			}
		}
		else if (btn.buttonType == ButtonMouseOver.ButtonTypes.Save)
		{
			if (evtType == ButtonMouseOver.EventType.Hover)
			{
				SpawnAudioClipInWorld(buttonHoveredSubmenu, Vector3.zero);
			}
			else
			{
				SpawnAudioClipInWorld(buttonClickedSubmenu, Vector3.zero);
			}
		}
	}
}
