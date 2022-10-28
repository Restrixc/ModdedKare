using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class OnPlayerEnter : MonoBehaviour
{
	[Serializable]
	public class Condition : SerializableCallback<bool>
	{
	}

	[Serializable]
	public class ConditionEventPair
	{
		[SerializeField]
		public List<Condition> conditions;

		public UnityEvent even;
	}

	public bool entered = false;

	public float delay = 1f;

	[SerializeField]
	public List<ConditionEventPair> onEnterEvents;

	[SerializeField]
	public List<ConditionEventPair> onExitEvents;

	private IEnumerator OnEnterDelay()
	{
		yield return new WaitForSeconds(delay);
		foreach (ConditionEventPair pair in onEnterEvents)
		{
			bool run = true;
			foreach (Condition cond in pair.conditions)
			{
				run &= cond.Invoke();
			}
			if (run)
			{
				pair.even.Invoke();
			}
		}
	}

	private IEnumerator OnExitDelay()
	{
		yield return new WaitForSeconds(delay);
		foreach (ConditionEventPair pair in onExitEvents)
		{
			bool run = true;
			foreach (Condition cond in pair.conditions)
			{
				run &= cond.Invoke();
			}
			if (run)
			{
				pair.even.Invoke();
			}
		}
	}

	public void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.layer == LayerMask.NameToLayer("Player") && other.transform.root.gameObject.layer == LayerMask.NameToLayer("Player"))
		{
			PlayerPossession p = other.transform.root.GetComponentInChildren<PlayerPossession>();
			if (p != null && p.gameObject.activeInHierarchy)
			{
				StopAllCoroutines();
				StartCoroutine(OnEnterDelay());
			}
		}
	}

	public void OnTriggerExit(Collider other)
	{
		if (other.gameObject.layer == LayerMask.NameToLayer("Player") && other.transform.root.gameObject.layer == LayerMask.NameToLayer("Player"))
		{
			PlayerPossession p = other.transform.root.GetComponentInChildren<PlayerPossession>();
			if (p != null && p.gameObject.activeInHierarchy)
			{
				StopAllCoroutines();
				StartCoroutine(OnExitDelay());
			}
		}
	}
}
