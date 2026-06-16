using Audio;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_DragAndDropWindow : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ItemStack ItemStackControl;

	[PublicizedFrom(EAccessModifier.Private)]
	public AudioClip placeSound;

	public Vector2i PositionOffset = new Vector2i(0, 0);

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemStack itemStack = new ItemStack(new ItemValue(0), 0);

	public bool InMenu;

	public XUiC_ItemStack.StackLocationTypes PickUpType;

	public EntityPlayerLocal entityPlayer
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return xui?.playerUI?.entityPlayer;
		}
	}

	public ItemStack CurrentStack
	{
		get
		{
			return itemStack;
		}
		set
		{
			itemStack = value;
			ItemStackControl.ItemStack = value;
			if (value != null && !value.IsEmpty())
			{
				ItemStackControl.PlayPickupSound();
			}
			if (entityPlayer != null)
			{
				entityPlayer.DragAndDropItem = value;
				RefreshBindings();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public void DropCurrentItem()
	{
		xui.PlayerInventory.DropItem(CurrentStack);
		CurrentStack = ItemStack.Empty;
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public void DropCurrentItem(int _count)
	{
		if (_count < CurrentStack.count)
		{
			ItemStack itemStack = CurrentStack.Clone();
			itemStack.count = _count;
			CurrentStack.count -= _count;
			xui.PlayerInventory.DropItem(itemStack);
			CurrentStack = this.itemStack;
		}
		else
		{
			DropCurrentItem();
		}
	}

	public override void Init()
	{
		base.Init();
		ItemStackControl = GetChildByType<XUiC_ItemStack>();
		ItemStackControl.IsDragAndDrop = true;
		ItemStackControl.ItemStack = ItemStack.Empty;
		xui.DragAndDropWindow = this;
	}

	public override void Cleanup()
	{
		base.Cleanup();
		xui.DragAndDropWindow = null;
	}

	public override void Update(float _dt)
	{
		if (!InMenu)
		{
			PlaceItemBackInInventory();
		}
		if (itemStack != null && !itemStack.IsEmpty())
		{
			base.ViewComponent.Position = xui.GetMouseXUiPosition() + PositionOffset;
		}
		RefreshBindings();
		base.Update(_dt);
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (entityPlayer != null)
		{
			ItemStack dragAndDropItem = entityPlayer.DragAndDropItem;
			if (dragAndDropItem != null && !dragAndDropItem.IsEmpty())
			{
				CurrentStack = entityPlayer.DragAndDropItem;
				PlaceItemBackInInventory();
			}
		}
	}

	public override void OnClose()
	{
		base.OnClose();
		PlaceItemBackInInventory();
	}

	public void PlaceItemBackInInventory()
	{
		if (!CurrentStack.IsEmpty())
		{
			if (xui.PlayerInventory.AddItem(itemStack))
			{
				Manager.PlayXUiSound(placeSound, 0.75f);
				CurrentStack = ItemStack.Empty;
			}
			else
			{
				xui.PlayerInventory.DropItem(itemStack);
				CurrentStack = ItemStack.Empty;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		if (_bindingName == "hasItemStack")
		{
			_value = (itemStack != null && !itemStack.IsEmpty()).ToString();
			return true;
		}
		return base.GetBindingValueInternal(ref _value, _bindingName);
	}

	public override bool ParseAttribute(string _name, string _value)
	{
		if (!(_name == "position_offset"))
		{
			if (_name == "place_sound")
			{
				xui.LoadData(_value, [PublicizedFrom(EAccessModifier.Private)] (AudioClip _o) =>
				{
					placeSound = _o;
				});
				return true;
			}
			return base.ParseAttribute(_name, _value);
		}
		PositionOffset = StringParsers.ParseVector2i(_value);
		return true;
	}
}
