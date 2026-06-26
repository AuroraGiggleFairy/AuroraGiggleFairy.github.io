using UnityEngine.Scripting;

[Preserve]
public class XUiC_DewCollectorModGrid : XUiC_ItemStackGrid
{
	[PublicizedFrom(EAccessModifier.Private)]
	public string requiredMods = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public bool requiredModsOnly;

	[PublicizedFrom(EAccessModifier.Private)]
	public TileEntityDewCollector tileEntity;

	public override XUiC_ItemStack.StackLocationTypes StackLocation
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return XUiC_ItemStack.StackLocationTypes.Workstation;
		}
	}

	public override void Init()
	{
		base.Init();
		string[] array = requiredMods.Split(',');
		for (int i = 0; i < itemControllers.Length; i++)
		{
			if (itemControllers[i] is XUiC_RequiredItemStack xUiC_RequiredItemStack)
			{
				if (i < array.Length)
				{
					xUiC_RequiredItemStack.RequiredItemClass = ItemClass.GetItemClass(array[i]);
					xUiC_RequiredItemStack.RequiredItemOnly = requiredModsOnly;
				}
				else
				{
					xUiC_RequiredItemStack.RequiredItemClass = null;
					xUiC_RequiredItemStack.RequiredItemOnly = false;
				}
				xUiC_RequiredItemStack.StackLocation = StackLocation;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void UpdateBackend(ItemStack[] stackList)
	{
		base.UpdateBackend(stackList);
		tileEntity.ModSlots = stackList;
		windowGroup.Controller.SetAllChildrenDirty();
	}

	public void SetTileEntity(TileEntityDewCollector te)
	{
		tileEntity = te;
		SetStacks(te.ModSlots);
	}

	public override bool ParseAttribute(string name, string value, XUiController _parent)
	{
		bool flag = base.ParseAttribute(name, value, _parent);
		if (!flag)
		{
			if (!(name == "required_mods"))
			{
				if (!(name == "required_mods_only"))
				{
					return false;
				}
				requiredModsOnly = StringParsers.ParseBool(value);
			}
			else
			{
				requiredMods = value;
			}
			return true;
		}
		return flag;
	}

	public bool TryAddMod(ItemClass newItemClass, ItemStack newItemStack)
	{
		if (!requiredModsOnly)
		{
			return false;
		}
		for (int i = 0; i < itemControllers.Length; i++)
		{
			XUiC_RequiredItemStack xUiC_RequiredItemStack = (XUiC_RequiredItemStack)itemControllers[i];
			if (xUiC_RequiredItemStack.RequiredItemClass == newItemClass && xUiC_RequiredItemStack.ItemStack.IsEmpty())
			{
				xUiC_RequiredItemStack.ItemStack = newItemStack.Clone();
				return true;
			}
		}
		return false;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		base.xui.currentDewCollectorModGrid = this;
	}

	public override void OnClose()
	{
		base.OnClose();
		base.xui.currentDewCollectorModGrid = null;
	}
}
