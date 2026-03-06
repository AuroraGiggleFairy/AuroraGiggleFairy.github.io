using System.Xml.Linq;
using UnityEngine.Scripting;

[Preserve]
public class MinEventActionAddProgressionLevel : MinEventActionTargetedBase
{
	[PublicizedFrom(EAccessModifier.Private)]
	public bool showMessage = true;

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
			if (progressionValue == null)
			{
				continue;
			}
			int num = progressionValue.Level;
			progressionValue.Level += level;
			if (progressionValue.Level > progressionValue.ProgressionClass.MaxLevel)
			{
				progressionValue.Level = progressionValue.ProgressionClass.MaxLevel;
			}
			if (progressionValue.Level < 0)
			{
				progressionValue.Level = 0;
			}
			if (num != progressionValue.Level && progressionValue.ProgressionClass.IsCrafting && targets[i] is EntityPlayerLocal)
			{
				EntityPlayerLocal entityPlayerLocal = targets[i] as EntityPlayerLocal;
				entityPlayerLocal.PlayerUI.xui.CollectedItemList.AddCraftingSkillNotification(progressionValue);
				if (showMessage)
				{
					progressionValue.ProgressionClass.HandleCheckCrafting(entityPlayerLocal, num, progressionValue.Level);
				}
			}
			targets[i].Progression.bProgressionStatsChanged = !targets[i].isEntityRemote;
			targets[i].bPlayerStatsChanged |= !targets[i].isEntityRemote;
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
			switch (_attribute.Name.LocalName)
			{
			case "progression_name":
				progressionName = _attribute.Value;
				break;
			case "level":
				level = StringParsers.ParseSInt32(_attribute.Value);
				break;
			case "show_message":
				showMessage = StringParsers.ParseBool(_attribute.Value);
				break;
			}
		}
		return flag;
	}
}
