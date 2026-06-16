using System.Xml.Linq;
using UnityEngine.Scripting;

namespace Challenges;

[Preserve]
public class ChallengeObjectiveChallengeStatAwarded : BaseChallengeObjective
{
	[PublicizedFrom(EAccessModifier.Private)]
	public string challengeStat = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public string statText = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public string trackerIndexName;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string trackerNavObjectName;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float trackDistance = 20f;

	public TrackingEntry trackingEntry;

	public override ChallengeObjectiveType ObjectiveType => ChallengeObjectiveType.ChallengeStatAwarded;

	public override string DescriptionText => Localization.Get(statText);

	public override void HandleAddHooks()
	{
		QuestEventManager.Current.ChallengeAwardCredit += Current_ChallengeAwardCredit;
		if (trackerIndexName != null && trackingEntry == null)
		{
			trackingEntry = new TrackingEntry
			{
				Owner = this,
				blockIndexName = trackerIndexName,
				navObjectName = ((trackerNavObjectName != null) ? trackerNavObjectName : "quest_resource"),
				trackDistance = trackDistance
			};
			trackingEntry.TrackingHelper = Owner.GetTrackingHelper();
		}
	}

	public override void HandleRemoveHooks()
	{
		QuestEventManager.Current.ChallengeAwardCredit -= Current_ChallengeAwardCredit;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Current_ChallengeAwardCredit(string stat, int awardCount)
	{
		if (!CheckBaseRequirements() && challengeStat.EqualsCaseInsensitive(stat))
		{
			base.Current += awardCount;
			if (base.Current >= MaxCount)
			{
				base.Current = MaxCount;
				CheckObjectiveComplete();
			}
		}
	}

	public override void HandleTrackingStarted()
	{
		base.HandleTrackingStarted();
		if (trackingEntry != null)
		{
			Owner.AddTrackingEntry(trackingEntry);
			trackingEntry.TrackingHelper = Owner.TrackingHandler;
			trackingEntry.AddHooks();
		}
	}

	public override void HandleTrackingEnded()
	{
		base.HandleTrackingEnded();
		if (trackingEntry != null)
		{
			trackingEntry.RemoveHooks();
			Owner.RemoveTrackingEntry(trackingEntry);
		}
	}

	public override void ParseElement(XElement e)
	{
		base.ParseElement(e);
		if (e.HasAttribute("challenge_stat"))
		{
			challengeStat = e.GetAttribute("challenge_stat");
		}
		if (e.HasAttribute("stat_text_key"))
		{
			statText = Localization.Get(e.GetAttribute("stat_text_key"));
		}
		else if (e.HasAttribute("stat_text"))
		{
			statText = e.GetAttribute("stat_text");
		}
		if (e.HasAttribute("tracker_index"))
		{
			trackerIndexName = e.GetAttribute("tracker_index");
		}
		if (e.HasAttribute("tracker_nav_object"))
		{
			trackerNavObjectName = e.GetAttribute("tracker_nav_object");
		}
		if (e.HasAttribute("track_distance"))
		{
			trackDistance = StringParsers.ParseFloat(e.GetAttribute("track_distance"));
		}
	}

	public override BaseChallengeObjective Clone()
	{
		return new ChallengeObjectiveChallengeStatAwarded
		{
			challengeStat = challengeStat,
			statText = statText,
			trackerIndexName = trackerIndexName,
			trackerNavObjectName = trackerNavObjectName,
			trackDistance = trackDistance
		};
	}
}
