using System.Collections.Generic;
using Twitch;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_TwitchHowToWindow : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public int tipIndex;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<string> TipNames = new List<string>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<string> TipText = new List<string>();

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblTipHeader;

	public override void Init()
	{
		base.Init();
		GetChildById("leftButton").OnPress += Left_OnPress;
		GetChildById("rightButton").OnPress += Right_OnPress;
		lblTipHeader = Localization.Get("TwitchInfo_TipHeader");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Left_OnPress(XUiController _sender, int _mouseButton)
	{
		tipIndex--;
		if (tipIndex == -1)
		{
			tipIndex = TipNames.Count - 1;
		}
		RefreshBindings();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Right_OnPress(XUiController _sender, int _mouseButton)
	{
		tipIndex++;
		if (tipIndex == TipNames.Count)
		{
			tipIndex = 0;
		}
		RefreshBindings();
	}

	public override void OnOpen()
	{
		base.OnOpen();
		TipNames = TwitchManager.Current.tipTitleList;
		TipText = TwitchManager.Current.tipDescriptionList;
		RefreshBindings();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string value, string bindingName)
	{
		switch (bindingName)
		{
		case "tipheader":
			value = lblTipHeader;
			return true;
		case "tiptitle":
			value = ((TipNames.Count > 0) ? TipNames[tipIndex] : "");
			return true;
		case "tiptext":
			value = ((TipText.Count > 0) ? TipText[tipIndex] : "");
			return true;
		default:
			return false;
		}
	}
}
