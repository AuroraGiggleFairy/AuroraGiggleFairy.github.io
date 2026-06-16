using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdDebugJiggle : ConsoleCmdAbstract
{
	public override bool IsExecuteOnClient => true;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[2] { "debugjiggle", "dgj" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		World world = GameManager.Instance.World;
		if (world == null)
		{
			return;
		}
		if (_params.Count > 0)
		{
			if (!int.TryParse(_params[0], out var result))
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Expected an entity id");
				return;
			}
			Entity entity = world.GetEntity(result);
			if (entity == null)
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output($"Could not find entity with id {result}");
			}
			else
			{
				LogEntityJiggles(entity);
			}
			return;
		}
		foreach (EntityAlive entityAlife in world.EntityAlives)
		{
			LogEntityJiggles(entityAlife);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void LogEntityJiggles(Entity entity)
	{
		EModelBase emodel = entity.emodel;
		if (!(emodel == null))
		{
			emodel.LogJiggles();
		}
	}
}
