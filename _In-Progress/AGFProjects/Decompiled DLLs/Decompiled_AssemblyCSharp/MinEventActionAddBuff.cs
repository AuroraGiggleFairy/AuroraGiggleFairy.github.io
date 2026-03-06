using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine.Scripting;

[Preserve]
public class MinEventActionAddBuff : MinEventActionBuffModifierBase
{
	[PublicizedFrom(EAccessModifier.Private)]
	public float duration;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool durationAltered;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool cvarRef;

	[PublicizedFrom(EAccessModifier.Private)]
	public string refCvarName = string.Empty;

	public override void Execute(MinEventParams _params)
	{
		bool netSync = !_params.Self.isEntityRemote | _params.IsLocal;
		int num = -1;
		if (_params.Buff != null)
		{
			num = _params.Buff.InstigatorId;
		}
		if (num == -1)
		{
			num = _params.Self.entityId;
		}
		for (int i = 0; i < targets.Count; i++)
		{
			string[] array = buffNames;
			if (buffOneOnly && buffWeights != null)
			{
				float randomFloat = targets[i].rand.RandomFloat;
				float num2 = 0f;
				for (int j = 0; j < buffWeights.Length; j++)
				{
					num2 += buffWeights[j];
					if (num2 >= randomFloat)
					{
						array = new string[1] { buffNames[j] };
						break;
					}
				}
			}
			else if (buffWeights != null)
			{
				List<string> list = new List<string>();
				for (int k = 0; k < buffWeights.Length; k++)
				{
					float randomFloat2 = targets[i].rand.RandomFloat;
					if (buffWeights[k] >= randomFloat2)
					{
						list.Add(buffNames[k]);
					}
				}
				array = list.ToArray();
			}
			foreach (string name in array)
			{
				BuffClass buff = BuffManager.GetBuff(name);
				if (buff == null)
				{
					continue;
				}
				if (durationAltered && cvarRef)
				{
					if (targets[i].Buffs.HasCustomVar(refCvarName))
					{
						duration = targets[i].Buffs.GetCustomVar(refCvarName);
					}
					else
					{
						duration = buff.InitialDurationMax;
					}
				}
				if (durationAltered)
				{
					targets[i].Buffs.AddBuff(name, num, netSync, _fromElectrical: false, duration);
				}
				else
				{
					targets[i].Buffs.AddBuff(name, num, netSync);
				}
			}
		}
	}

	public override bool ParseXmlAttribute(XAttribute _attribute)
	{
		bool flag = base.ParseXmlAttribute(_attribute);
		if (!flag && _attribute.Name.LocalName == "duration")
		{
			if (_attribute.Value.StartsWith("@"))
			{
				cvarRef = true;
				refCvarName = _attribute.Value.Substring(1);
			}
			else
			{
				duration = StringParsers.ParseFloat(_attribute.Value);
			}
			durationAltered = true;
			return true;
		}
		return flag;
	}
}
