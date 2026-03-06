using System.Xml.Linq;
using UnityEngine.Scripting;

[Preserve]
public class MinEventActionGiveSkillExp : MinEventActionTargetedBase
{
	[PublicizedFrom(EAccessModifier.Private)]
	public string skill;

	[PublicizedFrom(EAccessModifier.Private)]
	public int exp = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public float level_percent = -1f;

	public override void Execute(MinEventParams _params)
	{
		if (targets == null)
		{
			return;
		}
		for (int i = 0; i < targets.Count; i++)
		{
			if (exp != -1)
			{
				targets[i].Progression.AddLevelExp(exp);
				targets[i].Progression.bProgressionStatsChanged = !targets[i].isEntityRemote;
				targets[i].bPlayerStatsChanged |= !targets[i].isEntityRemote;
			}
			else if (level_percent != -1f)
			{
				targets[i].Progression.AddLevelExp(exp);
				targets[i].Progression.bProgressionStatsChanged = !targets[i].isEntityRemote;
				targets[i].bPlayerStatsChanged |= !targets[i].isEntityRemote;
			}
		}
	}

	public override bool CanExecute(MinEventTypes _eventType, MinEventParams _params)
	{
		if (base.CanExecute(_eventType, _params) && skill != null)
		{
			if (exp == -1)
			{
				return level_percent != -1f;
			}
			return true;
		}
		return false;
	}

	public override bool ParseXmlAttribute(XAttribute _attribute)
	{
		bool flag = base.ParseXmlAttribute(_attribute);
		if (!flag)
		{
			switch (_attribute.Name.LocalName)
			{
			case "skill":
				skill = _attribute.Value;
				break;
			case "experience":
				exp = StringParsers.ParseSInt32(_attribute.Value);
				break;
			case "level_percentage":
				level_percent = StringParsers.ParseFloat(_attribute.Value);
				break;
			}
		}
		return flag;
	}
}
