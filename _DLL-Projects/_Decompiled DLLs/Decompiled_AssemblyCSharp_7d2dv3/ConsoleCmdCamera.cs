using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdCamera : ConsoleCmdAbstract
{
	public override bool IsExecuteOnClient => true;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[2] { "camera", "cam" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Lock/unlock camera movement or load/save a specific camera position";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "Usage:\n   1. cam save <name> [comment]\n   2. cam load <name>\n   3. cam list\n   4. cam lock\n   5. cam unlock\n1. Save the current player's position and camera view or the camera position\nand view if in detached mode under the given name. Optionally a more descriptive\ncomment can be supplied.\n2. Load the position and direction with the given name. If in detached camera\nmode the camera itself will be adjusted, otherwise the player will be teleported.\n3. List the saved camera positions.\n4/5. Lock/unlock the camera rotation. Can also be achieved with the \"Lock Camera\" key.";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (_params.Count >= 1)
		{
			if (!_senderInfo.IsLocalGame)
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Command can only be used on clients");
				return;
			}
			EntityPlayerLocal primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
			if (_params[0].EqualsCaseInsensitive("lock"))
			{
				ExecuteLock(_params, primaryPlayer);
			}
			else if (_params[0].EqualsCaseInsensitive("unlock"))
			{
				ExecuteUnlock(_params, primaryPlayer);
			}
			else if (_params[0].EqualsCaseInsensitive("save"))
			{
				ExecuteSave(_params, primaryPlayer);
			}
			else if (_params[0].EqualsCaseInsensitive("load"))
			{
				ExecuteLoad(_params, primaryPlayer);
			}
			else if (_params[0].EqualsCaseInsensitive("list"))
			{
				ExecuteList(_params);
			}
			else
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Invalid sub command \"" + _params[0] + "\".");
			}
		}
		else
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("No sub command given.");
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ExecuteLock(List<string> _params, EntityPlayerLocal _epl)
	{
		_epl.movementInput.bCameraPositionLocked = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ExecuteUnlock(List<string> _params, EntityPlayerLocal _epl)
	{
		_epl.movementInput.bCameraPositionLocked = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ExecuteSave(List<string> _params, EntityPlayerLocal _epl)
	{
		if (_params.Count < 2)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Command requires a name for the position.");
			return;
		}
		string text = _params[1];
		string comment = ((_params.Count > 2) ? _params[2] : null);
		CameraPerspectives cameraPerspectives = new CameraPerspectives();
		cameraPerspectives.Perspectives[text] = new CameraPerspectives.Perspective(text, _epl, comment);
		cameraPerspectives.Save();
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Position saved with name \"" + text + "\"");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ExecuteLoad(List<string> _params, EntityPlayerLocal _epl)
	{
		CameraPerspectives.Perspective value;
		if (_params.Count < 2)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("No position name given.");
		}
		else if (!new CameraPerspectives().Perspectives.TryGetValue(_params[1], out value))
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Position name not found.");
		}
		else
		{
			value.ToPlayer(_epl);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ExecuteList(List<string> _params)
	{
		CameraPerspectives cameraPerspectives = new CameraPerspectives();
		string text = ((_params.Count > 1) ? _params[1] : null);
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Saved camera positions:");
		foreach (var (_, perspective2) in cameraPerspectives.Perspectives)
		{
			if (text == null || perspective2.Name.ContainsCaseInsensitive(text) || perspective2.Comment.ContainsCaseInsensitive(text))
			{
				string text3 = (string.IsNullOrEmpty(perspective2.Comment) ? "" : (" (" + perspective2.Comment + ")"));
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("  " + perspective2.Name + text3);
			}
		}
	}
}
