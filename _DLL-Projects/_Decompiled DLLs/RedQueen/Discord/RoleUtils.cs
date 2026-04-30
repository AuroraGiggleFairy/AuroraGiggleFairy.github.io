namespace Discord;

internal static class RoleUtils
{
	public static int Compare(IRole left, IRole right)
	{
		if (left == null)
		{
			return -1;
		}
		if (right == null)
		{
			return 1;
		}
		int num = left.Position.CompareTo(right.Position);
		if (num != 0)
		{
			return num;
		}
		return left.Id.CompareTo(right.Id);
	}
}
