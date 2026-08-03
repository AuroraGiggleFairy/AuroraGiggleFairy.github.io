using System.Collections.Generic;
using UnityEngine;

public class XUiC_SmeltTimerModeBar : XUiController
{
	private static readonly HashSet<int> ShiftedWindows = new HashSet<int>();

	private XUiC_SimpleButton btnSingle;
	private XUiC_SimpleButton btnTotal;
	private bool handlersBound;
	private bool layoutApplied;

	public override void Init()
	{
		base.Init();
		btnSingle = GetChildById("btnSmeltTimerSingle") as XUiC_SimpleButton;
		btnTotal = GetChildById("btnSmeltTimerTotal") as XUiC_SimpleButton;
		EnsureHandlers();
		ApplyLayoutShift();
		RefreshSelected();
	}

	public override void OnOpen()
	{
		base.OnOpen();
		EnsureHandlers();
		ApplyLayoutShift();
		RefreshSelected();
		IsDirty = true;
	}

	public override void OnClose()
	{
		RemoveHandlers();
		base.OnClose();
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (IsDirty)
		{
			RefreshSelected();
			RefreshBindingsSelfAndChildren();
			IsDirty = false;
		}
	}

	public override bool GetBindingValueInternal(ref string value, string bindingName)
	{
		if (string.IsNullOrEmpty(bindingName))
		{
			return base.GetBindingValueInternal(ref value, bindingName);
		}

		switch (bindingName.ToLowerInvariant())
		{
			case "smelttimer_single_selected":
				value = (!SmeltTimerModeSettings.IsTotal).ToString().ToLowerInvariant();
				return true;
			case "smelttimer_total_selected":
				value = SmeltTimerModeSettings.IsTotal.ToString().ToLowerInvariant();
				return true;
			default:
				return base.GetBindingValueInternal(ref value, bindingName);
		}
	}

	private void EnsureHandlers()
	{
		if (handlersBound)
		{
			return;
		}

		if (btnSingle != null)
		{
			btnSingle.OnPressed += OnSinglePressed;
		}

		if (btnTotal != null)
		{
			btnTotal.OnPressed += OnTotalPressed;
		}

		handlersBound = true;
	}

	private void RemoveHandlers()
	{
		if (!handlersBound)
		{
			return;
		}

		if (btnSingle != null)
		{
			btnSingle.OnPressed -= OnSinglePressed;
		}

		if (btnTotal != null)
		{
			btnTotal.OnPressed -= OnTotalPressed;
		}

		handlersBound = false;
	}

	private void OnSinglePressed(XUiController _sender, int _mouseButton)
	{
		SmeltTimerModeSettings.Current = SmeltTimerModeSettings.Mode.Single;
		RefreshSelected();
		IsDirty = true;
	}

	private void OnTotalPressed(XUiController _sender, int _mouseButton)
	{
		SmeltTimerModeSettings.Current = SmeltTimerModeSettings.Mode.Total;
		RefreshSelected();
		IsDirty = true;
	}

	private void RefreshSelected()
	{
		bool total = SmeltTimerModeSettings.IsTotal;
		if (btnSingle?.Button != null)
		{
			btnSingle.Button.Selected = !total;
		}

		if (btnTotal?.Button != null)
		{
			btnTotal.Button.Selected = total;
		}
	}

	/// <summary>
	/// Makes room under the Smelting header for this strip, even when other mods
	/// (e.g. SmeltingPlus) rewrite forge window sizes after our XML loads.
	/// </summary>
	private void ApplyLayoutShift()
	{
		if (layoutApplied || ViewComponent == null)
		{
			return;
		}

		XUiController windowController = GetParentWindowController();
		if (windowController?.ViewComponent == null)
		{
			return;
		}

		int id = windowController.GetHashCode();
		if (ShiftedWindows.Contains(id))
		{
			layoutApplied = true;
			return;
		}

		int h = (int)SmeltTimerModeSettings.ModeBarHeight;
		float dy = -h;

		// Move the capacity border + both panels together so relative vanilla/SmeltingPlus
		// alignment (even ore row spacing) is preserved. Do not resize content2.
		TryNudge(windowController, "backgroundMain", dy, growHeight: false, growBy: 0);
		TryNudge(windowController, "content", dy, growHeight: false, growBy: 0);
		TryNudge(windowController, "content2", dy, growHeight: false, growBy: 0);

		if (windowController.ViewComponent is XUiV_Window win)
		{
			win.Size = new Vector2i(win.Size.x, win.Size.y + h);
		}

		ShiftedWindows.Add(id);
		layoutApplied = true;
	}

	private static void TryNudge(XUiController windowController, string childName, float dy, bool growHeight, int growBy)
	{
		XUiController child = windowController.GetChildById(childName);
		if (child?.ViewComponent == null)
		{
			return;
		}

		if (dy != 0f)
		{
			Vector2i pos = child.ViewComponent.Position;
			child.ViewComponent.Position = new Vector2i(pos.x, pos.y + (int)dy);
		}

		if (growHeight && growBy != 0)
		{
			Vector2i size = child.ViewComponent.Size;
			child.ViewComponent.Size = new Vector2i(size.x, size.y + growBy);
		}
	}

	private XUiController GetParentWindowController()
	{
		XUiController current = this;
		while (current != null)
		{
			if (current.ViewComponent is XUiV_Window)
			{
				return current;
			}

			current = current.Parent;
		}

		return null;
	}
}
