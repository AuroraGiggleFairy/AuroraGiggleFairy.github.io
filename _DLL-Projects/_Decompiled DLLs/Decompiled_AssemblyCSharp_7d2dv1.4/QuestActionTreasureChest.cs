using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class QuestActionTreasureChest : BaseQuestAction
{
	public override void SetupAction()
	{
	}

	public override void PerformAction(Quest ownerQuest)
	{
		World world = GameManager.Instance.World;
		EntityPlayer ownerPlayer = ownerQuest.OwnerJournal.OwnerPlayer;
		float num = ((Value == "" || Value == null) ? 50f : StringParsers.ParseFloat(Value));
		GameRandom gameRandom = world.GetGameRandom();
		Vector3 vector = new Vector3(-1f + 2f * gameRandom.RandomFloat, 0f, -1f + 2f * gameRandom.RandomFloat);
		vector.Normalize();
		Vector3 vector2 = ownerPlayer.position + vector * num;
		int num2 = (int)vector2.x;
		int num3 = (int)vector2.z;
		int num4 = world.GetHeight(num2, num3) - 3;
		BlockValue blockValue = new BlockValue
		{
			type = 372
		};
		Vector3i blockPos = new Vector3i(num2, num4, num3);
		world.SetBlockRPC(blockPos, blockValue, sbyte.MaxValue);
		ownerQuest.DataVariables.Add("treasurecontainer", $"{num2},{num4},{num3}");
	}

	public override BaseQuestAction Clone()
	{
		QuestActionTreasureChest questActionTreasureChest = new QuestActionTreasureChest();
		CopyValues(questActionTreasureChest);
		return questActionTreasureChest;
	}
}
