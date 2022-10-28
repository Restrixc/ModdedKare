using UnityEngine;

public class Cloud : PooledItem
{
	private Bounds bounds;

	private float speed;

	public void SetBounds(Bounds newBounds)
	{
		bounds = newBounds;
	}

	private void Start()
	{
		speed = Random.Range(2f, 8f);
		base.transform.localScale = Vector3.one * Random.Range(25f, 40f);
		base.transform.rotation = Random.rotation;
	}

	public override void Reset()
	{
		speed = Random.Range(2f, 8f);
		base.transform.position = new Vector3(bounds.center.x + bounds.extents.x, Random.Range(bounds.center.y - bounds.extents.y, bounds.center.y + bounds.extents.y), Random.Range(bounds.center.z - bounds.extents.z, bounds.center.z + bounds.extents.z));
		base.Reset();
	}

	private void Update()
	{
		base.transform.position -= Vector3.right * (Time.deltaTime * speed);
		base.transform.rotation = Quaternion.AngleAxis((0f - Time.deltaTime) * speed, Vector3.forward) * base.transform.rotation;
		if (base.transform.position.x < bounds.center.x - bounds.extents.x)
		{
			Reset();
		}
	}
}
