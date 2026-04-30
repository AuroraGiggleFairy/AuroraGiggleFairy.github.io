using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdPermissionsAllowed : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Private)]
	public delegate bool ExecuteSubCommand(ReadOnlySpan<string> parameters);

	[PublicizedFrom(EAccessModifier.Private)]
	public delegate EUserPerms MaskModifier(EUserPerms previous, EUserPerms input);

	[PublicizedFrom(EAccessModifier.Private)]
	public bool m_resolveCoroutineRunning;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ExecuteSubCommand ExecuteGrant = CreateExecuteMaskModifier(MaskModifierGrant);

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ExecuteSubCommand ExecuteRevoke = CreateExecuteMaskModifier(MaskModifierRevoke);

	public override bool IsExecuteOnClient => true;

	public override bool AllowedInMainMenu => true;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[3] { "permissionsallowed", "pallowed", "pa" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Apply a mask to permissions for testing purposes (respects the existing conditions though).";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		string text = string.Join("|", EnumUtils.Values<EUserPerms>());
		return string.Join('\n', "pa i[nfo] - Prints info about the current permissions.", "pa g[rant] <" + text + "> - Adds the given permissions to the current debug permissions mask (still respects existing permissions though).", "pa rev[oke] <" + text + "> - Removes the given permissions from the current debug permissions mask.", "pa res[olve] <" + text + "> <true|false> - Attempt to resolve the specified permissions. True allows prompting the user for input, otherwise it is a silent resolution.");
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		string[] array = _params.ToArray();
		if (_params.Count > 0)
		{
			ExecuteSubCommand executeSubCommand;
			switch (_params[0].ToLowerInvariant())
			{
			case "i":
			case "info":
				executeSubCommand = ExecuteInfo;
				break;
			case "g":
			case "grant":
				executeSubCommand = ExecuteGrant;
				break;
			case "rev":
			case "revoke":
				executeSubCommand = ExecuteRevoke;
				break;
			case "res":
			case "resolve":
				executeSubCommand = ExecuteResolve;
				break;
			default:
				executeSubCommand = null;
				break;
			}
			ExecuteSubCommand executeSubCommand2 = executeSubCommand;
			if (executeSubCommand2 == null)
			{
				Log.Warning("Unknown sub-command: " + _params[0]);
			}
			else if (executeSubCommand2(array[1..]))
			{
				return;
			}
		}
		Log.Warning(GetHelp());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool ExecuteInfo(ReadOnlySpan<string> parameters)
	{
		EUserPerms permissions = PlatformManager.NativePlatform.User.Permissions;
		StringBuilder stringBuilder = new StringBuilder(string.Format("User Native: {0}\nUser Cross: {1}", permissions, (PlatformManager.CrossplatformPlatform?.User?.Permissions)?.ToString() ?? "N/A"));
		foreach (PermissionsManager.PermissionSources value in Enum.GetValues(typeof(PermissionsManager.PermissionSources)))
		{
			stringBuilder.Append($"\n{value}: {PermissionsManager.GetPermissions(value)}");
		}
		Log.Out(stringBuilder.ToString());
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static EUserPerms MaskModifierGrant(EUserPerms previous, EUserPerms input)
	{
		return previous | input;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static EUserPerms MaskModifierRevoke(EUserPerms previous, EUserPerms input)
	{
		return previous & ~input;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static ExecuteSubCommand CreateExecuteMaskModifier(MaskModifier modifier)
	{
		return ExecuteMaskModifier;
		[PublicizedFrom(EAccessModifier.Internal)]
		bool ExecuteMaskModifier(ReadOnlySpan<string> parameters)
		{
			if (parameters.Length != 1)
			{
				Log.Warning("Expected a single argument.");
				return false;
			}
			if (!TryParsePermission(parameters[0], out var result))
			{
				return false;
			}
			EUserPerms debugPermissionsMask = PermissionsManager.DebugPermissionsMask;
			EUserPerms eUserPerms = (PermissionsManager.DebugPermissionsMask = modifier(debugPermissionsMask, result));
			if (debugPermissionsMask == eUserPerms)
			{
				Log.Out(string.Format("{0} had no change. Mask: {1}", "DebugPermissionsMask", eUserPerms));
			}
			else
			{
				Log.Out(string.Format("{0} is now '{1}' (was '{2}').", "DebugPermissionsMask", eUserPerms, debugPermissionsMask));
			}
			return true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool ExecuteResolve(ReadOnlySpan<string> parameters)
	{
		if (parameters.Length != 2)
		{
			Log.Warning("Expected a two arguments.");
			return false;
		}
		if (!TryParsePermission(parameters[0], out var result))
		{
			return false;
		}
		if (!TryParseBoolean(parameters[1], out var result2))
		{
			return false;
		}
		ThreadManager.StartCoroutine(ResolveCoroutine(result, result2));
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator ResolveCoroutine(EUserPerms permissionsToResolve, bool shouldPrompt)
	{
		if (m_resolveCoroutineRunning)
		{
			Log.Warning("[Perms Resolve] Resolving in progress already.");
			yield break;
		}
		yield return null;
		try
		{
			m_resolveCoroutineRunning = true;
			Log.Out($"[Perms Resolve] Resolving Permissions '{permissionsToResolve}'.");
			yield return PermissionsManager.ResolvePermissions(permissionsToResolve, shouldPrompt);
		}
		finally
		{
			m_resolveCoroutineRunning = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool TryParseBoolean(string input, out bool result)
	{
		switch (input.ToLowerInvariant())
		{
		case "on":
		case "t":
		case "true":
		case "1":
			result = true;
			return true;
		case "off":
		case "f":
		case "false":
		case "0":
			result = false;
			return true;
		default:
			Log.Warning("Expected true/false instead of: " + input);
			result = false;
			return false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool TryParsePermission(string input, out EUserPerms result)
	{
		if (EnumUtils.TryParse<EUserPerms>(input, out result, _ignoreCase: true))
		{
			return true;
		}
		EUserPerms? eUserPerms = null;
		foreach (var item in EnumUtils.Values<EUserPerms>().Zip(EnumUtils.Names<EUserPerms>(), [PublicizedFrom(EAccessModifier.Internal)] (EUserPerms perm, string name) => (perm: perm, name: name)))
		{
			var (eUserPerms2, _) = item;
			if (item.name.StartsWith(input, StringComparison.OrdinalIgnoreCase))
			{
				if (eUserPerms.HasValue)
				{
					Log.Warning($"Input '{input}' is ambiguous between '{eUserPerms.Value}' and '{eUserPerms2}'.");
					result = (EUserPerms)0;
					return false;
				}
				eUserPerms = eUserPerms2;
			}
		}
		if (!eUserPerms.HasValue)
		{
			Log.Warning("Input '" + input + "' did not match any permissions.");
			result = (EUserPerms)0;
			return false;
		}
		result = eUserPerms.Value;
		return true;
	}
}
