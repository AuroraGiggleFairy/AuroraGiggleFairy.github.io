using System;
using System.Xml.Linq;
using UnityEngine.Scripting;

[Preserve]
public class MinEventActionSetAudioMixerState : MinEventActionTargetedBase
{
	public enum AudioMixerStates
	{
		Stunned,
		Deafened
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public AudioMixerStates State
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool Value
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
			EntityPlayerLocal entityPlayerLocal = targets[i] as EntityPlayerLocal;
			if (entityPlayerLocal != null)
			{
				switch (State)
				{
				case AudioMixerStates.Deafened:
					entityPlayerLocal.isDeafened = Value;
					break;
				case AudioMixerStates.Stunned:
					entityPlayerLocal.isStunned = Value;
					break;
				}
			}
		}
	}

	public override bool ParseXmlAttribute(XAttribute _attribute)
	{
		bool flag = base.ParseXmlAttribute(_attribute);
		if (!flag)
		{
			string localName = _attribute.Name.LocalName;
			if (!(localName == "state"))
			{
				if (localName == "enabled")
				{
					Value = StringParsers.ParseBool(_attribute.Value);
				}
			}
			else
			{
				State = (AudioMixerStates)Enum.Parse(typeof(AudioMixerStates), _attribute.Value);
			}
		}
		return flag;
	}
}
