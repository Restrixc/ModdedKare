using System;
using System.Text;

[Serializable]
public class CommandHelp : Command
{
	public override string GetArg0()
	{
		return "/help";
	}

	public override void Execute(StringBuilder output, Kobold k, string[] args)
	{
		base.Execute(output, k, args);
		foreach (Command command in CheatsProcessor.GetCommands())
		{
			output.Append(command.GetArg0() + "\n");
			if (command.GetDescription() != null && !command.GetDescription().IsEmpty)
			{
				output.Append("\t" + command.GetDescription().GetLocalizedString() + "\n");
			}
		}
	}
}
