using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_DewCollectorStack : XUiC_RequiredItemStack
{
	public bool IsCurrentStack;

	public bool IsBlocked;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isModded;

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiV_Sprite fillIcon;

	[PublicizedFrom(EAccessModifier.Protected)]
	public TileEntityCollector tileEntity;

	[PublicizedFrom(EAccessModifier.Private)]
	public string standardFillColorString = "202,190,33,255";

	[PublicizedFrom(EAccessModifier.Private)]
	public Color standardFillColor = new Color32(202, 190, 33, byte.MaxValue);

	[PublicizedFrom(EAccessModifier.Private)]
	public string moddedFillColorString = "0,173,216,255";

	[PublicizedFrom(EAccessModifier.Private)]
	public Color moddedFillColor = new Color32(0, 173, 216, byte.MaxValue);

	[PublicizedFrom(EAccessModifier.Private)]
	public Color disabledColor = new Color32(64, 64, 64, byte.MaxValue);

	[PublicizedFrom(EAccessModifier.Private)]
	public Color blockedColor = new Color32(byte.MaxValue, 0, 0, byte.MaxValue);

	[PublicizedFrom(EAccessModifier.Private)]
	public Color currentFillColor;

	[PublicizedFrom(EAccessModifier.Private)]
	public float fillAmount = -1f;

	public float MaxFill = 15f;

	public bool IsModded
	{
		get
		{
			return isModded;
		}
		set
		{
			isModded = value;
			currentFillColor = (isModded ? moddedFillColor : standardFillColor);
		}
	}

	public float FillAmount
	{
		get
		{
			return fillAmount;
		}
		set
		{
			if (fillAmount != value)
			{
				fillAmount = value;
			}
		}
	}

	public override void Init()
	{
		base.Init();
		XUiController childById = GetChildById("fillIcon");
		if (childById != null)
		{
			fillIcon = childById.ViewComponent as XUiV_Sprite;
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (IsBlocked)
		{
			fillIcon.Color = blockedColor;
			return;
		}
		if (IsCurrentStack)
		{
			float num = Mathf.PingPong(Time.time, 0.5f);
			fillIcon.Color = Color.Lerp(Color.grey, currentFillColor, num * 4f);
		}
		else if (fillIcon.Color != disabledColor)
		{
			fillIcon.Color = disabledColor;
		}
		base.ViewComponent.IsNavigatable = !base.ItemStack.IsEmpty();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		switch (_bindingName)
		{
		case "hasfill":
			_value = (IsCurrentStack ? (FillAmount != -1f).ToString() : "false");
			return true;
		case "waterfill":
			_value = (IsCurrentStack ? (FillAmount / MaxFill).ToString() : "0");
			return true;
		case "showitem":
			_value = (!itemStack.IsEmpty()).ToString();
			return true;
		case "fillcolor":
			if (IsBlocked)
			{
				_value = "255,0,0,255";
			}
			else if (isModded)
			{
				_value = moddedFillColorString;
			}
			else
			{
				_value = standardFillColorString;
			}
			return true;
		case "showempty":
			_value = itemStack.IsEmpty().ToString();
			return true;
		default:
			return base.GetBindingValueInternal(ref _value, _bindingName);
		}
	}

	public void SetStandardFillColor(string colorString)
	{
		standardFillColorString = colorString;
		standardFillColor = StringParsers.ParseColor32(standardFillColorString);
	}

	public void SetModdedFillColor(string colorString)
	{
		moddedFillColorString = colorString;
		moddedFillColor = StringParsers.ParseColor32(moddedFillColorString);
	}

	public override bool ParseAttribute(string name, string value, XUiController _parent)
	{
		bool flag = base.ParseAttribute(name, value, _parent);
		if (!flag)
		{
			if (!(name == "standard_fill_color"))
			{
				if (!(name == "modded_fill_color"))
				{
					return false;
				}
				moddedFillColorString = value;
				moddedFillColor = StringParsers.ParseColor32(moddedFillColorString);
			}
			else
			{
				standardFillColorString = value;
				standardFillColor = StringParsers.ParseColor32(standardFillColorString);
			}
			return true;
		}
		return flag;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnHovered(bool _isOver)
	{
		if (itemStack.IsEmpty())
		{
			base.OnHovered(false);
		}
		else
		{
			base.OnHovered(_isOver);
		}
	}

	public void SetTileEntity(TileEntityCollector te)
	{
		tileEntity = te;
		if (tileEntity != null && fillIcon != null)
		{
			fillIcon.SpriteName = (tileEntity.blockValue.Block as BlockCollector).ItemIconBackdrop;
		}
	}
}
