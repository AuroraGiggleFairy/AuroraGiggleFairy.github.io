using System.Xml.Linq;
using UnityEngine.Scripting;

[Preserve]
public class MinEventActionModifyScreenEffect : MinEventActionBase
{
	[PublicizedFrom(EAccessModifier.Private)]
	public string effect_name = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public float intensity = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float fade = 4f;

	public override void Execute(MinEventParams _params)
	{
		EntityPlayerLocal entityPlayerLocal = _params.Self as EntityPlayerLocal;
		if (entityPlayerLocal != null)
		{
			entityPlayerLocal.ScreenEffectManager.SetScreenEffect(effect_name, intensity, fade);
		}
	}

	public override bool CanExecute(MinEventTypes _eventType, MinEventParams _params)
	{
		if (base.CanExecute(_eventType, _params) && _params.Self is EntityPlayerLocal)
		{
			return effect_name != "";
		}
		return false;
	}

	public override bool ParseXmlAttribute(XAttribute _attribute)
	{
		bool flag = base.ParseXmlAttribute(_attribute);
		if (!flag)
		{
			switch (_attribute.Name.LocalName)
			{
			case "effect_name":
				effect_name = _attribute.Value;
				return true;
			case "intensity":
				intensity = StringParsers.ParseFloat(_attribute.Value);
				return true;
			case "fade":
				fade = StringParsers.ParseFloat(_attribute.Value);
				return true;
			}
		}
		return flag;
	}
}
