using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;

public static class EventsFromXml
{
	public readonly struct EventDefinition
	{
		public readonly string Name;

		public readonly DateTime Start;

		public readonly DateTime End;

		public bool Active
		{
			get
			{
				DateTime now = Now;
				if (now >= Start)
				{
					return now < End;
				}
				return false;
			}
		}

		public EventDefinition(string _name, DateTime _start, DateTime _end)
		{
			Name = _name;
			Start = _start;
			End = _end;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public delegate DateTime SpecialDateDelegate(string _origString);

	[PublicizedFrom(EAccessModifier.Private)]
	public const string XMLName = "events.xml";

	public static DateTime ForceTestDateTime;

	public static readonly Dictionary<string, EventDefinition> Events;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Dictionary<string, SpecialDateDelegate> specialDateHandlers;

	public static DateTime Now
	{
		get
		{
			if (!(ForceTestDateTime != DateTime.MinValue))
			{
				return DateTime.Now;
			}
			return ForceTestDateTime;
		}
	}

	public static IEnumerator Load(XmlFile _xmlFile)
	{
		XElement root = _xmlFile.XmlDoc.Root;
		if (root == null)
		{
			yield break;
		}
		foreach (XElement item in root.Elements("event"))
		{
			parseEvent(item);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void parseEvent(XElement _element)
	{
		if (!_element.HasAttribute("name"))
		{
			throw new XmlLoadException("events.xml", _element, "Attribute 'name' missing");
		}
		string attribute = _element.GetAttribute("name");
		DateTime _date = DateTime.MinValue;
		int result = int.MinValue;
		int result2 = int.MinValue;
		DateTime _date2 = DateTime.MinValue;
		int result3 = int.MinValue;
		DateTime _date3 = DateTime.MinValue;
		if (_element.TryGetAttribute("base_date", out var _result) && !TryParseDate(_result, out _date))
		{
			throw new XmlLoadException("events.xml", _element, "Attribute 'base_date' has invalid format");
		}
		if (_element.TryGetAttribute("start_offset", out var _result2) && !int.TryParse(_result2, out result))
		{
			throw new XmlLoadException("events.xml", _element, "Attribute 'start_offset' is not a valid integer number");
		}
		if (_element.TryGetAttribute("end_offset", out var _result3) && !int.TryParse(_result3, out result2))
		{
			throw new XmlLoadException("events.xml", _element, "Attribute 'end_offset' is not a valid integer number");
		}
		if (_element.TryGetAttribute("start_date", out var _result4) && !TryParseDate(_result4, out _date2))
		{
			throw new XmlLoadException("events.xml", _element, "Attribute 'start_date' has invalid format");
		}
		if (_element.TryGetAttribute("end_date", out var _result5) && !TryParseDate(_result5, out _date3))
		{
			throw new XmlLoadException("events.xml", _element, "Attribute 'end_date' has invalid format");
		}
		if (_element.TryGetAttribute("duration", out var _result6) && (!int.TryParse(_result6, out result3) || result3 < 1))
		{
			throw new XmlLoadException("events.xml", _element, "Attribute 'duration' is not a valid integer number or not greater/equal 1");
		}
		if (result > int.MinValue && _date == DateTime.MinValue)
		{
			throw new XmlLoadException("events.xml", _element, "Event has 'start_offset' but no 'base_date'");
		}
		if (result2 > int.MinValue && _date == DateTime.MinValue)
		{
			throw new XmlLoadException("events.xml", _element, "Event has 'end_offset' but no 'base_date'");
		}
		if (result > int.MinValue && _date2 != DateTime.MinValue)
		{
			throw new XmlLoadException("events.xml", _element, "Event has both 'start_offset' and 'start_date'");
		}
		if (result2 > int.MinValue && _date3 != DateTime.MinValue)
		{
			throw new XmlLoadException("events.xml", _element, "Event has both 'end_offset' and 'end_date'");
		}
		if (result2 > int.MinValue && result3 > int.MinValue)
		{
			throw new XmlLoadException("events.xml", _element, "Event has both 'end_offset' and 'duration'");
		}
		if (result3 > int.MinValue && _date3 != DateTime.MinValue)
		{
			throw new XmlLoadException("events.xml", _element, "Event has both 'duration' and 'end_date'");
		}
		if (_date2 == DateTime.MinValue)
		{
			if (_date == DateTime.MinValue)
			{
				throw new XmlLoadException("events.xml", _element, "Event has neither 'base_date' nor 'start_date'");
			}
			if (result == int.MinValue)
			{
				result = 0;
			}
			_date2 = _date.AddDays(result);
		}
		if (_date3 == DateTime.MinValue)
		{
			if (result3 == int.MinValue && result2 == int.MinValue)
			{
				throw new XmlLoadException("events.xml", _element, "Event has neither 'end_offset' nor 'duration' nor 'end_date'");
			}
			if (result2 > int.MinValue)
			{
				_date3 = _date.AddDays(result2);
			}
			if (result3 > int.MinValue)
			{
				_date3 = _date2.AddDays(result3);
			}
		}
		DateTime now = Now;
		if (_date3 < _date2)
		{
			_date3 = _date3.AddYears(1);
		}
		if (_date3.Year > now.Year)
		{
			_date2 = _date2.AddYears(-1);
			_date3 = _date3.AddYears(-1);
		}
		if (_date2 < now && _date3 < now)
		{
			_date2 = _date2.AddYears(1);
			_date3 = _date3.AddYears(1);
		}
		EventDefinition value = new EventDefinition(attribute, _date2, _date3);
		if (!Events.TryAdd(attribute, value))
		{
			Log.Error("Event with the same name '" + attribute + "' already defined");
		}
	}

	public static bool TryParseDate(string _dateString, out DateTime _date)
	{
		_dateString = _dateString.Trim();
		if (specialDateHandlers.TryGetValue(_dateString, out var value))
		{
			_date = value(_dateString);
			return true;
		}
		if (!DateTime.TryParseExact(_dateString, "MM/dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _date))
		{
			return false;
		}
		_date = _date.ChangeYear(DateTime.Now.Year);
		return true;
	}

	public static DateTime ChangeYear(this DateTime _dt, int _newYear)
	{
		return _dt.AddYears(_newYear - _dt.Year);
	}

	public static void Cleanup()
	{
		Events.Clear();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	static EventsFromXml()
	{
		ForceTestDateTime = DateTime.MinValue;
		Events = new CaseInsensitiveStringDictionary<EventDefinition>();
		specialDateHandlers = new CaseInsensitiveStringDictionary<SpecialDateDelegate>();
		specialDateHandlers["easter"] = EasterDate;
		specialDateHandlers["advent"] = FirstSundayOfAdvent;
		specialDateHandlers["thanksgiving"] = ThanksgivingDate;
	}

	public static DateTime EasterDate(string _origString)
	{
		return EasterSunday(DateTime.Now.Year);
	}

	public static DateTime EasterSunday(int _year)
	{
		EasterSunday(_year, out var _month, out var _day);
		return new DateTime(_year, _month, _day);
	}

	public static void EasterSunday(int _year, out int _month, out int _day)
	{
		int num = _year % 19;
		int num2 = _year / 100;
		int num3 = (num2 - num2 / 4 - (8 * num2 + 13) / 25 + 19 * num + 15) % 30;
		int num4 = num3 - num3 / 28 * (1 - num3 / 28 * (29 / (num3 + 1)) * ((21 - num) / 11));
		_day = num4 - (_year + _year / 4 + num4 + 2 - num2 + num2 / 4) % 7 + 28;
		_month = 3;
		if (_day > 31)
		{
			_month++;
			_day -= 31;
		}
	}

	public static DateTime FirstSundayOfAdvent(string _origString)
	{
		return FirstSundayOfAdvent(DateTime.Now.Year);
	}

	public static DateTime FirstSundayOfAdvent(int _year)
	{
		int num = 4;
		int num2 = 0;
		DateTime dateTime = new DateTime(_year, 12, 25);
		if (dateTime.DayOfWeek != DayOfWeek.Sunday)
		{
			num--;
			num2 = (int)dateTime.DayOfWeek;
		}
		return dateTime.AddDays(-1 * (num * 7 + num2));
	}

	public static DateTime ThanksgivingDate(string _origString)
	{
		DateTime result = default(DateTime);
		for (int i = 22; i <= 28; i++)
		{
			result = new DateTime(DateTime.Now.Year, 11, i);
			if (result.DayOfWeek == DayOfWeek.Thursday)
			{
				break;
			}
		}
		return result;
	}
}
