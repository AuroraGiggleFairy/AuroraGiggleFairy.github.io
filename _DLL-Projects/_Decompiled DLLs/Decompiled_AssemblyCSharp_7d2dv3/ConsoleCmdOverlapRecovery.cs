using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdOverlapRecovery : ConsoleCmdAbstract
{
	public override bool AllowedInMainMenu => false;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[1] { "overlap" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Toggle LocalPlayer's Character Controller Overlap Recovery";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (GameManager.Instance.World.GetPrimaryPlayer()?.m_characterController is CharacterControllerUnity characterControllerUnity)
		{
			characterControllerUnity.enableOverlapRecovery = !characterControllerUnity.enableOverlapRecovery;
			Log.Out($"CharacterController.enableOverlapRecovery set to {characterControllerUnity.enableOverlapRecovery}");
		}
	}
}
