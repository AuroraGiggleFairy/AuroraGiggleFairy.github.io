using UnityEngine.Scripting;

namespace GameEvent.SequenceRequirements;

[Preserve]
public class RequirementEventActive : BaseRequirement
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public string EventName = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropEventName = "event_name";

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnInit()
	{
	}

	public override bool CanPerform(Entity target)
	{
		if (!EventsFromXml.Events.ContainsKey(EventName))
		{
			return Invert;
		}
		if (EventsFromXml.Events[EventName].Active)
		{
			return !Invert;
		}
		return Invert;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		properties.ParseString(PropEventName, ref EventName);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseRequirement CloneChildSettings()
	{
		return new RequirementEventActive
		{
			EventName = EventName,
			Invert = Invert
		};
	}
}
