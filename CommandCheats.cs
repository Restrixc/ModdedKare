using System;
using System.Text;
using Photon.Pun;

[Serializable]
public class CommandCheats : Command
{
	public override string GetArg0()
	{
		return "/cheats";
	}

	public override void Execute(StringBuilder output, Kobold k, string[] args)
	{
		base.Execute(output, k, args);
		if ((Kobold)PhotonNetwork.MasterClient.TagObject != k)
		{
			throw new CheatsProcessor.CommandException("Not the owner of the server, cannot enable cheats.");
		}
		if (args.Length < 2)
		{
			CheatsProcessor.SetCheatsEnabled(!CheatsProcessor.GetCheatsEnabled());
			output.Append($"Cheats enabled: {CheatsProcessor.GetCheatsEnabled()}\n");
			return;
		}
		if (int.TryParse(args[1], out var value))
		{
			CheatsProcessor.SetCheatsEnabled(value == 1);
			output.Append($"Cheats enabled: {CheatsProcessor.GetCheatsEnabled()}\n");
			return;
		}
		throw new CheatsProcessor.CommandException("Failed to parse " + args[1] + " as an integer!");
	}
}
