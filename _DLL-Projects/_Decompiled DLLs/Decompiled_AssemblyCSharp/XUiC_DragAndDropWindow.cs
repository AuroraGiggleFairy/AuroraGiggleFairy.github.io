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

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemStack itemStack = new ItemStack(new ItemValue(0), 0);

	public bool InMenu;

	public XUiC_ItemStack.StackLocationTypes PickUpType;

	public EntityPlayerLocal entityPlayer
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return base.xui?.playerUI?.entityPlayer;
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
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public void DropCurrentItem()
	{
		base.xui.PlayerInventory.DropItem(CurrentStack);
		CurrentStack = ItemStack.Empty.Clone();
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public void DropCurrentItem(int _count)
	{
		if (_count < CurrentStack.count)
		{
			ItemStack itemStack = CurrentStack.Clone();
			itemStack.count = _count;
			CurrentStack.count -= _count;
			base.xui.PlayerInventory.DropItem(itemStack);
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
		ItemStackControl.ItemStack = ItemStack.Empty.Clone();
		base.ViewComponent.IsSnappable = false;
	}

	public override void Update(float _dt)
	{
		if (!InMenu)
		{
			PlaceItemBackInInventory();
		}
		if (itemStack != null && !itemStack.IsEmpty())
		{
			((XUiV_Window)base.ViewComponent).Panel.alpha = 1f;
			Vector2 screenPosition = base.xui.playerUI.CursorController.GetScreenPosition();
			Vector3 position = base.xui.playerUI.camera.ScreenToWorldPoint(screenPosition);
			Transform transform = base.xui.transform;
			position.z = transform.position.z - 3f * transform.lossyScale.z;
			base.ViewComponent.UiTransform.position = position;
		}
		else
		{
			((XUiV_Window)base.ViewComponent).Panel.alpha = 0f;
		}
		base.Update(_dt);
	}

	public override void OnOpen()
	{
		base.OnOpen();
		EntityPlayerLocal entityPlayerLocal = entityPlayer;
		if ((object)entityPlayerLocal != null && entityPlayerLocal.DragAndDropItem != ItemStack.Empty)
		{
			CurrentStack = entityPlayerLocal.DragAndDropItem;
			PlaceItemBackInInventory();
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
			if (base.xui.PlayerInventory.AddItem(itemStack))
			{
				Manager.PlayXUiSound(placeSound, 0.75f);
				CurrentStack = ItemStack.Empty.Clone();
			}
			else
			{
				base.xui.PlayerInventory.DropItem(itemStack);
				CurrentStack = ItemStack.Empty.Clone();
			}
		}
	}

	public override bool ParseAttribute(string name, string value, XUiController _parent)
	{
		if (name == "place_sound")
		{
			base.xui.LoadData(value, [PublicizedFrom(EAccessModifier.Private)] (AudioClip o) =>
			{
				placeSound = o;
			});
			return true;
		}
		return base.ParseAttribute(name, value, _parent);
	}
}
