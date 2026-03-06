using System.Xml.Linq;
using UnityEngine.Scripting;

[Preserve]
public class MinEventActionGiveExp : MinEventActionTargetedBase
{
	[PublicizedFrom(EAccessModifier.Private)]
	public int exp = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool cvarRef;

	[PublicizedFrom(EAccessModifier.Private)]
	public string refCvarName;

	public override void Execute(MinEventParams _params)
	{
		if (targets == null)
		{
			return;
		}
		for (int i = 0; i < targets.Count; i++)
		{
			EntityAlive entityAlive = targets[i];
			if (entityAlive.Progression != null)
			{
				entityAlive.Progression.AddLevelExp((!cvarRef) ? exp : ((int)entityAlive.Buffs.GetCustomVar(refCvarName)));
				entityAlive.Progression.bProgressionStatsChanged = !entityAlive.isEntityRemote;
			}
			entityAlive.bPlayerStatsChanged |= !entityAlive.isEntityRemote;
		}
	}

	public override bool CanExecute(MinEventTypes _eventType, MinEventParams _params)
	{
		if (base.CanExecute(_eventType, _params))
		{
			if (!cvarRef)
			{
				return exp > 0;
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
			string localName = _attribute.Name.LocalName;
			if (localName == "experience" || localName == "exp")
			{
				if (_attribute.Value.StartsWith("@"))
				{
					cvarRef = true;
					refCvarName = _attribute.Value.Substring(1);
				}
				else
				{
					exp = StringParsers.ParseSInt32(_attribute.Value);
				}
			}
		}
		return flag;
	}
}
