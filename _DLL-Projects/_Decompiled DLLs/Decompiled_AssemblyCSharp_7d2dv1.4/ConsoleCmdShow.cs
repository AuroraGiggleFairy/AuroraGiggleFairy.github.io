using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdShow : ConsoleCmdAbstract
{
	public class DebugView
	{
		public string cmd;

		public string key;

		public bool disableShadows;

		public bool disableSSAO;

		public bool disableDOF;

		public DebugView(string _cmd, string _key, bool _disableShadows, bool _disableSSAO, bool _disableDOF)
		{
			cmd = _cmd;
			key = _key;
			disableShadows = _disableShadows;
			disableSSAO = _disableSSAO;
			disableDOF = _disableDOF;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool DISABLE_SHADOWS = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool ENABLE_SHADOWS = false;

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool DISABLE_SSAO = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool ENABLE_SSAO = false;

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool DISABLE_DOF = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool ENABLE_DOF = false;

	[PublicizedFrom(EAccessModifier.Private)]
	public static DebugView[] Commands = new DebugView[4]
	{
		new DebugView("blockAO", "SHOW_BLOCK_AO", DISABLE_SHADOWS, DISABLE_SSAO, DISABLE_DOF),
		new DebugView("occlusion", "SHOW_OCCLUSION", DISABLE_SHADOWS, DISABLE_SSAO, DISABLE_DOF),
		new DebugView("lighting", "SHOW_LIGHTING", ENABLE_SHADOWS, ENABLE_SSAO, DISABLE_DOF),
		new DebugView("normals", "SHOW_NORMALS", DISABLE_SHADOWS, DISABLE_SSAO, DISABLE_DOF)
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public static string enabledKeyword = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public static int savedShadowsOption = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool savedSSAOOption = false;

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool savedDOFOption = false;

	public override bool IsExecuteOnClient => true;

	public override bool AllowedInMainMenu => true;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[1] { "show" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Shows custom layers of rendering.";
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void Enable(DebugView dView)
	{
		Disable();
		enabledKeyword = dView.key;
		Shader.EnableKeyword(enabledKeyword);
		savedShadowsOption = GamePrefs.GetInt(EnumGamePrefs.OptionsGfxShadowQuality);
		savedSSAOOption = GamePrefs.GetBool(EnumGamePrefs.OptionsGfxSSAO);
		savedDOFOption = GamePrefs.GetBool(EnumGamePrefs.OptionsGfxDOF);
		if (dView.disableShadows)
		{
			GamePrefs.Set(EnumGamePrefs.OptionsGfxShadowQuality, 0);
		}
		if (dView.disableSSAO)
		{
			GamePrefs.Set(EnumGamePrefs.OptionsGfxSSAO, _value: false);
		}
		if (dView.disableDOF)
		{
			GamePrefs.Set(EnumGamePrefs.OptionsGfxDOF, _value: false);
		}
		GameManager.Instance.ApplyAllOptions();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void Disable()
	{
		if (enabledKeyword.Length >= 1)
		{
			Shader.DisableKeyword(enabledKeyword);
			enabledKeyword = "";
			GamePrefs.Set(EnumGamePrefs.OptionsGfxShadowQuality, savedShadowsOption);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxSSAO, savedSSAOOption);
			GamePrefs.Set(EnumGamePrefs.OptionsGfxDOF, savedDOFOption);
			GameManager.Instance.ApplyAllOptions();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool IsEnabled(string key)
	{
		if (enabledKeyword.Length < 1)
		{
			return false;
		}
		return enabledKeyword.EqualsCaseInsensitive(key);
	}

	public static void Init()
	{
		for (int i = 0; i < Commands.Length; i++)
		{
			Shader.DisableKeyword(Commands[i].key);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void Switch(DebugView dView)
	{
		if (IsEnabled(dView.key))
		{
			Disable();
		}
		else
		{
			Enable(dView);
		}
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (_params.Count == 0)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output(GetDescription());
		}
		else if (_params.Count == 1)
		{
			for (int i = 0; i < Commands.Length; i++)
			{
				if (_params[0].EqualsCaseInsensitive(Commands[i].cmd))
				{
					Switch(Commands[i]);
					return;
				}
			}
			if (_params[0].EqualsCaseInsensitive("none") || _params[0].EqualsCaseInsensitive("off"))
			{
				Disable();
			}
		}
		else
		{
			if (_params.Count > 1)
			{
				StringParsers.ParseFloat(_params[1]);
			}
			_params[0].EqualsCaseInsensitive("NA");
		}
	}
}
