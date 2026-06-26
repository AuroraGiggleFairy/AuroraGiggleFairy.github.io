using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdForceEventDate : ConsoleCmdAbstract
{
	public override bool AllowedInMainMenu => true;

	public override bool IsExecuteOnClient => true;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[1] { "ForceEventDate" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Specify date for testing event dates";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (_params.Count < 1)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Current forced date: " + ((EventsFromXml.ForceTestDateTime == DateTime.MinValue) ? "-none-" : EventsFromXml.ForceTestDateTime.ToShortDateString()));
			return;
		}
		string text = _params[0];
		DateTime _date;
		if (text == "now")
		{
			_date = DateTime.MinValue;
		}
		else if (!EventsFromXml.TryParseDate(text, out _date))
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Failed parsing date argument, must be in the form 'mm/dd'");
			return;
		}
		EventsFromXml.ForceTestDateTime = _date;
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Forced date: " + _date.ToShortDateString());
		foreach (var (_, eventDefinition2) in EventsFromXml.Events)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output($"Name={eventDefinition2.Name}, Start={eventDefinition2.Start}, End={eventDefinition2.End}, Active={eventDefinition2.Active}");
		}
	}
}
