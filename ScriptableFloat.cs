using System;
using UnityEngine;
using UnityEngine.Events;

public class ScriptableFloat : ScriptableObject
{
	[Serializable]
	public class UnityFloatEvent : UnityEvent<float>
	{
	}

	public float startingValue = 0f;

	public float startingMax = 100f;

	public float startingMin = 0f;

	public UnityFloatEvent OnExhaust;

	public UnityFloatEvent OnFull;

	public UnityFloatEvent OnChanged;

	[NonSerialized]
	private float lastValue = 0f;

	[NonSerialized]
	public float max = 100f;

	[NonSerialized]
	public float min = 0f;

	public float value { get; private set; }

	private void OnEnable()
	{
		value = startingValue;
		lastValue = startingValue;
		max = startingMax;
		min = startingMin;
	}

	public void setWithoutNotify(float amount)
	{
		value = amount;
		lastValue = amount;
	}

	public void set(float amount)
	{
		value = Mathf.Min(Mathf.Max(min, amount), max);
		Check();
	}

	public void deplete()
	{
		value = min;
		Check();
	}

	public void fill()
	{
		if (value != max)
		{
			value = max;
		}
		Check();
	}

	public bool has(float amount)
	{
		return value >= amount;
	}

	public bool charge(float amount)
	{
		if (value >= amount)
		{
			take(amount);
			return true;
		}
		return false;
	}

	public void take(float amount)
	{
		if (!(value <= min))
		{
			value = Mathf.Max(value - amount, min);
			Check();
		}
	}

	public void give(float amount)
	{
		if (!(value >= max))
		{
			value = Mathf.Min(value + amount, max);
			Check();
		}
	}

	public void setMin(float amount)
	{
		min = amount;
		value = Mathf.Max(value, min);
		Check();
	}

	public void setMax(float amount)
	{
		max = amount;
		value = Mathf.Min(value, max);
		Check();
	}

	private void Check()
	{
		if (lastValue != value)
		{
			lastValue = value;
			OnChanged.Invoke(value);
		}
		if (value <= min)
		{
			OnExhaust.Invoke(value);
		}
		if (value >= max)
		{
			OnFull.Invoke(value);
		}
	}
}
