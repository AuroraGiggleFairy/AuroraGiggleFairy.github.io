using System.IO;

public class InventoryOperation
{
	public enum EnumOperation
	{
		SetAbsolute,
		SetRelative,
		SetAll
	}

	public EnumOperation Operation;

	public ItemStack Stack;

	public int Index;

	public ItemStack[] NewStacks;

	public static InventoryOperation CreateSetAbsolute(ItemStack _stack, int _index)
	{
		return new InventoryOperation
		{
			Operation = EnumOperation.SetAbsolute,
			Stack = _stack.Clone(),
			Index = _index
		};
	}

	public static InventoryOperation CreateSetRelative(ItemStack _stack, int _index)
	{
		return new InventoryOperation
		{
			Operation = EnumOperation.SetRelative,
			Stack = _stack.Clone(),
			Index = _index
		};
	}

	public static InventoryOperation CreateSetAll(ItemStack[] _newStacks)
	{
		return new InventoryOperation
		{
			Operation = EnumOperation.SetAll,
			NewStacks = ItemStack.Clone(_newStacks)
		};
	}

	public static InventoryOperation Read(BinaryReader _br)
	{
		switch ((EnumOperation)_br.ReadInt16())
		{
		case EnumOperation.SetAbsolute:
		{
			ItemStack stack2 = ItemStack.Empty.Read(_br);
			int index2 = _br.ReadInt32();
			return CreateSetAbsolute(stack2, index2);
		}
		case EnumOperation.SetRelative:
		{
			ItemStack stack = ItemStack.Empty.Read(_br);
			int index = _br.ReadInt32();
			return CreateSetRelative(stack, index);
		}
		case EnumOperation.SetAll:
			return CreateSetAll(ItemStack.ReadArray(_br));
		default:
			return null;
		}
	}

	public void Write(BinaryWriter _bw)
	{
		_bw.Write((short)Operation);
		switch (Operation)
		{
		case EnumOperation.SetAbsolute:
		case EnumOperation.SetRelative:
			Stack.Write(_bw);
			_bw.Write(Index);
			break;
		case EnumOperation.SetAll:
			ItemStack.WriteArray(_bw, NewStacks);
			break;
		}
	}
}
