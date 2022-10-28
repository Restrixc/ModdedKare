using System;
using System.Text;
using Photon.Pun;

[Serializable]
public class CommandEquip : Command
{
	public override string GetArg0()
	{
		return "/equip";
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
			throw new CheatsProcessor.CommandException("/equip requires at least one argument. Use `/list equipment` to find what you can equip.");
		}
		Equipment tryEquipment = EquipmentDatabase.GetEquipment(args[1]);
		if (tryEquipment != null)
		{
			if (tryEquipment is DickEquipment)
			{
				output.Append("Equipped " + tryEquipment.name + " by modifying Kobold genes.");
				kobold.photonView.RPC("SetDickRPC", RpcTarget.All, EquipmentDatabase.GetID(tryEquipment));
			}
			else
			{
				output.Append("Equipped " + tryEquipment.name + ".");
				kobold.photonView.RPC("PickupEquipmentRPC", RpcTarget.All, EquipmentDatabase.GetID(tryEquipment), -1);
			}
			return;
		}
		if (args[1] == "None")
		{
			kobold.photonView.RPC("SetDickRPC", RpcTarget.All, byte.MaxValue);
			output.Append("Removed dick by modifying Kobold genes.");
		}
		throw new CheatsProcessor.CommandException("There is no equipment with name " + args[1] + ".");
	}
}
