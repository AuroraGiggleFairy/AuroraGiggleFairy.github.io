using System;
using System.Collections.Generic;
using System.IO;

public class RecipeQueueItem
{
	public short Multiplier;

	public float CraftingTimeLeft;

	public float OneItemCraftTime = -1f;

	public bool IsCrafting;

	public ItemValue RepairItem;

	public ushort AmountToRepair;

	public byte Quality;

	public int StartingEntityId = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public int recipeHashCode;

	[PublicizedFrom(EAccessModifier.Private)]
	public Recipe cachedRecipe;

	public Recipe Recipe
	{
		get
		{
			if (cachedRecipe == null)
			{
				return cachedRecipe = CraftingManager.GetRecipe(recipeHashCode);
			}
			return cachedRecipe;
		}
		set
		{
			cachedRecipe = value;
			recipeHashCode = ((cachedRecipe != null) ? cachedRecipe.GetHashCode() : 0);
		}
	}

	public void Write(BinaryWriter _bw, uint version)
	{
		bool flag = Recipe != null;
		_bw.Write(flag ? Recipe.GetHashCode() : 0);
		_bw.Write(Multiplier);
		_bw.Write(IsCrafting);
		_bw.Write(CraftingTimeLeft);
		bool flag2 = RepairItem != null;
		_bw.Write(flag2);
		if (flag2)
		{
			RepairItem.Write(_bw);
			_bw.Write(AmountToRepair);
		}
		_bw.Write(Quality);
		_bw.Write(StartingEntityId);
		_bw.Write(OneItemCraftTime);
		_bw.Write(Recipe != null && Recipe.scrapable);
		if (Recipe != null && Recipe.scrapable)
		{
			_bw.Write(Recipe.itemValueType);
			_bw.Write(Recipe.count);
			_bw.Write(Recipe.ingredients.Count);
			for (int i = 0; i < Recipe.ingredients.Count; i++)
			{
				Recipe.ingredients[i].Write(_bw);
			}
			_bw.Write(Recipe.craftingTime);
			_bw.Write(Recipe.craftExpGain);
			_bw.Write(Recipe.IsScrap);
		}
		if (flag)
		{
			ItemClass outputItemClass = Recipe.GetOutputItemClass();
			(outputItemClass.IsBlock() ? Block.nameIdMapping : ItemClass.nameIdMapping)?.AddMapping(outputItemClass.Id, outputItemClass.Name);
		}
	}

	public void Read(BinaryReader _br, uint version)
	{
		recipeHashCode = _br.ReadInt32();
		cachedRecipe = CraftingManager.GetRecipe(recipeHashCode);
		Multiplier = _br.ReadInt16();
		IsCrafting = _br.ReadBoolean();
		CraftingTimeLeft = _br.ReadSingle();
		if (_br.ReadBoolean())
		{
			if (version > 39)
			{
				RepairItem = ItemValue.ReadOrNull(_br);
			}
			else
			{
				RepairItem = new ItemValue(_br.ReadInt32());
			}
			AmountToRepair = _br.ReadUInt16();
		}
		if (version != 0)
		{
			Quality = _br.ReadByte();
			StartingEntityId = _br.ReadInt32();
		}
		if (version > 41)
		{
			OneItemCraftTime = _br.ReadSingle();
		}
		if (version > 43 && _br.ReadBoolean())
		{
			cachedRecipe = new Recipe();
			cachedRecipe.itemValueType = _br.ReadInt32();
			cachedRecipe.count = _br.ReadInt32();
			cachedRecipe.scrapable = true;
			int num = _br.ReadInt32();
			Recipe.ingredients = new List<ItemStack>();
			for (int i = 0; i < num; i++)
			{
				Recipe.ingredients.Add(new ItemStack().Read(_br));
			}
			cachedRecipe.craftingTime = _br.ReadSingle();
			cachedRecipe.craftExpGain = _br.ReadInt32();
			if (version > 46)
			{
				cachedRecipe.IsScrap = _br.ReadBoolean();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public void WriteDelta(BinaryWriter _bw, RecipeQueueItem _last)
	{
		_bw.Write((Recipe != null) ? Recipe.GetHashCode() : 0);
		if (Multiplier < 0)
		{
			Log.Error("Multiplier is less than 0!");
			Log.Out(Environment.StackTrace);
			Multiplier = 0;
		}
		_bw.Write(CraftingTimeLeft - _last.CraftingTimeLeft);
		_last.CraftingTimeLeft += CraftingTimeLeft - _last.CraftingTimeLeft;
		_bw.Write((short)(Multiplier - _last.Multiplier));
		_last.Multiplier += (short)(Multiplier - _last.Multiplier);
		_bw.Write(IsCrafting);
		bool flag = RepairItem != null;
		_bw.Write(flag);
		if (flag)
		{
			RepairItem.Write(_bw);
			_bw.Write(AmountToRepair);
		}
		_bw.Write(Quality);
		_bw.Write(StartingEntityId);
		_bw.Write(OneItemCraftTime);
		_bw.Write(Recipe != null && Recipe.scrapable);
		if (Recipe != null && Recipe.scrapable)
		{
			_bw.Write(Recipe.itemValueType);
			_bw.Write(Recipe.count);
			_bw.Write(Recipe.scrapable);
			_bw.Write(Recipe.ingredients.Count);
			for (int i = 0; i < Recipe.ingredients.Count; i++)
			{
				Recipe.ingredients[i].Write(_bw);
			}
			_bw.Write(Recipe.craftingTime);
			_bw.Write(Recipe.craftExpGain);
			_bw.Write(Recipe.IsScrap);
		}
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public void ReadDelta(BinaryReader _br, RecipeQueueItem _last)
	{
		recipeHashCode = _br.ReadInt32();
		cachedRecipe = CraftingManager.GetRecipe(recipeHashCode);
		float num = _br.ReadSingle();
		CraftingTimeLeft = _last.CraftingTimeLeft + num;
		int num2 = _br.ReadInt16();
		Multiplier = (short)(_last.Multiplier + num2);
		IsCrafting = _br.ReadBoolean();
		if (_br.ReadBoolean())
		{
			RepairItem = ItemValue.ReadOrNull(_br);
			AmountToRepair = _br.ReadUInt16();
		}
		Quality = _br.ReadByte();
		StartingEntityId = _br.ReadInt32();
		OneItemCraftTime = _br.ReadSingle();
		if (_br.ReadBoolean())
		{
			cachedRecipe = new Recipe();
			cachedRecipe.itemValueType = _br.ReadInt32();
			cachedRecipe.count = _br.ReadInt32();
			cachedRecipe.scrapable = true;
			int num3 = _br.ReadInt32();
			Recipe.ingredients = new List<ItemStack>();
			for (int i = 0; i < num3; i++)
			{
				Recipe.ingredients.Add(new ItemStack().Read(_br));
			}
			cachedRecipe.craftingTime = _br.ReadSingle();
			cachedRecipe.craftExpGain = _br.ReadInt32();
			cachedRecipe.IsScrap = _br.ReadBoolean();
		}
	}
}
