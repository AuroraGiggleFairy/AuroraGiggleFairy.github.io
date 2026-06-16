using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdOcclusion : ConsoleCmdAbstract
{
	public override bool IsExecuteOnClient => true;

	public override bool AllowedInMainMenu => true;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[1] { "occlusion" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Control OcclusionManager";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (GameManager.IsDedicatedServer)
		{
			return;
		}
		if (_params.Count == 0)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("occlusion off, partial, full, toggleVisible, togglePrints");
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output(" off (set in main menu, not a map)");
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("   turns off gpu occlusion ");
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output(" full (set in main menu, not a map)");
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("   hides occluded objects from both the camera and the sun");
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output(" toggleVisible");
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("   toggles between forcing all meshes visible and normal gpu culling");
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output(" view");
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("   toggles view of occlusion texture");
			return;
		}
		OcclusionManager instance = OcclusionManager.Instance;
		if (instance == null)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("No occlusion manager!");
		}
		else
		{
			if (_params.Count != 1)
			{
				return;
			}
			if (_params[0].EqualsCaseInsensitive("off"))
			{
				instance.EnableCulling(isCulling: false);
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Occlusion disabled");
			}
			else if (_params[0].EqualsCaseInsensitive("full"))
			{
				instance.EnableCulling(isCulling: true);
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Occlusion enabled full");
			}
			else if (_params[0].EqualsCaseInsensitive("toggleVisible"))
			{
				instance.forceAllVisible = !instance.forceAllVisible;
				if (instance.forceAllVisible)
				{
					SingletonMonoBehaviour<SdtdConsole>.Instance.Output("All meshes are forced to visible");
				}
				else
				{
					SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Normal GPU occlusion");
				}
			}
			else if (_params[0].EqualsCaseInsensitive("view"))
			{
				instance.ToggleDebugView();
			}
		}
	}
}
