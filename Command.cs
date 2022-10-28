using System;
using System.Text;
using UnityEngine;
using UnityEngine.Localization;

[Serializable]
public class Command
{
	[SerializeField]
	private LocalizedString description;

	public virtual string GetArg0()
	{
		return "null";
	}

	public virtual LocalizedString GetDescription()
	{
		return description;
	}

	public virtual void Execute(StringBuilder output, Kobold k, string[] args)
	{
	}

	public virtual void OnValidate()
	{
	}
}
