using System.IO;
using System.Xml.Linq;
using UnityEngine.Scripting;

namespace Challenges;

[Preserve]
public class BaseChallengeObjective
{
	public static byte FileVersion = 1;

	public int MaxCount = 1;

	public bool IsRequirement;

	public Challenge Owner;

	public ChallengeClass OwnerClass;

	public bool ShowRequirements = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool complete;

	public bool IsTracking;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int current;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public byte CurrentFileVersion { get; set; }

	public virtual ChallengeObjectiveType ObjectiveType => ChallengeObjectiveType.Invalid;

	public virtual ChallengeClass.UINavTypes NavType => ChallengeClass.UINavTypes.None;

	public bool Complete
	{
		get
		{
			return complete;
		}
		set
		{
			if (complete != value)
			{
				complete = value;
				if (this.ValueChanged != null)
				{
					this.ValueChanged();
				}
			}
		}
	}

	public int Current
	{
		get
		{
			return current;
		}
		set
		{
			if (current != value)
			{
				current = value;
				if (this.ValueChanged != null)
				{
					this.ValueChanged();
				}
			}
		}
	}

	public EntityPlayerLocal Player => Owner.Owner.Player;

	public string ObjectiveText => $"{DescriptionText} {StatusText}";

	public virtual string DescriptionText => "";

	public virtual string StatusText => $"{current}/{MaxCount}";

	public virtual float FillAmount => (float)current / (float)MaxCount;

	public event ObjectiveValueChanged ValueChanged;

	public virtual void BaseInit()
	{
	}

	public void ResetComplete()
	{
		Complete = false;
		current = 0;
	}

	public virtual void Init()
	{
	}

	public virtual void HandleOnCreated()
	{
	}

	public virtual bool HandleCheckStatus()
	{
		Complete = CheckObjectiveComplete(handleComplete: false);
		return Complete;
	}

	public virtual void UpdateStatus()
	{
	}

	public virtual void HandleAddHooks()
	{
	}

	public virtual void HandleRemoveHooks()
	{
	}

	public virtual void HandleTrackingStarted()
	{
		IsTracking = true;
	}

	public virtual void CopyValues(BaseChallengeObjective obj, BaseChallengeObjective objFromClass)
	{
		current = obj.current;
		MaxCount = objFromClass.MaxCount;
		ShowRequirements = objFromClass.ShowRequirements;
		complete = Current >= MaxCount;
	}

	public virtual void HandleTrackingEnded()
	{
		IsTracking = false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void HandleUpdatingCurrent()
	{
	}

	public static BaseChallengeObjective ReadObjective(byte _currentVersion, ChallengeObjectiveType _type, BinaryReader _br)
	{
		BaseChallengeObjective baseChallengeObjective = null;
		switch (_type)
		{
		case ChallengeObjectiveType.BlockPlace:
			baseChallengeObjective = new ChallengeObjectiveBlockPlace();
			break;
		case ChallengeObjectiveType.BlockUpgrade:
			baseChallengeObjective = new ChallengeObjectiveBlockUpgrade();
			break;
		case ChallengeObjectiveType.Bloodmoon:
			baseChallengeObjective = new ChallengeObjectiveBloodmoon();
			break;
		case ChallengeObjectiveType.Craft:
			baseChallengeObjective = new ChallengeObjectiveCraft();
			break;
		case ChallengeObjectiveType.CureDebuff:
			baseChallengeObjective = new ChallengeObjectiveCureDebuff();
			break;
		case ChallengeObjectiveType.EnterBiome:
			baseChallengeObjective = new ChallengeObjectiveEnterBiome();
			break;
		case ChallengeObjectiveType.Gather:
			baseChallengeObjective = new ChallengeObjectiveGather();
			break;
		case ChallengeObjectiveType.GatherIngredient:
			baseChallengeObjective = new ChallengeObjectiveGatherIngredient();
			break;
		case ChallengeObjectiveType.Harvest:
			baseChallengeObjective = new ChallengeObjectiveHarvest();
			break;
		case ChallengeObjectiveType.Hold:
			baseChallengeObjective = new ChallengeObjectiveHold();
			break;
		case ChallengeObjectiveType.Kill:
			baseChallengeObjective = new ChallengeObjectiveKill();
			break;
		case ChallengeObjectiveType.QuestComplete:
			baseChallengeObjective = new ChallengeObjectiveQuestComplete();
			break;
		case ChallengeObjectiveType.Scrap:
			baseChallengeObjective = new ChallengeObjectiveScrap();
			break;
		case ChallengeObjectiveType.Survive:
			baseChallengeObjective = new ChallengeObjectiveSurvive();
			break;
		case ChallengeObjectiveType.Trader:
			baseChallengeObjective = new ChallengeObjectiveTrader();
			break;
		case ChallengeObjectiveType.Wear:
			baseChallengeObjective = new ChallengeObjectiveWear();
			break;
		case ChallengeObjectiveType.Use:
			baseChallengeObjective = new ChallengeObjectiveUseItem();
			break;
		case ChallengeObjectiveType.ChallengeComplete:
			baseChallengeObjective = new ChallengeObjectiveChallengeComplete();
			break;
		case ChallengeObjectiveType.MeetTrader:
			baseChallengeObjective = new ChallengeObjectiveMeetTrader();
			break;
		case ChallengeObjectiveType.KillByTag:
			baseChallengeObjective = new ChallengeObjectiveKillByTag();
			break;
		case ChallengeObjectiveType.ChallengeStatAwarded:
			baseChallengeObjective = new ChallengeObjectiveChallengeStatAwarded();
			break;
		case ChallengeObjectiveType.SpendSkillPoint:
			baseChallengeObjective = new ChallengeObjectiveSpendSkillPoint();
			break;
		case ChallengeObjectiveType.Twitch:
			baseChallengeObjective = new ChallengeObjectiveTwitch();
			break;
		}
		baseChallengeObjective?.Read(_currentVersion, _br);
		return baseChallengeObjective;
	}

	public virtual Recipe GetRecipeItem()
	{
		return null;
	}

	public virtual Recipe[] GetRecipeItems()
	{
		if (Owner.NeedsPreRequisites)
		{
			Recipe recipeFromRequirements = Owner.GetRecipeFromRequirements();
			if (recipeFromRequirements != null)
			{
				return new Recipe[1] { recipeFromRequirements };
			}
		}
		return null;
	}

	public virtual void Read(byte _currentVersion, BinaryReader _br)
	{
		current = _br.ReadInt32();
	}

	public void WriteObjective(BinaryWriter _bw)
	{
		_bw.Write((byte)ObjectiveType);
		Write(_bw);
	}

	public virtual void Write(BinaryWriter _bw)
	{
		_bw.Write(current);
	}

	public virtual bool CheckObjectiveComplete(bool handleComplete = true)
	{
		HandleUpdatingCurrent();
		if (Current >= MaxCount)
		{
			Current = MaxCount;
			Complete = true;
			if (handleComplete)
			{
				Owner.HandleComplete();
			}
			return true;
		}
		if (handleComplete)
		{
			Owner.HandleComplete();
		}
		Complete = false;
		return false;
	}

	public virtual BaseChallengeObjective Clone()
	{
		return null;
	}

	public virtual void ParseElement(XElement e)
	{
		if (e.HasAttribute("count"))
		{
			MaxCount = StringParsers.ParseSInt32(e.GetAttribute("count"));
		}
		if (e.HasAttribute("show_requirements"))
		{
			ShowRequirements = StringParsers.ParseBool(e.GetAttribute("show_requirements"));
		}
	}
}
