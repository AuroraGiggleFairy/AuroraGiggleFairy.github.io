using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_CollectedItem : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public ItemStack itemStack = ItemStack.Empty.Clone();

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemClass itemClass;

	public GameObject Item;

	public float TimeAdded;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterXuiRgbaColor itemiconcolorFormatter = new CachedStringFormatterXuiRgbaColor();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<int> itemcountFormatter = new CachedStringFormatter<int>([PublicizedFrom(EAccessModifier.Internal)] (int _i) => "+" + _i);

	public ItemStack ItemStack
	{
		get
		{
			return itemStack;
		}
		set
		{
			itemStack = value;
			itemClass = itemStack.itemValue.ItemClass;
			RefreshBindings(_forceAll: true);
		}
	}

	public override void Init()
	{
		base.Init();
		TweenColor tweenColor = base.ViewComponent.UiTransform.gameObject.AddComponent<TweenColor>();
		tweenColor.enabled = false;
		tweenColor.from = Color.white;
		tweenColor.to = new Color(1f, 1f, 1f, 0f);
		tweenColor.duration = 0.8f;
		base.ViewComponent.IsVisible = false;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
	}

	public void ShowItem()
	{
		TweenColor component = base.ViewComponent.UiTransform.gameObject.GetComponent<TweenColor>();
		component.from = Color.white;
		component.to = Color.white;
		component.duration = 0.1f;
		component.enabled = true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string value, string bindingName)
	{
		switch (bindingName)
		{
		case "itemicon":
			value = ((itemClass != null) ? itemClass.GetIconName() : "");
			return true;
		case "itemiconcolor":
		{
			Color32 v = Color.white;
			if (itemStack != null && itemStack.itemValue.type != 0)
			{
				v = itemClass.GetIconTint(itemStack.itemValue);
			}
			value = itemiconcolorFormatter.Format(v);
			return true;
		}
		case "itemcount":
			value = "";
			if (ItemStack != null && itemStack.itemValue.type != 0)
			{
				value = ((ItemStack.count > 0) ? itemcountFormatter.Format(ItemStack.count) : "0");
			}
			return true;
		case "itembackground":
			value = "menu_empty";
			if (itemClass != null && itemStack.itemValue.type != 0)
			{
				value = "ui_game_popup";
			}
			return true;
		case "itembackgroundcolor":
			value = "255, 255, 255, 0";
			if (itemClass != null && itemStack.itemValue.type != 0)
			{
				value = "255, 255, 255, 255";
			}
			return true;
		default:
			return false;
		}
	}
}
