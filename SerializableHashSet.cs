using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SerializableHashSet<T> : ISerializationCallbackReceiver
{
	public HashSet<T> hashSet = new HashSet<T>();

	[SerializeField]
	public List<T> values = new List<T>();

	public void OnBeforeSerialize()
	{
		values = new List<T>(hashSet);
	}

	public void OnAfterDeserialize()
	{
		hashSet = new HashSet<T>(values);
	}
}
