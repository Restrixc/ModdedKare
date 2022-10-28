using System;
using System.Text;
using Photon.Pun;
using Photon.Realtime;

[Serializable]
public class CommandKick : Command
{
	public override string GetArg0()
	{
		return "/kick";
	}

	public override void Execute(StringBuilder output, Kobold k, string[] args)
	{
		base.Execute(output, k, args);
		if (args.Length != 2)
		{
			throw new CheatsProcessor.CommandException("Usage: /kick {actor number}");
		}
		if (!int.TryParse(args[1], out var actorNum))
		{
			throw new CheatsProcessor.CommandException("Must use actor number to identify player, use `/list players` to find that.");
		}
		if (k != (Kobold)PhotonNetwork.LocalPlayer.TagObject || !PhotonNetwork.IsMasterClient)
		{
			throw new CheatsProcessor.CommandException("Not allowed to kick players.");
		}
		Player[] playerList = PhotonNetwork.PlayerList;
		foreach (Player player in playerList)
		{
			if (player.ActorNumber == actorNum && object.Equals(player, PhotonNetwork.LocalPlayer))
			{
				throw new CheatsProcessor.CommandException("Don't kick yourself :(");
			}
			if (player.ActorNumber == actorNum)
			{
				PhotonNetwork.CloseConnection(player);
				return;
			}
		}
		throw new CheatsProcessor.CommandException($"No player found with id {actorNum}, use `/list players`.");
	}
}
