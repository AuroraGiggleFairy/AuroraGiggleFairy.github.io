using UnityEngine.Scripting;

[Preserve]
public class ItemClassQuest : ItemClass
{
	public static ItemClassQuest[] questItemList = new ItemClassQuest[100];

	public override bool CanDrop(ItemValue _iv = null)
	{
		return false;
	}

	public override bool CanStack()
	{
		return false;
	}

	public override bool KeepOnDeath()
	{
		return true;
	}

	public new static void Cleanup()
	{
		questItemList = null;
	}

	public override bool CanPlaceInContainer()
	{
		return false;
	}

	public static ItemClassQuest GetItemQuestById(ushort _questTypeID)
	{
		if (questItemList == null)
		{
			return null;
		}
		if (_questTypeID < 0 || _questTypeID >= questItemList.Length)
		{
			return null;
		}
		return questItemList[_questTypeID];
	}
}
