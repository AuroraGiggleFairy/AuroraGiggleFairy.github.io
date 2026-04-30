using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_SliderBar : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_Slider sliderController;

	public override void Init()
	{
		base.Init();
		sliderController = GetParentByType<XUiC_Slider>();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnPressed(int _mouseButton)
	{
		Vector2i mouseXUIPosition = base.xui.GetMouseXUIPosition();
		XUiController xUiController = this;
		Vector2i position = xUiController.ViewComponent.Position;
		while (xUiController.Parent != null && xUiController.Parent.ViewComponent != null)
		{
			xUiController = xUiController.Parent;
			position += xUiController.ViewComponent.Position;
		}
		position += new Vector2i((int)xUiController.ViewComponent.UiTransform.parent.localPosition.x, (int)xUiController.ViewComponent.UiTransform.parent.localPosition.y);
		int num = (position + base.ViewComponent.Size).x - position.x;
		float newVal = (float)(mouseXUIPosition.x - position.x) / (float)num;
		sliderController.ValueChanged(newVal);
		sliderController.updateThumb();
		sliderController.IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnScrolled(float _delta)
	{
		base.OnScrolled(_delta);
		float value = sliderController.Value;
		value += Mathf.Clamp(_delta, 0f - sliderController.Step, sliderController.Step);
		sliderController.ValueChanged(value);
		sliderController.updateThumb();
		sliderController.IsDirty = true;
	}

	public bool PageUpAction()
	{
		OnScrolled(sliderController.Step);
		return true;
	}

	public bool PageDownAction()
	{
		OnScrolled(0f - sliderController.Step);
		return true;
	}
}
