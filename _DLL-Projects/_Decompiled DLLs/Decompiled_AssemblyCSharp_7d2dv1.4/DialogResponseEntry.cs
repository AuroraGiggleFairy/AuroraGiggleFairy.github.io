using UnityEngine.Scripting;

[Preserve]
public class DialogResponseEntry : BaseResponseEntry
{
	public DialogResponseEntry(string _ID)
	{
		base.ID = _ID;
		base.ResponseType = ResponseTypes.Response;
	}
}
