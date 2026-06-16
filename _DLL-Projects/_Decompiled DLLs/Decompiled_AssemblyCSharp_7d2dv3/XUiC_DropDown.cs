using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_DropDown : XUiController
{
	[Preserve]
	public class Entry : XUiController
	{
		public XUiC_DropDown Owner;

		[PublicizedFrom(EAccessModifier.Private)]
		public string text;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool hovered;

		[XuiXmlBinding("name")]
		public string Text
		{
			get
			{
				return text ?? "";
			}
			set
			{
				if (!(value == text))
				{
					text = value;
					IsDirty = true;
				}
			}
		}

		[XuiXmlBinding("textcolor")]
		public Color TextColor
		{
			get
			{
				if (Owner != null)
				{
					if (!hovered)
					{
						return Owner.DropdownTextColor;
					}
					return Owner.DropdownHoverColor;
				}
				return new Color(100f, 100f, 100f);
			}
		}

		public override void OnOpen()
		{
			base.OnOpen();
			hovered = false;
			IsDirty = true;
		}

		[XuiBindEvent("OnPress", null)]
		[PublicizedFrom(EAccessModifier.Protected)]
		public void OnPressed(XUiController _sender, int _mouseButton)
		{
			Owner.Text = text;
			Owner.DropdownOpen = false;
			Owner.sendChangedEvent(_changeFromCode: false);
			Owner.sendSubmitEvent();
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void OnHovered(bool _isOver)
		{
			base.OnHovered(_isOver);
			hovered = _isOver;
			IsDirty = true;
		}

		public override void Update(float _dt)
		{
			base.Update(_dt);
			handleDirtyUpdateDefault();
		}
	}

	public delegate void XUiEvent_InputOnChangedEventHandler(XUiController _sender, string _text, bool _validInput, bool _changeFromCode);

	public readonly List<string> AllEntries = new List<string>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<string> filteredEntries = new List<string>();

	[XuiBindComponent(true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Entry[] listEntryControllers;

	[XuiBindComponent("input", false)]
	public readonly XUiC_TextInput TextInput;

	[PublicizedFrom(EAccessModifier.Private)]
	public Func<bool> handlePageDownAction;

	[PublicizedFrom(EAccessModifier.Private)]
	public Func<bool> handlePageUpAction;

	[PublicizedFrom(EAccessModifier.Private)]
	public float thumbAreaSize;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color dropdownHoverColor;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color dropdownTextColor;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool sortEntries;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool clearOnOpen;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool allowCustomValue = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public int page;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool dropdownOpen;

	public int PageLength
	{
		get
		{
			Entry[] array = listEntryControllers;
			if (array == null)
			{
				return 1;
			}
			return array.Length;
		}
	}

	[XuiXmlBinding("dropdown_open")]
	public bool DropdownOpen
	{
		get
		{
			return dropdownOpen;
		}
		set
		{
			if (value != dropdownOpen)
			{
				dropdownOpen = value;
				IsDirty = true;
			}
		}
	}

	public int Page
	{
		get
		{
			return page;
		}
		set
		{
			int num = Mathf.Clamp(value, 0, LastPage);
			if (num != page)
			{
				page = num;
				IsDirty = true;
			}
		}
	}

	public int LastPage => Math.Max(0, Mathf.CeilToInt((float)filteredEntries.Count / (float)PageLength) - 1);

	public int EntryCount => filteredEntries.Count;

	public string Text
	{
		get
		{
			return TextInput.Text;
		}
		set
		{
			if (!(value == TextInput.Text))
			{
				TextInput.Text = value;
				UpdateFilteredList();
			}
		}
	}

	[XuiXmlAttribute("dropdown_hovercolor", false)]
	public Color DropdownHoverColor
	{
		get
		{
			return dropdownHoverColor;
		}
		set
		{
			if (!(value == dropdownHoverColor))
			{
				dropdownHoverColor = value;
				IsDirty = true;
			}
		}
	}

	[XuiXmlAttribute("dropdown_textcolor", false)]
	public Color DropdownTextColor
	{
		get
		{
			return dropdownTextColor;
		}
		set
		{
			if (!(value == dropdownTextColor))
			{
				dropdownTextColor = value;
				IsDirty = true;
			}
		}
	}

	[XuiXmlBinding("valid_input")]
	public bool ValidInput
	{
		get
		{
			if (!AllowCustomValue)
			{
				if (TextInput != null)
				{
					return AllEntries.Contains(Text);
				}
				return false;
			}
			return true;
		}
	}

	[XuiXmlAttribute("thumbareasize", false)]
	public float ThumbAreaSize
	{
		get
		{
			return thumbAreaSize;
		}
		set
		{
			if (!Mathf.Approximately(thumbAreaSize, value))
			{
				thumbAreaSize = value;
				IsDirty = true;
			}
		}
	}

	[XuiXmlAttribute("sortentries", false)]
	public bool SortEntries
	{
		get
		{
			return sortEntries;
		}
		set
		{
			if (value != sortEntries)
			{
				sortEntries = value;
				IsDirty = true;
			}
		}
	}

	[XuiXmlAttribute("clearonopen", false)]
	public bool ClearOnOpen
	{
		get
		{
			return clearOnOpen;
		}
		set
		{
			if (value != clearOnOpen)
			{
				clearOnOpen = value;
				IsDirty = true;
			}
		}
	}

	[XuiXmlAttribute("allowcustomvalue", false)]
	public bool AllowCustomValue
	{
		get
		{
			return allowCustomValue;
		}
		set
		{
			if (value != allowCustomValue)
			{
				allowCustomValue = value;
				IsDirty = true;
			}
		}
	}

	[XuiXmlBinding("thumb_size")]
	public int ThumbSize
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return Mathf.RoundToInt(ThumbAreaSize / (float)(LastPage + 1));
		}
	}

	[XuiXmlBinding("thumb_position")]
	public int ThumbPosition
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return Mathf.RoundToInt((float)Page / (float)(LastPage + 1) * ThumbAreaSize);
		}
	}

	public event XUiEvent_InputOnSubmitEventHandler OnSubmitHandler;

	public event XUiEvent_InputOnChangedEventHandler OnChangeHandler;

	public override void Init()
	{
		base.Init();
		XUiController childById = GetChildById("pageUp");
		if (childById != null)
		{
			childById.OnPress += handlePageUpPress;
		}
		XUiController childById2 = GetChildById("pageDown");
		if (childById2 != null)
		{
			childById2.OnPress += handlePageDownPress;
		}
		handlePageDownAction = handlePageDown;
		handlePageUpAction = handlePageUp;
		for (int i = 0; i < listEntryControllers.Length; i++)
		{
			if (listEntryControllers[i] != null)
			{
				listEntryControllers[i].Owner = this;
			}
			else
			{
				Log.Warning("[XUi] DropDown elements do not have the correct controller set (should be \"XUiC_DropDown+Entry\")");
			}
		}
		XUiController childById3 = GetChildById("btnDropdown");
		if (childById3 != null)
		{
			childById3.OnPress += BtnDropdown_OnPress;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnDropdown_OnPress(XUiController _sender, int _mouseButton)
	{
		DropdownOpen = !DropdownOpen;
	}

	[XuiBindEvent("OnInputSelectedHandler", "TextInput")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void OnInputSelected(XUiController _sender, bool _selected)
	{
		if (_selected)
		{
			DropdownOpen = true;
		}
		else
		{
			ThreadManager.StartCoroutine(closeDropdownLater());
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator closeDropdownLater()
	{
		while (xui.playerUI.playerInput.GUIActions.LeftClick.IsPressed)
		{
			yield return null;
		}
		yield return null;
		if (!TextInput.IsSelected)
		{
			DropdownOpen = false;
		}
	}

	[XuiBindEvent("OnSubmitHandler", "TextInput")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void OnInputSubmit(XUiController _sender, string _text)
	{
		DropdownOpen = false;
		TextInput.SetSelected(_selected: false);
		OnInputChanged(_sender, _text, _changeFromCode: false);
		sendSubmitEvent();
	}

	[XuiBindEvent("OnChangeHandler", "TextInput")]
	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnInputChanged(XUiController _sender, string _text, bool _changeFromCode)
	{
		UpdateFilteredList();
		RefreshBindings();
		sendChangedEvent(_changeFromCode);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void sendChangedEvent(bool _changeFromCode)
	{
		this.OnChangeHandler?.Invoke(this, Text, ValidInput, _changeFromCode);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void sendSubmitEvent()
	{
		this.OnSubmitHandler?.Invoke(this, Text);
	}

	public void UpdateFilteredList()
	{
		string text = TextInput?.Text;
		filteredEntries.Clear();
		if (!string.IsNullOrEmpty(text))
		{
			foreach (string allEntry in AllEntries)
			{
				if (allEntry.ContainsCaseInsensitive(text))
				{
					filteredEntries.Add(allEntry);
				}
			}
		}
		else
		{
			filteredEntries.AddRange(AllEntries);
		}
		if (SortEntries)
		{
			filteredEntries.Sort();
		}
		Page = 0;
		IsDirty = true;
	}

	[XuiBindEvent("OnScroll", null)]
	[XuiBindEvent("OnScroll", "listEntryControllers")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleOnScroll(XUiController _sender, float _delta)
	{
		if (_delta > 0f)
		{
			handlePageDown();
		}
		else
		{
			handlePageUp();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void handlePageDownPress(XUiController _sender, int _mouseButton)
	{
		handlePageDown();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void handlePageUpPress(XUiController _sender, int _mouseButton)
	{
		handlePageUp();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool handlePageDown()
	{
		if (!TextInput.IsSelected)
		{
			TextInput.SetSelected();
		}
		if (page > 0)
		{
			Page--;
			return true;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool handlePageUp()
	{
		if (!TextInput.IsSelected)
		{
			TextInput.SetSelected();
		}
		if ((page + 1) * PageLength < filteredEntries.Count)
		{
			Page++;
			return true;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateCurrentPageContents()
	{
		for (int i = 0; i < PageLength; i++)
		{
			int num = i + PageLength * page;
			listEntryControllers[i].Text = ((num < filteredEntries.Count) ? filteredEntries[num] : null);
		}
	}

	public override void Update(float _dt)
	{
		if (IsDirty)
		{
			if (page > LastPage)
			{
				Page = LastPage;
			}
			updateCurrentPageContents();
			RefreshBindings();
			IsDirty = false;
		}
		base.Update(_dt);
		if (base.ViewComponent.IsVisible)
		{
			XUi.HandlePaging(xui, handlePageUpAction, handlePageDownAction);
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (ClearOnOpen)
		{
			Text = string.Empty;
		}
		DropdownOpen = false;
		IsDirty = true;
		RefreshBindings();
	}
}
