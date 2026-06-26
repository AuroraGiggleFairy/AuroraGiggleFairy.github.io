public static class EnumDamageSourceExtensions
{
	public static bool AffectedByArmor(this EnumDamageSource s)
	{
		return s == EnumDamageSource.External;
	}
}
