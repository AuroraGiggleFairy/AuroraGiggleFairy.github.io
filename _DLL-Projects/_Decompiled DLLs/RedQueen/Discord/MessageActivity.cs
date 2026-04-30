using System.Diagnostics;

namespace Discord;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
internal class MessageActivity
{
	public MessageActivityType Type { get; internal set; }

	public string PartyId { get; internal set; }

	private string DebuggerDisplay => string.Format("{0}{1}", Type, string.IsNullOrWhiteSpace(PartyId) ? "" : (" " + PartyId));

	public override string ToString()
	{
		return DebuggerDisplay;
	}
}
