using System.Xml.Linq;
using UnityEngine.Scripting;

namespace Challenges;

[Preserve]
public class ChallengeBaseTrackedItemObjective : BaseChallengeObjective
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public ItemValue expectedItem = ItemValue.None.Clone();

	[PublicizedFrom(EAccessModifier.Protected)]
	public ItemClass expectedItemClass;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string itemClassID = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public string overrideTrackerIndexName;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float trackDistance = 20f;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool disableTracking;

	public TrackingEntry trackingEntry;

	public override void Init()
	{
		expectedItem = ItemClass.GetItem(itemClassID);
		expectedItemClass = ItemClass.GetItemClass(itemClassID);
	}

	public void SetupItem(string itemID)
	{
		itemClassID = itemID;
	}

	public override void HandleAddHooks()
	{
		if (expectedItemClass != null)
		{
			string text = ((overrideTrackerIndexName != null) ? overrideTrackerIndexName : expectedItemClass.TrackerIndexName);
			if (text != null && trackingEntry == null && !disableTracking)
			{
				trackingEntry = new TrackingEntry
				{
					TrackedItem = expectedItemClass,
					Owner = this,
					blockIndexName = text,
					navObjectName = ((expectedItemClass.TrackerNavObject != null) ? expectedItemClass.TrackerNavObject : "quest_resource"),
					trackDistance = trackDistance
				};
				trackingEntry.TrackingHelper = Owner.GetTrackingHelper();
			}
		}
		base.HandleAddHooks();
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
		if (e.HasAttribute("item"))
		{
			itemClassID = e.GetAttribute("item");
		}
		if (e.HasAttribute("override_tracker_index"))
		{
			overrideTrackerIndexName = e.GetAttribute("override_tracker_index");
		}
		if (e.HasAttribute("track_distance"))
		{
			trackDistance = StringParsers.ParseFloat(e.GetAttribute("track_distance"));
		}
		if (e.HasAttribute("disable_tracking"))
		{
			disableTracking = StringParsers.ParseBool(e.GetAttribute("disable_tracking"));
		}
	}

	public override void CopyValues(BaseChallengeObjective obj, BaseChallengeObjective objFromClass)
	{
		base.CopyValues(obj, objFromClass);
		if (objFromClass is ChallengeBaseTrackedItemObjective challengeBaseTrackedItemObjective)
		{
			itemClassID = challengeBaseTrackedItemObjective.itemClassID;
			overrideTrackerIndexName = challengeBaseTrackedItemObjective.overrideTrackerIndexName;
			trackDistance = challengeBaseTrackedItemObjective.trackDistance;
			disableTracking = challengeBaseTrackedItemObjective.disableTracking;
		}
	}

	public override BaseChallengeObjective Clone()
	{
		return null;
	}

	public override void CompleteObjective(bool handleComplete = true)
	{
		base.Current = MaxCount;
		base.Complete = true;
		if (handleComplete)
		{
			Owner.HandleComplete();
		}
	}
}
