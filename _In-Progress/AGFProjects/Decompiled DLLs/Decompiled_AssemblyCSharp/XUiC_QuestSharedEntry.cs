using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_QuestSharedEntry : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public string enabledColor;

	[PublicizedFrom(EAccessModifier.Private)]
	public string disabledColor;

	[PublicizedFrom(EAccessModifier.Private)]
	public string failedColor;

	[PublicizedFrom(EAccessModifier.Private)]
	public string failedIcon;

	[PublicizedFrom(EAccessModifier.Private)]
	public string rowColor;

	[PublicizedFrom(EAccessModifier.Private)]
	public string hoverColor;

	public new bool Selected;

	public bool IsHovered;

	[PublicizedFrom(EAccessModifier.Private)]
	public QuestClass questClass;

	[PublicizedFrom(EAccessModifier.Private)]
	public Quest quest;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<float, string> distanceFormatter = new CachedStringFormatter<float, string>([PublicizedFrom(EAccessModifier.Internal)] (float _f, string _s) => _f.ToCultureInvariantString("0.0") + " " + _s);

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<string> zerodistanceFormatter = new CachedStringFormatter<string>([PublicizedFrom(EAccessModifier.Internal)] (string _s) => "0 " + _s);

	public Quest Quest
	{
		get
		{
			return quest;
		}
		set
		{
			quest = value;
			questClass = ((value != null) ? QuestClass.GetQuest(quest.ID) : null);
			IsDirty = true;
		}
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public XUiC_QuestWindowGroup QuestUIHandler { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool Tracked { get; set; }

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string value, string bindingName)
	{
		bool flag = quest != null;
		switch (bindingName)
		{
		case "questname":
			value = (flag ? questClass.Name : "");
			return true;
		case "questicon":
			value = "";
			if (flag)
			{
				value = ((quest.CurrentState != Quest.QuestState.Failed) ? questClass.Icon : failedIcon);
			}
			return true;
		case "iconcolor":
			value = "255,255,255,255";
			if (flag)
			{
				switch (quest.CurrentState)
				{
				case Quest.QuestState.InProgress:
					value = enabledColor;
					break;
				case Quest.QuestState.Completed:
					value = disabledColor;
					break;
				case Quest.QuestState.Failed:
					value = failedColor;
					break;
				}
			}
			return true;
		case "textstatecolor":
			value = "255,255,255,255";
			if (flag)
			{
				if (quest.CurrentState == Quest.QuestState.InProgress)
				{
					value = enabledColor;
				}
				else
				{
					value = disabledColor;
				}
			}
			return true;
		case "rowstatecolor":
			value = (Selected ? "255,255,255,255" : (IsHovered ? hoverColor : rowColor));
			return true;
		case "rowstatesprite":
			value = (Selected ? "ui_game_select_row" : "menu_empty");
			return true;
		case "istracking":
			value = (flag ? quest.Tracked.ToString() : "false");
			return true;
		case "distance":
			if (flag && quest.Active && quest.HasPosition)
			{
				Vector3 position = quest.Position;
				Vector3 position2 = base.xui.playerUI.entityPlayer.GetPosition();
				position.y = 0f;
				position2.y = 0f;
				float num = (position - position2).magnitude;
				float num2 = num;
				string text = "m";
				if (num >= 1000f)
				{
					num /= 1000f;
					text = "km";
				}
				if (quest.MapObject is MapObjectTreasureChest mapObjectTreasureChest)
				{
					float num3 = mapObjectTreasureChest.DefaultRadius;
					if (num2 < num3)
					{
						num3 = EffectManager.GetValue(PassiveEffects.TreasureRadius, null, num3, base.xui.playerUI.entityPlayer);
						num3 = Mathf.Clamp(num3, 0f, num3);
						if (num2 < num3)
						{
							value = zerodistanceFormatter.Format(text);
						}
					}
					else
					{
						value = distanceFormatter.Format(num, text);
					}
				}
				else
				{
					value = distanceFormatter.Format(num, text);
				}
			}
			else
			{
				value = "";
			}
			return true;
		default:
			return false;
		}
	}

	public override void Init()
	{
		base.Init();
		IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnHovered(bool _isOver)
	{
		base.OnHovered(_isOver);
		if (Quest == null)
		{
			IsHovered = false;
		}
		else if (IsHovered != _isOver)
		{
			IsHovered = _isOver;
			RefreshBindings();
		}
	}

	public override void Update(float _dt)
	{
		RefreshBindings(IsDirty);
		IsDirty = false;
		base.Update(_dt);
	}

	public void Refresh()
	{
		IsDirty = true;
	}

	public override bool ParseAttribute(string name, string value, XUiController _parent)
	{
		switch (name)
		{
		case "enabled_color":
			enabledColor = value;
			return true;
		case "disabled_color":
			disabledColor = value;
			return true;
		case "failed_color":
			failedColor = value;
			return true;
		case "row_color":
			rowColor = value;
			return true;
		case "hover_color":
			hoverColor = value;
			return true;
		case "failed_icon":
			failedIcon = value;
			return true;
		default:
			return base.ParseAttribute(name, value, _parent);
		}
	}
}
