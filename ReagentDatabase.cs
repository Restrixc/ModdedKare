using System.Collections.Generic;
using UnityEngine;

public class ReagentDatabase : MonoBehaviour
{
	private static ReagentDatabase instance;

	private Dictionary<string, ScriptableReagent> reagentDictionary;

	public List<ScriptableReagent> reagents;

	public List<ScriptableReagentReaction> reactions;

	private static ReagentDatabase GetInstance()
	{
		if (instance == null)
		{
			instance = Object.FindObjectOfType<ReagentDatabase>();
		}
		return instance;
	}

	public void Awake()
	{
		if (instance != null)
		{
			Object.Destroy(this);
		}
		else
		{
			instance = this;
		}
		reagentDictionary = new Dictionary<string, ScriptableReagent>();
		foreach (ScriptableReagent reagent in reagents)
		{
			reagentDictionary.Add(reagent.name, reagent);
		}
	}

	public static ScriptableReagent GetReagent(string name)
	{
		if (GetInstance().reagentDictionary.ContainsKey(name))
		{
			return GetInstance().reagentDictionary[name];
		}
		return null;
	}

	public static ScriptableReagent GetReagent(short id)
	{
		return GetInstance().reagents[id];
	}

	public static short GetID(ScriptableReagent reagent)
	{
		if (GetInstance() == null)
		{
			return 0;
		}
		return (short)GetInstance().reagents.IndexOf(reagent);
	}

	public static List<ScriptableReagent> GetReagents()
	{
		return GetInstance().reagents;
	}

	public static void DoReactions(GenericReagentContainer container, ScriptableReagent introducedReactant)
	{
		DoReactions(container, GetID(introducedReactant));
	}

	public static void DoReactions(GenericReagentContainer container, short introducedReactant)
	{
		foreach (ScriptableReagentReaction reaction in GetInstance().reactions)
		{
			reaction.DoReaction(container);
		}
	}
}
