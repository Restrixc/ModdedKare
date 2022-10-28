using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Photon.Pun;
using UnityEngine;

public class CheatsProcessor : MonoBehaviour
{
	public delegate void OutputChangedAction(string newOutput);

	public class CommandException : Exception
	{
		public CommandException(string message)
			: base(message)
		{
		}
	}

	private static CheatsProcessor instance;

	private bool cheatsEnabled = false;

	[SerializeField]
	[SerializeReference]
	[SerializeReferenceButton]
	private List<Command> commands;

	private StringBuilder commandOutput;

	private event OutputChangedAction outputChanged;

	public static void AppendText(string text)
	{
		instance.commandOutput.Append(text);
		instance.outputChanged?.Invoke(instance.commandOutput.ToString());
	}

	public static void AddOutputChangedListener(OutputChangedAction action)
	{
		instance.outputChanged += action;
	}

	public static void RemoveOutputChangedListener(OutputChangedAction action)
	{
		instance.outputChanged -= action;
	}

	public static void SetCheatsEnabled(bool cheatsEnabled)
	{
		instance.cheatsEnabled = cheatsEnabled;
	}

	public static bool GetCheatsEnabled()
	{
		return instance.cheatsEnabled || Application.isEditor;
	}

	public static ReadOnlyCollection<Command> GetCommands()
	{
		return instance.commands.AsReadOnly();
	}

	private void Awake()
	{
		instance = this;
		commandOutput = new StringBuilder();
	}

	private void ProcessCommand(Kobold kobold, string[] args)
	{
		if (args.Length == 0 || args[0].Length <= 0 || args[0][0] != '/')
		{
			return;
		}
		foreach (Command command in commands)
		{
			if (command.GetArg0() == args[0])
			{
				command.Execute(commandOutput, kobold, args);
				this.outputChanged?.Invoke(commandOutput.ToString());
				return;
			}
		}
		throw new CommandException("`" + args[0] + "` Not a command. Use /help to see the available commands.");
	}

	public static void ProcessCommand(Kobold kobold, string command)
	{
		if (!PhotonNetwork.IsMasterClient)
		{
			return;
		}
		string[] args = command.Split(' ');
		try
		{
			instance.ProcessCommand(kobold, args);
		}
		catch (CommandException exception)
		{
			instance.commandOutput.Append("<#ff4f00>" + exception.Message + "</color>\n");
			instance.outputChanged?.Invoke(instance.commandOutput.ToString());
		}
	}

	private void OnValidate()
	{
		if (commands == null)
		{
			return;
		}
		foreach (Command command in commands)
		{
			command.OnValidate();
		}
	}
}
