using System.IO;
using System.Xml.Linq;
using UnityEngine.Scripting;

namespace Challenges;

[Preserve]
public class ChallengeObjectiveTime : BaseChallengeObjective
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public float currentTime;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float maxTime;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float nextCheck;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool isInvalid = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayerLocal player;

	public float CurrentTime
	{
		get
		{
			return currentTime;
		}
		set
		{
			currentTime = value;
			HandleValueChanged();
		}
	}

	public override ChallengeObjectiveType ObjectiveType => ChallengeObjectiveType.Time;

	public override string DescriptionText
	{
		get
		{
			if (Biome == "")
			{
				return Localization.Get("challengeObjectiveTime") + ":";
			}
			return string.Format(Localization.Get("challengeObjectiveTimeInBiome"), Localization.Get("biome_" + Biome));
		}
	}

	public override string StatusText
	{
		get
		{
			if (currentTime == 0f)
			{
				return string.Format("{0}{1}", currentTime, Localization.Get("timeAbbreviationSeconds")) + " / " + XUiM_PlayerBuffs.GetTimeString(maxTime);
			}
			return XUiM_PlayerBuffs.GetTimeString(currentTime) + "/ " + XUiM_PlayerBuffs.GetTimeString(maxTime);
		}
	}

	public override float FillAmount => currentTime / maxTime;

	public override bool NeedsConstantUIUpdate => !base.Complete;

	public override void Init()
	{
	}

	public override void HandleAddHooks()
	{
		QuestEventManager.Current.AddObjectiveToBeUpdated(this);
	}

	public override void HandleRemoveHooks()
	{
		QuestEventManager.Current.RemoveObjectiveToBeUpdated(this);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Update(float deltaTime)
	{
		nextCheck -= deltaTime;
		if (nextCheck <= 0f)
		{
			isInvalid = CheckBaseRequirements();
			nextCheck = 2f;
		}
		if (player == null)
		{
			player = Owner.Owner.Player;
		}
		if (!isInvalid && !(player == null) && !player.IsDead())
		{
			CurrentTime += deltaTime;
			if (currentTime >= maxTime)
			{
				base.Current = MaxCount;
				CheckObjectiveComplete();
			}
		}
	}

	public override void ParseElement(XElement e)
	{
		base.ParseElement(e);
		if (e.HasAttribute("max_time"))
		{
			maxTime = StringParsers.ParseFloat(e.GetAttribute("max_time"));
		}
	}

	public override void Read(byte _currentVersion, BinaryReader _br)
	{
		base.Read(_currentVersion, _br);
		currentTime = _br.ReadSingle();
	}

	public override void Write(BinaryWriter _bw)
	{
		base.Write(_bw);
		_bw.Write(currentTime);
	}

	public override void CopyValues(BaseChallengeObjective obj, BaseChallengeObjective objFromClass)
	{
		base.CopyValues(obj, objFromClass);
		if (obj is ChallengeObjectiveTime challengeObjectiveTime)
		{
			currentTime = challengeObjectiveTime.currentTime;
		}
	}

	public override BaseChallengeObjective Clone()
	{
		return new ChallengeObjectiveTime
		{
			Biome = Biome,
			maxTime = maxTime,
			currentTime = currentTime,
			nextCheck = nextCheck,
			isInvalid = isInvalid
		};
	}
}
