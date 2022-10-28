using System;
using System.Collections.Generic;
using System.Text;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

[Serializable]
public class CommandList : Command
{
	public override string GetArg0()
	{
		return "/list";
	}

	public override void Execute(StringBuilder output, Kobold k, string[] args)
	{
		base.Execute(output, k, args);
		bool didSomething = false;
		if (args.Length == 1 || args[1] == "prefabs" || args[1] == "objects")
		{
			if (!(PhotonNetwork.PrefabPool is DefaultPool pool))
			{
				throw new CheatsProcessor.CommandException("Failed to find PhotonNetwork pool, are you online??");
			}
			output.Append("Objects = {");
			foreach (KeyValuePair<string, GameObject> item in pool.ResourceCache)
			{
				output.Append(item.Key + ",\n");
			}
			output.Append("}\n");
			didSomething = true;
		}
		if (args.Length == 1 || args[1] == "reagents")
		{
			output.Append("Reagents = {");
			foreach (ScriptableReagent reagent in ReagentDatabase.GetReagents())
			{
				output.Append(reagent.name + ",\n");
			}
			output.Append("}\n");
			didSomething = true;
		}
		if (args.Length == 1 || args[1] == "equipment")
		{
			output.Append("Equipment = {");
			foreach (Equipment equipment in EquipmentDatabase.GetEquipments())
			{
				output.Append(equipment.name + ",\n");
			}
			output.Append("}\n");
			didSomething = true;
		}
		if (args.Length == 1 || args[1] == "players")
		{
			output.Append("Players = {\n");
			Player[] playerList = PhotonNetwork.PlayerList;
			foreach (Player player in playerList)
			{
				output.Append($"{player.ActorNumber} {player.NickName},\n");
			}
			output.Append("}\n");
			didSomething = true;
		}
		if (!didSomething)
		{
			throw new CheatsProcessor.CommandException("Usage: /list {prefabs,objects,reagents,}");
		}
	}
}
