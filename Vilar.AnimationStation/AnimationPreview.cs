using UnityEngine;
using Vilar.IK;

namespace Vilar.AnimationStation;

public class AnimationPreview : MonoBehaviour
{
	public static AnimationPreview instance;

	public IKSolver IKSolver;

	private int previewCountdown = 1;

	public static void EnsureInstance()
	{
		if (instance == null)
		{
			instance = Object.FindObjectOfType<AnimationPreview>();
		}
	}

	public static void SetTarget(int index, Vector3 position, Quaternion rotation)
	{
		EnsureInstance();
		instance._SetTarget(index, position, rotation);
	}

	public static void MovePreviews(Vector3 position, Quaternion rotation)
	{
		EnsureInstance();
		instance._MovePreviews(position, rotation);
	}

	public static void Solve()
	{
		instance._Solve();
	}

	public static void Initialize()
	{
		EnsureInstance();
		instance._Initialize();
	}

	private void OnEnable()
	{
		instance = this;
		_Initialize();
	}

	public void Idle()
	{
		previewCountdown--;
		if (previewCountdown == 0)
		{
			Initialize();
		}
	}

	[ContextMenu("INITIALIZE")]
	public void _Initialize()
	{
		IKSolver.Initialize();
		base.transform.position = -Vector3.up * 100f;
	}

	public void _MovePreviews(Vector3 position, Quaternion rotation)
	{
		previewCountdown = 3;
		base.transform.position = position;
		base.transform.rotation = rotation;
	}

	public void _SetTarget(int index, Vector3 position, Quaternion rotation)
	{
		IKSolver.SetTarget(index, position, rotation);
	}

	public void _Solve()
	{
		IKSolver.Solve();
	}
}
