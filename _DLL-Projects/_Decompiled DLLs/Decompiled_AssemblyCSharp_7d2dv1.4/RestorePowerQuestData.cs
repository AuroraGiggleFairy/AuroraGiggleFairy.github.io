using UnityEngine.Scripting;

[Preserve]
public class RestorePowerQuestData : BaseQuestData
{
	public string CompleteEvent = "";

	[field: PublicizedFrom(EAccessModifier.Private)]
	public Vector3i PrefabPosition
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public RestorePowerQuestData(int _questCode, int _entityID, Vector3i _position, string _completeEvent)
	{
		CompleteEvent = _completeEvent;
		questCode = _questCode;
		entityList.Add(_entityID);
		PrefabPosition = _position;
	}

	public void UpdatePosition(Vector3i _pos)
	{
		PrefabPosition = _pos;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void RemoveFromDictionary()
	{
		QuestEventManager.Current.BlockActivateQuestDictionary.Remove(questCode);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnRemove(EntityPlayer player)
	{
		if (CompleteEvent != "")
		{
			GameEventManager.Current.HandleAction(CompleteEvent, null, player, twitchActivated: false, PrefabPosition);
		}
	}
}
