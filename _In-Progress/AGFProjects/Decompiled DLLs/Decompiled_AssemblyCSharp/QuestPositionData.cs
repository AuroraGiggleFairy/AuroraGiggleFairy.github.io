using System.IO;

public class QuestPositionData
{
	public int questCode;

	public Quest.PositionDataTypes positionDataType;

	public Vector3i blockPosition;

	public QuestPositionData()
	{
	}

	public QuestPositionData(int questCode, Quest.PositionDataTypes positionDataType, Vector3i blockPosition)
	{
		this.questCode = questCode;
		this.positionDataType = positionDataType;
		this.blockPosition = blockPosition;
	}

	public static QuestPositionData Read(BinaryReader reader)
	{
		return new QuestPositionData
		{
			questCode = reader.ReadInt32(),
			positionDataType = (Quest.PositionDataTypes)reader.ReadInt32(),
			blockPosition = StreamUtils.ReadVector3i(reader)
		};
	}

	public void Write(BinaryWriter writer)
	{
		writer.Write(questCode);
		writer.Write((int)positionDataType);
		StreamUtils.Write(writer, blockPosition);
	}
}
