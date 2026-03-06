using System.Xml.Linq;
using UnityEngine.Scripting;

namespace Challenges;

[Preserve]
public class ChallengeObjectiveCraft : BaseChallengeObjective
{
	[PublicizedFrom(EAccessModifier.Private)]
	public ItemValue expectedItem = ItemValue.None.Clone();

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemClass expectedItemClass;

	public string[] itemClassIDs;

	public string itemClassID = "";

	public Recipe itemRecipe;

	public override ChallengeObjectiveType ObjectiveType => ChallengeObjectiveType.Craft;

	public override string DescriptionText => Localization.Get("lblContextActionCraft") + " " + Localization.Get(itemClassID) + ":";

	public override ChallengeClass.UINavTypes NavType
	{
		get
		{
			if (itemRecipe != null)
			{
				if (!(itemRecipe.craftingArea == ""))
				{
					return ChallengeClass.UINavTypes.None;
				}
				return ChallengeClass.UINavTypes.Crafting;
			}
			return ChallengeClass.UINavTypes.None;
		}
	}

	public override void Init()
	{
		expectedItem = ItemClass.GetItem(itemClassID);
		expectedItemClass = ItemClass.GetItemClass(itemClassID);
		itemRecipe = CraftingManager.GetRecipe(itemClassID);
	}

	public override void HandleOnCreated()
	{
		base.HandleOnCreated();
		CreateRequirements();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CreateRequirements()
	{
		if (itemClassIDs.Length <= 1 && ShowRequirements)
		{
			Owner.SetRequirementGroup(new RequirementObjectiveGroupGatherIngredients(itemClassID)
			{
				CraftObj = this
			});
		}
	}

	public override void HandleAddHooks()
	{
		QuestEventManager.Current.CraftItem -= Current_CraftItem;
		QuestEventManager.Current.CraftItem += Current_CraftItem;
	}

	public override void HandleRemoveHooks()
	{
		QuestEventManager.Current.CraftItem -= Current_CraftItem;
	}

	public override void HandleTrackingStarted()
	{
		base.HandleTrackingStarted();
	}

	public override void HandleTrackingEnded()
	{
		base.HandleTrackingEnded();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Current_CraftItem(ItemStack stack)
	{
		if (!CheckBaseRequirements())
		{
			ItemClass itemClass = stack.itemValue.ItemClass;
			if (itemClass != null && itemClassIDs.ContainsCaseInsensitive(itemClass.Name))
			{
				base.Current += stack.count;
				CheckObjectiveComplete();
			}
		}
	}

	public override bool CheckObjectiveComplete(bool handleComplete = true)
	{
		if (IsRequirement && CheckForNeededItem())
		{
			base.Complete = true;
			HandleRecipeListUpdate();
			return true;
		}
		base.Complete = false;
		HandleRecipeListUpdate();
		return base.CheckObjectiveComplete(handleComplete);
	}

	public override Recipe GetRecipeItem()
	{
		return itemRecipe;
	}

	public override Recipe[] GetRecipeItems()
	{
		Recipe recipeFromRequirements = Owner.GetRecipeFromRequirements();
		if (recipeFromRequirements == null)
		{
			return new Recipe[1] { itemRecipe };
		}
		return new Recipe[2] { recipeFromRequirements, itemRecipe };
	}

	public override void ParseElement(XElement e)
	{
		base.ParseElement(e);
		if (e.HasAttribute("item"))
		{
			SetupItem(e.GetAttribute("item"));
		}
	}

	public void SetupItem(string itemID)
	{
		itemClassID = itemID;
		if (itemClassID.Contains(','))
		{
			itemClassIDs = itemClassID.Split(',');
			itemClassID = itemClassIDs[0];
		}
		else
		{
			itemClassIDs = new string[1];
			itemClassIDs[0] = itemClassID;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool CheckForNeededItem()
	{
		LocalPlayerUI uIForPlayer = LocalPlayerUI.GetUIForPlayer(Owner.Owner.Player);
		XUiM_PlayerInventory playerInventory = uIForPlayer.xui.PlayerInventory;
		ItemValue itemValue = new ItemValue(itemRecipe.itemValueType);
		int itemCount = playerInventory.Backpack.GetItemCount(itemValue);
		itemCount += playerInventory.Toolbelt.GetItemCount(itemValue);
		ItemStack currentStack = uIForPlayer.xui.dragAndDrop.CurrentStack;
		if (!currentStack.IsEmpty() && currentStack.itemValue.type == itemRecipe.itemValueType)
		{
			itemCount += currentStack.count;
		}
		base.Current = itemCount;
		return itemCount >= MaxCount;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleRecipeListUpdate()
	{
		LocalPlayerUI uIForPlayer = LocalPlayerUI.GetUIForPlayer(Owner.Owner.Player);
		if (uIForPlayer.xui.QuestTracker.TrackedChallenge == Owner)
		{
			uIForPlayer.xui.QuestTracker.HandleTrackedChallengeChanged();
		}
	}

	public override BaseChallengeObjective Clone()
	{
		return new ChallengeObjectiveCraft
		{
			itemClassIDs = itemClassIDs,
			itemClassID = itemClassID,
			itemRecipe = itemRecipe,
			expectedItem = expectedItem,
			expectedItemClass = expectedItemClass
		};
	}

	public override void CompleteObjective(bool handleComplete = true)
	{
		base.Current = MaxCount;
		base.Complete = true;
		if (handleComplete)
		{
			Owner.HandleComplete();
		}
	}
}
