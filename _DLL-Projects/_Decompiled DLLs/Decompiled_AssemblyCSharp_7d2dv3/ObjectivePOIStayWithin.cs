using System.IO;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ObjectivePOIStayWithin : BaseObjective
{
	public static string PropRadius = "radius";

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 position;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool positionSet;

	[PublicizedFrom(EAccessModifier.Private)]
	public Rect outerRect;

	[PublicizedFrom(EAccessModifier.Private)]
	public Rect innerRect;

	[PublicizedFrom(EAccessModifier.Private)]
	public float offset;

	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject prefabBounds;

	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject goBounds;

	public override ObjectiveValueTypes ObjectiveValueType => ObjectiveValueTypes.Boolean;

	public override bool UpdateUI => base.ObjectiveState != ObjectiveStates.Failed;

	public override bool useUpdateLoop
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return true;
		}
	}

	public override bool ShowInQuestLog => false;

	public override void SetupObjective()
	{
		keyword = Localization.Get("ObjectiveStayWithin_keyword");
	}

	public override void SetupDisplay()
	{
		base.Description = $"{keyword}";
	}

	public override void AddHooks()
	{
		QuestEventManager.Current.AddObjectiveToBeUpdated(this);
	}

	public override void RemoveHooks()
	{
		QuestEventManager.Current.RemoveObjectiveToBeUpdated(this);
		if (base.OwnerQuest == base.OwnerQuest.OwnerJournal.ActiveQuest)
		{
			QuestEventManager.Current.QuestBounds = default(Rect);
		}
		if (goBounds != null)
		{
			Object.Destroy(goBounds);
		}
	}

	public override void Refresh()
	{
		SetupDisplay();
		if (base.ObjectiveState != ObjectiveStates.NotStarted || base.OwnerQuest.CurrentState != Quest.QuestState.InProgress)
		{
			base.Complete = base.OwnerQuest.CurrentState != Quest.QuestState.Failed;
		}
	}

	public override void Read(BinaryReader _br)
	{
	}

	public override void Write(BinaryWriter _bw)
	{
	}

	public override BaseObjective Clone()
	{
		ObjectivePOIStayWithin objectivePOIStayWithin = new ObjectivePOIStayWithin();
		CopyValues(objectivePOIStayWithin);
		return objectivePOIStayWithin;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void CopyValues(BaseObjective objective)
	{
		base.CopyValues(objective);
		ObjectivePOIStayWithin obj = (ObjectivePOIStayWithin)objective;
		obj.outerRect = outerRect;
		obj.innerRect = innerRect;
		obj.offset = offset;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public GameObject CreateBoundsViewer()
	{
		if (prefabBounds == null)
		{
			prefabBounds = Resources.Load<GameObject>("Prefabs/prefabPOIBounds");
		}
		GameObject gameObject = Object.Instantiate(prefabBounds);
		gameObject.name = "QuestBounds";
		return gameObject;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 GetPosition()
	{
		if (base.OwnerQuest.GetPositionData(out position, Quest.PositionDataTypes.POIPosition) && base.OwnerQuest.GetPositionData(out var pos, Quest.PositionDataTypes.POISize))
		{
			PrefabInstance prefabAtPosition = GameManager.Instance.GetDynamicPrefabDecorator().GetPrefabAtPosition(position);
			if (prefabAtPosition != null)
			{
				base.OwnerQuest.Position = position;
				positionSet = true;
				outerRect = new Rect(position.x - offset, position.z - offset, pos.x + offset * 2f, pos.z + offset * 2f);
				innerRect = new Rect(position.x, position.z, pos.x, pos.z);
				float rotationAngle = prefabAtPosition.RotationAngle;
				outerRect = GeometryUtils.RotateRectAboutY(outerRect, rotationAngle);
				innerRect = GeometryUtils.RotateRectAboutY(innerRect, rotationAngle);
				QuestEventManager.Current.QuestBounds = outerRect;
				if (goBounds == null)
				{
					goBounds = CreateBoundsViewer();
				}
				goBounds.GetComponent<POIBoundsHelper>().SetPosition(new Vector3(outerRect.center.x, base.OwnerQuest.OwnerJournal.OwnerPlayer.position.y, outerRect.center.y) - Origin.position, new Vector3(outerRect.width, 200f, outerRect.height));
				base.CurrentValue = 2;
				return position;
			}
		}
		return Vector3.zero;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void UpdateState_NeedSetup()
	{
		_ = GetPosition() != Vector3.zero;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void UpdateState_Update()
	{
		if (!positionSet)
		{
			GetPosition();
			return;
		}
		Vector3 point = base.OwnerQuest.OwnerJournal.OwnerPlayer.position;
		_ = base.OwnerQuest.Position;
		point.y = point.z;
		if (!outerRect.Contains(point))
		{
			base.Complete = false;
			base.ObjectiveState = ObjectiveStates.Failed;
			base.OwnerQuest.CloseQuest(Quest.QuestState.Failed);
		}
		else if (innerRect.Contains(point))
		{
			base.ObjectiveState = ObjectiveStates.Complete;
		}
		else
		{
			base.ObjectiveState = ObjectiveStates.Warning;
		}
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		if (properties.Values.ContainsKey(PropRadius))
		{
			offset = StringParsers.ParseSInt32(properties.Values[PropRadius]);
		}
	}
}
