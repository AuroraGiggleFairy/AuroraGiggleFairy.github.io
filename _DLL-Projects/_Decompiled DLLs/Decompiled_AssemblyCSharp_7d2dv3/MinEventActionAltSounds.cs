using System.Xml.Linq;
using Audio;
using UnityEngine.Scripting;

[Preserve]
public class MinEventActionAltSounds : MinEventActionTargetedBase
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool enabled;

	public override void Execute(MinEventParams _params)
	{
		for (int i = 0; i < targets.Count; i++)
		{
			if (targets[i] is EntityPlayerLocal)
			{
				EntityVehicle entityVehicle = targets[i].AttachedToEntity as EntityVehicle;
				if (entityVehicle != null)
				{
					entityVehicle.vehicle.FireEvent(Vehicle.Event.Stop);
					Manager.Instance.bUseAltSounds = enabled;
					entityVehicle.vehicle.FireEvent(Vehicle.Event.Start);
				}
				else
				{
					Manager.Instance.bUseAltSounds = enabled;
				}
			}
		}
	}

	public override bool ParseXmlAttribute(XAttribute _attribute)
	{
		bool num = base.ParseXmlAttribute(_attribute);
		if (!num && _attribute.Name.LocalName == "enabled")
		{
			enabled = StringParsers.ParseBool(_attribute.Value);
		}
		return num;
	}
}
