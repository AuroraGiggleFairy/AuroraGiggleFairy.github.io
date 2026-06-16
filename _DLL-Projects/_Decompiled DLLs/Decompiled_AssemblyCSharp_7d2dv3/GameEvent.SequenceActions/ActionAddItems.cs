using UnityEngine;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionAddItems : ActionBaseClientAction
{
	public string[] AddItems;

	public string[] AddItemCounts;

	public static string PropAddItems = "added_items";

	public static string PropAddItemCounts = "added_item_counts";

	public override void OnClientPerform(Entity target)
	{
		if (!(target is EntityPlayerLocal entityPlayerLocal))
		{
			return;
		}
		for (int i = 0; i < AddItems.Length; i++)
		{
			string value = ((AddItemCounts != null && AddItemCounts.Length > i) ? AddItemCounts[i] : "1");
			int defaultValue = 1;
			ItemClass itemClass = ItemClass.GetItemClass(AddItems[i]);
			ItemValue itemValue = null;
			defaultValue = GameEventManager.GetIntValue(entityPlayerLocal, value, defaultValue);
			if (itemClass.HasQuality)
			{
				itemValue = new ItemValue(itemClass.Id, defaultValue, defaultValue);
				defaultValue = 1;
			}
			else
			{
				itemValue = new ItemValue(itemClass.Id);
			}
			ItemStack itemStack = new ItemStack(itemValue, defaultValue);
			if (!LocalPlayerUI.GetUIForPlayer(entityPlayerLocal).xui.PlayerInventory.AddItem(itemStack))
			{
				GameManager.Instance.ItemDropServer(itemStack, entityPlayerLocal.GetPosition(), Vector3.zero);
			}
		}
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		if (properties.Values.ContainsKey(PropAddItems))
		{
			AddItems = properties.Values[PropAddItems].Replace(" ", "").Split(',');
			if (properties.Values.ContainsKey(PropAddItemCounts))
			{
				AddItemCounts = properties.Values[PropAddItemCounts].Replace(" ", "").Split(',');
			}
			else
			{
				AddItemCounts = null;
			}
		}
		else
		{
			AddItems = null;
			AddItemCounts = null;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseAction CloneChildSettings()
	{
		return new ActionAddItems
		{
			AddItems = AddItems,
			AddItemCounts = AddItemCounts,
			targetGroup = targetGroup
		};
	}
}
