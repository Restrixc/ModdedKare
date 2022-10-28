using UnityEngine;

public class InherentWorth : MonoBehaviour, IValuedGood
{
	[SerializeField]
	private float worth;

	public float GetWorth()
	{
		return worth;
	}
}
