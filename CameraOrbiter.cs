using UnityEngine;

public class CameraOrbiter : MonoBehaviour
{
	public float _distance;

	public Vector3 _offset;

	public float rotspeed = 0.1f;

	private Vector3 _origin;

	private void Start()
	{
		_origin = base.transform.position;
	}

	private void Update()
	{
		base.transform.position = _origin + new Vector3(Mathf.Sin(Time.time * rotspeed), 0f, Mathf.Cos(Time.time * rotspeed)) * _distance + _offset;
		base.transform.rotation = Quaternion.LookRotation(_origin - base.transform.position);
	}
}
