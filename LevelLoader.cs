using System.Collections;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
	public delegate void SceneEventAction();

	public static LevelLoader instance;

	public static bool loadingLevel;

	public event SceneEventAction sceneLoadStart;

	public event SceneEventAction sceneLoadEnd;

	private void Awake()
	{
		if (instance == null)
		{
			instance = this;
		}
		else if (instance != this)
		{
			Object.Destroy(base.gameObject);
		}
	}

	public Coroutine LoadLevel(string name)
	{
		StopAllCoroutines();
		return StartCoroutine(LoadLevelRoutine(name));
	}

	public IEnumerator LoadLevelRoutine(string name)
	{
		GameManager.instance.Pause(pause: false);
		this.sceneLoadStart?.Invoke();
		loadingLevel = true;
		yield return new WaitForSecondsRealtime(1f);
		PhotonNetwork.LoadLevel(name);
		while (!SceneManager.GetSceneByName(name).isLoaded)
		{
			yield return new WaitForEndOfFrame();
		}
		loadingLevel = false;
		SingletonScriptableObject<PopupHandler>.instance.ClearAllPopups();
		GameManager.instance.Pause(pause: false);
		this.sceneLoadEnd?.Invoke();
	}
}
