using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace UnityScriptableSettings;

public class CameraSettingListener : MonoBehaviour
{
	public SettingInt antiAliasing;

	public SettingFloat fov;

	private UniversalAdditionalCameraData camData;

	private Camera cam;

	private void OnEnable()
	{
		cam = GetComponent<Camera>();
		camData = GetComponent<UniversalAdditionalCameraData>();
		antiAliasing.changed += OnAntiAliasingChanged;
		fov.changed += OnFOVChanged;
		OnAntiAliasingChanged(antiAliasing.GetValue());
		OnFOVChanged(fov.GetValue());
	}

	private void OnDisable()
	{
		antiAliasing.changed -= OnAntiAliasingChanged;
		fov.changed -= OnFOVChanged;
	}

	private void OnAntiAliasingChanged(int value)
	{
		cam.allowMSAA = (float)value != 0f;
		camData.antialiasing = ((value != 0) ? AntialiasingMode.SubpixelMorphologicalAntiAliasing : AntialiasingMode.None);
		switch (value)
		{
		case 1:
			camData.antialiasingQuality = AntialiasingQuality.Low;
			break;
		case 2:
			camData.antialiasingQuality = AntialiasingQuality.Medium;
			break;
		case 3:
			camData.antialiasingQuality = AntialiasingQuality.High;
			break;
		}
	}

	private void OnFOVChanged(float value)
	{
		cam.fieldOfView = value;
	}
}
