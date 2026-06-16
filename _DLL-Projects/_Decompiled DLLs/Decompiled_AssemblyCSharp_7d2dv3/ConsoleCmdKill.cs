using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdKill : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[1] { "kill" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Kill a given entity";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "Kill a given entity.\nUsage:\n   1. kill <entity id>\n   2. kill <player name / steam id>\n1. can be used to kill any entity that can be killed (zombies, players).\n2. can only be used to kill players.";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (_params.Count != 1)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Wrong number of arguments, expected 1, found " + _params.Count + ".");
			return;
		}
		Entity entity = null;
		ClientInfo forNameOrId = SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.GetForNameOrId(_params[0]);
		int result;
		if (forNameOrId != null)
		{
			entity = GameManager.Instance.World.Players.dict[forNameOrId.entityId];
		}
		else if (int.TryParse(_params[0], out result) && GameManager.Instance.World.Entities.dict.ContainsKey(result))
		{
			entity = GameManager.Instance.World.Entities.dict[result];
		}
		if (entity == null)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Playername or entity id not found.");
			return;
		}
		entity.DamageEntity(new DamageSource(EnumDamageSource.Internal, EnumDamageTypes.Suicide), 99999, _criticalHit: false);
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Gave 99999 damage to entity " + _params[0]);
	}
}
