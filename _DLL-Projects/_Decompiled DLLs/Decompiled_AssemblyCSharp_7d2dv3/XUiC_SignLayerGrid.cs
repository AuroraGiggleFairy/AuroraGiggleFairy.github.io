using System;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_SignLayerGrid : XUiController
{
	public delegate void MultiSelectionDeselectCallback(bool wasPrimary);

	[PublicizedFrom(EAccessModifier.Private)]
	public int baseLayerIndex;

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiC_SignLayer[] signLayerControllers;

	[PublicizedFrom(EAccessModifier.Private)]
	public GlobalSignId currentId;

	[PublicizedFrom(EAccessModifier.Private)]
	public int hoveredLayerIndex = -1;

	public Action<int> OnLayerSelected;

	public Action<int> OnLayerHovered;

	public MultiSelectionDeselectCallback OnMultiSelectionDeselect;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SignLayer selectionOverrideLayer;

	public readonly List<int> MultiSelectedLayerIndices = new List<int>();

	public readonly List<int> draggedLayerIndices = new List<int>();

	[PublicizedFrom(EAccessModifier.Private)]
	public int placeholderIndex = -1;

	public Action<int> OnDragAndDropStarted;

	public Action<int> OnDragAndDropReleased;

	public Action<int> OnAddLayerPressed;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController activeArea;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isInsideActiveArea;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnAddLayer;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isBtnAddLayerHovered;

	[PublicizedFrom(EAccessModifier.Private)]
	public int nearestInsertIndex = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController dragAndDropIndicator;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController insertIndicator;

	public bool isDraggingLayers;

	[PublicizedFrom(EAccessModifier.Private)]
	public int dragStartIdx;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public int SelectedLayerIndex
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	} = -1;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public int SlotCount
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public int LayerCount
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public int ItemCount => LayerCount + (HasPlaceholder ? 1 : 0);

	[field: PublicizedFrom(EAccessModifier.Private)]
	public Action OnAddLayerSelected
	{
		get; [PublicizedFrom(EAccessModifier.Internal)]
		set;
	}

	public bool HasPlaceholder => placeholderIndex >= 0;

	public override void Init()
	{
		base.Init();
		XUiController childById = GetChildById("grid");
		signLayerControllers = childById.GetChildrenByType<XUiC_SignLayer>();
		SlotCount = signLayerControllers.Length;
		IsDirty = false;
		for (int i = 0; i < childById.Children.Count; i++)
		{
			childById.Children[i].OnScroll += HandleOnScroll;
		}
		for (int j = 0; j < signLayerControllers.Length; j++)
		{
			XUiC_SignLayer obj = signLayerControllers[j];
			obj.OnClicked = (Action<XUiC_SignLayer>)Delegate.Combine(obj.OnClicked, new Action<XUiC_SignLayer>(HandleOnClick));
			signLayerControllers[j].OnHover += HandleOnHover;
			XUiC_SignLayer obj2 = signLayerControllers[j];
			obj2.OnBecameSelected = (Action<XUiC_SignGridEntry>)Delegate.Combine(obj2.OnBecameSelected, new Action<XUiC_SignGridEntry>(HandleOnSelect));
			XUiC_SignLayer obj3 = signLayerControllers[j];
			obj3.OnBecameCursorSelected = (Action<XUiC_SignLayer>)Delegate.Combine(obj3.OnBecameCursorSelected, new Action<XUiC_SignLayer>(HandleOnCursorSelect));
			signLayerControllers[j].OnDrag += HandleOnDrag;
		}
		childById.OnScroll += HandleOnScroll;
		XUiController childById2 = GetChildById("btnLayerPrevious");
		XUiController childById3 = GetChildById("btnLayerNext");
		childById2.OnPress += BtnPreviousLayerOnPress;
		childById3.OnPress += BtnNextLayerOnPress;
		activeArea = GetChildById("activeArea");
		btnAddLayer = GetChildById("btnAddLayer") as XUiC_SimpleButton;
		btnAddLayer.OnPressed += HandleAddLayer;
		btnAddLayer.OnHovered += [PublicizedFrom(EAccessModifier.Private)] (XUiController sender, bool isOver) =>
		{
			isBtnAddLayerHovered = isOver;
			RefreshBindings();
		};
		dragAndDropIndicator = GetChildById("dragAndDropIndicator");
		insertIndicator = GetChildById("insertIndicator");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleOnDrag(XUiController sender, EDragType dragType, Vector2 mousePositionDelta)
	{
		if (isDraggingLayers || hoveredLayerIndex <= -1 || signLayerControllers[hoveredLayerIndex - baseLayerIndex].layer == null)
		{
			return;
		}
		dragStartIdx = hoveredLayerIndex;
		isDraggingLayers = true;
		draggedLayerIndices.Clear();
		draggedLayerIndices.AddRange(MultiSelectedLayerIndices);
		if (!draggedLayerIndices.Contains(hoveredLayerIndex))
		{
			if (!InputUtils.ControlKeyPressed)
			{
				draggedLayerIndices.Clear();
			}
			draggedLayerIndices.Add(hoveredLayerIndex);
		}
		draggedLayerIndices.Sort();
		OnDragAndDropStarted?.Invoke(dragStartIdx);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnNextLayerOnPress(XUiController sender, int mouseButton)
	{
		TryScroll(1);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnPreviousLayerOnPress(XUiController sender, int mouseButton)
	{
		TryScroll(-1);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleOnCursorSelect(XUiC_SignLayer layer)
	{
		if (xui.playerUI.CursorController.navigationTarget?.Controller is XUiC_SignLayer xUiC_SignLayer)
		{
			if (layer == signLayerControllers[0] && xUiC_SignLayer == signLayerControllers[SlotCount - 1])
			{
				TryScroll(-ItemCount);
				return;
			}
			if (xUiC_SignLayer == signLayerControllers[0] && layer == signLayerControllers[SlotCount - 1])
			{
				TryScroll(ItemCount);
				return;
			}
		}
		if (layer.index < baseLayerIndex + 1)
		{
			if (TryScroll(-1))
			{
				selectionOverrideLayer = signLayerControllers[1];
			}
		}
		else if (layer.index > baseLayerIndex + SlotCount - 2 && TryScroll(1))
		{
			selectionOverrideLayer = signLayerControllers[SlotCount - 2];
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleOnHover(XUiController _sender, bool _isOver)
	{
		if (_sender is XUiC_SignLayer xUiC_SignLayer)
		{
			hoveredLayerIndex = (_isOver ? xUiC_SignLayer.index : (-1));
			OnLayerHovered?.Invoke(hoveredLayerIndex);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleOnClick(XUiC_SignLayer layerthumb)
	{
		if (!InputUtils.ControlKeyPressed)
		{
			MultiSelectedLayerIndices.Clear();
			if (InputUtils.ShiftKeyPressed)
			{
				int num = math.min(SelectedLayerIndex, layerthumb.index);
				int num2 = math.max(SelectedLayerIndex, layerthumb.index);
				for (int i = num; i <= num2; i++)
				{
					MultiSelectedLayerIndices.Add(i);
				}
			}
			else
			{
				MultiSelectedLayerIndices.Add(layerthumb.index);
			}
			layerthumb.IsSelected = true;
		}
		else if (InputUtils.ShiftKeyPressed)
		{
			int num3 = math.min(SelectedLayerIndex, layerthumb.index);
			int num4 = math.min(math.max(SelectedLayerIndex, layerthumb.index), LayerCount - 1);
			for (int j = num3; j <= num4; j++)
			{
				if (!MultiSelectedLayerIndices.Contains(layerthumb.index))
				{
					MultiSelectedLayerIndices.Add(j);
				}
			}
			layerthumb.IsSelected = true;
		}
		else if (!MultiSelectedLayerIndices.Contains(layerthumb.index))
		{
			MultiSelectedLayerIndices.Add(layerthumb.index);
			layerthumb.IsSelected = true;
		}
		else
		{
			if (MultiSelectedLayerIndices.Count <= 1)
			{
				return;
			}
			MultiSelectedLayerIndices.Remove(layerthumb.index);
			if (SelectedLayerIndex == layerthumb.index)
			{
				SelectedLayerIndex = MultiSelectedLayerIndices[MultiSelectedLayerIndices.Count - 1];
				if (SelectedLayerIndex > baseLayerIndex && SelectedLayerIndex < baseLayerIndex + ItemCount)
				{
					signLayerControllers[SelectedLayerIndex - baseLayerIndex].IsSelected = true;
				}
				else
				{
					layerthumb.IsSelected = false;
					OnLayerSelected?.Invoke(SelectedLayerIndex);
				}
				OnMultiSelectionDeselect(wasPrimary: true);
			}
			else
			{
				OnMultiSelectionDeselect(wasPrimary: false);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleOnSelect(XUiC_SignGridEntry gridEntry)
	{
		if (gridEntry is XUiC_SignLayer xUiC_SignLayer)
		{
			SelectedLayerIndex = xUiC_SignLayer.index;
			OnLayerSelected?.Invoke(SelectedLayerIndex);
		}
		else
		{
			Log.Error("Failed to cast selected element as XUiC_SignLayer.");
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleOnScroll(XUiController _sender, float _delta)
	{
		TryScroll((!(_delta > 0f)) ? 1 : (-1));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool TryScroll(int _delta)
	{
		int num = Mathf.Clamp(baseLayerIndex + _delta, 0, Mathf.Max(ItemCount - SlotCount, 0));
		if (num == baseLayerIndex)
		{
			return false;
		}
		if (hoveredLayerIndex > -1)
		{
			hoveredLayerIndex += num - baseLayerIndex;
			OnLayerHovered?.Invoke(hoveredLayerIndex);
		}
		baseLayerIndex = num;
		IsDirty = true;
		return true;
	}

	public void ReleaseDragAndDrop()
	{
		if (isDraggingLayers)
		{
			isDraggingLayers = false;
			OnDragAndDropReleased?.Invoke(isInsideActiveArea ? nearestInsertIndex : (-1));
			RefreshBindings();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleAddLayer(XUiController sender, int mouseButton)
	{
		OnAddLayerPressed?.Invoke(nearestInsertIndex);
	}

	public int GridIndexToLayerIndex(int gridIndex)
	{
		if (!HasPlaceholder)
		{
			return gridIndex;
		}
		if (gridIndex < placeholderIndex)
		{
			return gridIndex;
		}
		if (gridIndex == placeholderIndex)
		{
			return -1;
		}
		return gridIndex - 1;
	}

	public void RefreshSelectedLayerBindings()
	{
		int num = SelectedLayerIndex - baseLayerIndex;
		if (num >= 0 && num < SlotCount)
		{
			signLayerControllers[num].RefreshBindings();
		}
	}

	public void UpdateLayers(GlobalSignId signId, int selectedIndex, bool placeholderActive, bool autoScroll = true)
	{
		int num = placeholderIndex;
		placeholderIndex = (placeholderActive ? selectedIndex : (-1));
		bool flag = num != placeholderIndex;
		if (placeholderActive)
		{
			MultiSelectedLayerIndices.Clear();
		}
		xui.GetChildByType<XUiC_SignGalleryWindow>();
		currentId = signId;
		SignDataManager.Instance.TryGetSignData(signId, out var signData);
		LayerCount = signData?.layers.Count ?? 0;
		if (autoScroll)
		{
			int num2 = selectedIndex - baseLayerIndex;
			if (num2 < 0)
			{
				TryScroll(num2);
			}
			else if (num2 >= SlotCount)
			{
				TryScroll(num2 - SlotCount + 1);
			}
		}
		for (int i = 0; i < SlotCount; i++)
		{
			int num3 = baseLayerIndex + i;
			XUiC_SignLayer xUiC_SignLayer = signLayerControllers[i];
			if (HasPlaceholder && num3 == placeholderIndex)
			{
				xUiC_SignLayer.SetAsPlaceholder(signId, num3);
			}
			else
			{
				int num4 = GridIndexToLayerIndex(num3);
				if (num4 >= 0 && num4 < LayerCount)
				{
					xUiC_SignLayer.SetLayer(signId, signData.layers[num4], num3);
				}
				else
				{
					xUiC_SignLayer.SetLayer(GlobalSignId.InvalidId, null, num3);
				}
			}
			if (num3 == selectedIndex)
			{
				xUiC_SignLayer.IsSelected = true;
			}
			else if (xUiC_SignLayer.IsSelected)
			{
				xUiC_SignLayer.IsSelected = false;
			}
		}
		if (SelectedLayerIndex != selectedIndex)
		{
			Log.Error($"Error in sign layer grid: SelectedLayerIndex '{SelectedLayerIndex}' does not match the input index '{selectedIndex}' after updating all layer controllers.");
		}
		if (flag)
		{
			RefreshBindings();
		}
		IsDirty = false;
	}

	public void SetLayerName(string name)
	{
		signLayerControllers[SelectedLayerIndex - baseLayerIndex].layer.name = name;
		signLayerControllers[SelectedLayerIndex - baseLayerIndex].RefreshTooltip();
	}

	public bool MousePositionInsideActiveArea()
	{
		Vector3 point = xui.playerUI.camera.ScreenToWorldPoint(Input.mousePosition);
		point.z = activeArea.ViewComponent.ColliderCenter.z;
		return activeArea.ViewComponent.ColliderBounds.Contains(point);
	}

	public override void OnOpen()
	{
		if (base.ViewComponent != null && !base.ViewComponent.IsVisible)
		{
			base.ViewComponent.IsVisible = true;
		}
	}

	public override void OnClose()
	{
		MultiSelectedLayerIndices.Clear();
		if (base.ViewComponent != null && base.ViewComponent.IsVisible)
		{
			base.ViewComponent.IsVisible = false;
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		bool flag = isInsideActiveArea;
		isInsideActiveArea = MousePositionInsideActiveArea();
		if (isInsideActiveArea)
		{
			int nearestVisibleInsertSlot = GetNearestVisibleInsertSlot();
			float insertPositionX = GetInsertPositionX(nearestVisibleInsertSlot);
			int num = baseLayerIndex + nearestVisibleInsertSlot;
			float t = 1f - Mathf.Exp(-20f * _dt);
			float x = Mathf.Lerp(btnAddLayer.ViewComponent.UiTransform.position.x, insertPositionX, t);
			btnAddLayer.ViewComponent.UiTransform.position = new Vector2(x, btnAddLayer.ViewComponent.UiTransform.position.y);
			insertIndicator.ViewComponent.UiTransform.position = new Vector2(insertPositionX, insertIndicator.ViewComponent.UiTransform.position.y);
			nearestInsertIndex = num;
			if (isDraggingLayers)
			{
				dragAndDropIndicator.ViewComponent.UiTransform.position = new Vector2(insertPositionX, dragAndDropIndicator.ViewComponent.UiTransform.position.y);
			}
		}
		if (isDraggingLayers || flag != isInsideActiveArea)
		{
			RefreshBindings();
		}
		if (IsDirty)
		{
			selectionOverrideLayer?.SelectCursorElement(_withDelay: false, _overrideCursorMode: true);
			selectionOverrideLayer = null;
			UpdateLayers(currentId, SelectedLayerIndex, HasPlaceholder, autoScroll: false);
			RefreshBindings();
			IsDirty = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int GetNearestVisibleInsertSlot()
	{
		float x = xui.playerUI.camera.ScreenToWorldPoint(Input.mousePosition).x;
		float leftInsertX = signLayerControllers[0].LeftInsertX;
		float num = ((SlotCount > 1) ? ((signLayerControllers[SlotCount - 1].LeftInsertX - signLayerControllers[0].LeftInsertX) / (float)(SlotCount - 1)) : (signLayerControllers[0].RightInsertX - signLayerControllers[0].LeftInsertX));
		int value = Mathf.RoundToInt((x - leftInsertX) / num);
		int max = 0;
		for (int i = 0; i < SlotCount && signLayerControllers[i].LayerValid; i++)
		{
			max = i + 1;
		}
		return Mathf.Clamp(value, 0, max);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float GetInsertPositionX(int visibleSlot)
	{
		if (visibleSlot != 0)
		{
			return signLayerControllers[visibleSlot - 1].RightInsertX;
		}
		return signLayerControllers[0].LeftInsertX;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		if (base.GetBindingValueInternal(ref _value, _bindingName))
		{
			return true;
		}
		switch (_bindingName)
		{
		case "showlayernext":
			_value = (ItemCount > baseLayerIndex + SlotCount).ToString();
			return true;
		case "showlayerprev":
			_value = (baseLayerIndex > 0).ToString();
			return true;
		case "showaddlayer":
			_value = (!isDraggingLayers && isInsideActiveArea && !HasPlaceholder).ToString();
			return true;
		case "showinsertindicator":
			_value = (!isDraggingLayers && isInsideActiveArea && isBtnAddLayerHovered).ToString();
			return true;
		case "isdraganddropvalid":
			_value = (isDraggingLayers && isInsideActiveArea).ToString();
			return true;
		default:
			return false;
		}
	}

	[Conditional("DEBUG_LOG_SIGN_LAYER_GRID")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void DebugLog(string message)
	{
	}
}
