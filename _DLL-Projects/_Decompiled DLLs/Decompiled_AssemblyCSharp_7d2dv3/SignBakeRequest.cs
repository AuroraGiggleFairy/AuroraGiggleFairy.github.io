using System;

public readonly struct SignBakeRequest(int groupIndex, int tier, float groupMinDistanceSquared) : IComparable<SignBakeRequest>
{
	public readonly int GroupIndex = groupIndex;

	public readonly int Tier = tier;

	public readonly float GroupMinDistanceSquared = groupMinDistanceSquared;

	public int CompareTo(SignBakeRequest other)
	{
		int tier = Tier;
		int num = tier.CompareTo(other.Tier);
		if (num != 0)
		{
			return num;
		}
		float groupMinDistanceSquared = GroupMinDistanceSquared;
		return groupMinDistanceSquared.CompareTo(other.GroupMinDistanceSquared);
	}
}
