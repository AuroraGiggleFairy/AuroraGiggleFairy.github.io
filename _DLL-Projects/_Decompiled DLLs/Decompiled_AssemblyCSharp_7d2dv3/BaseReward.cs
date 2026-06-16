using System.IO;

public abstract class BaseReward
{
	public enum RewardTypes
	{
		Exp,
		Item,
		Level,
		Quest,
		Recipe,
		ShowTip,
		Skill,
		SkillPoints
	}

	public enum ReceiveStages
	{
		QuestStart,
		QuestCompletion,
		AfterCompleteNotification
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool displaySetup;

	[PublicizedFrom(EAccessModifier.Private)]
	public string description = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public string valueText = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public string icon = "";

	public static string PropID = "id";

	public static string PropValue = "value";

	public static string PropOptional = "optional";

	public static string PropReceiveStage = "stage";

	public static string PropHidden = "hidden";

	public static string PropIsChosen = "ischosen";

	public static string PropIsChain = "chainreward";

	public static string PropIsFixed = "isfixed";

	[field: PublicizedFrom(EAccessModifier.Private)]
	public string ID { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public string Value { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public Quest OwnerQuest { get; set; }

	public string Description
	{
		get
		{
			if (!displaySetup)
			{
				SetupReward();
				displaySetup = true;
			}
			return description;
		}
		set
		{
			description = value;
		}
	}

	public string ValueText
	{
		get
		{
			if (!displaySetup)
			{
				SetupReward();
				displaySetup = true;
			}
			return valueText;
		}
		set
		{
			valueText = value;
		}
	}

	public string Icon
	{
		get
		{
			if (!displaySetup)
			{
				SetupReward();
				displaySetup = true;
			}
			return icon;
		}
		set
		{
			icon = value;
		}
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public string IconAtlas { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool HiddenReward { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool Optional { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool isChosenReward { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool isChainReward { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool isFixedLocation { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public ReceiveStages ReceiveStage { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public byte RewardIndex { get; set; }

	public BaseReward()
	{
		IconAtlas = "UIAtlas";
		ReceiveStage = ReceiveStages.QuestCompletion;
		isFixedLocation = false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void CopyValues(BaseReward reward)
	{
		reward.ID = ID;
		reward.Value = Value;
		reward.ReceiveStage = ReceiveStage;
		reward.HiddenReward = HiddenReward;
		reward.Optional = Optional;
		reward.isChosenReward = isChosenReward;
		reward.isChainReward = isChainReward;
		reward.isFixedLocation = isFixedLocation;
		reward.RewardIndex = RewardIndex;
	}

	public virtual void HandleVariables()
	{
		ID = OwnerQuest.ParseVariable(ID);
		Value = OwnerQuest.ParseVariable(Value);
	}

	public virtual void SetupReward()
	{
	}

	public virtual void GiveReward(EntityPlayer player)
	{
	}

	public void GiveReward()
	{
		GiveReward(OwnerQuest.OwnerJournal.OwnerPlayer);
	}

	public virtual ItemStack GetRewardItem()
	{
		return ItemStack.Empty;
	}

	public virtual BaseReward Clone()
	{
		return null;
	}

	public virtual void SetupGlobalRewardSettings()
	{
	}

	public virtual void Read(BinaryReader _br)
	{
		RewardIndex = _br.ReadByte();
	}

	public virtual void Write(BinaryWriter _bw)
	{
		_bw.Write(RewardIndex);
	}

	public virtual void ParseProperties(DynamicProperties properties)
	{
		if (properties.Values.ContainsKey(PropID))
		{
			ID = properties.Values[PropID];
		}
		if (properties.Values.ContainsKey(PropValue))
		{
			Value = properties.Values[PropValue];
		}
		if (properties.Values.ContainsKey(PropReceiveStage))
		{
			switch (properties.Values[PropReceiveStage])
			{
			case "start":
				ReceiveStage = ReceiveStages.QuestStart;
				break;
			case "complete":
				ReceiveStage = ReceiveStages.QuestCompletion;
				break;
			case "aftercomplete":
				ReceiveStage = ReceiveStages.AfterCompleteNotification;
				break;
			}
		}
		if (properties.Values.ContainsKey(PropOptional))
		{
			StringParsers.TryParseBool(properties.Values[PropOptional], out var _result);
			Optional = _result;
		}
		if (properties.Values.ContainsKey(PropHidden))
		{
			StringParsers.TryParseBool(properties.Values[PropHidden], out var _result2);
			HiddenReward = _result2;
		}
		if (properties.Values.ContainsKey(PropIsChosen))
		{
			StringParsers.TryParseBool(properties.Values[PropIsChosen], out var _result3);
			isChosenReward = _result3;
		}
		if (properties.Values.ContainsKey(PropIsFixed))
		{
			StringParsers.TryParseBool(properties.Values[PropIsFixed], out var _result4);
			isFixedLocation = _result4;
		}
		if (properties.Values.ContainsKey(PropIsChain))
		{
			StringParsers.TryParseBool(properties.Values[PropIsChain], out var _result5);
			isChainReward = _result5;
		}
	}

	public virtual string GetRewardText()
	{
		return "";
	}
}
