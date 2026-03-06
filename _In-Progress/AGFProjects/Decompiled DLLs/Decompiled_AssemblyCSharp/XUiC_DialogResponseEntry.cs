using UnityEngine.Scripting;

[Preserve]
public class XUiC_DialogResponseEntry : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public string rowColor;

	[PublicizedFrom(EAccessModifier.Private)]
	public string hoverColor;

	public new bool Selected;

	public bool IsHovered;

	public bool HasRequirement = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public static string enabledColor = "255,255,255,255";

	[PublicizedFrom(EAccessModifier.Private)]
	public static string disabledColor = "200,200,200,255";

	[PublicizedFrom(EAccessModifier.Private)]
	public DialogResponse currentResponse;

	public DialogResponse CurrentResponse
	{
		get
		{
			return currentResponse;
		}
		set
		{
			currentResponse = value;
			HasRequirement = true;
			base.ViewComponent.Enabled = value != null;
			if (currentResponse != null && currentResponse.RequirementList.Count > 0)
			{
				for (int i = 0; i < currentResponse.RequirementList.Count; i++)
				{
					if (!currentResponse.RequirementList[i].CheckRequirement(base.xui.playerUI.entityPlayer, base.xui.Dialog.Respondent))
					{
						HasRequirement = false;
						if (currentResponse.RequirementList[i].RequirementVisibilityType == BaseDialogRequirement.RequirementVisibilityTypes.Hide)
						{
							currentResponse = null;
						}
						break;
					}
				}
			}
			RefreshBindings(_forceAll: true);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string value, string bindingName)
	{
		bool flag = currentResponse != null;
		switch (bindingName)
		{
		case "response":
			value = "";
			if (flag)
			{
				value = (HasRequirement ? currentResponse.Text : currentResponse.GetRequiredDescription(base.xui.playerUI.entityPlayer));
			}
			return true;
		case "textstatecolor":
			value = "255,255,255,255";
			if (flag)
			{
				value = (HasRequirement ? enabledColor : disabledColor);
			}
			return true;
		case "rowstatecolor":
			value = "255,255,255,255";
			if (flag)
			{
				if (HasRequirement)
				{
					value = (Selected ? "255,255,255,255" : (IsHovered ? hoverColor : enabledColor));
				}
				else
				{
					value = disabledColor;
				}
			}
			return true;
		case "rowstatesprite":
			value = (Selected ? "ui_game_select_row" : "menu_empty");
			return true;
		case "showresponse":
			value = flag.ToString();
			return true;
		default:
			return false;
		}
	}

	public override void Init()
	{
		base.Init();
		IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnHovered(bool _isOver)
	{
		base.OnHovered(_isOver);
		if (currentResponse == null)
		{
			IsHovered = false;
		}
		else if (IsHovered != _isOver)
		{
			IsHovered = _isOver;
			RefreshBindings();
		}
	}

	public override void Update(float _dt)
	{
		RefreshBindings(IsDirty);
		IsDirty = false;
		base.Update(_dt);
	}

	public void Refresh()
	{
		IsDirty = true;
	}

	public override bool ParseAttribute(string name, string value, XUiController _parent)
	{
		switch (name)
		{
		case "enabled_color":
			enabledColor = value;
			return true;
		case "disabled_color":
			disabledColor = value;
			return true;
		case "row_color":
			rowColor = value;
			return true;
		case "hover_color":
			hoverColor = value;
			return true;
		default:
			return base.ParseAttribute(name, value, _parent);
		}
	}
}
