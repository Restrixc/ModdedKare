using UnityEngine;

public class ResetFloats : MonoBehaviour
{
	public ScriptableFloat grabCount;

	public ScriptableFloat money;

	private void Start()
	{
		money.deplete();
		money.give(30f);
		grabCount.deplete();
		grabCount.give(1f);
	}

	private void OnEnable()
	{
		money.deplete();
		money.give(30f);
		grabCount.deplete();
		grabCount.give(1f);
	}
}
