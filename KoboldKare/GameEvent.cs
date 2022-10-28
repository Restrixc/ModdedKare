using System;
using UnityEngine;

namespace KoboldKare;

[Serializable]
public class GameEvent<T> : ScriptableObject
{
	public delegate void GameEventActionGeneric(T arg);

	[NonSerialized]
	private T lastInvokeValue;

	private event GameEventActionGeneric raised;

	public void AddListener(GameEventActionGeneric listener)
	{
		raised += listener;
	}

	public void RemoveListener(GameEventActionGeneric listener)
	{
		raised -= listener;
	}

	public T GetLastInvokeValue()
	{
		return lastInvokeValue;
	}

	public void Raise(T arg)
	{
		lastInvokeValue = arg;
		this.raised?.Invoke(arg);
	}
}
