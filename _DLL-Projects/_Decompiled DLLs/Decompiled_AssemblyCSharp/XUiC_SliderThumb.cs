using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_SliderThumb : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2i lastMousePos = new Vector2i(-100000, -100000);

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isOver;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isDragging;

	[PublicizedFrom(EAccessModifier.Private)]
	public float left;

	[PublicizedFrom(EAccessModifier.Private)]
	public float width;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_Slider sliderController;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SliderBar sliderBarController;

	public float ThumbPosition
	{
		get
		{
			return (base.ViewComponent.UiTransform.localPosition.x - left) / width;
		}
		set
		{
			base.ViewComponent.Position = new Vector2i((int)(value * width + left), base.ViewComponent.Position.y);
			base.ViewComponent.UiTransform.localPosition = new Vector3((int)(value * width + left), base.ViewComponent.Position.y, 0f);
		}
	}

	public bool IsDragging => isDragging;

	public override void Init()
	{
		base.Init();
		base.ViewComponent.EventOnHover = true;
		sliderController = GetParentByType<XUiC_Slider>();
		sliderBarController = sliderController.GetChildByType<XUiC_SliderBar>();
	}

	public void SetDimensions(float _left, float _width)
	{
		left = _left;
		width = _width;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnHovered(bool _isOver)
	{
		base.OnHovered(_isOver);
		isOver = _isOver;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnDragged(EDragType _dragType, Vector2 _mousePositionDelta)
	{
		base.OnDragged(_dragType, _mousePositionDelta);
		if (isDragging || isOver)
		{
			Vector2i mouseXUIPosition = base.xui.GetMouseXUIPosition();
			switch (_dragType)
			{
			case EDragType.DragStart:
				lastMousePos = mouseXUIPosition;
				isDragging = true;
				break;
			case EDragType.DragEnd:
				isDragging = false;
				break;
			}
			if (mouseXUIPosition.x - lastMousePos.x != 0)
			{
				float value = base.ViewComponent.UiTransform.localPosition.x + (float)(mouseXUIPosition.x - lastMousePos.x);
				value = Mathf.Clamp(value, left, left + width);
				lastMousePos = mouseXUIPosition;
				base.ViewComponent.UiTransform.localPosition = new Vector3(value, base.ViewComponent.UiTransform.localPosition.y, base.ViewComponent.UiTransform.localPosition.z);
				base.ViewComponent.Position = new Vector2i((int)value, base.ViewComponent.Position.y);
				sliderController.ValueChanged(ThumbPosition);
				sliderController.IsDirty = true;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnScrolled(float _delta)
	{
		base.OnScrolled(_delta);
		if (sliderBarController != null)
		{
			sliderBarController.Scrolled(_delta);
		}
	}
}
