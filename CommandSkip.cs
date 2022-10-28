using System;
using System.Text;

[Serializable]
public class CommandSkip : Command
{
	public override string GetArg0()
	{
		return "/skip";
	}

	public override void Execute(StringBuilder output, Kobold k, string[] args)
	{
		base.Execute(output, k, args);
		if (!CheatsProcessor.GetCheatsEnabled())
		{
			throw new CheatsProcessor.CommandException("Cheats are not enabled, use `/cheats 1` to enable cheats.");
		}
		switch (args.Length)
		{
		case 1:
			output.Append("Skipped objective.\n");
			ObjectiveManager.SkipObjective();
			break;
		case 2:
		{
			if (!int.TryParse(args[1], out var skipCount))
			{
				throw new CheatsProcessor.CommandException("Usage: /skip [int]");
			}
			for (int i = 0; i < skipCount; i++)
			{
				ObjectiveManager.SkipObjective();
			}
			output.Append("Skipped " + skipCount + " objectives.\n");
			break;
		}
		default:
			throw new CheatsProcessor.CommandException("Usage: /skip [int]");
		}
	}
}
