using UnityEngine;
using UnityEngine.SceneManagement;

public class ActiveOnScene : MonoBehaviour
{
	[SerializeField]
	private string activateOnSceneName;

	private void OnEnable()
	{
		SceneManager.sceneLoaded += OnSceneChange;
	}

	private void OnSceneChange(Scene scene, LoadSceneMode mode)
	{
		base.gameObject.SetActive(scene.name == activateOnSceneName);
	}
}
