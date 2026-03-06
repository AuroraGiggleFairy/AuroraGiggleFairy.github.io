using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdTransformDebug : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const string EMPTY_NAME = "TransformDebug";

	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject m_empty;

	public override bool IsExecuteOnClient => true;

	public override bool AllowedInMainMenu => true;

	public ConsoleCmdTransformDebug()
	{
		ToggleDebugging();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[2] { "transformdebug", "tdbg" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Transform Debugging";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "tdbg - Toggle Transform Debugging";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (_params.Count == 0)
		{
			ToggleDebugging();
		}
		else
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output(getHelp());
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ToggleDebugging()
	{
		if (!m_empty)
		{
			Log.Out("Creating TransformDebug GameObject");
			m_empty = new GameObject("TransformDebug");
			m_empty.AddComponent<TransformDebug>();
			Object.DontDestroyOnLoad(m_empty);
		}
		else
		{
			Log.Out("Destroying TransformDebug GameObject");
			Object.Destroy(m_empty);
		}
	}
}
