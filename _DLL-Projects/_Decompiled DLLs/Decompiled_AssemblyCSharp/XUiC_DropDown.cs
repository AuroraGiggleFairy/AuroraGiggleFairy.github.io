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

		public string Text
		{
			set
			{
				if (value != text)
				{
					text = value;
					IsDirty = true;
				}
			}
		}

		public override void OnOpen()
		{
			base.OnOpen();
			hovered = false;
			IsDirty = true;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void OnPressed(int _mouseButton)
		{
			base.OnPressed(_mouseButton);
			Owner.Text = text;
			Owner.DropdownOpen = false;
			Owner.SendChangedEvent(_changeFromCode: true);
			Owner.SendSubmitEvent();
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
			if (IsDirty)
			{
				RefreshBindings();
				IsDirty = false;
			}
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override bool GetBindingValueInternal(ref string _value, string _bindingName)
		{
			if (!(_bindingName == "name"))
			{
				if (_bindingName == "textcolor")
				{
					_value = ((Owner == null) ? "100,100,100" : (hovered ? Owner.dropdownHovercolor : Owner.dropdownTextcolor));
					return true;
				}
				return base.GetBindingValueInternal(ref _value, _bindingName);
			}
			_value = text;
			return true;
		}
	}

	public readonly List<string> AllEntries = new List<string>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<string> filteredEntries = new List<string>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Entry[] listEntryControllers;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput input;

	[PublicizedFrom(EAccessModifier.Private)]
	public Func<bool> handlePageDownAction;

	[PublicizedFrom(EAccessModifier.Private)]
	public Func<bool> handlePageUpAction;

	[PublicizedFrom(EAccessModifier.Private)]
	public float thumbAreaSize;

	[PublicizedFrom(EAccessModifier.Private)]
	public string dropdownHovercolor;

	[PublicizedFrom(EAccessModifier.Private)]
	public string dropdownTextcolor;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool sortEntries;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool clearOnOpen;

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
			return input.Text;
		}
		set
		{
			if (value != input.Text)
			{
				input.Text = value;
				UpdateFilteredList();
			}
		}
	}

	public XUiC_TextInput TextInput => input;

	public event XUiEvent_InputOnSubmitEventHandler OnSubmitHandler;

	public event XUiEvent_InputOnChangedEventHandler OnChangeHandler;

	public override void Init()
	{
		base.Init();
		base.OnScroll += HandleOnScroll;
		XUiController childById = GetChildById("pageUp");
		if (childById != null)
		{
			childById.OnPress += HandlePageUpPress;
		}
		XUiController childById2 = GetChildById("pageDown");
		if (childById2 != null)
		{
			childById2.OnPress += HandlePageDownPress;
		}
		handlePageDownAction = HandlePageDown;
		handlePageUpAction = HandlePageUp;
		XUiController childById3 = GetChildById("list");
		if (childById3 != null)
		{
			listEntryControllers = new Entry[childById3.Children.Count];
			for (int i = 0; i < childById3.Children.Count; i++)
			{
				listEntryControllers[i] = childById3.Children[i] as Entry;
				if (listEntryControllers[i] != null)
				{
					listEntryControllers[i].OnScroll += HandleOnScroll;
					listEntryControllers[i].Owner = this;
				}
				else
				{
					Log.Warning("[XUi] DropDown elements do not have the correct controller set (should be \"XUiC_DropDown+Entry\")");
				}
			}
		}
		input = GetChildById("input") as XUiC_TextInput;
		if (input != null)
		{
			input.OnChangeHandler += OnInputChanged;
			input.OnSubmitHandler += OnInputSubmit;
			input.OnSelect += OnInputSelected;
		}
		XUiController childById4 = GetChildById("btnDropdown");
		if (childById4 != null)
		{
			childById4.OnPress += BtnDropdown_OnPress;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnDropdown_OnPress(XUiController _sender, int _mouseButton)
	{
		DropdownOpen = !DropdownOpen;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnInputSelected(XUiController _sender, bool _selected)
	{
		if (_selected)
		{
			DropdownOpen = true;
		}
		else
		{
			ThreadManager.StartCoroutine(CloseDropdownLater());
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator CloseDropdownLater()
	{
		while (base.xui.playerUI.playerInput.GUIActions.LeftClick.IsPressed)
		{
			yield return null;
		}
		yield return null;
		if (!input.IsSelected)
		{
			DropdownOpen = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnInputSubmit(XUiController _sender, string _text)
	{
		DropdownOpen = false;
		input.SetSelected(_selected: false);
		OnInputChanged(_sender, _text, _changeFromCode: false);
		SendSubmitEvent();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnInputChanged(XUiController _sender, string _text, bool _changeFromCode)
	{
		UpdateFilteredList();
		SendChangedEvent(_changeFromCode);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SendChangedEvent(bool _changeFromCode)
	{
		this.OnChangeHandler?.Invoke(this, Text, _changeFromCode);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SendSubmitEvent()
	{
		this.OnSubmitHandler?.Invoke(this, Text);
	}

	public void UpdateFilteredList()
	{
		string text = input?.Text;
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
		if (sortEntries)
		{
			filteredEntries.Sort();
		}
		Page = 0;
		IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleOnScroll(XUiController _sender, float _delta)
	{
		if (_delta > 0f)
		{
			HandlePageDown();
		}
		else
		{
			HandlePageUp();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandlePageDownPress(XUiController _sender, int _mouseButton)
	{
		HandlePageDown();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandlePageUpPress(XUiController _sender, int _mouseButton)
	{
		HandlePageUp();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool HandlePageDown()
	{
		input.SetSelected();
		if (page > 0)
		{
			Page--;
			return true;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool HandlePageUp()
	{
		input.SetSelected();
		if ((page + 1) * PageLength < filteredEntries.Count)
		{
			Page++;
			return true;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateCurrentPageContents()
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
			UpdateCurrentPageContents();
			RefreshBindings();
			IsDirty = false;
		}
		base.Update(_dt);
		if (base.ViewComponent.IsVisible)
		{
			XUi.HandlePaging(base.xui, handlePageUpAction, handlePageDownAction);
		}
	}

	public override bool ParseAttribute(string _name, string _value, XUiController _parent)
	{
		switch (_name)
		{
		case "thumbareasize":
			thumbAreaSize = StringParsers.ParseFloat(_value);
			return true;
		case "dropdown_textcolor":
			dropdownTextcolor = _value;
			return true;
		case "dropdown_hovercolor":
			dropdownHovercolor = _value;
			return true;
		case "sortentries":
			sortEntries = StringParsers.ParseBool(_value);
			return true;
		case "clearonopen":
			clearOnOpen = StringParsers.ParseBool(_value);
			return true;
		default:
			return base.ParseAttribute(_name, _value, _parent);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		int num = Mathf.RoundToInt(thumbAreaSize / (float)(LastPage + 1));
		switch (_bindingName)
		{
		case "flip_dropdownbutton":
			_value = (dropdownOpen ? UIBasicSprite.Flip.Vertically : UIBasicSprite.Flip.Nothing).ToStringCached();
			return true;
		case "dropdown_open":
			_value = dropdownOpen.ToString();
			return true;
		case "thumb_size":
			_value = num.ToString();
			return true;
		case "thumb_position":
			_value = Mathf.RoundToInt((float)Page / (float)(LastPage + 1) * thumbAreaSize).ToString();
			return true;
		default:
			return base.GetBindingValueInternal(ref _value, _bindingName);
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (clearOnOpen)
		{
			Text = string.Empty;
		}
		DropdownOpen = false;
		IsDirty = true;
		RefreshBindings();
	}
}
