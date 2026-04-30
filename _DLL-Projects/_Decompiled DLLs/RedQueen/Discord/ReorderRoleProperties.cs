namespace Discord;

internal class ReorderRoleProperties
{
	public ulong Id { get; }

	public int Position { get; }

	public ReorderRoleProperties(ulong id, int pos)
	{
		Id = id;
		Position = pos;
	}
}
