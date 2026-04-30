using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdAIDirectorSpawnSupplyCrate : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[1] { "spawnsupplycrate" };
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		World world = GameManager.Instance.World;
		EntityPlayer entityPlayer;
		if (_senderInfo.IsLocalGame)
		{
			entityPlayer = world.GetPrimaryPlayer();
		}
		else
		{
			if (_senderInfo.RemoteClientInfo == null)
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Command RemoteClientInfo null");
				return;
			}
			entityPlayer = world.Players.dict[_senderInfo.RemoteClientInfo.entityId];
		}
		Vector3 position = entityPlayer.position;
		position.y += 8f;
		Entity entity = EntityFactory.CreateEntity(EntityClass.FromString("sc_General"), position, new Vector3(0f, world.GetGameRandom().RandomFloat * 360f, 0f));
		world.SpawnEntityInWorld(entity);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Spawns a supply crate where the player is";
	}
}
