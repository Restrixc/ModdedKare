using System.Collections;
using UnityEngine;

public static class Screenshotter
{
	public delegate void ScreenshotFinishedAction(Texture2D screenshot);

	public static void GetScreenshot(ScreenshotFinishedAction action)
	{
		GameManager.instance.StartCoroutine(ScreenshotRoutine(action));
	}

	private static IEnumerator ScreenshotRoutine(ScreenshotFinishedAction action, bool resize = true)
	{
		Camera cam = Camera.main;
		if (cam == null)
		{
			action(null);
			yield break;
		}
		GameManager.SetUIVisible(visible: false);
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		Texture2D texture = ScreenCapture.CaptureScreenshotAsTexture();
		action(resize ? Resize(texture, 640, 360) : texture);
		GameManager.SetUIVisible(visible: true);
	}

	private static Texture2D Resize(Texture2D texture2D, int targetX, int targetY)
	{
		RenderTexture rt = (RenderTexture.active = new RenderTexture(targetX, targetY, 24));
		Graphics.Blit(texture2D, rt);
		Texture2D result = new Texture2D(targetX, targetY);
		result.ReadPixels(new Rect(0f, 0f, targetX, targetY), 0, 0);
		result.Apply();
		return result;
	}
}
