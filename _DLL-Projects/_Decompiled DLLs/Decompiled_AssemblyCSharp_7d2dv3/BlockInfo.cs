using System;
using UnityEngine.Scripting;

[Preserve]
public class BlockInfo : Block
{
	public enum InformationTypes
	{
		Time,
		Day,
		TimeAndDay
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropTakeDelay = "TakeDelay";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropInfoType = "InfoType";

	public InformationTypes InformationType;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float TakeDelay = 2f;

	[PublicizedFrom(EAccessModifier.Private)]
	public new BlockActivationCommand[] cmds = new BlockActivationCommand[1]
	{
		new BlockActivationCommand("take", "hand", _enabled: false)
	};

	public override void Init()
	{
		base.Init();
		if (base.Properties.Values.ContainsKey(PropInfoType))
		{
			InformationType = Enum.Parse<InformationTypes>(base.Properties.Values[PropInfoType]);
		}
		base.Properties.ParseFloat(PropTakeDelay, ref TakeDelay);
	}

	public override bool HasBlockActivationCommands(WorldBase _world, BlockValue _blockValue, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		return TakeDelay > 0f;
	}

	public override BlockActivationCommand[] GetBlockActivationCommands(WorldBase _world, BlockValue _blockValue, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		bool flag = _world.IsMyLandProtectedBlock(_blockPos, _world.GetGameManager().GetPersistentLocalPlayer());
		cmds[0].enabled = flag && TakeDelay > 0f;
		return cmds;
	}

	public override string GetActivationText(WorldBase _world, BlockValue _blockValue, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		return InformationType switch
		{
			InformationTypes.Time => "The current time is [DECEA3]" + GameUtils.WorldTimeToHourMinutesString(GameManager.Instance.World.worldTime) + "[-].", 
			InformationTypes.Day => $"The current Day is [DECEA3]{GameUtils.WorldTimeToDays(GameManager.Instance.World.worldTime)}[-].", 
			InformationTypes.TimeAndDay => $"The current Day is [DECEA3]{GameUtils.WorldTimeToDays(GameManager.Instance.World.worldTime)}[-] and the time is [DECEA3]{GameUtils.WorldTimeToHourMinutesString(GameManager.Instance.World.worldTime)}[-].", 
			_ => "", 
		};
	}

	public override bool OnBlockActivated(string _commandName, WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
	{
		if (_commandName == "take")
		{
			takeItemWithTimer(_blockPos, _blockValue, _player, TakeDelay);
			return true;
		}
		return false;
	}
}
