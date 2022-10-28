using UnityEngine;

public class LoadingListener : MonoBehaviour
{
	private void Start()
	{
		LevelLoader.instance.sceneLoadStart += SceneLoadStart;
		LevelLoader.instance.sceneLoadEnd += SceneLoadEnd;
		base.gameObject.SetActive(value: false);
	}

	private void SceneLoadStart()
	{
		base.gameObject.SetActive(value: true);
		base.gameObject.GetComponent<CanvasGroup>().alpha = 1f;
	}

	private void OnDestroy()
	{
		LevelLoader.instance.sceneLoadStart -= SceneLoadStart;
		LevelLoader.instance.sceneLoadEnd -= SceneLoadEnd;
	}

	private void SceneLoadEnd()
	{
		base.gameObject.SetActive(value: false);
		base.gameObject.GetComponent<CanvasGroup>().alpha = 0f;
	}

	public void Show()
	{
		base.gameObject.SetActive(value: true);
		base.gameObject.GetComponent<CanvasGroup>().alpha = 1f;
	}

	public void Hide()
	{
		base.gameObject.SetActive(value: false);
		base.gameObject.GetComponent<CanvasGroup>().alpha = 0f;
	}
}
