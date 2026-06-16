public class vp_Gameplay
{
	public static bool isMultiplayer = false;

	[PublicizedFrom(EAccessModifier.Protected)]
	public static bool m_IsMaster = true;

	public static bool isMaster
	{
		get
		{
			if (!isMultiplayer)
			{
				return true;
			}
			return m_IsMaster;
		}
		set
		{
			if (isMultiplayer)
			{
				m_IsMaster = value;
			}
		}
	}
}
