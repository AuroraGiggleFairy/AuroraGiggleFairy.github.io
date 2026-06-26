using System.Linq;
using System.Xml.Linq;

public class MinEventActionBuffModifierBase : MinEventActionTargetedBase
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public string[] buffNames;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float[] buffWeights;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool buffOneOnly;

	public override bool ParseXmlAttribute(XAttribute _attribute)
	{
		bool flag = base.ParseXmlAttribute(_attribute);
		if (!flag)
		{
			switch (_attribute.Name.LocalName)
			{
			case "buffs":
			case "buff":
				if (_attribute.Value.Contains(","))
				{
					buffNames = _attribute.Value.Replace(" ", "").Split(',');
				}
				else
				{
					buffNames = new string[1] { _attribute.Value };
				}
				return true;
			case "weights":
			case "weight":
				if (_attribute.Value.Contains(","))
				{
					string[] array = _attribute.Value.Replace(" ", "").Split(',');
					buffWeights = new float[array.Length];
					for (int i = 0; i < array.Length; i++)
					{
						buffWeights[i] = StringParsers.ParseFloat(array[i]);
					}
				}
				else
				{
					buffNames = new string[1] { _attribute.Value };
					buffWeights = new float[1] { 1f };
				}
				return true;
			case "fireOneBuff":
				buffOneOnly = StringParsers.ParseBool(_attribute.Value);
				return true;
			}
		}
		return flag;
	}

	public override void ParseXMLPostProcess()
	{
		base.ParseXMLPostProcess();
		if (buffOneOnly && buffWeights != null)
		{
			float weightSum = buffWeights.Sum();
			buffWeights = buffWeights.Select([PublicizedFrom(EAccessModifier.Internal)] (float w) => w / weightSum).ToArray();
		}
		if (!buffOneOnly && buffWeights != null && buffWeights.Any([PublicizedFrom(EAccessModifier.Internal)] (float w) => w > 1f || w < 0f))
		{
			Log.Warning("Warning: Invalid \"Buffs.xml\" configuration. User has specified weights outside of range 0-1 and fireOneBuff=\"false\" or missing. When fireOneBuff=\"false\", the weights represent probabilities between 0-1 for the buffs to be added.");
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void Remove(MinEventParams _params)
	{
		bool netSync = !_params.Self.isEntityRemote | _params.IsLocal;
		for (int i = 0; i < buffNames.Length; i++)
		{
			string name = buffNames[i];
			if (BuffManager.GetBuff(name) != null)
			{
				for (int j = 0; j < targets.Count; j++)
				{
					targets[j].Buffs.RemoveBuff(name, netSync);
				}
			}
		}
	}
}
