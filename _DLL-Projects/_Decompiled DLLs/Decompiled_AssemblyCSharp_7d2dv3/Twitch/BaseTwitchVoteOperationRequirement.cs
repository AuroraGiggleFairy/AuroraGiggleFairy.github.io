namespace Twitch;

public class BaseTwitchVoteOperationRequirement : BaseTwitchVoteRequirement
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public enum OperationTypes
	{
		None,
		Equals,
		EQ,
		E,
		NotEquals,
		NEQ,
		NE,
		Less,
		LessThan,
		LT,
		Greater,
		GreaterThan,
		GT,
		LessOrEqual,
		LessThanOrEqualTo,
		LTE,
		GreaterOrEqual,
		GreaterThanOrEqualTo,
		GTE
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public OperationTypes operation;

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropOperation = "operation";

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnInit()
	{
	}

	public override bool CanPerform(EntityPlayer player)
	{
		float num = LeftSide(player);
		float num2 = RightSide(player);
		switch (operation)
		{
		case OperationTypes.Equals:
		case OperationTypes.EQ:
		case OperationTypes.E:
			return num == num2;
		case OperationTypes.NotEquals:
		case OperationTypes.NEQ:
		case OperationTypes.NE:
			return num != num2;
		case OperationTypes.Less:
		case OperationTypes.LessThan:
		case OperationTypes.LT:
			return num < num2;
		case OperationTypes.Greater:
		case OperationTypes.GreaterThan:
		case OperationTypes.GT:
			return num > num2;
		case OperationTypes.LessOrEqual:
		case OperationTypes.LessThanOrEqualTo:
		case OperationTypes.LTE:
			return num <= num2;
		case OperationTypes.GreaterOrEqual:
		case OperationTypes.GreaterThanOrEqualTo:
		case OperationTypes.GTE:
			return num >= num2;
		default:
			return true;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual float LeftSide(EntityPlayer player)
	{
		return 0f;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual float RightSide(EntityPlayer player)
	{
		return 0f;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		properties.ParseEnum(PropOperation, ref operation);
	}
}
