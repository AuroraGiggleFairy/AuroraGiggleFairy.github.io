using System.Collections.Generic;

public class TraderStageTemplateGroup
{
	public string Name = "";

	public List<TraderStageTemplate> Templates = new List<TraderStageTemplate>();

	public bool IsWithin(int traderStage, int quality)
	{
		for (int i = 0; i < Templates.Count; i++)
		{
			if (Templates[i].IsWithin(traderStage, quality))
			{
				return true;
			}
		}
		return false;
	}
}
