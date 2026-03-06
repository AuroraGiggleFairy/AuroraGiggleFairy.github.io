using System.Xml.Linq;

public class MinEventActionSoundBase : MinEventActionTargetedBase
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public string soundGroup;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool localPlayerOnly;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool loop;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool toggleDMS;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool playAtSelf;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool silentOnEquip;

	[PublicizedFrom(EAccessModifier.Private)]
	public static char[] convertChars = new char[2] { '#', '$' };

	public override bool ParseXmlAttribute(XAttribute _attribute)
	{
		bool flag = base.ParseXmlAttribute(_attribute);
		if (!flag)
		{
			switch (_attribute.Name.LocalName)
			{
			case "soundGroup":
			case "sound":
				soundGroup = _attribute.Value.Trim();
				return true;
			case "play_in_head":
				localPlayerOnly = StringParsers.ParseBool(_attribute.Value);
				return true;
			case "loop":
				loop = StringParsers.ParseBool(_attribute.Value);
				return true;
			case "toggle_dms":
				toggleDMS = StringParsers.ParseBool(_attribute.Value);
				return true;
			case "play_at_self":
				playAtSelf = StringParsers.ParseBool(_attribute.Value);
				return true;
			case "silent_on_equip":
				silentOnEquip = StringParsers.ParseBool(_attribute.Value);
				return true;
			}
		}
		return flag;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public string GetSoundGroupForTarget()
	{
		int num = soundGroup.IndexOfAny(convertChars);
		if (num < 0)
		{
			return soundGroup;
		}
		if (soundGroup[num] == '#')
		{
			return soundGroup.Replace("#", targets[0].IsMale ? "1" : "2");
		}
		return soundGroup.Replace("$", targets[0].IsMale ? "Male" : "Female");
	}
}
