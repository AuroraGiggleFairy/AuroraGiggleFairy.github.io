using UnityEngine.Scripting;

namespace GameEvent.SequenceRequirements;

[Preserve]
public class BaseRequirement
{
	public DynamicProperties Properties;

	public GameEventActionSequence Owner;

	public bool Invert;

	public static string PropInvert = "invert";

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnInit()
	{
	}

	public void Init()
	{
		OnInit();
	}

	public virtual bool CanPerform(Entity target)
	{
		return true;
	}

	public virtual void ParseProperties(DynamicProperties properties)
	{
		Properties = properties;
		Owner.HandleVariablesForProperties(properties);
		if (properties.Values.ContainsKey(PropInvert))
		{
			Invert = StringParsers.ParseBool(properties.Values[PropInvert]);
		}
	}

	public virtual BaseRequirement Clone()
	{
		BaseRequirement baseRequirement = CloneChildSettings();
		if (Properties != null)
		{
			baseRequirement.Properties = new DynamicProperties();
			baseRequirement.Properties.CopyFrom(Properties);
		}
		return baseRequirement;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual BaseRequirement CloneChildSettings()
	{
		return null;
	}
}
