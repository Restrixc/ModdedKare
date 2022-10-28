using System;
using System.Text;
using Photon.Pun;
using UnityEngine;

[Serializable]
public class CommandGive : Command
{
	[SerializeField]
	private PhotonGameObjectReference bucket;

	public override string GetArg0()
	{
		return "/give";
	}

	public override void Execute(StringBuilder output, Kobold kobold, string[] args)
	{
		base.Execute(output, kobold, args);
		if (!CheatsProcessor.GetCheatsEnabled())
		{
			throw new CheatsProcessor.CommandException("Cheats are not enabled, use `/cheats 1` to enable cheats.");
		}
		if (args.Length < 2)
		{
			throw new CheatsProcessor.CommandException("/give requires at least one argument. Use /list to find what you can spawn.");
		}
		DefaultPool pool = PhotonNetwork.PrefabPool as DefaultPool;
		Transform koboldTransform = kobold.hip.transform;
		if (pool != null && pool.ResourceCache.ContainsKey(args[1]))
		{
			PhotonNetwork.InstantiateRoomObject(args[1], koboldTransform.position + koboldTransform.forward, Quaternion.identity, 0);
			output.Append("Spawned " + args[1] + ".\n");
			return;
		}
		if (ReagentDatabase.GetReagent(args[1]) != null)
		{
			GameObject obj = PhotonNetwork.InstantiateRoomObject(bucket.photonName, koboldTransform.position + koboldTransform.forward, Quaternion.identity, 0);
			ReagentContents contents = new ReagentContents();
			if (args.Length > 2 && float.TryParse(args[2], out var value3))
			{
				contents.AddMix(ReagentDatabase.GetReagent(args[1]).GetReagent(value3));
				obj.GetPhotonView().RPC("ForceMixRPC", RpcTarget.All, contents, kobold.photonView.ViewID);
				output.Append($"Spawned bucket filled with {value3} {args[1]}.\n");
			}
			else
			{
				contents.AddMix(ReagentDatabase.GetReagent(args[1]).GetReagent(20f));
				obj.GetPhotonView().RPC("ForceMixRPC", RpcTarget.All, contents, kobold.photonView.ViewID);
				output.Append($"Spawned bucket filled with {20f} {args[1]}.\n");
			}
			return;
		}
		if (args[1].ToLower() == "stars")
		{
			if (args.Length > 2 && int.TryParse(args[2], out var value2))
			{
				ObjectiveManager.GiveStars(value2);
				output.Append($"Gave {value2} {args[1]}.\n");
			}
			else
			{
				ObjectiveManager.GiveStars(999);
				output.Append("Gave 999 " + args[1] + ".\n");
			}
			return;
		}
		if (args[1].ToLower() == "money" || args[1].ToLower() == "dosh" || args[1].ToLower() == "dollars")
		{
			if (args.Length > 2 && float.TryParse(args[2], out var value))
			{
				kobold.photonView.RPC("AddMoney", RpcTarget.All, value);
				output.Append($"Gave {value} {args[1]} to {kobold.photonView.Owner.NickName}.\n");
				return;
			}
			kobold.photonView.RPC("AddMoney", RpcTarget.All, 999f);
			output.Append("Gave 999 " + args[1] + " to " + kobold.photonView.Owner.NickName + ".\n");
			return;
		}
		throw new CheatsProcessor.CommandException("There is no prefab, reagent, or resource with name " + args[1] + ".");
	}

	public override void OnValidate()
	{
		base.OnValidate();
		bucket.OnValidate();
	}
}
