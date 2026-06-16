using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_SignGalleryWindow : XUiController
{
	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label resultCount;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SignStackGrid signGrid;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_Paging pager;

	[PublicizedFrom(EAccessModifier.Private)]
	public int page;

	[PublicizedFrom(EAccessModifier.Private)]
	public int length;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool updateFilters;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool includeDefaultLibrary = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool includePrefabLibrary = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtInput;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnNew;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblTotal;

	[PublicizedFrom(EAccessModifier.Private)]
	public GlobalSignId currentSignId = GlobalSignId.InvalidId;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SignInfoWindow infoWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SignInstanceWindow instanceWindow;

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiV_Button filterDefaultButton;

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiV_Button filterPrefabButton;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<GlobalSignId> currentItems = new List<GlobalSignId>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<GlobalSignId, string> signNamesById = new Dictionary<GlobalSignId, string>();

	[field: PublicizedFrom(EAccessModifier.Private)]
	public SignCanvas targetCanvas
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public TEFeatureCanvas targetEntity
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public int Page
	{
		get
		{
			return page;
		}
		set
		{
			if (page != value)
			{
				page = value;
				signGrid.Page = page;
				pager?.SetPage(page);
			}
		}
	}

	public override void Init()
	{
		base.Init();
		ID = base.WindowGroup.Id;
		resultCount = (XUiV_Label)GetChildById("resultCount").ViewComponent;
		pager = GetChildByType<XUiC_Paging>();
		if (pager != null)
		{
			pager.OnPageChanged += [PublicizedFrom(EAccessModifier.Private)] () =>
			{
				Page = pager.CurrentPageNumber;
			};
		}
		for (int num = 0; num < children.Count; num++)
		{
			children[num].OnScroll += HandleOnScroll;
		}
		base.OnScroll += HandleOnScroll;
		signGrid = base.Parent.GetChildByType<XUiC_SignStackGrid>();
		XUiC_SignStack[] childrenByType = signGrid.GetChildrenByType<XUiC_SignStack>();
		for (int num2 = 0; num2 < childrenByType.Length; num2++)
		{
			childrenByType[num2].OnScroll += HandleOnScroll;
			XUiC_SignStack obj = childrenByType[num2];
			obj.OnBecameSelected = (Action<XUiC_SignStack>)Delegate.Combine(obj.OnBecameSelected, new Action<XUiC_SignStack>(HandleOnSelect));
		}
		length = childrenByType.Length;
		txtInput = (XUiC_TextInput)windowGroup.Controller.GetChildById("searchInput");
		if (txtInput != null)
		{
			txtInput.OnChangeHandler += HandleOnChangedHandler;
			txtInput.Text = "";
		}
		btnNew = (XUiC_SimpleButton)GetChildById("btnNew");
		btnNew.OnPressed += BtnNew_OnPressed;
		infoWindow = windowGroup.Controller.GetChildByType<XUiC_SignInfoWindow>();
		instanceWindow = windowGroup.Controller.GetChildByType<XUiC_SignInstanceWindow>();
		lblTotal = Localization.Get("lblTotalItems");
		XUiController childById = GetChildById("filterDefault");
		filterDefaultButton = childById.ViewComponent as XUiV_Button;
		childById.OnPress += BtnFilterDefault_OnPressed;
		XUiController childById2 = GetChildById("filterPrefab");
		filterPrefabButton = childById2.ViewComponent as XUiV_Button;
		childById2.OnPress += BtnFilterPrefab_OnPressed;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnNew_OnPressed(XUiController _sender, int _mouseButton)
	{
		if (!PrefabEditModeManager.Instance.IsActive())
		{
			Log.Error("New signs are saved to prefabs and can only be created in the Prefab Editor.");
			return;
		}
		string fileNameNoExtension = PrefabEditModeManager.Instance.LoadedPrefab.FileNameNoExtension;
		SignData newSignData = SignData.GetNewSignData(Localization.Get("lblSignNewSign"));
		GlobalSignId globalSignId = SignDataManager.Instance.AddSignToLibrary(fileNameNoExtension, newSignData);
		PrefabEditModeManager.Instance.NeedsSaving = true;
		RefreshAndSelect(globalSignId);
		if (targetEntity != null)
		{
			XUiC_SignEditorWindow.Open(xui.playerUI, targetEntity);
		}
		else
		{
			XUiC_SignEditorWindow.Open(xui.playerUI, newSignData, globalSignId);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleOnSelect(XUiC_SignStack _sender)
	{
		if (currentSignId != _sender.SignId)
		{
			currentSignId = _sender.SignId;
			if (targetCanvas != null)
			{
				targetCanvas.SignId = currentSignId;
			}
			instanceWindow.RefreshPreviewRenderers();
			PrefabEditModeManager.Instance.NeedsSaving = true;
		}
		infoWindow.SetSignInfo(_sender.SignId);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleOnChangedHandler(XUiController _sender, string _text, bool _changeFromCode)
	{
		Page = 0;
		RefreshItems(txtInput.Text);
		UpdateGrid(forceRefreshInfoPanel: false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleOnScroll(XUiController _sender, float _delta)
	{
		if (_delta > 0f)
		{
			pager?.PageDown();
		}
		else
		{
			pager?.PageUp();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RefreshItems(string _filter)
	{
		currentItems.Clear();
		length = signGrid.Length;
		Page = 0;
		if (includeDefaultLibrary)
		{
			SignDataManager.Instance.AddFilteredSignIdsToList("[D]", _filter, currentItems);
		}
		if (PrefabEditModeManager.Instance.IsActive() && includePrefabLibrary)
		{
			string fileNameNoExtension = PrefabEditModeManager.Instance.LoadedPrefab.FileNameNoExtension;
			if (fileNameNoExtension != null && SignDataManager.Instance.HasSignLibrary(fileNameNoExtension))
			{
				SignDataManager.Instance.AddFilteredSignIdsToList(PrefabEditModeManager.Instance.LoadedPrefab.FileNameNoExtension, _filter, currentItems);
			}
		}
		signNamesById.Clear();
		foreach (GlobalSignId currentItem in currentItems)
		{
			signNamesById[currentItem] = GetNiceName(currentItem);
		}
		currentItems.Sort([PublicizedFrom(EAccessModifier.Private)] (GlobalSignId a, GlobalSignId b) =>
		{
			int num = a.libraryId.CompareTo(b.libraryId);
			return (num != 0) ? num : string.Compare(signNamesById[a], signNamesById[b]);
		});
		pager?.SetLastPageByElementsAndPageLength(currentItems.Count, length);
		resultCount.Text = string.Format(lblTotal, currentItems.Count.ToString());
		[PublicizedFrom(EAccessModifier.Internal)]
		static string GetNiceName(GlobalSignId signId)
		{
			if (SignDataManager.Instance.TryGetSignData(signId, out var signData))
			{
				return signData.name;
			}
			Log.Error("Failed to retrieve sign data for global id: '" + signId.ToString() + "'.");
			return string.Empty;
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		RefreshItems(txtInput.Text);
		IsDirty = true;
		if (targetCanvas != null)
		{
			currentSignId = targetCanvas.SignId;
		}
		else
		{
			currentSignId = GlobalSignId.InvalidId;
		}
		AutoPage();
		UpdateGrid(forceRefreshInfoPanel: true);
		instanceWindow.InitialiseTo(targetEntity);
		RefreshFilterButtons();
		windowGroup.Controller.GetChildByType<XUiC_WindowNonPagingHeader>().SetHeader(Localization.Get("xuiSigns").ToUpper());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void AutoPage()
	{
		int num = currentItems.IndexOf(currentSignId);
		if (num >= 0)
		{
			Page = num / signGrid.Length;
		}
	}

	public override void OnClose()
	{
		base.OnClose();
		xui.ToolTipWindow.ToolTip = "";
		if (xui.playerUI.windowManager.IsWindowOpen("windowpaging"))
		{
			xui.playerUI.windowManager.Close("windowpaging");
		}
		if (targetCanvas != null)
		{
			targetCanvas.SignId = currentSignId;
			targetCanvas = null;
		}
		targetEntity = null;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (viewComponent.IsVisible && IsDirty)
		{
			if (updateFilters)
			{
				RefreshFilterButtons();
				Page = 0;
				RefreshItems(txtInput.Text);
				UpdateGrid(forceRefreshInfoPanel: false);
				updateFilters = false;
			}
			RefreshBindings();
			signGrid.IsDirty = true;
			IsDirty = false;
		}
	}

	public static void Open(LocalPlayerUI _playerUi, TEFeatureCanvas _feature)
	{
		XUiC_SignGalleryWindow childByType = _playerUi.xui.FindWindowGroupByName(ID).GetChildByType<XUiC_SignGalleryWindow>();
		childByType.targetCanvas = _feature?.Canvas;
		childByType.targetEntity = _feature;
		_playerUi.windowManager.Open(ID, _bModal: true);
	}

	public void RefreshAndSelect(GlobalSignId newSignId)
	{
		RefreshItems(txtInput.Text);
		IsDirty = true;
		currentSignId = newSignId;
		if (targetCanvas != null)
		{
			targetCanvas.SignId = currentSignId;
		}
		AutoPage();
		UpdateGrid(forceRefreshInfoPanel: true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateGrid(bool forceRefreshInfoPanel)
	{
		signGrid.SetSignIds(currentItems, currentSignId);
		if (forceRefreshInfoPanel && !currentItems.Contains(currentSignId))
		{
			infoWindow.SetSignInfo(currentSignId);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RefreshFilterButtons()
	{
		filterDefaultButton.Selected = includeDefaultLibrary;
		filterPrefabButton.Selected = includePrefabLibrary;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnFilterDefault_OnPressed(XUiController _sender, int _mouseButton)
	{
		includeDefaultLibrary = !includeDefaultLibrary;
		updateFilters = true;
		IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnFilterPrefab_OnPressed(XUiController _sender, int _mouseButton)
	{
		includePrefabLibrary = !includePrefabLibrary;
		updateFilters = true;
		IsDirty = true;
	}
}
