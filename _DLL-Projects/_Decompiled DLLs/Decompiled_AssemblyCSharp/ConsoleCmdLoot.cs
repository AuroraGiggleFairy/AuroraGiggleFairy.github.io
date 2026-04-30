using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdLoot : ConsoleCmdAbstract
{
	public override bool IsExecuteOnClient => true;

	public override bool AllowedInMainMenu => true;

	public override int DefaultPermissionLevel => 1000;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[1] { "loot" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Loot commands";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "Loot commands:\ncontainer [name] <count> <stage> <abundance> - list loot from named container for count times";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (_params.Count == 0)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output(GetHelp());
			return;
		}
		string text = _params[0].ToLower();
		if (text == "c" || text == "container")
		{
			if (_params.Count >= 2)
			{
				if (!LootContainer.IsLoaded())
				{
					WorldStaticData.InitSync(_bForce: true, _bDediServer: false, _cleanup: false);
				}
				int result = 1;
				if (_params.Count >= 3)
				{
					int.TryParse(_params[2], out result);
				}
				int result2 = 1;
				if (_params.Count >= 4)
				{
					int.TryParse(_params[3], out result2);
				}
				float result3 = 1f;
				if (_params.Count >= 5)
				{
					float.TryParse(_params[4], out result3);
				}
				ContainerList(_params[1], result, result2, result3);
			}
		}
		else
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Unknown command " + text);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ContainerList(string _name, int _count, int _stage, float _abundance)
	{
		LootContainer lootContainer = LootContainer.GetLootContainer(_name, _errorOnMiss: false);
		if (lootContainer != null)
		{
			GameRandom gameRandom = new GameRandom();
			gameRandom.SetSeed(0);
			int num = 0;
			List<ItemStack> list = new List<ItemStack>();
			for (int i = 0; i < _count; i++)
			{
				int num2 = CountItems(list);
				int slotsLeft = 999999;
				LootContainer.SpawnLootItemsFromList(gameRandom, lootContainer.itemsToSpawn, 1, _abundance, list, ref slotsLeft, _stage, 0f, lootContainer.lootQualityTemplate, null, FastTags<TagGroup.Global>.none, uniqueItems: false, ignoreLootProb: false, _forceStacking: true, null);
				if (num2 == CountItems(list))
				{
					num++;
				}
			}
			list.Sort([PublicizedFrom(EAccessModifier.Internal)] (ItemStack a, ItemStack b) =>
			{
				int num5 = b.count.CompareTo(a.count);
				if (num5 == 0)
				{
					num5 = a.itemValue.ItemClass.Name.CompareTo(b.itemValue.ItemClass.Name);
					if (num5 == 0)
					{
						num5 = a.itemValue.Quality.CompareTo(b.itemValue.Quality);
					}
				}
				return num5;
			});
			for (int num3 = list.Count - 1; num3 > 0; num3--)
			{
				ItemStack itemStack = list[num3];
				ItemStack itemStack2 = list[num3 - 1];
				if (itemStack.itemValue.type == itemStack2.itemValue.type && itemStack.itemValue.Quality == itemStack2.itemValue.Quality)
				{
					itemStack2.count += itemStack.count;
					list.RemoveAt(num3);
				}
			}
			for (int num4 = 0; num4 < list.Count; num4++)
			{
				ItemStack itemStack3 = list[num4];
				Print("#{0} {1}, q{2}, count {3}", num4, itemStack3.itemValue.ItemClass.GetItemName(), itemStack3.itemValue.Quality, itemStack3.count);
			}
			Print("Loot Container {0}, unique items {1}, empties {2}", lootContainer.Name, list.Count, num);
		}
		else
		{
			Print("Unknown container " + _name);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int CountItems(List<ItemStack> _list)
	{
		int num = 0;
		for (int i = 0; i < _list.Count; i++)
		{
			ItemStack itemStack = _list[i];
			num += itemStack.count;
		}
		return num;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Print(string _s, params object[] _values)
	{
		string line = string.Format(_s, _values);
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output(line);
	}
}
