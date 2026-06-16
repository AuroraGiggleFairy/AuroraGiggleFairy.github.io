using UnityEngine.Scripting;

[Preserve]
public class RewardShowMessageWindow : BaseReward
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static string PropMessage = "message";

	[PublicizedFrom(EAccessModifier.Private)]
	public static string PropTitle = "title";

	[PublicizedFrom(EAccessModifier.Private)]
	public string message = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public string title = "";

	public RewardShowMessageWindow()
	{
		base.HiddenReward = true;
	}

	public override void SetupReward()
	{
		base.HiddenReward = true;
	}

	public override void GiveReward(EntityPlayer player)
	{
		XUiC_TipWindow.ShowTip(message, title, player as EntityPlayerLocal, null);
	}

	public override BaseReward Clone()
	{
		RewardShowMessageWindow rewardShowMessageWindow = new RewardShowMessageWindow();
		CopyValues(rewardShowMessageWindow);
		rewardShowMessageWindow.title = title;
		rewardShowMessageWindow.message = message;
		return rewardShowMessageWindow;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		properties.ParseString(PropMessage, ref message);
		properties.ParseString(PropTitle, ref title);
	}
}
