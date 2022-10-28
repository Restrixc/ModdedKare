using UnityEngine;
using UnityEngine.InputSystem;

public class LookAtCursor : MonoBehaviour
{
	[Range(0f, 20f)]
	public float distanceFromCamera = 5f;

	private CharacterControllerAnimator characterAnimator;

	private Animator animator;

	private void Start()
	{
		characterAnimator = GetComponentInParent<CharacterControllerAnimator>();
		animator = GetComponentInChildren<Animator>();
	}

	private void Update()
	{
		Vector2 mousePos = Mouse.current.position.ReadValue();
		Vector3 headPos = animator.GetBoneTransform(HumanBodyBones.Head).position;
		Vector3 lookPoint = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, distanceFromCamera), Camera.MonoOrStereoscopicEye.Mono);
		characterAnimator.SetEyeDir((lookPoint - headPos).normalized);
	}
}
