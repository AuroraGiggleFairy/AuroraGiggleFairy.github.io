using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdShowClouds : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[1] { "showClouds" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Artist command to show one layer of clouds.";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "type \"showClouds myCloudTexture\" where \"myCloudTexture\" is the name of the texture you want to see.\ntype \"showClouds\" to turn off this view.\nNote: cloud textures MUST be locasted at ./resources/textures/environment/spectrums/default\n";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (_params.Count != 0)
		{
			Resources.Load("Textures/Environment/Spectrums/default/" + _params[0], typeof(Texture));
		}
	}
}
