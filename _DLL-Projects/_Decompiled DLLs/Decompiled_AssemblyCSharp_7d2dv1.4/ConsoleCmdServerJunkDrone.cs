using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdServerJunkDrone : ConsoleCmdAbstract
{
	public override bool IsExecuteOnClient => false;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[1] { "jds" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Server drone commands";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return base.getHelp();
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (_params.Count == 0)
		{
			Log.Out("JSD");
			return;
		}
		if (_params[0].ContainsCaseInsensitive("assign"))
		{
			string playerName = _params[1];
			int result = -1;
			if (int.TryParse(_params[2], out result))
			{
				EntityPlayer entityPlayer = GameManager.Instance.World.Players.list.Find([PublicizedFrom(EAccessModifier.Internal)] (EntityPlayer p) => p.EntityName.ContainsCaseInsensitive(playerName));
				EntityDrone entityDrone = GameManager.Instance.World.GetEntity(result) as EntityDrone;
				if ((bool)entityPlayer && (bool)entityDrone)
				{
					entityDrone.position = entityPlayer.getChestPosition() - entityPlayer.GetForwardVector() * 2f;
					entityPlayer.AddOwnedEntity(entityDrone);
					entityDrone.DebugUnstuck();
					Log.Out("Drone {0} assigned to {1}", entityDrone.entityId, entityPlayer.EntityName);
				}
			}
		}
		if (!_params[0].ContainsCaseInsensitive("mas"))
		{
			return;
		}
		string playerName2 = _params[1];
		int result2 = -1;
		if (int.TryParse(_params[2], out result2))
		{
			GameManager.Instance.World.Players.list.Find([PublicizedFrom(EAccessModifier.Internal)] (EntityPlayer p) => p.EntityName.ContainsCaseInsensitive(playerName2));
		}
	}
}
