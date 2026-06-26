namespace Twitch;

public class BaseTwitchVoteRequirement
{
	public TwitchVote Owner;

	public bool Invert;

	public static string PropInvert = "invert";

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnInit()
	{
	}

	public void Init()
	{
		OnInit();
	}

	public virtual bool CanPerform(EntityPlayer player)
	{
		return true;
	}

	public virtual void ParseProperties(DynamicProperties properties)
	{
		if (properties.Values.ContainsKey(PropInvert))
		{
			Invert = StringParsers.ParseBool(properties.Values[PropInvert]);
		}
	}
}
