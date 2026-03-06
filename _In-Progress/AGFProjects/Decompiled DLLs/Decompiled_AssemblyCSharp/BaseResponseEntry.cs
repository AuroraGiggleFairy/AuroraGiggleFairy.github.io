using UnityEngine.Scripting;

[Preserve]
public class BaseResponseEntry
{
	public enum ResponseTypes
	{
		Response,
		QuestAdd
	}

	public string UniqueID = "";

	public DialogResponse Response;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public string ID { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public ResponseTypes ResponseType { get; set; }
}
