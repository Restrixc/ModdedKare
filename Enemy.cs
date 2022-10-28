using UnityEngine;

public class Enemy : MonoBehaviour
{
	public enum AIType
	{
		Generic
	}

	public enum AIStates
	{
		Sleep,
		Idle,
		Hunt,
		Flee
	}

	public AIType myAI;

	public AIStates curAIState;
}
