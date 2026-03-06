using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_OptionsSelector : XUiController
{
	public enum BoundsHandlingTypes
	{
		Clamp,
		Wrap
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiController leftArrow;

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiController rightArrow;

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiController textArea;

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiV_Label lblTitle;

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiV_Label lblSelected;

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiView clickable;

	public BoundsHandlingTypes BoundsHandling = BoundsHandlingTypes.Wrap;

	[PublicizedFrom(EAccessModifier.Private)]
	public int selectedIndex = 1;

	public int MaxCount = 1;

	public int Step = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<string> items = new List<string>();

	public int SelectedIndex
	{
		get
		{
			return selectedIndex;
		}
		set
		{
			selectedIndex = value;
			IsDirty = true;
		}
	}

	public string Title
	{
		get
		{
			return lblTitle.Text;
		}
		set
		{
			lblTitle.Text = value;
		}
	}

	public event XUiEvent_OnOptionSelectionChanged OnSelectionChanged;

	public override void Init()
	{
		base.Init();
		leftArrow = GetChildById("leftArrow");
		rightArrow = GetChildById("rightArrow");
		textArea = GetChildById("textArea");
		clickable = GetChildById("clickable").ViewComponent;
		lblSelected = textArea.GetChildById("lblText").ViewComponent as XUiV_Label;
		lblTitle = GetChildById("lblTitle").ViewComponent as XUiV_Label;
		leftArrow.OnPress += HandleLeftArrowOnPress;
		rightArrow.OnPress += HandleRightArrowOnPress;
		rightArrow.ViewComponent.Position = new Vector2i(base.ViewComponent.Size.x - 30, rightArrow.ViewComponent.Position.y);
		textArea.ViewComponent.Size = new Vector2i(base.ViewComponent.Size.x - 80, textArea.ViewComponent.Size.y);
		clickable.IsNavigatable = (clickable.IsSnappable = true);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void HandleSelectionChangedEvent()
	{
		if (this.OnSelectionChanged != null)
		{
			this.OnSelectionChanged(this, selectedIndex);
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (base.xui.playerUI.CursorController.navigationTarget == clickable)
		{
			XUi.HandlePaging(base.xui, CycleRight, CycleLeft);
		}
		if (IsDirty)
		{
			if (items.Count > SelectedIndex)
			{
				lblSelected.Text = items[SelectedIndex];
			}
			else
			{
				lblSelected.Text = "";
			}
			IsDirty = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void HandleLeftArrowOnPress(XUiController _sender, int _mouseButton)
	{
		CycleLeft();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void HandleRightArrowOnPress(XUiController _sender, int _mouseButton)
	{
		CycleRight();
	}

	public bool CycleLeft()
	{
		SelectedIndex -= Step;
		if (BoundsHandling == BoundsHandlingTypes.Clamp)
		{
			if (SelectedIndex < 0)
			{
				SelectedIndex = 0;
				return false;
			}
		}
		else if (SelectedIndex < 0)
		{
			SelectedIndex = MaxCount - 1;
		}
		HandleSelectionChangedEvent();
		return true;
	}

	public bool CycleRight()
	{
		SelectedIndex += Step;
		if (BoundsHandling == BoundsHandlingTypes.Clamp)
		{
			if (SelectedIndex >= MaxCount)
			{
				SelectedIndex = MaxCount - 1;
				return false;
			}
		}
		else if (SelectedIndex >= MaxCount)
		{
			SelectedIndex = 0;
		}
		HandleSelectionChangedEvent();
		return true;
	}

	public void SetIndex(int newIndex)
	{
		if (SelectedIndex != newIndex)
		{
			SelectedIndex = newIndex;
			HandleSelectionChangedEvent();
		}
	}

	public void ClearItems()
	{
		items.Clear();
		SelectedIndex = 0;
		MaxCount = 0;
		IsDirty = true;
	}

	public int AddItem(string item)
	{
		items.Add(item);
		MaxCount = items.Count;
		IsDirty = true;
		return items.Count - 1;
	}
}
