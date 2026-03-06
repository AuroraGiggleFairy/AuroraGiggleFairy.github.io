using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_UnlockByEntry : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public bool isDirty;

	[PublicizedFrom(EAccessModifier.Private)]
	public RecipeUnlockData unlockData;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterXuiRgbaColor itemicontintcolorFormatter = new CachedStringFormatterXuiRgbaColor();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt havecountFormatter = new CachedStringFormatterInt();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt needcountFormatter = new CachedStringFormatterInt();

	[field: PublicizedFrom(EAccessModifier.Private)]
	public Recipe Recipe { get; set; }

	public RecipeUnlockData UnlockData
	{
		get
		{
			return unlockData;
		}
		set
		{
			unlockData = value;
			isDirty = true;
		}
	}

	public override void Init()
	{
		base.Init();
		isDirty = false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string value, string bindingName)
	{
		bool flag = UnlockData != null;
		switch (bindingName)
		{
		case "name":
			if (flag)
			{
				value = UnlockData.GetName();
			}
			else
			{
				value = "";
			}
			return true;
		case "itemicon":
			if (flag)
			{
				value = UnlockData.GetIcon();
			}
			else
			{
				value = "";
			}
			return true;
		case "itemiconatlas":
			if (flag)
			{
				value = UnlockData.GetIconAtlas();
			}
			else
			{
				value = "UIAtlas";
			}
			return true;
		case "itemicontint":
		{
			Color32 v = Color.white;
			if (flag)
			{
				v = UnlockData.GetItemTint();
			}
			value = itemicontintcolorFormatter.Format(v);
			return true;
		}
		case "level":
			if (flag)
			{
				value = UnlockData.GetLevel(base.xui.playerUI.entityPlayer, Recipe.GetOutputItemClass().Name);
			}
			else
			{
				value = "";
			}
			return true;
		default:
			return false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleOnCountChanged(XUiController _sender, OnCountChangedEventArgs _e)
	{
		isDirty = true;
	}

	public override void Update(float _dt)
	{
		if (isDirty)
		{
			RefreshBindings();
			base.ViewComponent.IsVisible = true;
			isDirty = false;
		}
		base.Update(_dt);
	}
}
