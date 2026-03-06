using UnityEngine.Scripting;

[Preserve]
public class XUiC_RecipeTrackerWindow : XUiController
{
	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_RecipeTrackerIngredientsList ingredientList;

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayerLocal localPlayer;

	public int Count = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public Recipe currentRecipe;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt trackerheightFormatter = new CachedStringFormatterInt();

	public Recipe CurrentRecipe
	{
		get
		{
			return currentRecipe;
		}
		set
		{
			currentRecipe = value;
			IsDirty = true;
			RefreshBindings(_forceAll: true);
		}
	}

	public override void Init()
	{
		base.Init();
		ID = base.WindowGroup.ID;
		ingredientList = GetChildByType<XUiC_RecipeTrackerIngredientsList>();
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (localPlayer == null)
		{
			localPlayer = base.xui.playerUI.entityPlayer;
		}
		if (base.ViewComponent.IsVisible && localPlayer.IsDead())
		{
			IsDirty = true;
		}
		if (!IsDirty)
		{
			return;
		}
		ingredientList.Count = Count;
		if (currentRecipe != null)
		{
			int craftingTier = currentRecipe.GetCraftingTier(base.xui.playerUI.entityPlayer);
			if (currentRecipe.GetOutputItemClass().HasQuality)
			{
				currentRecipe.craftingTier = base.xui.Recipes.TrackedRecipeQuality;
			}
			else
			{
				currentRecipe.craftingTier = craftingTier;
			}
		}
		ingredientList.Recipe = currentRecipe;
		RefreshBindings(_forceAll: true);
		IsDirty = false;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		CurrentRecipe = base.xui.Recipes.TrackedRecipe;
		base.xui.Recipes.OnTrackedRecipeChanged += RecipeTracker_OnTrackedRecipeChanged;
		IsDirty = true;
	}

	public override void OnClose()
	{
		base.OnClose();
		if (XUi.IsGameRunning())
		{
			base.xui.Recipes.OnTrackedRecipeChanged -= RecipeTracker_OnTrackedRecipeChanged;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RecipeTracker_OnTrackedRecipeChanged()
	{
		CurrentRecipe = base.xui.Recipes.TrackedRecipe;
		Count = base.xui.Recipes.TrackedRecipeCount;
		IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string value, string bindingName)
	{
		switch (bindingName)
		{
		case "recipename":
			value = ((currentRecipe != null) ? Localization.Get(currentRecipe.GetName()) : "");
			return true;
		case "recipetitle":
			value = ((currentRecipe != null) ? (Localization.Get(currentRecipe.GetName()) + ((Count > 1) ? $" (x{Count})" : "")) : "");
			return true;
		case "recipeicon":
			value = ((currentRecipe != null) ? currentRecipe.GetIcon() : "");
			return true;
		case "showrecipe":
			value = (currentRecipe != null && XUi.IsGameRunning() && localPlayer != null && !localPlayer.IsDead()).ToString();
			return true;
		case "showempty":
			value = (currentRecipe == null).ToString();
			return true;
		case "trackerheight":
			if (currentRecipe == null)
			{
				value = "0";
			}
			else
			{
				value = trackerheightFormatter.Format(ingredientList.GetActiveIngredientCount() * 35);
			}
			return true;
		default:
			return false;
		}
	}
}
