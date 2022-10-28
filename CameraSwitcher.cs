using UnityEngine;

public class CameraSwitcher : MonoBehaviour
{
	public enum CameraMode
	{
		FirstPerson,
		ThirdPerson,
		FreeCam,
		FreeCamLocked
	}

	public GameObject FPSCanvas;

	public Camera firstperson;

	public Camera thirdperson;

	public Camera freecam;

	public Transform uiSlider;

	private bool initialized = false;

	public PlayerPossession possession;

	public CameraMode mode = CameraMode.FirstPerson;

	public void OnEnable()
	{
		initialized = false;
		SwitchCamera(CameraMode.FirstPerson);
	}

	public void OnDisable()
	{
		firstperson.enabled = false;
		thirdperson.enabled = false;
		freecam.enabled = false;
	}

	public void Update()
	{
		uiSlider.transform.localPosition = Vector3.Lerp(uiSlider.transform.localPosition, -Vector3.right * (30f * ((float)mode + 0.5f)), Time.deltaTime * 2f);
	}

	public void OnSwitchCamera()
	{
		int index = (int)(mode + 1) % 4;
		SwitchCamera((CameraMode)index);
	}

	public void OnFirstPerson()
	{
		SwitchCamera(CameraMode.FirstPerson);
	}

	public void OnThirdPerson()
	{
		SwitchCamera(CameraMode.ThirdPerson);
	}

	public void OnFreeCamera()
	{
		SwitchCamera(CameraMode.FreeCam);
	}

	public void OnLockedCamera()
	{
		SwitchCamera(CameraMode.FreeCamLocked);
	}

	public void SwitchCamera(CameraMode cameraMode)
	{
		if (Cursor.lockState != CursorLockMode.Locked && initialized)
		{
			return;
		}
		initialized = true;
		mode = cameraMode;
		firstperson.enabled = false;
		thirdperson.enabled = false;
		freecam.enabled = false;
		possession.enabled = true;
		freecam.GetComponent<SimpleCameraController>().enabled = false;
		switch (mode)
		{
		case CameraMode.FirstPerson:
			firstperson.enabled = true;
			if (!FPSCanvas.activeInHierarchy)
			{
				FPSCanvas.SetActive(value: true);
			}
			break;
		case CameraMode.ThirdPerson:
			thirdperson.enabled = true;
			if (!FPSCanvas.activeInHierarchy)
			{
				FPSCanvas.SetActive(value: true);
			}
			break;
		case CameraMode.FreeCam:
			freecam.enabled = true;
			freecam.GetComponent<SimpleCameraController>().enabled = true;
			possession.enabled = false;
			possession.controller.inputDir = Vector3.zero;
			possession.controller.inputJump = false;
			if (FPSCanvas.activeInHierarchy)
			{
				FPSCanvas.SetActive(value: false);
			}
			break;
		case CameraMode.FreeCamLocked:
			freecam.enabled = true;
			freecam.GetComponent<SimpleCameraController>().enabled = false;
			possession.enabled = true;
			if (!FPSCanvas.activeInHierarchy)
			{
				FPSCanvas.SetActive(value: true);
			}
			break;
		}
	}
}
