using Twitch;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_TwitchVoteEntry : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public TwitchVoteEntry vote;

	public bool isDirty;

	public bool isWinner;

	[PublicizedFrom(EAccessModifier.Private)]
	public string positiveColor = "0,0,255";

	[PublicizedFrom(EAccessModifier.Private)]
	public string negativeColor = "255,0,0";

	[PublicizedFrom(EAccessModifier.Private)]
	public string textBadColor = "255,175,175";

	[PublicizedFrom(EAccessModifier.Private)]
	public string textGoodColor = "175,175,255";

	[PublicizedFrom(EAccessModifier.Private)]
	public string selectedTextColor = "222,206,163";

	[PublicizedFrom(EAccessModifier.Private)]
	public string disabledColor = "80,80,80";

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isReady;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<int> voteCountFormatterInt = new CachedStringFormatter<int>([PublicizedFrom(EAccessModifier.Internal)] (int _i) => _i + "%");

	[field: PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TwitchWindow Owner { get; set; }

	public TwitchVoteEntry Vote
	{
		get
		{
			return vote;
		}
		set
		{
			vote = value;
			isDirty = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string value, string bindingName)
	{
		bool flag = vote != null;
		switch (bindingName)
		{
		case "hasvote":
			value = flag.ToString();
			return true;
		case "votename":
			if (flag)
			{
				if (vote.Index == 2 && vote.Owner.UseMystery)
				{
					value = "?????";
				}
				else
				{
					value = vote.VoteClass.Display;
				}
			}
			else
			{
				value = "";
			}
			return true;
		case "votecommand":
			if (isWinner)
			{
				value = "";
			}
			else
			{
				value = (flag ? vote.VoteCommand : "");
			}
			return true;
		case "votecount":
			if (flag)
			{
				if (!isWinner)
				{
					float num = 0f;
					if (vote.VoteCount > 0)
					{
						num = (float)vote.VoteCount / (float)vote.Owner.VoteCount;
					}
					value = voteCountFormatterInt.Format((int)(num * 100f));
				}
				else
				{
					value = "";
				}
			}
			else
			{
				value = "";
			}
			return true;
		case "votefill":
			if (flag)
			{
				if (vote.VoteCount > 0)
				{
					value = ((float)vote.VoteCount / (float)vote.Owner.VoteCount).ToString();
				}
				else
				{
					value = "0";
				}
			}
			else
			{
				value = "0";
			}
			return true;
		case "votecolor":
			if (flag)
			{
				if (vote.Owner.IsHighest(vote))
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
				value = "0,0,0,0";
			}
			return true;
		case "voteline1":
			if (flag)
			{
				value = vote.VoteClass.VoteLine1;
			}
			return true;
		case "hasvoteline1":
			if (flag)
			{
				value = (vote.VoteClass.VoteLine1 != "").ToString();
			}
			else
			{
				value = "false";
			}
			return true;
		case "line1textcolor":
			if (flag)
			{
				switch (vote.VoteClass.DisplayType)
				{
				case TwitchVote.VoteDisplayTypes.GoodBad:
					value = textBadColor;
					break;
				case TwitchVote.VoteDisplayTypes.HordeBuffed:
					value = selectedTextColor;
					break;
				case TwitchVote.VoteDisplayTypes.Special:
					value = textGoodColor;
					break;
				}
			}
			else
			{
				value = "255,255,255";
			}
			return true;
		case "voteline2":
			if (flag)
			{
				value = vote.VoteClass.VoteLine2;
			}
			return true;
		case "hasvoteline2":
			if (flag)
			{
				value = (vote.VoteClass.VoteLine2 != "").ToString();
			}
			else
			{
				value = "false";
			}
			return true;
		case "line2textcolor":
			if (flag)
			{
				if (vote.VoteClass.DisplayType == TwitchVote.VoteDisplayTypes.GoodBad)
				{
					value = textBadColor;
				}
			}
			else
			{
				value = "255,255,255";
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
		case "selected_color":
			selectedTextColor = value;
			return true;
		case "bad_color":
			textBadColor = value;
			return true;
		case "good_color":
			textGoodColor = value;
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
		if (Vote != null && Vote.UIDirty)
		{
			isDirty = true;
			Vote.UIDirty = false;
		}
		if (isDirty)
		{
			RefreshBindings(isDirty);
			isDirty = false;
		}
		base.Update(_dt);
	}
}
