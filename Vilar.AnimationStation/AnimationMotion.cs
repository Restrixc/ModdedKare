using UnityEngine;

namespace Vilar.AnimationStation;


public class AnimationMotion : ScriptableObject
{
	public AnimationCurve X = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 0f));

	public AnimationCurve Y = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 0f));

	public AnimationCurve Z = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 0f));

	public Vector3 scale = Vector3.zero;

	public Vector3 timescale = Vector3.one;

	public Vector3 offset = Vector3.zero;

	public AnimationCurve RX = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 0f));

	public AnimationCurve RY = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 0f));

	public AnimationCurve RZ = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 0f));

	public Vector3 scaleR = Vector3.zero;

	public Vector3 timescaleR = Vector3.one;

	public Vector3 offsetR = Vector3.zero;
}
