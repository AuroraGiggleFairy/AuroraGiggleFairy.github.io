using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdKillAll : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[1] { "killall" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Kill all entities";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "Kills all matching entities (but never players)\nUsage:\n   killall (all enemies)\n   killall alive (all EntityAlive types except vehicles and turrets)\n   killall all (all types)";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		bool flag = _params.Count > 0 && _params[0].EqualsCaseInsensitive("alive");
		bool flag2 = _params.Count > 0 && _params[0].EqualsCaseInsensitive("all");
		List<Entity> list = new List<Entity>(GameManager.Instance.World.Entities.list);
		for (int i = 0; i < list.Count; i++)
		{
			Entity entity = list[i];
			if (entity != null && !(entity is EntityPlayer) && (flag2 || (entity is EntityAlive && ((flag && !(entity is EntityVehicle) && !(entity is EntityTurret)) || EntityClass.list[entity.entityClass].bIsEnemyEntity))))
			{
				entity.DamageEntity(new DamageSource(EnumDamageSource.Internal, EnumDamageTypes.Suicide), 99999, _criticalHit: false);
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Gave " + 99999 + " damage to entity " + entity.GetDebugName());
			}
		}
	}
}
