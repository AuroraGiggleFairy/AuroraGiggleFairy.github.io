using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdSignEditorDebug : ConsoleCmdAbstract
{
	public override bool IsExecuteOnClient => true;

	public override int DefaultPermissionLevel => 1000;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[2] { "signeditordebug", "sed" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Toggles visibility of the Sign Editor debug panel. ";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "No params: Toggles visibility of the Sign Editor debug panel. ";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		XUiC_SignEditorWindow.ShowDebugPanel = !XUiC_SignEditorWindow.ShowDebugPanel;
		Log.Out("[SignEditor] Set Sign Editor debug panel visibility to " + XUiC_SignEditorWindow.ShowDebugPanel + ". Changes take effect the next time a layer is selected in the editor.");
	}
}
