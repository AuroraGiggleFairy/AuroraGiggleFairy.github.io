using System;
using System.Collections.Generic;
using Challenges;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdChallenges : ConsoleCmdAbstract
{
	public override bool IsExecuteOnClient => true;

	public override DeviceFlag AllowedDeviceTypesClient => DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[1] { "challenges" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Complete certain challenges";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return ("\n\t\t\t|Usage:\n\t\t\t|  1. " + PrimaryCommand + " list [group name]\n\t\t\t|  2. " + PrimaryCommand + " complete all [\"redeem\"/\"r\"]\n\t\t\t|  3. " + PrimaryCommand + " complete first [\"redeem\"/\"r\"]\n\t\t\t|  4. " + PrimaryCommand + " complete challenge <challenge name> [\"redeem\"/\"r\"]\n\t\t\t|  5. " + PrimaryCommand + " complete group <group name> [\"redeem\"/\"r\"]\n\t\t\t|  6. " + PrimaryCommand + " complete category <category name> [\"redeem\"/\"r\"]\n\t\t\t|  7. " + PrimaryCommand + " groups [category name]\n\t\t\t|Short forms: \"list\"->\"l\", \"complete\"->\"c\", \"groups\"->\"g\".\n\t\t\t|1. List challenges - optionally limited to a specific group.\n\t\t\t|2. Set all challenges to completed.\n\t\t\t|3. Set challenges of the first defined group to completed.\n\t\t\t|4. Set given challenge to completed.\n\t\t\t|5. Set all challenges in the given group to completed.\n\t\t\t|6. Set all challenges in the given category to completed.\n\t\t\t|2.-6. If the optional \"redeem\" or \"r\" is specified it will automatically redeem the completed challenges.\n\t\t\t|7. List all groups - optionally limited to a specific category.\n\t\t\t").Unindent();
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (GameManager.IsDedicatedServer)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Cannot execute " + PrimaryCommand + " on dedicated server, please execute as a client");
			return;
		}
		if (_params.Count < 1)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Missing arguments (see help)");
			return;
		}
		string text = _params[0];
		if (text.EqualsCaseInsensitive("list") || text.EqualsCaseInsensitive("l"))
		{
			executeList(_params);
		}
		else if (text.EqualsCaseInsensitive("complete") || text.EqualsCaseInsensitive("c"))
		{
			executeComplete(_params);
		}
		else if (text.EqualsCaseInsensitive("groups") || text.EqualsCaseInsensitive("g"))
		{
			executeListGroups(_params);
		}
		else
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Unknown subcommand '" + text + "'");
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void executeListGroups(List<string> _params)
	{
		Func<ChallengeGroup, bool> func = null;
		if (_params.Count > 1)
		{
			string filterValue = _params[1];
			if (!string.IsNullOrEmpty(filterValue))
			{
				func = [PublicizedFrom(EAccessModifier.Internal)] (ChallengeGroup _group) => _group.Category.EqualsCaseInsensitive(filterValue);
			}
		}
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Challenge groups:");
		foreach (var (_, challengeGroup2) in ChallengeGroup.s_ChallengeGroups)
		{
			if (func == null || func(challengeGroup2))
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("  " + challengeGroup2.Name + " (category: " + challengeGroup2.Category + ")");
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void executeList(List<string> _params)
	{
		Func<ChallengeClass, bool> func = null;
		if (_params.Count > 1)
		{
			string filterValue = _params[1];
			if (!string.IsNullOrEmpty(filterValue))
			{
				func = [PublicizedFrom(EAccessModifier.Internal)] (ChallengeClass _class) => _class.ChallengeGroup.Name.EqualsCaseInsensitive(filterValue);
			}
		}
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Challenges:");
		foreach (var (_, challengeClass2) in ChallengeClass.s_Challenges)
		{
			if (func == null || func(challengeClass2))
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("  " + challengeClass2.Name + " (group: " + challengeClass2.ChallengeGroup.Name + ", category: " + challengeClass2.ChallengeGroup.Category + ")");
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void executeComplete(List<string> _params)
	{
		if (_params.Count < 2)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Subcommand 'complete' expects at least one further argument");
			return;
		}
		string text = _params[1];
		string name = ((_params.Count > 2) ? _params[2] : null);
		Func<ChallengeClass, bool> func;
		if (text.EqualsCaseInsensitive("all"))
		{
			func = null;
		}
		else if (text.EqualsCaseInsensitive("first"))
		{
			func = null;
			using Dictionary<string, ChallengeGroup>.Enumerator enumerator = ChallengeGroup.s_ChallengeGroups.GetEnumerator();
			if (enumerator.MoveNext())
			{
				enumerator.Current.Deconstruct(out var _, out var value);
				ChallengeGroup group = value;
				func = [PublicizedFrom(EAccessModifier.Internal)] (ChallengeClass _class) => _class.ChallengeGroup.Name.Equals(group.Name);
			}
		}
		else if (text.EqualsCaseInsensitive("challenge"))
		{
			if (string.IsNullOrEmpty(name))
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Subcommand 'complete' with argument 'challenge': Expects challenge name");
				return;
			}
			if (!ChallengeClass.s_Challenges.ContainsKey(name))
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Challenge '" + name + "' does not exist");
				return;
			}
			func = [PublicizedFrom(EAccessModifier.Internal)] (ChallengeClass _class) => _class.Name.EqualsCaseInsensitive(name);
		}
		else if (text.EqualsCaseInsensitive("group"))
		{
			if (string.IsNullOrEmpty(name))
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Subcommand 'complete' with argument 'group': Expects group name");
				return;
			}
			if (!ChallengeGroup.s_ChallengeGroups.ContainsKey(name))
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Group '" + name + "' does not exist");
				return;
			}
			func = [PublicizedFrom(EAccessModifier.Internal)] (ChallengeClass _class) => _class.ChallengeGroup.Name.EqualsCaseInsensitive(name);
		}
		else
		{
			if (!text.EqualsCaseInsensitive("category"))
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Subcommand 'complete': Invalid argument '" + text + "'");
				return;
			}
			if (string.IsNullOrEmpty(name))
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Subcommand 'complete' with argument 'category': Expects category name");
				return;
			}
			if (!ChallengeCategory.s_ChallengeCategories.ContainsKey(name))
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Category '" + name + "' does not exist");
				return;
			}
			func = [PublicizedFrom(EAccessModifier.Internal)] (ChallengeClass _class) => _class.ChallengeGroup.Category.EqualsCaseInsensitive(name);
		}
		string a = _params[_params.Count - 1];
		bool flag = a.EqualsCaseInsensitive("redeem") || a.EqualsCaseInsensitive("r");
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Marking challenges as completed:");
		foreach (Challenge challenge in GameManager.Instance.World.GetPrimaryPlayer().challengeJournal.Challenges)
		{
			ChallengeClass challengeClass = challenge.ChallengeClass;
			if (func == null || func(challengeClass))
			{
				if (!challenge.IsActive && (!flag || challenge.ChallengeState != Challenge.ChallengeStates.Completed))
				{
					SingletonMonoBehaviour<SdtdConsole>.Instance.Output("  " + challengeClass.Name + " already complete (group: " + challengeClass.ChallengeGroup.Name + ", category: " + challengeClass.ChallengeGroup.Category + ")");
				}
				else
				{
					SingletonMonoBehaviour<SdtdConsole>.Instance.Output("  " + challengeClass.Name + " (group: " + challengeClass.ChallengeGroup.Name + ", category: " + challengeClass.ChallengeGroup.Category + ")");
					challenge.CompleteChallenge(flag);
				}
			}
		}
	}
}
