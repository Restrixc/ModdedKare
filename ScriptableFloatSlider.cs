using UnityEngine.UI;

public class ScriptableFloatSlider : Slider
{
	public ScriptableFloat val;

	public override float value
	{
		get
		{
			if (val != null)
			{
				return val.value;
			}
			return base.value;
		}
		set
		{
			if (val != null)
			{
				val.set(value);
			}
			base.value = value;
		}
	}

	public new void Start()
	{
		base.maxValue = val.max;
		base.minValue = val.min;
	}
}
