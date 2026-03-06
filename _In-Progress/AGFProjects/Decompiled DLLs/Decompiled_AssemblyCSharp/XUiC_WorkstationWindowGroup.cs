using Audio;
using GUI_2;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_WorkstationWindowGroup : XUiC_CraftingWindowGroup
{
	public XUiM_Workstation WorkstationData;

	[PublicizedFrom(EAccessModifier.Private)]
	public BlockValue workstationBlock;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_WorkstationToolGrid toolWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_WorkstationInputGrid inputWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_WorkstationOutputGrid outputWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_WorkstationFuelGrid fuelWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public new XUiC_CraftingQueue craftingQueue;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label burnTimeLeft;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_WindowNonPagingHeader nonPagingHeaderWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool wasCrafting;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool activeKeyDown;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool wasReleased;

	[PublicizedFrom(EAccessModifier.Private)]
	public float openTEUpdateTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float openTEUpdateTimeMax = 0.5f;

	public void SetTileEntity(TileEntityWorkstation _te)
	{
		WorkstationData = new XUiM_Workstation(_te);
		workstationBlock = _te.GetChunk().GetBlock(_te.localChunkPos);
		base.Workstation = workstationBlock.Block.GetBlockName();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TileEntity_Destroyed(ITileEntity te)
	{
		if (WorkstationData == null || WorkstationData.TileEntity == te)
		{
			base.xui.playerUI.windowManager.Close(windowGroup.ID);
		}
		else
		{
			te.Destroyed -= TileEntity_Destroyed;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TileEntity_ToolChanged()
	{
		if (toolWindow != null)
		{
			toolWindow.SetSlots(WorkstationData.GetToolStacks());
			SetAllChildrenDirty();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TileEntity_OutputChanged()
	{
		if (outputWindow != null)
		{
			outputWindow.SetSlots(WorkstationData.GetOutputStacks());
			SetAllChildrenDirty();
		}
	}

	public void TileEntity_InputChanged()
	{
		if (inputWindow != null)
		{
			inputWindow.SetSlots(WorkstationData.GetInputStacks());
			SetAllChildrenDirty();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TileEntity_FuelChanged()
	{
		if (fuelWindow != null)
		{
			fuelWindow.SetSlots(WorkstationData.GetFuelStacks());
			SetAllChildrenDirty();
		}
	}

	public override void Init()
	{
		base.Init();
		XUiController childByType = GetChildByType<XUiC_WorkstationToolGrid>();
		if (childByType != null)
		{
			toolWindow = (XUiC_WorkstationToolGrid)childByType;
		}
		childByType = GetChildByType<XUiC_WorkstationInputGrid>();
		if (childByType != null)
		{
			inputWindow = (XUiC_WorkstationInputGrid)childByType;
		}
		childByType = GetChildByType<XUiC_WorkstationOutputGrid>();
		if (childByType != null)
		{
			outputWindow = (XUiC_WorkstationOutputGrid)childByType;
		}
		childByType = GetChildByType<XUiC_WorkstationFuelGrid>();
		if (childByType != null)
		{
			fuelWindow = (XUiC_WorkstationFuelGrid)childByType;
		}
		childByType = GetChildByType<XUiC_CraftingQueue>();
		if (childByType != null)
		{
			craftingQueue = (XUiC_CraftingQueue)childByType;
		}
		childByType = GetChildByType<XUiC_WindowNonPagingHeader>();
		if (childByType != null)
		{
			nonPagingHeaderWindow = GetChildByType<XUiC_WindowNonPagingHeader>();
		}
		childByType = GetChildById("burnTimeLeft");
		if (childByType != null)
		{
			burnTimeLeft = (XUiV_Label)GetChildById("burnTimeLeft").ViewComponent;
		}
	}

	public override bool CraftingRequirementsValid(Recipe _recipe)
	{
		bool flag = true;
		if (toolWindow != null)
		{
			flag &= toolWindow.HasRequirement(_recipe);
		}
		if (inputWindow != null)
		{
			flag &= inputWindow.HasRequirement(_recipe);
		}
		if (outputWindow != null)
		{
			flag &= outputWindow.HasRequirement(_recipe);
		}
		if (fuelWindow != null)
		{
			flag &= fuelWindow.HasRequirement(_recipe);
		}
		return flag;
	}

	public override string CraftingRequirementsInvalidMessage(Recipe _recipe)
	{
		if (toolWindow != null && !toolWindow.HasRequirement(_recipe))
		{
			return Localization.Get("ttMissingCraftingTools");
		}
		if (inputWindow != null && !inputWindow.HasRequirement(_recipe))
		{
			return Localization.Get("ttMissingCraftingResources");
		}
		if (outputWindow != null)
		{
			outputWindow.HasRequirement(_recipe);
		}
		if (fuelWindow != null && !fuelWindow.HasRequirement(_recipe))
		{
			return Localization.Get("ttMissingCraftingFuel");
		}
		return "";
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		bool flag = craftingQueue.IsCrafting();
		if (flag != wasCrafting)
		{
			wasCrafting = flag;
			syncTEfromUI();
		}
		if (windowGroup.isShowing)
		{
			if (WorkstationData != null && GameManager.Instance != null && GameManager.Instance.World != null && WorkstationData.TileEntity.IsBurning)
			{
				if (openTEUpdateTime <= 0f)
				{
					WorkstationData.TileEntity.UpdateTick(GameManager.Instance.World);
					openTEUpdateTime = 0.5f;
				}
				else
				{
					openTEUpdateTime -= _dt;
				}
			}
			if (!base.xui.playerUI.playerInput.PermanentActions.Activate.IsPressed)
			{
				wasReleased = true;
			}
			if (wasReleased)
			{
				if (base.xui.playerUI.playerInput.PermanentActions.Activate.IsPressed)
				{
					activeKeyDown = true;
				}
				if (base.xui.playerUI.playerInput.PermanentActions.Activate.WasReleased && activeKeyDown)
				{
					activeKeyDown = false;
					if (!base.xui.playerUI.windowManager.IsInputActive())
					{
						base.xui.playerUI.windowManager.CloseAllOpenWindows();
					}
				}
			}
		}
		if (fuelWindow != null && craftingQueue != null && burnTimeLeft != null && WorkstationData != null)
		{
			float totalBurnTimeLeft = WorkstationData.GetTotalBurnTimeLeft();
			if ((!WorkstationData.GetIsBurning() || totalBurnTimeLeft == 0f) && craftingQueue.IsCrafting())
			{
				craftingQueue.HaltCrafting();
			}
			else if (WorkstationData.GetIsBurning() && totalBurnTimeLeft == 0f && !craftingQueue.IsCrafting())
			{
				craftingQueue.ResumeCrafting();
			}
			burnTimeLeft.Text = string.Format("{0}:{1}", ((int)(totalBurnTimeLeft / 60f)).ToString("00"), ((int)(totalBurnTimeLeft % 60f)).ToString("00"));
		}
	}

	public override bool AlwaysUpdate()
	{
		return false;
	}

	public override bool AddItemToQueue(Recipe _recipe)
	{
		if (fuelWindow != null)
		{
			fuelWindow.TurnOn();
		}
		return base.AddItemToQueue(_recipe);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void syncTEfromUI()
	{
		TileEntityWorkstation tileEntity = WorkstationData.TileEntity;
		tileEntity.SetDisableModifiedCheck(_b: true);
		if (toolWindow != null)
		{
			WorkstationData.SetToolStacks(toolWindow.GetSlots());
		}
		if (inputWindow != null)
		{
			WorkstationData.SetInputStacks(inputWindow.GetSlots());
		}
		if (outputWindow != null)
		{
			WorkstationData.SetOutputStacks(outputWindow.GetSlots());
		}
		if (fuelWindow != null)
		{
			WorkstationData.SetFuelStacks(fuelWindow.GetSlots());
		}
		if (craftingQueue != null)
		{
			XUiC_RecipeStack[] recipesToCraft = craftingQueue.GetRecipesToCraft();
			RecipeQueueItem[] array = new RecipeQueueItem[recipesToCraft.Length];
			for (int i = 0; i < recipesToCraft.Length; i++)
			{
				RecipeQueueItem recipeQueueItem = new RecipeQueueItem();
				recipeQueueItem.Recipe = recipesToCraft[i].GetRecipe();
				recipeQueueItem.Multiplier = (short)recipesToCraft[i].GetRecipeCount();
				recipeQueueItem.CraftingTimeLeft = recipesToCraft[i].GetRecipeCraftingTimeLeft();
				recipeQueueItem.IsCrafting = recipesToCraft[i].IsCrafting;
				recipeQueueItem.Quality = (byte)recipesToCraft[i].OutputQuality;
				recipeQueueItem.StartingEntityId = recipesToCraft[i].StartingEntityId;
				recipeQueueItem.OneItemCraftTime = recipesToCraft[i].GetOneItemCraftTime();
				array[i] = recipeQueueItem;
			}
			WorkstationData.SetRecipeQueueItems(array);
		}
		tileEntity.SetDisableModifiedCheck(_b: false);
		tileEntity.SetModified();
		tileEntity.ResetTickTime();
	}

	public override void OnOpen()
	{
		WorkstationData.SetUserAccessing(isUserAccessing: true);
		WorkstationData.TileEntity.FuelChanged += TileEntity_FuelChanged;
		WorkstationData.TileEntity.InputChanged += TileEntity_InputChanged;
		WorkstationData.TileEntity.Destroyed += TileEntity_Destroyed;
		base.xui.currentWorkstation = workstation;
		openTEUpdateTime = 0.5f;
		if (base.ViewComponent != null && !base.ViewComponent.IsVisible)
		{
			base.ViewComponent.OnOpen();
			base.ViewComponent.IsVisible = true;
		}
		if (recipeList != null && categoryList != null && categoryList.SetupCategoriesByWorkstation(base.Workstation))
		{
			recipeList.Workstation = base.Workstation;
			categoryList.SetCategoryToFirst();
		}
		if (nonPagingHeaderWindow != null)
		{
			nonPagingHeaderWindow.SetHeader(Localization.Get(base.Workstation));
		}
		_ = base.xui.playerUI.windowManager;
		syncUIfromTE();
		if (craftingQueue != null)
		{
			craftingQueue.ClearQueue();
			RecipeQueueItem[] recipeQueueItems = WorkstationData.GetRecipeQueueItems();
			for (int i = 0; i < recipeQueueItems.Length; i++)
			{
				if (recipeQueueItems[i] != null)
				{
					craftingQueue.AddRecipeToCraftAtIndex(i, recipeQueueItems[i].Recipe, recipeQueueItems[i].Multiplier, recipeQueueItems[i].CraftingTimeLeft, recipeQueueItems[i].IsCrafting, recipeModification: false, recipeQueueItems[i].Quality, recipeQueueItems[i].StartingEntityId, recipeQueueItems[i].OneItemCraftTime);
				}
				else
				{
					craftingQueue.AddRecipeToCraftAtIndex(i, null, 0, -1f, isCrafting: false, recipeModification: true);
				}
			}
			craftingQueue.IsDirty = true;
		}
		base.xui.RecenterWindowGroup(windowGroup);
		for (int j = 0; j < children.Count; j++)
		{
			children[j].OnOpen();
		}
		WorkstationData workstationData = CraftingManager.GetWorkstationData(workstation);
		if (workstationData != null)
		{
			Manager.BroadcastPlayByLocalPlayer(WorkstationData.TileEntity.ToWorldPos().ToVector3() + Vector3.one * 0.5f, workstationData.OpenSound);
			WorkstationData.TileEntity.CheckForCraftComplete(base.xui.playerUI.entityPlayer);
		}
		IsDirty = true;
		base.IsOpen = true;
		base.xui.calloutWindow.AddCallout(UIUtils.ButtonIcon.RightBumper, "igcoWorkstationTurnOnOff", XUiC_GamepadCalloutWindow.CalloutType.MenuShortcuts);
		base.xui.calloutWindow.EnableCallouts(XUiC_GamepadCalloutWindow.CalloutType.MenuShortcuts);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void syncUIfromTE()
	{
		if (WorkstationData != null && GameManager.Instance != null && GameManager.Instance.World != null)
		{
			WorkstationData.TileEntity.UpdateTick(GameManager.Instance.World);
		}
		if (toolWindow != null)
		{
			toolWindow.SetSlots(WorkstationData.GetToolStacks());
			toolWindow.IsDirty = true;
		}
		if (inputWindow != null)
		{
			inputWindow.SetSlots(WorkstationData.GetInputStacks());
			inputWindow.IsDirty = true;
		}
		if (outputWindow != null)
		{
			outputWindow.SetSlots(WorkstationData.GetOutputStacks());
			outputWindow.IsDirty = true;
		}
		if (fuelWindow != null)
		{
			fuelWindow.SetSlots(WorkstationData.GetFuelStacks());
			fuelWindow.IsDirty = true;
		}
		SetAllChildrenDirty();
	}

	public override void OnClose()
	{
		wasReleased = false;
		activeKeyDown = false;
		syncTEfromUI();
		WorkstationData.SetUserAccessing(isUserAccessing: false);
		WorkstationData.TileEntity.FuelChanged -= TileEntity_FuelChanged;
		WorkstationData.TileEntity.InputChanged -= TileEntity_InputChanged;
		WorkstationData.TileEntity.Destroyed -= TileEntity_Destroyed;
		base.OnClose();
		GameManager.Instance.TEUnlockServer(WorkstationData.TileEntity.GetClrIdx(), WorkstationData.TileEntity.ToWorldPos(), WorkstationData.TileEntity.entityId);
		WorkstationData workstationData = CraftingManager.GetWorkstationData(workstation);
		if (workstationData != null)
		{
			Manager.BroadcastPlayByLocalPlayer(WorkstationData.TileEntity.ToWorldPos().ToVector3() + Vector3.one * 0.5f, workstationData.CloseSound);
		}
		base.xui.currentWorkstation = "";
		base.xui.calloutWindow.ClearCallouts(XUiC_GamepadCalloutWindow.CalloutType.MenuShortcuts);
	}
}
