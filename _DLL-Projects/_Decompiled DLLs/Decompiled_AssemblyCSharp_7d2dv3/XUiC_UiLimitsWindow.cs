using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_UiLimitsWindow : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public float availableXuiHeight = 1f;

	[XuiXmlBinding("height")]
	public int Height
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return Mathf.FloorToInt(availableXuiHeight);
		}
	}

	[XuiXmlBinding("width_5_4")]
	public int Width5x4
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return calcArWidth(5, 4);
		}
	}

	[XuiXmlBinding("width_4_3")]
	public int Width4x3
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return calcArWidth(4, 3);
		}
	}

	[XuiXmlBinding("width_3_2")]
	public int Width3x2
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return calcArWidth(3, 2);
		}
	}

	[XuiXmlBinding("width_16_10")]
	public int Width16x10
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return calcArWidth(16, 10);
		}
	}

	[XuiXmlBinding("width_16_9")]
	public int Width16x9
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return calcArWidth(16, 9);
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		handleDirtyUpdateDefault();
	}

	public override void OnOpen()
	{
		base.OnOpen();
		int manualHeight = Object.FindAnyObjectByType<UIRoot>().manualHeight;
		float scale = xui.GetScale();
		availableXuiHeight = (float)manualHeight / scale;
		RefreshBindings();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int calcArWidth(int _ratioWidth, int _ratioHeight)
	{
		double uiSizeLimit = GameOptionsManager.GetUiSizeLimit((double)_ratioWidth / (double)_ratioHeight);
		int num = Mathf.RoundToInt((float)((double)(availableXuiHeight / (float)_ratioHeight * (float)_ratioWidth) / uiSizeLimit));
		if (num % 2 > 0)
		{
			num--;
		}
		return num;
	}
}
