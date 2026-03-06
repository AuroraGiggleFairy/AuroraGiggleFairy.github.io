using System.Xml.Linq;
using UnityEngine.Scripting;

[Preserve]
public class MinEventActionAwardChallenge : MinEventActionTargetedBase
{
	[PublicizedFrom(EAccessModifier.Private)]
	public string challengeStat;

	[PublicizedFrom(EAccessModifier.Private)]
	public int challengeAwardCount = 1;

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
			if (targets[i] is EntityPlayerLocal entityPlayerLocal)
			{
				QuestEventManager.Current.ChallengeAwardCredited(challengeStat, (!cvarRef) ? challengeAwardCount : ((int)entityPlayerLocal.Buffs.GetCustomVar(refCvarName)));
			}
		}
	}

	public override bool CanExecute(MinEventTypes _eventType, MinEventParams _params)
	{
		if (base.CanExecute(_eventType, _params))
		{
			if (!cvarRef)
			{
				return challengeAwardCount > 0;
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
			if (!(localName == "count"))
			{
				if (localName == "challenge_stat")
				{
					challengeStat = _attribute.Value;
				}
			}
			else if (_attribute.Value.StartsWith("@"))
			{
				cvarRef = true;
				refCvarName = _attribute.Value.Substring(1);
			}
			else
			{
				challengeAwardCount = StringParsers.ParseSInt32(_attribute.Value);
			}
		}
		return flag;
	}
}
