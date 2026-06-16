using System.Collections.Generic;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdListEntities : ConsoleCmdAbstract
{
	public override bool IsExecuteOnClient => true;

	public override DeviceFlag AllowedDeviceTypes => DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5;

	public override DeviceFlag AllowedDeviceTypesClient => DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[2] { "listents", "le" };
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		int num = 0;
		for (int num2 = GameManager.Instance.World.Entities.list.Count - 1; num2 >= 0; num2--)
		{
			Entity entity = GameManager.Instance.World.Entities.list[num2];
			EntityAlive entityAlive = null;
			if (entity is EntityAlive)
			{
				entityAlive = (EntityAlive)entity;
			}
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output(++num + ". id=" + entity.entityId + ", " + entity.ToString() + ", pos=" + entity.GetPosition().ToCultureInvariantString() + ", rot=" + entity.rotation.ToCultureInvariantString() + ", lifetime=" + ((entity.lifetime == float.MaxValue) ? "float.Max" : entity.lifetime.ToCultureInvariantString("0.0")) + ", remote=" + entity.isEntityRemote + ", dead=" + entity.IsDead() + ", " + ((entityAlive != null) ? ("health=" + entityAlive.Health) : ""));
		}
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Total of " + GameManager.Instance.World.Entities.Count + " in the game");
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "lists all entities";
	}
}
