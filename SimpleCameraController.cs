using UnityEngine;
using UnityEngine.InputSystem;
using UnityScriptableSettings;

public class SimpleCameraController : MonoBehaviour
{
	private class CameraState
	{
		public float yaw;

		public float pitch;

		public float roll;

		public float x;

		public float y;

		public float z;

		public void SetFromTransform(Transform t)
		{
			pitch = t.eulerAngles.x;
			yaw = t.eulerAngles.y;
			roll = t.eulerAngles.z;
			x = t.position.x;
			y = t.position.y;
			z = t.position.z;
		}

		public void Translate(Vector3 translation)
		{
			Vector3 rotatedTranslation = Quaternion.Euler(pitch, yaw, roll) * translation;
			x += rotatedTranslation.x;
			y += rotatedTranslation.y;
			z += rotatedTranslation.z;
		}

		public void LerpTowards(CameraState target, float positionLerpPct, float rotationLerpPct)
		{
			yaw = Mathf.Lerp(yaw, target.yaw, rotationLerpPct);
			pitch = Mathf.Lerp(pitch, target.pitch, rotationLerpPct);
			roll = Mathf.Lerp(roll, target.roll, rotationLerpPct);
			x = Mathf.Lerp(x, target.x, positionLerpPct);
			y = Mathf.Lerp(y, target.y, positionLerpPct);
			z = Mathf.Lerp(z, target.z, positionLerpPct);
		}

		public void UpdateTransform(Transform t)
		{
			t.eulerAngles = new Vector3(pitch, yaw, roll);
			t.position = new Vector3(x, y, z);
		}
	}

	public PlayerInput controls;

	private CameraState m_TargetCameraState = new CameraState();

	private CameraState m_InterpolatingCameraState = new CameraState();

	[Header("Movement Settings")]
	[Tooltip("Exponential boost factor on translation, controllable by mouse wheel.")]
	public float boost = 3.5f;

	[Range(0.001f, 1f)]
	[Tooltip("Time it takes to interpolate camera position 99% of the way to the target.")]
	public float positionLerpTime = 0.2f;

	[Header("Rotation Settings")]
	[Tooltip("X = Change in mouse position.\nY = Multiplicative factor for camera rotation.")]
	public AnimationCurve mouseSensitivityCurve = new AnimationCurve(new Keyframe(0f, 0.5f, 0f, 5f), new Keyframe(1f, 2.5f, 0f, 0f));

	[Range(0.001f, 1f)]
	[Tooltip("Time it takes to interpolate camera rotation 99% of the way to the target.")]
	public float rotationLerpTime = 0.01f;

	[Tooltip("Whether or not to invert our Y axis for mouse input to rotation.")]
	public bool invertY = false;

	public SettingFloat mouseSensitivity;

	private void OnEnable()
	{
		m_TargetCameraState.SetFromTransform(base.transform);
		m_InterpolatingCameraState.SetFromTransform(base.transform);
	}

	private Vector3 GetInputTranslationDirection()
	{
		Vector3 direction = default(Vector3);
		Vector2 moveInput = controls.actions["Move"].ReadValue<Vector2>();
		direction += new Vector3(moveInput.x, 0f, moveInput.y);
		if (controls.actions["Jump"].ReadValue<float>() > 0.5f)
		{
			direction += Vector3.up;
		}
		if (controls.actions["Crouch"].ReadValue<float>() > 0.5f)
		{
			direction += Vector3.down;
		}
		return direction;
	}

	private void LateUpdate()
	{
		Vector2 mouseMovement = Mouse.current.delta.ReadValue() + controls.actions["Look"].ReadValue<Vector2>() * 40f;
		m_TargetCameraState.yaw += mouseMovement.x * mouseSensitivity.GetValue();
		m_TargetCameraState.pitch -= mouseMovement.y * mouseSensitivity.GetValue();
		m_TargetCameraState.roll = 0f;
		Vector3 translation = GetInputTranslationDirection() * Time.deltaTime;
		if (controls.actions["Walk"].ReadValue<float>() > 0.5f)
		{
			translation /= 10f;
		}
		boost += controls.actions["Grab Push and Pull"].ReadValue<float>() * 0.002f;
		translation *= Mathf.Pow(2f, boost);
		m_TargetCameraState.Translate(translation);
		float positionLerpPct = 1f - Mathf.Exp(Mathf.Log(0.00999999f) / positionLerpTime * Time.deltaTime);
		float rotationLerpPct = 1f - Mathf.Exp(Mathf.Log(0.00999999f) / rotationLerpTime * Time.deltaTime);
		m_InterpolatingCameraState.LerpTowards(m_TargetCameraState, positionLerpPct, rotationLerpPct);
		m_InterpolatingCameraState.UpdateTransform(base.transform);
	}
}
