public class TraderStageTemplate
{
	public int Min = -1;

	public int Max = -1;

	public int Quality = -1;

	public bool IsWithin(int traderStage, int quality)
	{
		if ((Min == -1 || Min <= traderStage) && (Max == -1 || Max >= traderStage))
		{
			if (Quality != -1)
			{
				return quality == Quality;
			}
			return true;
		}
		return false;
	}
}
