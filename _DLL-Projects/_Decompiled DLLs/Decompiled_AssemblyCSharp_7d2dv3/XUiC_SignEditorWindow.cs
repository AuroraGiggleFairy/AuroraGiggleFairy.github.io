using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_SignEditorWindow : XUiController
{
	public enum AffectMode
	{
		None,
		Children,
		Pivot
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public class HistoryStateManager
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public class HistoryState
		{
			[field: PublicizedFrom(EAccessModifier.Private)]
			public string Description
			{
				get; [PublicizedFrom(EAccessModifier.Private)]
				set;
			}

			[field: PublicizedFrom(EAccessModifier.Private)]
			public DateTime Time
			{
				get; [PublicizedFrom(EAccessModifier.Private)]
				set;
			}

			[field: PublicizedFrom(EAccessModifier.Private)]
			public int SelectedLayerIndex
			{
				get; [PublicizedFrom(EAccessModifier.Private)]
				set;
			}

			[field: PublicizedFrom(EAccessModifier.Private)]
			public List<int> MultiSelectedLayerIndices
			{
				get; [PublicizedFrom(EAccessModifier.Private)]
				set;
			} = new List<int>(10);

			[field: PublicizedFrom(EAccessModifier.Private)]
			public SignData SnapshotSignData
			{
				get; [PublicizedFrom(EAccessModifier.Private)]
				set;
			}

			[field: PublicizedFrom(EAccessModifier.Private)]
			public AffectMode AffectMode
			{
				get; [PublicizedFrom(EAccessModifier.Private)]
				set;
			}

			public bool HasSnapshot => SnapshotSignData != null;

			public void InitPending(string description)
			{
				Description = description;
				Time = DateTime.Now;
			}

			public void Refresh()
			{
				Time = DateTime.Now;
			}

			public void SetSnapshotData(XUiC_SignLayerGrid signLayerGrid, SignData workingSignData, XUiC_SignEditorWindow editorWindow)
			{
				SetSnapshotData(signLayerGrid.SelectedLayerIndex, signLayerGrid.MultiSelectedLayerIndices, workingSignData, editorWindow.CurrentAffectMode);
			}

			public void SetSnapshotData(int selectedLayerIndex, List<int> multiSelectedLayerIndices, SignData workingSignData, AffectMode affectMode)
			{
				SelectedLayerIndex = selectedLayerIndex;
				MultiSelectedLayerIndices.Clear();
				MultiSelectedLayerIndices.AddRange(multiSelectedLayerIndices);
				SnapshotSignData = SignData.Duplicate(workingSignData);
				AffectMode = affectMode;
			}

			public void Clear()
			{
				MultiSelectedLayerIndices.Clear();
				SnapshotSignData = null;
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public const int cMaxHistoryStates = 100;

		[PublicizedFrom(EAccessModifier.Private)]
		public XUiC_SignLayerGrid signLayerGrid;

		[PublicizedFrom(EAccessModifier.Private)]
		public GlobalSignId workingSignId = GlobalSignId.InvalidId;

		[PublicizedFrom(EAccessModifier.Private)]
		public SignData workingSignData;

		[PublicizedFrom(EAccessModifier.Private)]
		public XUiC_TextInput txtSignName;

		[PublicizedFrom(EAccessModifier.Private)]
		public XUiC_SignEditorWindow editorWindow;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly Stack<HistoryState> historyStatePool = new Stack<HistoryState>(100);

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly List<HistoryState> historyStates = new List<HistoryState>(100);

		[PublicizedFrom(EAccessModifier.Private)]
		public int currentStateIndex = -1;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool historyStateDirty;

		public void Init(string baseChangeDescription, XUiC_SignLayerGrid signLayerGrid, XUiC_TextInput txtSignName, GlobalSignId workingSignId, SignData workingSignData, XUiC_SignEditorWindow editorWindow)
		{
			Reset();
			this.signLayerGrid = signLayerGrid;
			this.txtSignName = txtSignName;
			this.workingSignId = workingSignId;
			this.workingSignData = workingSignData;
			this.editorWindow = editorWindow;
			HistoryState historyState = UnpoolOrCreateHistoryState();
			historyState.InitPending(baseChangeDescription);
			historyStates.Add(historyState);
			currentStateIndex++;
		}

		public void Reset()
		{
			PoolHistoryStates(0, historyStates.Count);
			currentStateIndex = -1;
			signLayerGrid = null;
			workingSignData = null;
			workingSignId = GlobalSignId.InvalidId;
			editorWindow = null;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void PoolHistoryStates(int index, int count)
		{
			if (index < 0 || index + count > historyStates.Count)
			{
				Log.Error($"Invalid history state range specified in PoolHistoryStates. Inputs were ({index}, {count}). historyStates.Count is {historyStates.Count}.");
				return;
			}
			for (int i = index; i < index + count; i++)
			{
				historyStates[i].Clear();
				historyStatePool.Push(historyStates[i]);
			}
			historyStates.RemoveRange(index, count);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public HistoryState UnpoolOrCreateHistoryState()
		{
			if (historyStatePool.Count > 0)
			{
				return historyStatePool.Pop();
			}
			return new HistoryState();
		}

		public void ProcessPendingChange(string changeDescription, bool forceDirty = false)
		{
			if (currentStateIndex < historyStates.Count - 1)
			{
				PoolHistoryStates(currentStateIndex + 1, historyStates.Count - (currentStateIndex + 1));
			}
			if (currentStateIndex < 0)
			{
				Log.Warning($"Unexpected state in HistoryStateManager. Current index is {currentStateIndex}. Expected >= 0. Likely missed call to Init.");
			}
			else
			{
				historyStateDirty |= forceDirty;
				if (!historyStateDirty && historyStates[currentStateIndex].Description == changeDescription)
				{
					historyStates[currentStateIndex].Refresh();
					return;
				}
				if (!historyStates[currentStateIndex].HasSnapshot)
				{
					historyStates[currentStateIndex].SetSnapshotData(signLayerGrid, workingSignData, editorWindow);
				}
			}
			HistoryState historyState = UnpoolOrCreateHistoryState();
			historyState.InitPending(changeDescription);
			historyStates.Add(historyState);
			currentStateIndex++;
			if (historyStates.Count > 100)
			{
				int num = historyStates.Count - 100;
				PoolHistoryStates(0, num);
				currentStateIndex -= num;
			}
			if (currentStateIndex != historyStates.Count - 1)
			{
				Log.Error($"Invalid state index in HistoryStateManager. Current index is {currentStateIndex}. Expected {historyStates.Count - 1}.");
			}
			historyStateDirty = false;
		}

		public bool TryUndo()
		{
			if (currentStateIndex <= 0)
			{
				return false;
			}
			if (!historyStates[currentStateIndex].HasSnapshot)
			{
				historyStates[currentStateIndex].SetSnapshotData(signLayerGrid, workingSignData, editorWindow);
			}
			currentStateIndex--;
			ApplyHistoryState(historyStates[currentStateIndex]);
			return true;
		}

		public bool TryRedo()
		{
			if (currentStateIndex >= historyStates.Count - 1)
			{
				return false;
			}
			if (!historyStates[currentStateIndex].HasSnapshot)
			{
				Log.Error($"Unexpected state in HistoryStateManager. No snapshot exists for current state {currentStateIndex} which is not the most recent recorded state. " + "Only the most recent pending state should lack a snapshot.");
				historyStates[currentStateIndex].SetSnapshotData(signLayerGrid, workingSignData, editorWindow);
			}
			currentStateIndex++;
			ApplyHistoryState(historyStates[currentStateIndex]);
			return true;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void ApplyHistoryState(HistoryState historyState)
		{
			workingSignData.CloneLayersFrom(historyState.SnapshotSignData);
			workingSignData.name = historyState.SnapshotSignData.name;
			signLayerGrid.MultiSelectedLayerIndices.Clear();
			int count = workingSignData.layers.Count;
			if (count == 0)
			{
				editorWindow.BeginAddLayerFlow(0);
			}
			else
			{
				signLayerGrid.MultiSelectedLayerIndices.AddRange(historyState.MultiSelectedLayerIndices);
				signLayerGrid.UpdateLayers(workingSignId, Mathf.Clamp(historyState.SelectedLayerIndex, 0, count - 1), placeholderActive: false);
			}
			txtSignName.Text = historyState.SnapshotSignData.name;
			editorWindow.RestoreAffectModeFromHistory(historyState.AffectMode);
			ForceDirty();
		}

		public void ForceDirty()
		{
			historyStateDirty = true;
		}

		[Conditional("DEBUG_LOG_SIGN_HISTORY")]
		public void LogUndoRedoStack()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("[Sign History]\n");
			for (int i = 0; i < historyStates.Count; i++)
			{
				HistoryState historyState = historyStates[i];
				if (i == currentStateIndex)
				{
					stringBuilder.Append("-> ");
				}
				stringBuilder.Append("[" + historyState.Time.ToString("u") + "] " + historyState.Description + " (Snapshot: " + historyState.HasSnapshot + ")\n");
			}
			Log.Out(stringBuilder.ToString());
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public struct DragInfo
	{
		public Vector2 StartPosition;

		public Vector2 RawPosition;

		public bool IsDragging;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cMaxWarpsPerLayer = 4;

	public static string ID = "";

	public static bool ShowDebugPanel = false;

	public Action OnRefreshed;

	[PublicizedFrom(EAccessModifier.Private)]
	public SignCanvas targetCanvas;

	[PublicizedFrom(EAccessModifier.Private)]
	public TEFeatureCanvas targetEntity;

	[PublicizedFrom(EAccessModifier.Private)]
	public GlobalSignId sourceSignId = GlobalSignId.InvalidId;

	[PublicizedFrom(EAccessModifier.Private)]
	public SignData sourceSignData;

	[PublicizedFrom(EAccessModifier.Private)]
	public GlobalSignId workingSignId = GlobalSignId.InvalidId;

	[PublicizedFrom(EAccessModifier.Private)]
	public SignData workingSignData;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Texture signMaterial;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Texture pulseOverlayMaterial;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cHighlightPulseDuration = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float highlightStartTime = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Texture hoverOverlayMaterial;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cHoverFadeDuration = 0.25f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float hoverOverlayOpacity;

	[PublicizedFrom(EAccessModifier.Private)]
	public SignData.SignLayer hoveredLayer;

	[PublicizedFrom(EAccessModifier.Private)]
	public SignData.SignLayer hoverOverlayLayer;

	[PublicizedFrom(EAccessModifier.Private)]
	public float previewAspect;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_CategoryList categoryList;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SignLayerGrid signLayerGrid;

	[PublicizedFrom(EAccessModifier.Private)]
	public SignData.SignLayer selectedLayer;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<XUiC_SignLayerSettings> allSettings = new List<XUiC_SignLayerSettings>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<XUiC_SignWarpSettings> allWarpSettings = new List<XUiC_SignWarpSettings>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<XUiC_SignColorSettings> allColorSettings = new List<XUiC_SignColorSettings>();

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_AddWarpSettings addWarpSettings;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SignTransformSettings transformSettings;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController canvasView;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SignNewLayerPanel newLayerPanel;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnSave;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnSaveCopy;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnQuit;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtAspectHorizontal;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtAspectVertical;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnAspectFit;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnAspectSquare;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtSignName;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtLayerName;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SizeBar sizeBar;

	[PublicizedFrom(EAccessModifier.Private)]
	public static List<SignData.SignLayer> layerClipboard = new List<SignData.SignLayer>();

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ConfirmationPrompt confirmationPrompt;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SignDebugPanel debugPanel;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SignLayerDragAndDropIcon dragAndDropIcon;

	[PublicizedFrom(EAccessModifier.Private)]
	public string selectedLayerName;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool showNewLayerPanel;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool hasUnsavedChanges;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2 canvasAspect = Vector2.one;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool aspectMaskEnabled = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly HistoryStateManager historyStateManager = new HistoryStateManager();

	[PublicizedFrom(EAccessModifier.Private)]
	public AffectMode currentAffectMode;

	public Action<AffectMode> OnAffectModeChanged;

	public SignComplexityInfo signComplexityInfo;

	[PublicizedFrom(EAccessModifier.Private)]
	public string warpIconSkew;

	[PublicizedFrom(EAccessModifier.Private)]
	public string warpIconBulge;

	[PublicizedFrom(EAccessModifier.Private)]
	public string warpIconTwirl;

	[PublicizedFrom(EAccessModifier.Private)]
	public string warpIconKaleido;

	[PublicizedFrom(EAccessModifier.Private)]
	public string warpIconPerspective;

	[PublicizedFrom(EAccessModifier.Private)]
	public string warpIconArc;

	[PublicizedFrom(EAccessModifier.Private)]
	public string warpIconStretch;

	[PublicizedFrom(EAccessModifier.Private)]
	public string warpIconGrid;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly StringBuilder stringBuilder = new StringBuilder();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<int> originalLayerIndices = new List<int>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<SignData.SignLayer> originalLayers = new List<SignData.SignLayer>();

	[PublicizedFrom(EAccessModifier.Private)]
	public int addLayerIndex = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public float complexityHoverOffset;

	[PublicizedFrom(EAccessModifier.Private)]
	public DragInfo middleMouseDrag;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2 viewOffset = Vector2.zero;

	[PublicizedFrom(EAccessModifier.Private)]
	public float viewScale = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2 dragStartViewOffset = Vector2.zero;

	[PublicizedFrom(EAccessModifier.Private)]
	public float dragStartViewScale = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool dragStartViewScaleMode;

	[PublicizedFrom(EAccessModifier.Private)]
	public DragInfo leftMouseDrag;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<Vector2> layerDragStartPositions = new List<Vector2>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2 affectDragGroupStartPosition;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<Vector2> affectDragChildStartPositions = new List<Vector2>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<int> indicesToSelect = new List<int>();

	[PublicizedFrom(EAccessModifier.Private)]
	public DateTime heldInputRetriggerTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly TimeSpan firstRetriggerDelay = TimeSpan.FromSeconds(0.3499999940395355);

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly TimeSpan subsequentRetriggerDelay = TimeSpan.FromSeconds(0.05000000074505806);

	public AffectMode CurrentAffectMode => currentAffectMode;

	public override void Init()
	{
		base.Init();
		ID = base.WindowGroup.Id;
		base.WindowGroup.isEscClosable = false;
		canvasView = GetChildById("canvasView");
		canvasView.OnDrag += HandleCanvasDragged;
		canvasView.OnScroll += HandleCanvasScrolled;
		newLayerPanel = GetChildByType<XUiC_SignNewLayerPanel>();
		XUiC_SignNewLayerPanel xUiC_SignNewLayerPanel = newLayerPanel;
		xUiC_SignNewLayerPanel.OnLayerTypeSelected = (Action<SignData.SignLayer>)Delegate.Combine(xUiC_SignNewLayerPanel.OnLayerTypeSelected, new Action<SignData.SignLayer>(HandleLayerTypeSelected));
		signMaterial = GetChildById("canvasMaterial").ViewComponent as XUiV_Texture;
		signMaterial.CreateMaterial("Game/SignTech/UI");
		XUiV_Texture xUiV_Texture = signMaterial;
		xUiV_Texture.OnRenderTexture = (UIDrawCall.OnRenderCallback)Delegate.Combine(xUiV_Texture.OnRenderTexture, new UIDrawCall.OnRenderCallback(OnWillRender));
		signMaterial.Texture = Texture2D.whiteTexture;
		previewAspect = (float)signMaterial.Size.x / (float)signMaterial.Size.y;
		pulseOverlayMaterial = GetChildById("pulseOverlay").ViewComponent as XUiV_Texture;
		pulseOverlayMaterial.CreateMaterial("Game/SignTech/UI");
		XUiV_Texture xUiV_Texture2 = pulseOverlayMaterial;
		xUiV_Texture2.OnRenderTexture = (UIDrawCall.OnRenderCallback)Delegate.Combine(xUiV_Texture2.OnRenderTexture, new UIDrawCall.OnRenderCallback(OnPulseOverlayWillRender));
		pulseOverlayMaterial.Texture = null;
		hoverOverlayMaterial = GetChildById("hoverOverlay").ViewComponent as XUiV_Texture;
		hoverOverlayMaterial.CreateMaterial("Game/SignTech/UI");
		XUiV_Texture xUiV_Texture3 = hoverOverlayMaterial;
		xUiV_Texture3.OnRenderTexture = (UIDrawCall.OnRenderCallback)Delegate.Combine(xUiV_Texture3.OnRenderTexture, new UIDrawCall.OnRenderCallback(OnHoverOverlayWillRender));
		hoverOverlayMaterial.Texture = null;
		categoryList = GetChildByType<XUiC_CategoryList>();
		categoryList.CategoryChanged += CategoryList_CategoryChanged;
		signLayerGrid = GetChildByType<XUiC_SignLayerGrid>();
		XUiC_SignLayerGrid xUiC_SignLayerGrid = signLayerGrid;
		xUiC_SignLayerGrid.OnLayerSelected = (Action<int>)Delegate.Combine(xUiC_SignLayerGrid.OnLayerSelected, new Action<int>(HandleLayerSelected));
		XUiC_SignLayerGrid xUiC_SignLayerGrid2 = signLayerGrid;
		xUiC_SignLayerGrid2.OnLayerHovered = (Action<int>)Delegate.Combine(xUiC_SignLayerGrid2.OnLayerHovered, new Action<int>(HandleLayerHovered));
		XUiC_SignLayerGrid xUiC_SignLayerGrid3 = signLayerGrid;
		xUiC_SignLayerGrid3.OnMultiSelectionDeselect = (XUiC_SignLayerGrid.MultiSelectionDeselectCallback)Delegate.Combine(xUiC_SignLayerGrid3.OnMultiSelectionDeselect, new XUiC_SignLayerGrid.MultiSelectionDeselectCallback(HandleMultiSelectionDeselect));
		addWarpSettings = GetChildByType<XUiC_AddWarpSettings>();
		XUiC_AddWarpSettings xUiC_AddWarpSettings = addWarpSettings;
		xUiC_AddWarpSettings.OnWarpAdded = (Action<string>)Delegate.Combine(xUiC_AddWarpSettings.OnWarpAdded, new Action<string>(HandleWarpAdded));
		btnSave = (XUiC_SimpleButton)GetChildById("btnSave");
		btnSaveCopy = (XUiC_SimpleButton)GetChildById("btnSaveCopy");
		btnQuit = (XUiC_SimpleButton)GetChildById("btnQuit");
		btnSave.OnPressed += BtnSave_OnPressed;
		btnSaveCopy.OnPressed += BtnSaveCopy_OnPressed;
		btnQuit.OnPressed += BtnQuit_OnPressed;
		txtAspectHorizontal = (XUiC_TextInput)GetChildById("txtRatio").GetChildById("valueA");
		txtAspectVertical = (XUiC_TextInput)GetChildById("txtRatio").GetChildById("valueB");
		txtAspectHorizontal.OnChangeHandler += OnAspectText_OnChangeHandler;
		txtAspectVertical.OnChangeHandler += OnAspectText_OnChangeHandler;
		if (GetChildById("txtRatio") is XUiC_SignEditorControl xUiC_SignEditorControl)
		{
			xUiC_SignEditorControl.defaultValue = (1f, 1f);
		}
		btnAspectSquare = (XUiC_SimpleButton)GetChildById("btnAspectSquare");
		btnAspectFit = (XUiC_SimpleButton)GetChildById("btnAspectFit");
		btnAspectFit.OnPressed += BtnAspectFit_OnPressed;
		btnAspectSquare.OnPressed += BtnAspectSquare_OnPressed;
		confirmationPrompt = GetChildById("confirmation_prompt_controller") as XUiC_ConfirmationPrompt;
		txtSignName = GetChildById("txtSignName") as XUiC_TextInput;
		txtSignName.OnChangeHandler += TxtSignName_OnChangeHandler;
		txtLayerName = GetChildById("txtLayerName") as XUiC_TextInput;
		txtLayerName.OnChangeHandler += TxtLayerName_OnChangeHandler;
		sizeBar = GetChildByType<XUiC_SizeBar>();
		sizeBar.SetAllowance(600f);
		GetChildrenByType(allSettings);
		GetChildrenByType(allWarpSettings);
		GetChildrenByType(allColorSettings);
		transformSettings = GetChildByType<XUiC_SignTransformSettings>();
		foreach (XUiC_SignLayerSettings allSetting in allSettings)
		{
			allSetting.OnPreLayerSettingsChanged = (Action<string, bool>)Delegate.Combine(allSetting.OnPreLayerSettingsChanged, (Action<string, bool>)([PublicizedFrom(EAccessModifier.Private)] (string changeDescription, bool forceDirty) =>
			{
				OnBeforeApplyingChange(changeDescription, forceDirty);
			}));
			allSetting.OnLayerSettingsChanged = (Action)Delegate.Combine(allSetting.OnLayerSettingsChanged, new Action(MarkChanged));
			if (allSetting is XUiC_SignGroupSettings xUiC_SignGroupSettings)
			{
				xUiC_SignGroupSettings.OnAffectModePressed = (Action<AffectMode>)Delegate.Combine(xUiC_SignGroupSettings.OnAffectModePressed, new Action<AffectMode>(RequestAffectMode));
				OnAffectModeChanged = (Action<AffectMode>)Delegate.Combine(OnAffectModeChanged, new Action<AffectMode>(xUiC_SignGroupSettings.SetAffectModeVisual));
			}
		}
		foreach (XUiC_SignWarpSettings allWarpSetting in allWarpSettings)
		{
			allWarpSetting.OnWarpRemoved = (Action)Delegate.Combine(allWarpSetting.OnWarpRemoved, new Action(HandleWarpRemoved));
		}
		foreach (XUiC_SignColorSettings allColorSetting in allColorSettings)
		{
			allColorSetting.OnMaskModeChanged = (Action)Delegate.Combine(allColorSetting.OnMaskModeChanged, new Action(HandleMaskModeChanged));
		}
		debugPanel = GetChildByType<XUiC_SignDebugPanel>();
		sourceSignData = SignData.GetTestSignData("Sign Editor");
		sourceSignData.UnpackGroups(recursive: false);
		sourceSignId = SignDataManager.Instance.AddSignToLibrary("[I]", sourceSignData);
		dragAndDropIcon = GetChildByType<XUiC_SignLayerDragAndDropIcon>();
		XUiC_SignLayerGrid xUiC_SignLayerGrid4 = signLayerGrid;
		xUiC_SignLayerGrid4.OnDragAndDropStarted = (Action<int>)Delegate.Combine(xUiC_SignLayerGrid4.OnDragAndDropStarted, new Action<int>(OnDragAndDropStarted));
		XUiC_SignLayerGrid xUiC_SignLayerGrid5 = signLayerGrid;
		xUiC_SignLayerGrid5.OnDragAndDropReleased = (Action<int>)Delegate.Combine(xUiC_SignLayerGrid5.OnDragAndDropReleased, new Action<int>(OnDragAndDropReleased));
		XUiC_SignLayerGrid xUiC_SignLayerGrid6 = signLayerGrid;
		xUiC_SignLayerGrid6.OnAddLayerPressed = (Action<int>)Delegate.Combine(xUiC_SignLayerGrid6.OnAddLayerPressed, new Action<int>(BeginAddLayerFlow));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TxtLayerName_OnChangeHandler(XUiController sender, string text, bool changeFromCode)
	{
		if (!changeFromCode)
		{
			signLayerGrid.SetLayerName(text);
			MarkChanged();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleMultiSelectionDeselect(bool wasPrimary)
	{
		if (!wasPrimary)
		{
			RefreshComplexityInfo();
		}
	}

	public static void Open(LocalPlayerUI _playerUi, SignData signData, GlobalSignId signId)
	{
		XUiC_SignEditorWindow childByType = _playerUi.xui.FindWindowGroupByName(ID).GetChildByType<XUiC_SignEditorWindow>();
		childByType.targetCanvas = null;
		childByType.targetEntity = null;
		childByType.sourceSignId = signId;
		childByType.sourceSignData = signData;
		_playerUi.windowManager.Open(ID, _bModal: true);
	}

	public static void Open(LocalPlayerUI _playerUi, TEFeatureCanvas _feature)
	{
		XUiC_SignEditorWindow childByType = _playerUi.xui.FindWindowGroupByName(ID).GetChildByType<XUiC_SignEditorWindow>();
		SignCanvas signCanvas = (childByType.targetCanvas = _feature.Canvas);
		childByType.targetEntity = _feature;
		if (signCanvas != null && SignDataManager.Instance.TryGetSignData(signCanvas.SignId, out childByType.sourceSignData))
		{
			childByType.sourceSignId = signCanvas.SignId;
		}
		else
		{
			childByType.sourceSignId = GlobalSignId.InvalidId;
			childByType.sourceSignData = null;
		}
		_playerUi.windowManager.Open(ID, _bModal: true);
	}

	public override void OnOpen()
	{
		base.OnOpen();
		CreateWorkingCopy();
		UpdateAspect();
		ResetView();
		hasUnsavedChanges = false;
		showNewLayerPanel = false;
		txtSignName.Text = workingSignData.name;
		int count = workingSignData.layers.Count;
		if (count > 0)
		{
			signLayerGrid.MultiSelectedLayerIndices.Add(0);
		}
		signLayerGrid.UpdateLayers(workingSignId, 0, count == 0);
		Refresh();
		RefreshComplexityInfo();
		historyStateManager.Init("Editor Opened", signLayerGrid, txtSignName, workingSignId, workingSignData, this);
		xui.playerUI.windowManager.IsInputLocked = true;
	}

	public override void OnClose()
	{
		base.OnClose();
		xui.playerUI.windowManager.IsInputLocked = false;
		historyStateManager.Reset();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnBeforeApplyingChange(string changeDescription, bool forceDirty = false)
	{
		historyStateManager.ProcessPendingChange(changeDescription, forceDirty);
	}

	public void RequestAffectMode(AffectMode requestedMode)
	{
		if (!(selectedLayer is SignData.GroupSignLayer))
		{
			SetAffectMode(AffectMode.None);
		}
		else if (currentAffectMode == requestedMode)
		{
			SetAffectMode(AffectMode.None);
		}
		else
		{
			SetAffectMode(requestedMode);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetAffectMode(AffectMode newMode)
	{
		if (currentAffectMode != newMode)
		{
			currentAffectMode = newMode;
			historyStateManager.ForceDirty();
			OnAffectModeChanged?.Invoke(currentAffectMode);
		}
	}

	public void RestoreAffectModeFromHistory(AffectMode mode)
	{
		if (!(selectedLayer is SignData.GroupSignLayer))
		{
			mode = AffectMode.None;
		}
		if (currentAffectMode != mode)
		{
			currentAffectMode = mode;
			OnAffectModeChanged?.Invoke(currentAffectMode);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Vector2 WorldDeltaToGroupLocalDelta(SignData.GroupSignLayer group, Vector2 worldDelta)
	{
		float f = (0f - group.transform.rotation) * (MathF.PI / 180f);
		float num = Mathf.Cos(f);
		float num2 = Mathf.Sin(f);
		Vector2 vector = new Vector2(worldDelta.x * num - worldDelta.y * num2, worldDelta.x * num2 + worldDelta.y * num);
		Vector2 scale = group.transform.scale;
		float num3 = (Mathf.Approximately(scale.x, 0f) ? 0f : (1f / scale.x));
		float num4 = (Mathf.Approximately(scale.y, 0f) ? 0f : (1f / scale.y));
		return new Vector2(vector.x * num3, vector.y * num4);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void RotateGroupChildrenAroundOrigin(SignData.GroupSignLayer group, float deltaDegrees)
	{
		float f = deltaDegrees * (MathF.PI / 180f);
		float num = Mathf.Cos(f);
		float num2 = Mathf.Sin(f);
		for (int i = 0; i < group.layers.Count; i++)
		{
			SignData.SignLayer signLayer = group.layers[i];
			Vector2 position = signLayer.transform.position;
			signLayer.transform.position = new Vector2(position.x * num - position.y * num2, position.x * num2 + position.y * num);
			signLayer.transform.rotation += deltaDegrees;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ScaleGroupChildrenAroundOrigin(SignData.GroupSignLayer group, Vector2 factor)
	{
		for (int i = 0; i < group.layers.Count; i++)
		{
			SignData.SignLayer signLayer = group.layers[i];
			signLayer.transform.position = new Vector2(signLayer.transform.position.x * factor.x, signLayer.transform.position.y * factor.y);
			signLayer.transform.scale = new Vector2(signLayer.transform.scale.x * factor.x, signLayer.transform.scale.y * factor.y);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string DecorateChangeDescription(string baseDescription)
	{
		return currentAffectMode switch
		{
			AffectMode.Children => baseDescription + " [Affect Children]", 
			AffectMode.Pivot => baseDescription + " [Affect Pivot]", 
			_ => baseDescription, 
		};
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool TryGetAffectTargetGroup(out SignData.GroupSignLayer group)
	{
		group = null;
		if (currentAffectMode == AffectMode.None)
		{
			return false;
		}
		if (!(selectedLayer is SignData.GroupSignLayer { layers: not null } groupSignLayer) || groupSignLayer.layers.Count == 0)
		{
			return false;
		}
		group = groupSignLayer;
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateAspect()
	{
		if (targetCanvas != null && aspectMaskEnabled)
		{
			canvasAspect = ((targetCanvas.CanvasAspect > 1f) ? new Vector2(1f, 1f / targetCanvas.CanvasAspect) : new Vector2(targetCanvas.CanvasAspect, 1f));
			if (targetCanvas != null && (targetCanvas.CanvasRotation == CanvasRotationMode.Rot90 || targetCanvas.CanvasRotation == CanvasRotationMode.Rot270))
			{
				canvasAspect = new Vector2(canvasAspect.y, canvasAspect.x);
			}
		}
		else
		{
			canvasAspect = Vector2.one;
		}
		OnFrameAspectChanged();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnAspectText_OnChangeHandler(XUiController sender, string text, bool changeFromCode)
	{
		if (!changeFromCode && float.TryParse(txtAspectHorizontal.Text, out var result) && float.TryParse(txtAspectVertical.Text, out var result2) && !(result <= 0f) && !(result2 <= 0f))
		{
			float num = result / result2;
			canvasAspect = ((num > 1f) ? new Vector2(1f, 1f / num) : new Vector2(num, 1f));
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnAspectFit_OnPressed(XUiController sender, int mouseButton)
	{
		canvasAspect = ((targetCanvas.CanvasAspect > 1f) ? new Vector2(1f, 1f / targetCanvas.CanvasAspect) : new Vector2(targetCanvas.CanvasAspect, 1f));
		if (targetCanvas != null && (targetCanvas.CanvasRotation == CanvasRotationMode.Rot90 || targetCanvas.CanvasRotation == CanvasRotationMode.Rot270))
		{
			canvasAspect = new Vector2(canvasAspect.y, canvasAspect.x);
		}
		OnFrameAspectChanged();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnAspectSquare_OnPressed(XUiController sender, int mouseButton)
	{
		canvasAspect = Vector2.one;
		OnFrameAspectChanged();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnFrameAspectChanged()
	{
		txtAspectHorizontal.Text = canvasAspect.x.ToString();
		txtAspectVertical.Text = canvasAspect.y.ToString();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CreateWorkingCopy()
	{
		workingSignData = SignData.Duplicate(sourceSignData);
		workingSignId = SignDataManager.Instance.AddSignToLibrary("[TEMP]", workingSignData);
		SignDataManager.Instance.TryGetSignComplexityInfo(workingSignId, out signComplexityInfo);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void MarkChanged()
	{
		hasUnsavedChanges = true;
		SignDataManager.Instance.MarkSignDirty(workingSignId);
		IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RefreshComplexityInfo()
	{
		float num = 0f;
		if (signComplexityInfo.IsValid)
		{
			sizeBar.SetUsed(signComplexityInfo.TotalComplexity);
			foreach (int multiSelectedLayerIndex in signLayerGrid.MultiSelectedLayerIndices)
			{
				SignData.SignLayer signLayer = workingSignData.layers[multiSelectedLayerIndex];
				if (signComplexityInfo.TryGetLayerComplexityInfo(signLayer, out var layerComplexityInfo))
				{
					if (signLayer == selectedLayer)
					{
						debugPanel.Populate(in layerComplexityInfo);
					}
					num += layerComplexityInfo.Complexity;
				}
			}
			sizeBar.SetSelectedRegion(new XUiC_SizeBar.BarRegionFloat(0f, num));
			complexityHoverOffset = num;
		}
		else
		{
			sizeBar.SetUsed(0f);
			sizeBar.SetSelectedRegion(new XUiC_SizeBar.BarRegionFloat(0f, 0f));
			complexityHoverOffset = 0f;
			debugPanel.Clear();
			Log.Error($"Failed to retrieve valid complexity info for sign: {workingSignId}");
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetLayerSelected(SignData.SignLayer layer)
	{
		if (layer != null)
		{
			ToggleNewLayerPopup(visible: false);
			if (signLayerGrid.HasPlaceholder)
			{
				int num = workingSignData.layers.IndexOf(layer);
				signLayerGrid.MultiSelectedLayerIndices.Clear();
				signLayerGrid.MultiSelectedLayerIndices.Add(num);
				signLayerGrid.UpdateLayers(workingSignId, num, placeholderActive: false);
				return;
			}
		}
		else
		{
			ToggleNewLayerPopup(visible: true);
		}
		if (selectedLayer != layer)
		{
			selectedLayer = layer;
			if (currentAffectMode != AffectMode.None)
			{
				SetAffectMode(AffectMode.None);
			}
			if (layer != null)
			{
				highlightStartTime = Time.time;
				pulseOverlayMaterial.Texture = Texture2D.whiteTexture;
			}
			else
			{
				pulseOverlayMaterial.Texture = null;
			}
			RefreshLayerSettings(categoryList?.CurrentCategory?.CategoryName ?? string.Empty);
		}
		[PublicizedFrom(EAccessModifier.Private)]
		void ToggleNewLayerPopup(bool visible)
		{
			if (showNewLayerPanel != visible)
			{
				if (!visible)
				{
					addLayerIndex = -1;
				}
				showNewLayerPanel = visible;
				RefreshBindings();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RefreshLayerSettings(string autoSelectCategory)
	{
		int num = signLayerGrid.SelectedLayerIndex + 1;
		int i = 0;
		txtLayerName.Text = selectedLayer?.name;
		RefreshComplexityInfo();
		SignData.SignLayer signLayer = selectedLayer;
		if (!(signLayer is SignData.GroupSignLayer))
		{
			if (!(signLayer is SignData.TextSignLayer))
			{
				if (!(signLayer is SignData.PolygonSignLayer))
				{
					if (!(signLayer is SignData.NoiseSignLayer))
					{
						if (signLayer == null)
						{
							selectedLayerName = string.Empty;
						}
						else
						{
							Log.Error("Unsupported layer type selected.");
						}
					}
					else
					{
						selectedLayerName = string.Format(Localization.Get("lblSignLayerName"), num.ToString("00")) + " [" + Localization.Get("lblSignNoise") + "]";
						categoryList.SetCategoryEntry(i++, "Noise", "ui_game_symbol_chemistry", Localization.Get("lblSignNoise"));
						categoryList.SetCategoryEntry(i++, "Color", "ui_game_symbol_paint_bucket", Localization.Get("lblSignColor"));
						categoryList.SetCategoryEntry(i++, "Transform", "ui_game_symbol_wrench", Localization.Get("lblSignTransform"));
						AddWarpCategories();
					}
				}
				else
				{
					selectedLayerName = string.Format(Localization.Get("lblSignLayerName"), num.ToString("00")) + " [" + Localization.Get("lblSignPolygon") + "]";
					categoryList.SetCategoryEntry(i++, "Polygon", "ui_game_symbol_all_blocks", Localization.Get("lblSignPolygon"));
					categoryList.SetCategoryEntry(i++, "Color", "ui_game_symbol_paint_bucket", Localization.Get("lblSignColor"));
					categoryList.SetCategoryEntry(i++, "Transform", "ui_game_symbol_wrench", Localization.Get("lblSignTransform"));
					AddWarpCategories();
				}
			}
			else
			{
				selectedLayerName = string.Format(Localization.Get("lblSignLayerName"), num.ToString("00")) + " [" + Localization.Get("lblSignText") + "]";
				categoryList.SetCategoryEntry(i++, "Text", "ui_game_symbol_shape_text", Localization.Get("lblSignText"));
				categoryList.SetCategoryEntry(i++, "Color", "ui_game_symbol_paint_bucket", Localization.Get("lblSignColor"));
				categoryList.SetCategoryEntry(i++, "Transform", "ui_game_symbol_wrench", Localization.Get("lblSignTransform"));
				AddWarpCategories();
			}
		}
		else
		{
			selectedLayerName = string.Format(Localization.Get("lblSignLayerName"), num.ToString("00")) + " [" + Localization.Get("lblSignGroup") + "]";
			categoryList.SetCategoryEntry(i++, "Group", "ui_game_symbol_bundle", Localization.Get("lblSignGroup"));
			categoryList.SetCategoryEntry(i++, "Color", "ui_game_symbol_paint_bucket", Localization.Get("lblSignColor"));
			categoryList.SetCategoryEntry(i++, "Transform", "ui_game_symbol_wrench", Localization.Get("lblSignTransform"));
			AddWarpCategories();
		}
		foreach (XUiC_SignLayerSettings allSetting in allSettings)
		{
			allSetting.SetLayer(selectedLayer);
		}
		for (; i < categoryList.CategoryButtons.Count; i++)
		{
			categoryList.SetCategoryEmpty(i);
		}
		if (string.IsNullOrEmpty(autoSelectCategory) || !categoryList.TrySetCategory(autoSelectCategory))
		{
			categoryList.SetCategoryToFirst();
		}
		RefreshBindings();
		[PublicizedFrom(EAccessModifier.Private)]
		void AddWarpCategories()
		{
			for (int j = 0; j < selectedLayer.warps.Count; j++)
			{
				SignData.SignWarp signWarp = selectedLayer.warps[j];
				if (!(signWarp is SignData.SkewWarp))
				{
					if (!(signWarp is SignData.BulgeWarp))
					{
						if (!(signWarp is SignData.TwirlWarp))
						{
							if (!(signWarp is SignData.KaleidoWarp))
							{
								if (!(signWarp is SignData.PerspectiveWarp))
								{
									if (!(signWarp is SignData.ArcWarp))
									{
										if (!(signWarp is SignData.StretchWarp))
										{
											if (signWarp is SignData.GridWarp)
											{
												categoryList.SetCategoryEntry(i++, $"{typeof(SignData.GridWarp).Name}_{j}", warpIconGrid, Localization.Get("lblSignGridWarpHeader"));
											}
											else
											{
												Log.Error($"Unsupported layer warp type at index {j}.");
											}
										}
										else
										{
											categoryList.SetCategoryEntry(i++, $"{typeof(SignData.StretchWarp).Name}_{j}", warpIconStretch, Localization.Get("lblSignStretchWarpHeader"));
										}
									}
									else
									{
										categoryList.SetCategoryEntry(i++, $"{typeof(SignData.ArcWarp).Name}_{j}", warpIconArc, Localization.Get("lblSignArcWarpHeader"));
									}
								}
								else
								{
									categoryList.SetCategoryEntry(i++, $"{typeof(SignData.PerspectiveWarp).Name}_{j}", warpIconPerspective, Localization.Get("lblSignPerspectiveWarpHeader"));
								}
							}
							else
							{
								categoryList.SetCategoryEntry(i++, $"{typeof(SignData.KaleidoWarp).Name}_{j}", warpIconKaleido, Localization.Get("lblSignKaleidoWarpHeader"));
							}
						}
						else
						{
							categoryList.SetCategoryEntry(i++, $"{typeof(SignData.TwirlWarp).Name}_{j}", warpIconTwirl, Localization.Get("lblSignTwirlWarpHeader"));
						}
					}
					else
					{
						categoryList.SetCategoryEntry(i++, $"{typeof(SignData.BulgeWarp).Name}_{j}", warpIconBulge, Localization.Get("lblSignPinchBulgeWarpHeader"));
					}
				}
				else
				{
					categoryList.SetCategoryEntry(i++, $"{typeof(SignData.SkewWarp).Name}_{j}", warpIconSkew, Localization.Get("lblSignSkewWarpHeader"));
				}
			}
			if (selectedLayer.warps.Count < 4)
			{
				categoryList.SetCategoryEntry(i++, "AddWarp", "ui_game_symbol_add", Localization.Get("lblSignAddWarp"));
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleWarpAdded(string warpName)
	{
		RefreshLayerSettings((selectedLayer?.warps != null) ? $"{warpName}_{selectedLayer.warps.Count - 1}" : string.Empty);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleWarpRemoved()
	{
		int num = categoryList.CategoryButtons.IndexOf(categoryList.CurrentCategory);
		XUiC_CategoryEntry categoryByIndex = categoryList.GetCategoryByIndex(num + 1);
		RefreshLayerSettings((categoryByIndex == null) ? string.Empty : categoryByIndex.CategoryName);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleMaskModeChanged()
	{
		signLayerGrid.RefreshSelectedLayerBindings();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnWillRender(Material mat)
	{
		SignDataManager.Instance.TryApplyRenderingData(workingSignId, previewAspect, mat, null, SignUIStyle.MainCanvas);
		mat.SetVector(SignShaderIDs._CanvasAspect, canvasAspect);
		Vector4 value = new Vector4(2f, 2f, -1f - viewOffset.x, -1f - viewOffset.y) * viewScale;
		mat.SetVector(SignShaderIDs._AtlasArray_ST, value);
		if (currentAffectMode != AffectMode.None && selectedLayer is SignData.GroupSignLayer groupSignLayer)
		{
			mat.SetVector(SignShaderIDs._Crosshair, new Vector4(groupSignLayer.transform.position.x, groupSignLayer.transform.position.y, MathF.PI / 180f * groupSignLayer.transform.rotation, 1f));
		}
		else
		{
			mat.SetVector(SignShaderIDs._Crosshair, new Vector4(0f, 0f, 0f, 0f));
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnPulseOverlayWillRender(Material mat)
	{
		if (selectedLayer != null)
		{
			float value = Mathf.Clamp01((Time.time - highlightStartTime) / 1f);
			SignDataManager.Instance.TryApplyRenderingData(workingSignId, previewAspect, mat, selectedLayer, SignUIStyle.LayerSelectHighlight);
			mat.SetVector(SignShaderIDs._CanvasAspect, canvasAspect);
			mat.SetFloat(SignShaderIDs._HighlightPhase, value);
			Vector4 value2 = new Vector4(2f, 2f, -1f - viewOffset.x, -1f - viewOffset.y) * viewScale;
			mat.SetVector(SignShaderIDs._AtlasArray_ST, value2);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnHoverOverlayWillRender(Material mat)
	{
		if (hoverOverlayLayer != null)
		{
			SignDataManager.Instance.TryApplyRenderingData(workingSignId, previewAspect, mat, hoverOverlayLayer, SignUIStyle.LayerHoverHighlight);
			mat.SetVector(SignShaderIDs._CanvasAspect, canvasAspect);
			mat.SetFloat(SignShaderIDs._HighlightPhase, hoverOverlayOpacity);
			Vector4 value = new Vector4(2f, 2f, -1f - viewOffset.x, -1f - viewOffset.y) * viewScale;
			mat.SetVector(SignShaderIDs._AtlasArray_ST, value);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CategoryList_CategoryChanged(XUiC_CategoryEntry _categoryEntry)
	{
		int num = _categoryEntry.CategoryName.IndexOf("_");
		if (num > -1 && int.TryParse(_categoryEntry.CategoryName.Substring(num + 1), out var result))
		{
			foreach (XUiC_SignWarpSettings allWarpSetting in allWarpSettings)
			{
				allWarpSetting.SetWarp(selectedLayer.warps[result]);
			}
		}
		RefreshBindings();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnSave_OnPressed(XUiController _sender, int _mouseButton)
	{
		Save();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnSaveCopy_OnPressed(XUiController sender, int mouseButton)
	{
		SaveCopy();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnQuit_OnPressed(XUiController _sender, int _mouseButton)
	{
		Quit();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Save()
	{
		if (hasUnsavedChanges && GetSavingAllowed())
		{
			sourceSignData.CloneLayersFrom(workingSignData);
			sourceSignData.name = workingSignData.name;
			if (!SignDataManager.Instance.MarkSignDirty(sourceSignId))
			{
				Log.Error($"Failed to mark source sign data dirty. Source ID: {sourceSignId}");
			}
			hasUnsavedChanges = false;
			Refresh();
			if (PrefabEditModeManager.Instance != null)
			{
				PrefabEditModeManager.Instance.NeedsSaving = true;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SaveCopy()
	{
		if (GetSavingAllowed())
		{
			string fileNameNoExtension = PrefabEditModeManager.Instance.LoadedPrefab.FileNameNoExtension;
			SignData sign = SignData.Duplicate(workingSignData, SignDataManager.Instance.GetDuplicateName(fileNameNoExtension, workingSignData.name) ?? "");
			GlobalSignId globalSignId = SignDataManager.Instance.AddSignToLibrary(fileNameNoExtension, sign);
			sourceSignId = globalSignId;
			sourceSignData = sign;
			CreateWorkingCopy();
			txtSignName.Text = workingSignData.name;
			if (targetCanvas != null)
			{
				targetCanvas.SignId = sourceSignId;
			}
			signLayerGrid.UpdateLayers(workingSignId, 0, workingSignData.layers.Count == 0);
			historyStateManager.Init("Save As", signLayerGrid, txtSignName, workingSignId, workingSignData, this);
			hasUnsavedChanges = false;
			Refresh();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Quit(bool force = false)
	{
		if (hasUnsavedChanges && !force)
		{
			confirmationPrompt.ShowPrompt(Localization.Get("lblSignDiscardTitle"), Localization.Get("lblSignDiscardMessage"), Localization.Get("xuiCancel"), Localization.Get("xuiConfirm"), OnDiscardChangesPromptResult);
			return;
		}
		if (!SignDataManager.Instance.TryDeleteSign(workingSignId))
		{
			Log.Error($"Failed to clean up temporary editor sign data for sign ID: {workingSignId}");
		}
		XUiC_SignGalleryWindow.Open(xui.playerUI, targetEntity);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDiscardChangesPromptResult(XUiC_ConfirmationPrompt.Result result)
	{
		if (result != XUiC_ConfirmationPrompt.Result.Cancelled && result == XUiC_ConfirmationPrompt.Result.Confirmed)
		{
			Quit(force: true);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TxtSignName_OnChangeHandler(XUiController _sender, string _text, bool _changeFromCode)
	{
		if (!_changeFromCode)
		{
			OnBeforeApplyingChange("Changed Sign Name");
			workingSignData.name = _text;
			MarkChanged();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleCanvasScrolled(XUiController _sender, float _delta)
	{
		if (signLayerGrid.MultiSelectedLayerIndices.Count != 0 && !signLayerGrid.HasPlaceholder)
		{
			bool mouseButton = Input.GetMouseButton(1);
			float num = (mouseButton ? 0.995f : 0.95f);
			float num2 = ((_delta < 0f) ? num : (1f / num));
			if (InputUtils.ControlKeyPressed)
			{
				ScaleSelection(new Vector2(num2, num2));
			}
			else if (InputUtils.ShiftKeyPressed)
			{
				ScaleSelection(new Vector2(1f, num2));
			}
			else if (InputUtils.AltKeyPressed)
			{
				ScaleSelection(new Vector2(num2, 1f));
			}
			else
			{
				float num3 = (mouseButton ? 0.1f : 1f);
				RotateSelection(_delta * 50f * num3);
			}
			transformSettings.SetLayer(selectedLayer);
			MarkChanged();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleCanvasDragged(XUiController _sender, EDragType _dragType, Vector2 _mousePositionDelta)
	{
		switch (UICamera.currentKey)
		{
		case KeyCode.Mouse0:
			HandleLeftMouseDragged(_dragType, _mousePositionDelta);
			break;
		case KeyCode.Mouse2:
			HandleMiddleMouseDragged(_dragType, _mousePositionDelta);
			break;
		case KeyCode.Mouse1:
			break;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ResetView()
	{
		viewOffset = Vector2.zero;
		viewScale = 1f;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleMiddleMouseDragged(EDragType _dragType, Vector2 _mousePositionDelta)
	{
		switch (_dragType)
		{
		case EDragType.DragStart:
			BeginDrag();
			break;
		case EDragType.DragEnd:
			EndDrag();
			return;
		default:
			if (dragStartViewScaleMode != InputUtils.ControlKeyPressed)
			{
				EndDrag();
				BeginDrag();
			}
			break;
		}
		Vector2 vector = xui.GetPixelRatioFactor() * (2f * _mousePositionDelta / signMaterial.Size.y);
		middleMouseDrag.RawPosition += vector;
		UpdateViewOffset();
		[PublicizedFrom(EAccessModifier.Private)]
		void BeginDrag()
		{
			middleMouseDrag.StartPosition = xui.GetMouseXUiPosition().AsVector2();
			middleMouseDrag.RawPosition = middleMouseDrag.StartPosition;
			middleMouseDrag.IsDragging = true;
			dragStartViewOffset = viewOffset;
			dragStartViewScale = viewScale;
			dragStartViewScaleMode = InputUtils.ControlKeyPressed;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		void EndDrag()
		{
			middleMouseDrag.IsDragging = false;
			UpdateViewOffset();
		}
		[PublicizedFrom(EAccessModifier.Private)]
		void UpdateViewOffset()
		{
			Vector2 vector2 = middleMouseDrag.RawPosition - middleMouseDrag.StartPosition;
			if (dragStartViewScaleMode)
			{
				float num = (viewScale = dragStartViewScale * Mathf.Pow(5f, vector2.y));
				viewOffset = dragStartViewOffset * (dragStartViewScale / num);
			}
			else
			{
				viewScale = dragStartViewScale;
				viewOffset = dragStartViewOffset + vector2;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleLeftMouseDragged(EDragType _dragType, Vector2 _mousePositionDelta)
	{
		if (signLayerGrid.MultiSelectedLayerIndices.Count == 0 || signLayerGrid.HasPlaceholder)
		{
			leftMouseDrag.IsDragging = false;
			return;
		}
		Vector2 vector = xui.GetPixelRatioFactor() * (2f * _mousePositionDelta * viewScale / signMaterial.Size.y);
		SignData.GroupSignLayer group;
		bool flag = TryGetAffectTargetGroup(out group);
		switch (_dragType)
		{
		case EDragType.DragStart:
			layerDragStartPositions.Clear();
			foreach (int multiSelectedLayerIndex in signLayerGrid.MultiSelectedLayerIndices)
			{
				layerDragStartPositions.Add(workingSignData.layers[multiSelectedLayerIndex].transform.position);
			}
			leftMouseDrag.StartPosition = selectedLayer.transform.position;
			leftMouseDrag.RawPosition = leftMouseDrag.StartPosition;
			leftMouseDrag.IsDragging = true;
			affectDragChildStartPositions.Clear();
			if (flag)
			{
				affectDragGroupStartPosition = group.transform.position;
				for (int i = 0; i < group.layers.Count; i++)
				{
					affectDragChildStartPositions.Add(group.layers[i].transform.position);
				}
			}
			break;
		case EDragType.DragEnd:
			leftMouseDrag.IsDragging = false;
			return;
		}
		if (signLayerGrid.MultiSelectedLayerIndices.Count != layerDragStartPositions.Count)
		{
			Log.Error($"Data error in HandleCanvasDragged: Selected ({signLayerGrid.MultiSelectedLayerIndices.Count}) and tracked ({layerDragStartPositions.Count}) layer counts are not equal.");
			return;
		}
		leftMouseDrag.RawPosition += vector;
		Vector2 vector2 = leftMouseDrag.RawPosition - leftMouseDrag.StartPosition;
		if (InputUtils.ShiftKeyPressed)
		{
			if (Mathf.Abs(vector2.x) > Mathf.Abs(vector2.y))
			{
				vector2.y = 0f;
			}
			else
			{
				vector2.x = 0f;
			}
		}
		OnBeforeApplyingChange(DecorateChangeDescription("Changed Transform Position"));
		if (flag && affectDragChildStartPositions.Count == group.layers.Count)
		{
			Vector2 vector3 = WorldDeltaToGroupLocalDelta(group, vector2);
			if (currentAffectMode == AffectMode.Children)
			{
				for (int j = 0; j < group.layers.Count; j++)
				{
					group.layers[j].transform.position = affectDragChildStartPositions[j] + vector3;
				}
			}
			else if (currentAffectMode == AffectMode.Pivot)
			{
				group.transform.position = affectDragGroupStartPosition + vector2;
				for (int k = 0; k < group.layers.Count; k++)
				{
					group.layers[k].transform.position = affectDragChildStartPositions[k] - vector3;
				}
			}
		}
		else
		{
			for (int l = 0; l < signLayerGrid.MultiSelectedLayerIndices.Count; l++)
			{
				int index = signLayerGrid.MultiSelectedLayerIndices[l];
				workingSignData.layers[index].transform.position = layerDragStartPositions[l] + vector2;
			}
		}
		transformSettings.SetLayer(selectedLayer);
		MarkChanged();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDragAndDropStarted(int dragStartIdx)
	{
		dragAndDropIcon.SetLayer(workingSignId, workingSignData.layers[signLayerGrid.draggedLayerIndices[0]]);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDragAndDropReleased(int insertIndex)
	{
		if (insertIndex < 0)
		{
			dragAndDropIcon.SetLayer(workingSignId, null);
			return;
		}
		OnBeforeApplyingChange("Drag and dropped layer");
		originalLayerIndices.Clear();
		originalLayerIndices.AddRange(signLayerGrid.draggedLayerIndices);
		int count = originalLayerIndices.Count;
		originalLayers.Clear();
		for (int i = 0; i < count; i++)
		{
			originalLayers.Add(workingSignData.layers[originalLayerIndices[i]].Clone());
		}
		for (int num = count - 1; num >= 0; num--)
		{
			workingSignData.layers.RemoveAt(originalLayerIndices[num]);
		}
		int num2 = insertIndex;
		for (int j = 0; j < count; j++)
		{
			if (originalLayerIndices[j] < insertIndex)
			{
				num2--;
			}
		}
		for (int k = 0; k < count; k++)
		{
			workingSignData.layers.Insert(num2 + k, originalLayers[k]);
		}
		signLayerGrid.MultiSelectedLayerIndices.Clear();
		for (int l = 0; l < count; l++)
		{
			signLayerGrid.MultiSelectedLayerIndices.Add(num2 + l);
		}
		MarkChanged();
		signLayerGrid.UpdateLayers(workingSignId, num2, placeholderActive: false);
		dragAndDropIcon.SetLayer(workingSignId, null);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleLayerSelected(int selectedLayerIndex)
	{
		int num = signLayerGrid.GridIndexToLayerIndex(selectedLayerIndex);
		if (num < 0 || num >= workingSignData.layers.Count)
		{
			SetLayerSelected(null);
			return;
		}
		SignData.SignLayer signLayer = workingSignData.layers[num];
		SetLayerSelected(signLayer);
		if (signLayer == hoveredLayer)
		{
			HandleLayerHovered(selectedLayerIndex);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleLayerHovered(int hoveredDisplayIndex)
	{
		int num = signLayerGrid.GridIndexToLayerIndex(hoveredDisplayIndex);
		hoveredLayer = ((num >= 0 && num < workingSignData.layers.Count) ? workingSignData.layers[num] : null);
		if (hoveredLayer != null)
		{
			hoverOverlayLayer = hoveredLayer;
		}
		if (num < 0 || num >= workingSignData.layers.Count)
		{
			sizeBar.SetHoveredRegion(new XUiC_SizeBar.BarRegionFloat(0f, 0f));
			return;
		}
		SignData.SignLayer layer = workingSignData.layers[num];
		if (signComplexityInfo.TryGetLayerComplexityInfo(layer, out var layerComplexityInfo))
		{
			if (signLayerGrid.MultiSelectedLayerIndices.Contains(hoveredDisplayIndex))
			{
				sizeBar.SetHoveredRegion(new XUiC_SizeBar.BarRegionFloat(complexityHoverOffset - layerComplexityInfo.Complexity, layerComplexityInfo.Complexity));
			}
			else
			{
				sizeBar.SetHoveredRegion(new XUiC_SizeBar.BarRegionFloat(complexityHoverOffset, layerComplexityInfo.Complexity));
			}
		}
		else
		{
			sizeBar.SetHoveredRegion(new XUiC_SizeBar.BarRegionFloat(0f, 0f));
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BeginAddLayerFlow(int index)
	{
		addLayerIndex = index;
		signLayerGrid.UpdateLayers(workingSignId, addLayerIndex, placeholderActive: true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleLayerTypeSelected(SignData.SignLayer layer)
	{
		if (layer != null)
		{
			int num = addLayerIndex;
			OnBeforeApplyingChange("Added Layer", forceDirty: true);
			workingSignData.SetLayerDefaultName(layer);
			int num2 = ((workingSignData.layers.Count != 0) ? ((num < 0) ? (workingSignData.layers.Count - 1) : num) : 0);
			workingSignData.layers.Insert(num2, layer);
			signLayerGrid.MultiSelectedLayerIndices.Clear();
			signLayerGrid.MultiSelectedLayerIndices.Add(num2);
			MarkChanged();
			signLayerGrid.UpdateLayers(workingSignId, num2, placeholderActive: false);
			addLayerIndex = -1;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DeleteSelectedLayers(string changeDescription = "Deleted Layer(s)")
	{
		if (signLayerGrid.MultiSelectedLayerIndices.Count != 0 && !signLayerGrid.HasPlaceholder)
		{
			OnBeforeApplyingChange(changeDescription, forceDirty: true);
			signLayerGrid.MultiSelectedLayerIndices.Sort();
			int a = signLayerGrid.MultiSelectedLayerIndices[0];
			for (int num = signLayerGrid.MultiSelectedLayerIndices.Count - 1; num >= 0; num--)
			{
				workingSignData.layers.RemoveAt(signLayerGrid.MultiSelectedLayerIndices[num]);
			}
			signLayerGrid.MultiSelectedLayerIndices.Clear();
			a = Mathf.Min(a, workingSignData.layers.Count - 1);
			if (a >= 0)
			{
				signLayerGrid.MultiSelectedLayerIndices.Add(a);
				signLayerGrid.UpdateLayers(workingSignId, a, placeholderActive: false);
			}
			else
			{
				BeginAddLayerFlow(0);
			}
			MarkChanged();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SelectAllLayers()
	{
		if (workingSignData.layers.Count != 0)
		{
			signLayerGrid.MultiSelectedLayerIndices.Clear();
			for (int i = 0; i < workingSignData.layers.Count; i++)
			{
				signLayerGrid.MultiSelectedLayerIndices.Add(i);
			}
			signLayerGrid.UpdateLayers(workingSignId, 0, placeholderActive: false);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DuplicateSelectedLayers()
	{
		if (signLayerGrid.MultiSelectedLayerIndices.Count != 0 && !signLayerGrid.HasPlaceholder)
		{
			OnBeforeApplyingChange("Duplicated Layer(s)", forceDirty: true);
			signLayerGrid.MultiSelectedLayerIndices.Sort();
			int count = signLayerGrid.MultiSelectedLayerIndices.Count;
			int num = signLayerGrid.MultiSelectedLayerIndices[count - 1] + 1;
			for (int num2 = count - 1; num2 >= 0; num2--)
			{
				workingSignData.layers.Insert(num, workingSignData.layers[signLayerGrid.MultiSelectedLayerIndices[num2]].Clone());
			}
			signLayerGrid.MultiSelectedLayerIndices.Clear();
			for (int i = 0; i < count; i++)
			{
				signLayerGrid.MultiSelectedLayerIndices.Add(num + i);
			}
			MarkChanged();
			signLayerGrid.UpdateLayers(workingSignId, num + count - 1, placeholderActive: false);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CopySelectedLayersToClipboard()
	{
		if (signLayerGrid.MultiSelectedLayerIndices.Count == 0 || signLayerGrid.HasPlaceholder)
		{
			return;
		}
		layerClipboard.Clear();
		signLayerGrid.MultiSelectedLayerIndices.Sort();
		foreach (int multiSelectedLayerIndex in signLayerGrid.MultiSelectedLayerIndices)
		{
			layerClipboard.Add(workingSignData.layers[multiSelectedLayerIndex].Clone());
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PasteLayersFromClipboard()
	{
		if (layerClipboard.Count == 0)
		{
			return;
		}
		OnBeforeApplyingChange("Pasted Layer(s)", forceDirty: true);
		int num = Mathf.Min(signLayerGrid.SelectedLayerIndex, workingSignData.layers.Count - 1);
		signLayerGrid.MultiSelectedLayerIndices.Clear();
		foreach (SignData.SignLayer item in layerClipboard)
		{
			num++;
			workingSignData.layers.Insert(num, item.Clone());
			signLayerGrid.MultiSelectedLayerIndices.Add(num);
		}
		MarkChanged();
		signLayerGrid.UpdateLayers(workingSignId, num, placeholderActive: false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UngroupSelectedLayers()
	{
		if (signLayerGrid.MultiSelectedLayerIndices.Count < 1)
		{
			return;
		}
		OnBeforeApplyingChange("Ungrouped Layer(s)", forceDirty: true);
		signLayerGrid.MultiSelectedLayerIndices.Sort();
		int selectedIndex = signLayerGrid.MultiSelectedLayerIndices[0];
		indicesToSelect.Clear();
		for (int num = signLayerGrid.MultiSelectedLayerIndices.Count - 1; num >= 0; num--)
		{
			int num2 = signLayerGrid.MultiSelectedLayerIndices[num];
			if (workingSignData.TryUnpackTopLevelGroup(num2, recursive: false, out var unpackedLayerCount))
			{
				for (int i = 0; i < indicesToSelect.Count; i++)
				{
					indicesToSelect[i] += unpackedLayerCount - 1;
				}
				for (int j = 0; j < unpackedLayerCount; j++)
				{
					indicesToSelect.Add(num2 + j);
				}
			}
		}
		signLayerGrid.MultiSelectedLayerIndices.Clear();
		signLayerGrid.MultiSelectedLayerIndices.AddRange(indicesToSelect);
		MarkChanged();
		signLayerGrid.UpdateLayers(workingSignId, selectedIndex, placeholderActive: false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void GroupSelectedLayers()
	{
		if (signLayerGrid.MultiSelectedLayerIndices.Count >= 1)
		{
			OnBeforeApplyingChange("Grouped Layer(s)", forceDirty: true);
			signLayerGrid.MultiSelectedLayerIndices.Sort();
			int num = signLayerGrid.MultiSelectedLayerIndices[0];
			Vector2 vector = workingSignData.layers[num].transform.position;
			Vector2 vector2 = workingSignData.layers[num].transform.position;
			for (int i = 1; i < signLayerGrid.MultiSelectedLayerIndices.Count; i++)
			{
				int index = signLayerGrid.MultiSelectedLayerIndices[i];
				Vector2 position = workingSignData.layers[index].transform.position;
				vector = Vector2.Min(vector, position);
				vector2 = Vector2.Max(vector2, position);
			}
			Vector2 vector3 = 0.5f * (vector + vector2);
			SignData.GroupSignLayer groupSignLayer = new SignData.GroupSignLayer("Group", vector3, 0f, Vector2.one, new SignData.SignRenderSettings(Color.white), new List<SignData.SignWarp>(), new List<SignData.SignLayer>(signLayerGrid.MultiSelectedLayerIndices.Count));
			workingSignData.SetLayerDefaultName(groupSignLayer);
			for (int num2 = signLayerGrid.MultiSelectedLayerIndices.Count - 1; num2 >= 0; num2--)
			{
				int index2 = signLayerGrid.MultiSelectedLayerIndices[num2];
				SignData.SignLayer signLayer = workingSignData.layers[index2];
				workingSignData.layers.RemoveAt(index2);
				groupSignLayer.layers.Insert(0, signLayer);
				signLayer.transform.position -= vector3;
			}
			workingSignData.layers.Insert(num, groupSignLayer);
			signLayerGrid.MultiSelectedLayerIndices.Clear();
			signLayerGrid.MultiSelectedLayerIndices.Add(num);
			MarkChanged();
			signLayerGrid.UpdateLayers(workingSignId, num, placeholderActive: false);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TranslateSelection(Vector2 delta)
	{
		OnBeforeApplyingChange(DecorateChangeDescription("Changed Transform Position"));
		if (TryGetAffectTargetGroup(out var group))
		{
			Vector2 vector = WorldDeltaToGroupLocalDelta(group, delta);
			if (currentAffectMode == AffectMode.Children)
			{
				for (int i = 0; i < group.layers.Count; i++)
				{
					group.layers[i].transform.position += vector;
				}
				return;
			}
			group.transform.position += delta;
			for (int j = 0; j < group.layers.Count; j++)
			{
				group.layers[j].transform.position -= vector;
			}
			return;
		}
		foreach (int multiSelectedLayerIndex in signLayerGrid.MultiSelectedLayerIndices)
		{
			workingSignData.layers[multiSelectedLayerIndex].transform.position += delta;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RotateSelection(float deltaDegrees)
	{
		OnBeforeApplyingChange(DecorateChangeDescription("Changed Transform Rotation"));
		if (TryGetAffectTargetGroup(out var group))
		{
			if (currentAffectMode == AffectMode.Children)
			{
				RotateGroupChildrenAroundOrigin(group, deltaDegrees);
				return;
			}
			group.transform.rotation += deltaDegrees;
			RotateGroupChildrenAroundOrigin(group, 0f - deltaDegrees);
			return;
		}
		foreach (int multiSelectedLayerIndex in signLayerGrid.MultiSelectedLayerIndices)
		{
			SignData.SignLayer signLayer = workingSignData.layers[multiSelectedLayerIndex];
			OnBeforeApplyingChange("Changed Transform Rotation");
			signLayer.transform.rotation += deltaDegrees;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ScaleSelection(Vector2 scaleFactor)
	{
		OnBeforeApplyingChange(DecorateChangeDescription("Changed Transform Scale"));
		if (TryGetAffectTargetGroup(out var group))
		{
			if (currentAffectMode == AffectMode.Children)
			{
				ScaleGroupChildrenAroundOrigin(group, scaleFactor);
				return;
			}
			group.transform.scale = new Vector2(group.transform.scale.x * scaleFactor.x, group.transform.scale.y * scaleFactor.y);
			Vector2 factor = new Vector2(Mathf.Approximately(scaleFactor.x, 0f) ? 1f : (1f / scaleFactor.x), Mathf.Approximately(scaleFactor.y, 0f) ? 1f : (1f / scaleFactor.y));
			ScaleGroupChildrenAroundOrigin(group, factor);
			return;
		}
		foreach (int multiSelectedLayerIndex in signLayerGrid.MultiSelectedLayerIndices)
		{
			workingSignData.layers[multiSelectedLayerIndex].transform.scale *= scaleFactor;
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		UpdateInputs();
		if (!(Time.time - highlightStartTime < 1f) && pulseOverlayMaterial.Texture != null)
		{
			pulseOverlayMaterial.Texture = null;
		}
		float target = ((hoveredLayer != null) ? 1f : 0f);
		float num = 4f;
		hoverOverlayOpacity = Mathf.MoveTowards(hoverOverlayOpacity, target, num * _dt);
		if (hoverOverlayOpacity > 0f && hoverOverlayMaterial.Texture == null)
		{
			hoverOverlayMaterial.Texture = Texture2D.whiteTexture;
		}
		else if (hoverOverlayOpacity <= 0f && hoverOverlayMaterial.Texture != null)
		{
			hoverOverlayMaterial.Texture = null;
			hoverOverlayLayer = null;
		}
		if (viewComponent.IsVisible && IsDirty)
		{
			Refresh();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateInputs()
	{
		if (xui.playerUI.windowManager.IsInputActive())
		{
			return;
		}
		if (xui.playerUI.CursorController.GetMouseButtonDown(UICamera.MouseButton.LeftButton) | xui.playerUI.CursorController.GetMouseButtonUp(UICamera.MouseButton.LeftButton))
		{
			historyStateManager.ForceDirty();
		}
		if (xui.playerUI.playerInput != null && xui.playerUI.playerInput.PermanentActions.Cancel.WasPressed)
		{
			if (!confirmationPrompt.IsVisible)
			{
				Quit();
				return;
			}
			confirmationPrompt.Cancel();
		}
		if (confirmationPrompt.IsVisible)
		{
			return;
		}
		if (leftMouseDrag.IsDragging && (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift) || Input.GetKeyUp(KeyCode.LeftShift) || Input.GetKeyUp(KeyCode.RightShift)))
		{
			HandleLeftMouseDragged(EDragType.Dragging, Vector2.zero);
		}
		if (InputUtils.ControlKeyPressed)
		{
			DateTime now = DateTime.Now;
			if (Input.GetKeyDown(KeyCode.Z))
			{
				heldInputRetriggerTime = now + firstRetriggerDelay;
				if (historyStateManager.TryUndo())
				{
					MarkChanged();
				}
			}
			else if (Input.GetKey(KeyCode.Z) && now > heldInputRetriggerTime)
			{
				heldInputRetriggerTime = now + subsequentRetriggerDelay;
				if (historyStateManager.TryUndo())
				{
					MarkChanged();
				}
			}
			else if (Input.GetKeyDown(KeyCode.Y))
			{
				heldInputRetriggerTime = now + firstRetriggerDelay;
				if (historyStateManager.TryRedo())
				{
					MarkChanged();
				}
			}
			else if (Input.GetKey(KeyCode.Y) && now > heldInputRetriggerTime)
			{
				heldInputRetriggerTime = now + subsequentRetriggerDelay;
				if (historyStateManager.TryRedo())
				{
					MarkChanged();
				}
			}
			else if (Input.GetKeyDown(KeyCode.S))
			{
				if (InputUtils.ShiftKeyPressed)
				{
					SaveCopy();
				}
				else
				{
					Save();
				}
			}
			else if (Input.GetKeyDown(KeyCode.A))
			{
				SelectAllLayers();
			}
			else if (Input.GetKeyDown(KeyCode.D))
			{
				DuplicateSelectedLayers();
			}
			else if (Input.GetKeyDown(KeyCode.X))
			{
				CopySelectedLayersToClipboard();
				DeleteSelectedLayers("Cut Layer(s)");
			}
			else if (Input.GetKeyDown(KeyCode.C))
			{
				CopySelectedLayersToClipboard();
			}
			else if (Input.GetKeyDown(KeyCode.V))
			{
				PasteLayersFromClipboard();
			}
			else if (Input.GetKeyDown(KeyCode.G))
			{
				if (InputUtils.ShiftKeyPressed)
				{
					UngroupSelectedLayers();
				}
				else
				{
					GroupSelectedLayers();
				}
			}
			else if (Input.GetKeyDown(KeyCode.N) && signLayerGrid.MultiSelectedLayerIndices.Count > 0)
			{
				if (InputUtils.ShiftKeyPressed)
				{
					addLayerIndex = signLayerGrid.MultiSelectedLayerIndices[0];
				}
				else
				{
					addLayerIndex = signLayerGrid.MultiSelectedLayerIndices[0] + 1;
				}
				BeginAddLayerFlow(addLayerIndex);
			}
			return;
		}
		if (Input.GetKeyDown(KeyCode.F))
		{
			ResetView();
		}
		else if (Input.GetKeyDown(KeyCode.Delete))
		{
			DeleteSelectedLayers();
		}
		Vector2 zero = Vector2.zero;
		if (Input.GetKeyDown(KeyCode.UpArrow))
		{
			zero.y += 0.01f;
		}
		if (Input.GetKeyDown(KeyCode.DownArrow))
		{
			zero.y -= 0.01f;
		}
		if (Input.GetKeyDown(KeyCode.RightArrow))
		{
			zero.x += 0.01f;
		}
		if (Input.GetKeyDown(KeyCode.LeftArrow))
		{
			zero.x -= 0.01f;
		}
		if (zero != Vector2.zero && !signLayerGrid.HasPlaceholder)
		{
			bool altKeyPressed = InputUtils.AltKeyPressed;
			if (InputUtils.ShiftKeyPressed)
			{
				zero *= (altKeyPressed ? 5f : 10f);
			}
			else if (altKeyPressed)
			{
				zero *= 0.1f;
			}
			TranslateSelection(zero);
			transformSettings.SetLayer(selectedLayer);
			MarkChanged();
		}
		if (Input.GetMouseButtonUp(0))
		{
			signLayerGrid.ReleaseDragAndDrop();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Refresh()
	{
		SignDataManager.Instance.TryGetSignComplexityInfo(workingSignId, out signComplexityInfo);
		RefreshComplexityInfo();
		bool savingAllowed = GetSavingAllowed();
		btnSave.Enabled = hasUnsavedChanges && savingAllowed;
		btnSaveCopy.Enabled = savingAllowed;
		RefreshBindings();
		IsDirty = false;
		OnRefreshed?.Invoke();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool GetSavingAllowed()
	{
		if (!signComplexityInfo.IsValid)
		{
			return false;
		}
		if (signComplexityInfo.TotalComplexity > 600f)
		{
			return false;
		}
		if (signComplexityInfo.StackInfo.MaxCompStackIndex > 7)
		{
			return false;
		}
		if (signComplexityInfo.StackInfo.MaxUVStackIndex > 7)
		{
			return false;
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string value, string bindingName)
	{
		switch (bindingName)
		{
		case "showdebugpanel":
			value = ShowDebugPanel.ToString();
			return true;
		case "showtransformsettings":
			value = (categoryList?.CurrentCategory?.CategoryName?.EqualsCaseInsensitive("Transform") == true).ToString();
			return true;
		case "showpolygonsettings":
			value = (categoryList?.CurrentCategory?.CategoryName?.EqualsCaseInsensitive("Polygon") == true).ToString();
			return true;
		case "showtextsettings":
			value = (categoryList?.CurrentCategory?.CategoryName?.EqualsCaseInsensitive("Text") == true).ToString();
			return true;
		case "shownoisesettings":
			value = (categoryList?.CurrentCategory?.CategoryName?.EqualsCaseInsensitive("Noise") == true).ToString();
			return true;
		case "showgroupsettings":
			value = (categoryList?.CurrentCategory?.CategoryName?.EqualsCaseInsensitive("Group") == true).ToString();
			return true;
		case "showcolorsettings":
			value = (categoryList?.CurrentCategory?.CategoryName?.EqualsCaseInsensitive("Color") == true).ToString();
			return true;
		case "showaddwarpsettings":
			value = (categoryList?.CurrentCategory?.CategoryName?.EqualsCaseInsensitive("AddWarp") == true).ToString();
			return true;
		case "showskewwarpsettings":
			value = (categoryList?.CurrentCategory?.CategoryName?.ContainsCaseInsensitive(typeof(SignData.SkewWarp).Name) == true).ToString();
			return true;
		case "showbulgewarpsettings":
			value = (categoryList?.CurrentCategory?.CategoryName?.ContainsCaseInsensitive(typeof(SignData.BulgeWarp).Name) == true).ToString();
			return true;
		case "showtwirlwarpsettings":
			value = (categoryList?.CurrentCategory?.CategoryName?.ContainsCaseInsensitive(typeof(SignData.TwirlWarp).Name) == true).ToString();
			return true;
		case "showkaleidowarpsettings":
			value = (categoryList?.CurrentCategory?.CategoryName?.ContainsCaseInsensitive(typeof(SignData.KaleidoWarp).Name) == true).ToString();
			return true;
		case "showperspectivewarpsettings":
			value = (categoryList?.CurrentCategory?.CategoryName?.ContainsCaseInsensitive(typeof(SignData.PerspectiveWarp).Name) == true).ToString();
			return true;
		case "showarcwarpsettings":
			value = (categoryList?.CurrentCategory?.CategoryName?.ContainsCaseInsensitive(typeof(SignData.ArcWarp).Name) == true).ToString();
			return true;
		case "showstretchwarpsettings":
			value = (categoryList?.CurrentCategory?.CategoryName?.ContainsCaseInsensitive(typeof(SignData.StretchWarp).Name) == true).ToString();
			return true;
		case "showgridwarpsettings":
			value = (categoryList?.CurrentCategory?.CategoryName?.ContainsCaseInsensitive(typeof(SignData.GridWarp).Name) == true).ToString();
			return true;
		case "warp_icon_skew":
			value = warpIconSkew;
			return true;
		case "warp_icon_bulge":
			value = warpIconBulge;
			return true;
		case "warp_icon_twirl":
			value = warpIconTwirl;
			return true;
		case "warp_icon_kaleido":
			value = warpIconKaleido;
			return true;
		case "warp_icon_perspective":
			value = warpIconPerspective;
			return true;
		case "warp_icon_arc":
			value = warpIconArc;
			return true;
		case "warp_icon_stretch":
			value = warpIconStretch;
			return true;
		case "warp_icon_grid":
			value = warpIconGrid;
			return true;
		case "shownewlayerpanel":
			value = showNewLayerPanel.ToString();
			return true;
		case "selectedlayername":
			value = selectedLayerName;
			return true;
		case "showsavenotice":
		{
			bool flag = false;
			if (signComplexityInfo.IsValid)
			{
				flag = signComplexityInfo.TotalComplexity > 600f || signComplexityInfo.StackInfo.MaxCompStackIndex > 7 || signComplexityInfo.StackInfo.MaxUVStackIndex > 7;
			}
			value = flag.ToString();
			return true;
		}
		case "savenoticetext":
			stringBuilder.Clear();
			if (signComplexityInfo.IsValid)
			{
				if (signComplexityInfo.TotalComplexity > 600f)
				{
					stringBuilder.AppendLine(Localization.Get("xuiSignComplexityExceeded"));
				}
				if (signComplexityInfo.StackInfo.MaxCompStackIndex > 7)
				{
					if (stringBuilder.Length > 0)
					{
						stringBuilder.AppendLine();
					}
					stringBuilder.AppendLine(string.Format(Localization.Get("xuiSignNestedMaskLimit"), signComplexityInfo.StackInfo.MaxCompStackIndex, 7));
				}
				if (signComplexityInfo.StackInfo.MaxUVStackIndex > 7)
				{
					if (stringBuilder.Length > 0)
					{
						stringBuilder.AppendLine();
					}
					stringBuilder.AppendLine(string.Format(Localization.Get("xuiSignNestedWarpLimit"), signComplexityInfo.StackInfo.MaxUVStackIndex, 7));
				}
			}
			value = stringBuilder.ToString();
			return true;
		default:
			return base.GetBindingValueInternal(ref value, bindingName);
		}
	}

	public override bool ParseAttribute(string name, string value)
	{
		bool flag = base.ParseAttribute(name, value);
		if (!flag)
		{
			switch (name)
			{
			case "warp_icon_skew":
				warpIconSkew = value;
				return true;
			case "warp_icon_bulge":
				warpIconBulge = value;
				return true;
			case "warp_icon_twirl":
				warpIconTwirl = value;
				return true;
			case "warp_icon_kaleido":
				warpIconKaleido = value;
				return true;
			case "warp_icon_perspective":
				warpIconPerspective = value;
				return true;
			case "warp_icon_arc":
				warpIconArc = value;
				return true;
			case "warp_icon_stretch":
				warpIconStretch = value;
				return true;
			case "warp_icon_grid":
				warpIconGrid = value;
				return true;
			}
		}
		return flag;
	}
}
