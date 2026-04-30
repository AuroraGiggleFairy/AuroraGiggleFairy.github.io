using Twitch;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_TwitchCommandEntry : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public TwitchAction action;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isDirty;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isBool;

	[PublicizedFrom(EAccessModifier.Private)]
	public string positiveColor = "0,0,255";

	[PublicizedFrom(EAccessModifier.Private)]
	public string negativeColor = "255,0,0";

	[PublicizedFrom(EAccessModifier.Private)]
	public string disabledColor = "80,80,80";

	[PublicizedFrom(EAccessModifier.Private)]
	public string defaultCostColor = "255,255,255,255";

	[PublicizedFrom(EAccessModifier.Private)]
	public string specialCostColor = "0,125,125,255";

	[PublicizedFrom(EAccessModifier.Private)]
	public string bitCostColor = "145,70,255,255";

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isReady;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<string> objectiveOptionalFormatter = new CachedStringFormatter<string>([PublicizedFrom(EAccessModifier.Internal)] (string _s) => "(" + _s + ") ");

	[field: PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TwitchWindow Owner { get; set; }

	public TwitchAction Action
	{
		get
		{
			return action;
		}
		set
		{
			action = value;
			isDirty = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string value, string bindingName)
	{
		bool flag = action != null;
		switch (bindingName)
		{
		case "hascommand":
			value = flag.ToString();
			return true;
		case "commandname":
			value = (flag ? action.Command : "");
			return true;
		case "commandcost":
			if (flag)
			{
				if (isReady)
				{
					if (Owner != null)
					{
						switch (action.PointType)
						{
						case TwitchAction.PointTypes.PP:
							value = $"{action.CurrentCost} {Owner.lblPointsPP}";
							break;
						case TwitchAction.PointTypes.SP:
							value = $"{action.CurrentCost} {Owner.lblPointsSP}";
							break;
						case TwitchAction.PointTypes.Bits:
							value = "* ";
							break;
						}
					}
					else
					{
						value = "";
					}
				}
				else
				{
					value = "--";
				}
			}
			else
			{
				value = "";
			}
			return true;
		case "commandcolor":
			if (flag)
			{
				if (isReady)
				{
					if (action.IsPositive)
					{
						value = positiveColor;
					}
					else
					{
						value = negativeColor;
					}
				}
				else
				{
					value = disabledColor;
				}
			}
			return true;
		case "costcolor":
			if (flag)
			{
				if (isReady)
				{
					switch (action.PointType)
					{
					case TwitchAction.PointTypes.PP:
						value = defaultCostColor;
						break;
					case TwitchAction.PointTypes.SP:
						value = specialCostColor;
						break;
					case TwitchAction.PointTypes.Bits:
						value = bitCostColor;
						break;
					}
				}
				else
				{
					value = disabledColor;
				}
			}
			return true;
		case "commandtextwidth":
			if (action != null && isBool)
			{
				value = "150";
			}
			else
			{
				value = "150";
			}
			return true;
		default:
			return false;
		}
	}

	public override bool ParseAttribute(string name, string value, XUiController _parent)
	{
		switch (name)
		{
		case "positive_color":
			positiveColor = value;
			return true;
		case "negative_color":
			negativeColor = value;
			return true;
		case "disabled_color":
			disabledColor = value;
			return true;
		case "default_cost_color":
			defaultCostColor = value;
			return true;
		case "special_cost_color":
			specialCostColor = value;
			return true;
		default:
			return base.ParseAttribute(name, value, _parent);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleOnCountChanged(XUiController _sender, OnCountChangedEventArgs _e)
	{
		RefreshBindings(_forceAll: true);
	}

	public override void Update(float _dt)
	{
		if (Action != null)
		{
			isDirty = true;
			isReady = action.IsReady(Owner.twitchManager);
		}
		else
		{
			isReady = false;
		}
		if (isDirty)
		{
			RefreshBindings(isDirty);
			isDirty = false;
		}
		base.Update(_dt);
	}
}
