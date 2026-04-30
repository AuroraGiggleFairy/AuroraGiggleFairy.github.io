using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ToolTip : XUiController
{
	public string ID = "";

	public static float SHOW_DELAY_SEC = 0.3f;

	public XUiV_Label label;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite background;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite border;

	[PublicizedFrom(EAccessModifier.Private)]
	public string tooltip = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public int oldHeight;

	[PublicizedFrom(EAccessModifier.Private)]
	public int oldWidth;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool nextFrame;

	[PublicizedFrom(EAccessModifier.Private)]
	public float showDelay;

	public string ToolTip
	{
		get
		{
			return tooltip;
		}
		set
		{
			if (!(tooltip != value))
			{
				return;
			}
			if (value == null)
			{
				tooltip = "";
			}
			else if (value.Length > 0 && value[value.Length - 1] == '\n')
			{
				tooltip = value.Substring(0, value.Length - 1);
			}
			else
			{
				tooltip = value;
			}
			if (label != null && label.Label != null)
			{
				label.Label.overflowMethod = UILabel.Overflow.ResizeFreely;
				label.Text = tooltip;
				label.SetTextImmediately(tooltip);
				if (tooltip != "")
				{
					base.ViewComponent.Position = base.xui.GetMouseXUIPosition() + new Vector2i(0, -36);
					showDelay = Time.unscaledTime + SHOW_DELAY_SEC;
				}
			}
		}
	}

	public Vector2i ToolTipPosition
	{
		get
		{
			return base.ViewComponent.Position;
		}
		set
		{
			base.ViewComponent.Position = value;
		}
	}

	public override void Init()
	{
		base.Init();
		ID = base.WindowGroup.ID;
		base.xui.currentToolTip = this;
		label = (XUiV_Label)GetChildById("lblText").ViewComponent;
		background = (XUiV_Sprite)GetChildById("sprBackground").ViewComponent;
		border = (XUiV_Sprite)GetChildById("sprBackgroundBorder").ViewComponent;
		tooltip = "";
	}

	public override void Update(float _dt)
	{
		if (!GameManager.Instance.isAnyCursorWindowOpen())
		{
			tooltip = "";
		}
		if (tooltip != "")
		{
			if (Time.unscaledTime > showDelay)
			{
				((XUiV_Window)base.ViewComponent).TargetAlpha = 1f;
			}
			border.Size = new Vector2i(label.Label.width + 18, label.Label.height + 12);
			background.Size = new Vector2i(border.Size.x - 6, border.Size.y - 6);
			Vector2i xUiScreenSize = base.xui.GetXUiScreenSize();
			if (label.Label.width > xUiScreenSize.x / 4)
			{
				label.Label.overflowMethod = UILabel.Overflow.ResizeHeight;
				label.Label.width = xUiScreenSize.x / 4 - 10;
			}
			else
			{
				Vector2i vector2i = xUiScreenSize / 2;
				if ((base.ViewComponent.Position + border.Size).x > vector2i.x)
				{
					base.ViewComponent.Position -= new Vector2i(border.Size.x, 0);
				}
				if ((base.ViewComponent.Position - border.Size).y < -vector2i.y)
				{
					base.ViewComponent.Position += new Vector2i(20, 20 + border.Size.y);
				}
			}
		}
		else
		{
			((XUiV_Window)base.ViewComponent).TargetAlpha = 0.0015f;
		}
		base.Update(_dt);
	}
}
