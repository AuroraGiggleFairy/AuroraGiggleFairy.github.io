using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_QuestEntry : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public string enabledColor;

	[PublicizedFrom(EAccessModifier.Private)]
	public string disabledColor;

	[PublicizedFrom(EAccessModifier.Private)]
	public string failedColor;

	[PublicizedFrom(EAccessModifier.Private)]
	public string completeColor;

	[PublicizedFrom(EAccessModifier.Private)]
	public string finishedColor;

	[PublicizedFrom(EAccessModifier.Private)]
	public string sharedColor;

	[PublicizedFrom(EAccessModifier.Private)]
	public string failedIcon;

	[PublicizedFrom(EAccessModifier.Private)]
	public string completeIcon;

	[PublicizedFrom(EAccessModifier.Private)]
	public string finishedIcon;

	[PublicizedFrom(EAccessModifier.Private)]
	public string sharedIcon;

	[PublicizedFrom(EAccessModifier.Private)]
	public string questlimitedIcon = "ui_game_symbol_quest_limited";

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
			base.ViewComponent.Enabled = value != null;
			viewComponent.IsNavigatable = (base.ViewComponent.IsSnappable = value != null);
		}
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public XUiC_QuestWindowGroup QuestUIHandler { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool Tracked { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public SharedQuestEntry SharedQuestEntry { get; set; }

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
				if (quest.CurrentState == Quest.QuestState.Failed)
				{
					value = failedIcon;
				}
				else if (quest.CurrentState == Quest.QuestState.Completed)
				{
					value = completeIcon;
				}
				else if (quest.CurrentState == Quest.QuestState.InProgress || quest.CurrentState == Quest.QuestState.ReadyForTurnIn)
				{
					if (quest.CurrentPhase == questClass.HighestPhase && questClass.CompletionType == QuestClass.CompletionTypes.TurnIn)
					{
						value = finishedIcon;
					}
					else if (quest.QuestClass.AddsToTierComplete && !base.xui.playerUI.entityPlayer.QuestJournal.CanAddProgression)
					{
						value = questlimitedIcon;
					}
					else if (quest.SharedOwnerID == -1)
					{
						value = questClass.Icon;
					}
					else
					{
						value = sharedIcon;
					}
				}
				else if (!quest.QuestClass.AddsToTierComplete || base.xui.playerUI.entityPlayer.QuestJournal.CanAddProgression)
				{
					value = questClass.Icon;
				}
				else
				{
					value = questlimitedIcon;
				}
			}
			return true;
		case "iconcolor":
			value = "255,255,255,255";
			if (flag)
			{
				switch (quest.CurrentState)
				{
				case Quest.QuestState.NotStarted:
				case Quest.QuestState.InProgress:
				case Quest.QuestState.ReadyForTurnIn:
					if (quest.CurrentPhase == questClass.HighestPhase && questClass.CompletionType == QuestClass.CompletionTypes.TurnIn)
					{
						value = finishedColor;
					}
					else if (quest.QuestClass.AddsToTierComplete && !base.xui.playerUI.entityPlayer.QuestJournal.CanAddProgression)
					{
						value = failedColor;
					}
					else if (quest.SharedOwnerID == -1)
					{
						value = enabledColor;
					}
					else
					{
						value = sharedColor;
					}
					break;
				case Quest.QuestState.Completed:
					value = completeColor;
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
				Quest.QuestState currentState = quest.CurrentState;
				if ((uint)currentState <= 2u)
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
			if (flag && (quest.Active || SharedQuestEntry != null) && quest.HasPosition)
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
				if (quest.MapObject is MapObjectTreasureChest)
				{
					float num3 = (quest.MapObject as MapObjectTreasureChest).DefaultRadius;
					if (num2 < num3)
					{
						num3 = EffectManager.GetValue(PassiveEffects.TreasureRadius, null, num3, base.xui.playerUI.entityPlayer);
						num3 = Mathf.Clamp(num3, 0f, 13f);
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
		case "complete_color":
			completeColor = value;
			return true;
		case "finished_color":
			finishedColor = value;
			return true;
		case "shared_color":
			sharedColor = value;
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
		case "complete_icon":
			completeIcon = value;
			return true;
		case "finished_icon":
			finishedIcon = value;
			return true;
		case "shared_icon":
			sharedIcon = value;
			return true;
		case "quest_limited_icon":
			questlimitedIcon = value;
			return true;
		default:
			return base.ParseAttribute(name, value, _parent);
		}
	}
}
