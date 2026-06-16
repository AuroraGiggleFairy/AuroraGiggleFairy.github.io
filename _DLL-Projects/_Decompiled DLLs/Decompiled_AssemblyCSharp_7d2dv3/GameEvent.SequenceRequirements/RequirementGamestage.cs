using UnityEngine.Scripting;

namespace GameEvent.SequenceRequirements;

[Preserve]
public class RequirementGamestage : BaseOperationRequirement
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public string gamestageText;

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropGamestage = "game_stage";

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnInit()
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override object LeftSide(Entity target)
	{
		return (target is EntityPlayer entityPlayer) ? entityPlayer.gameStage : 0;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override object RightSide(Entity target)
	{
		return GameEventManager.GetIntValue(target as EntityAlive, gamestageText);
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		properties.ParseString(PropGamestage, ref gamestageText);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseRequirement CloneChildSettings()
	{
		return new RequirementGamestage
		{
			Invert = Invert,
			operation = operation,
			gamestageText = gamestageText
		};
	}
}
