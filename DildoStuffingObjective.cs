using System;
using System.Collections.Generic;
using PenetrationTech;
using UnityEngine;
using UnityEngine.Localization;

[Serializable]
public class DildoStuffingObjective : ObjectiveWithSpaceBeam
{
	[SerializeField]
	private LocalizedString description;

	[SerializeField]
	private int minDildosInserted = 3;

	private Dictionary<Kobold, HashSet<Dildo>> penetrationMemory;

	private void AddInsertion(Kobold k, Dildo d)
	{
		if (!penetrationMemory.ContainsKey(k))
		{
			penetrationMemory.Add(k, new HashSet<Dildo>());
		}
		penetrationMemory[k].Add(d);
		penetrationMemory[k].RemoveWhere((Dildo o) => o == null);
		TriggerUpdate();
		if (k != null && penetrationMemory[k].Count >= minDildosInserted)
		{
			Advance(k.transform.position);
		}
	}

	private void RemoveInsertion(Kobold k, Dildo d)
	{
		if (!penetrationMemory.ContainsKey(k))
		{
			penetrationMemory.Add(k, new HashSet<Dildo>());
		}
		penetrationMemory[k].Remove(d);
		TriggerUpdate();
	}

	public override void Register()
	{
		penetrationMemory = new Dictionary<Kobold, HashSet<Dildo>>();
		base.Register();
		Dildo.dildoPenetrateStart += OnDildoPenetrateStart;
		Dildo.dildoPenetrateEnd += OnDildoPenetrateEnd;
	}

	public override void Unregister()
	{
		base.Unregister();
		Dildo.dildoPenetrateStart -= OnDildoPenetrateStart;
		Dildo.dildoPenetrateEnd -= OnDildoPenetrateEnd;
		penetrationMemory.Clear();
	}

	private void OnDildoPenetrateStart(Penetrator penetrator, Penetrable penetrable)
	{
		Dildo d = penetrator.GetComponentInParent<Dildo>();
		Kobold i = penetrable.GetComponentInParent<Kobold>();
		if (!(d == null) && !(i == null))
		{
			AddInsertion(i, d);
		}
	}

	private void OnDildoPenetrateEnd(Penetrator penetrator, Penetrable penetrable)
	{
		Dildo d = penetrator.GetComponentInParent<Dildo>();
		Kobold i = penetrable.GetComponentInParent<Kobold>();
		if (!(d == null) && !(i == null))
		{
			RemoveInsertion(i, d);
		}
	}

	protected override void Advance(Vector3 position)
	{
		base.Advance(position);
		TriggerComplete();
	}

	public override string GetTitle()
	{
		int maxInsertedDildos = 0;
		if (penetrationMemory == null)
		{
			penetrationMemory = new Dictionary<Kobold, HashSet<Dildo>>();
		}
		foreach (KeyValuePair<Kobold, HashSet<Dildo>> item in penetrationMemory)
		{
			maxInsertedDildos = Mathf.Max(item.Value.Count, maxInsertedDildos);
		}
		return title.GetLocalizedString() + " " + maxInsertedDildos + "/" + minDildosInserted;
	}

	public override string GetTextBody()
	{
		return description.GetLocalizedString();
	}
}
