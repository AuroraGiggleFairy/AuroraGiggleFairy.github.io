using UnityEngine.Scripting;

namespace Twitch;

[Preserve]
public class BaseTwitchRequirement
{
	public enum OwnerTypes
	{
		Action,
		Vote,
		Event
	}

	public OwnerTypes OwnerType;

	public TwitchAction OwnerAction;

	public TwitchVote OwnerVote;

	public BaseTwitchEventEntry OwnerEvent;

	public DynamicProperties Properties;

	public bool HideAction;

	public bool Invert;

	public static string PropInvert = "invert";

	public static string PropHidesAction = "hide_action";

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnInit()
	{
	}

	public void Init()
	{
		OnInit();
	}

	public virtual bool CanPerform(Entity player)
	{
		return true;
	}

	public virtual void ParseProperties(DynamicProperties properties)
	{
		Properties = properties;
		properties.ParseBool(PropInvert, ref Invert);
		properties.ParseBool(PropHidesAction, ref HideAction);
	}
}
