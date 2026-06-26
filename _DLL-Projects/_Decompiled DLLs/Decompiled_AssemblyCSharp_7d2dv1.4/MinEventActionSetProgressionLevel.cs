using System.Xml.Linq;
using UnityEngine.Scripting;

[Preserve]
public class MinEventActionSetProgressionLevel : MinEventActionTargetedBase
{
	[field: PublicizedFrom(EAccessModifier.Private)]
	public string progressionName
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public int level
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public override void Execute(MinEventParams _params)
	{
		if (targets == null)
		{
			return;
		}
		for (int i = 0; i < targets.Count; i++)
		{
			if (targets[i].Progression == null)
			{
				continue;
			}
			ProgressionValue progressionValue = targets[i].Progression.GetProgressionValue(progressionName);
			if (progressionValue != null)
			{
				if (level != -1)
				{
					progressionValue.Level = level;
					targets[i].Progression.bProgressionStatsChanged = !targets[i].isEntityRemote;
					targets[i].bPlayerStatsChanged |= !targets[i].isEntityRemote;
				}
				else
				{
					progressionValue.Level = progressionValue.ProgressionClass.MaxLevel;
					targets[i].Progression.bProgressionStatsChanged = !targets[i].isEntityRemote;
					targets[i].bPlayerStatsChanged |= !targets[i].isEntityRemote;
				}
			}
		}
	}

	public override bool CanExecute(MinEventTypes _eventType, MinEventParams _params)
	{
		if (base.CanExecute(_eventType, _params) && progressionName != null)
		{
			return level >= -1;
		}
		return false;
	}

	public override bool ParseXmlAttribute(XAttribute _attribute)
	{
		bool flag = base.ParseXmlAttribute(_attribute);
		if (!flag)
		{
			string localName = _attribute.Name.LocalName;
			if (!(localName == "progression_name"))
			{
				if (localName == "level")
				{
					level = StringParsers.ParseSInt32(_attribute.Value);
				}
			}
			else
			{
				progressionName = _attribute.Value;
			}
		}
		return flag;
	}
}
