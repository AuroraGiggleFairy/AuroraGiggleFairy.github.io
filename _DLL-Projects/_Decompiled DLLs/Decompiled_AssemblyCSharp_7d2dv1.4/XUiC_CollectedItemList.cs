using System.Collections.Generic;
using Audio;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_CollectedItemList : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public class Data
	{
		public GameObject Item;

		public float TimeAdded;

		public ItemStack ItemStack;

		public ProgressionValue CraftingSkill;

		public string uiAtlasIcon;

		public int count;

		public bool isNegative;

		public bool isMissingNotifier;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayer localPlayer;

	[PublicizedFrom(EAccessModifier.Private)]
	public float height;

	[PublicizedFrom(EAccessModifier.Private)]
	public int yOffset;

	public Transform PrefabItems;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cItemMax = 12;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<Data> items = new List<Data>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<ItemStack> removeItemQueue = new List<ItemStack>();

	public override void Init()
	{
		base.Init();
		XUiController childById = GetChildById("item");
		PrefabItems = childById.ViewComponent.UiTransform;
		height = childById.ViewComponent.Size.y + 2;
		childById.xui.CollectedItemList = this;
	}

	public void AddRemoveItemQueueEntry(ItemStack stack)
	{
		removeItemQueue.Add(stack);
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (localPlayer == null)
		{
			localPlayer = base.xui.playerUI.entityPlayer;
		}
		base.ViewComponent.IsVisible = !localPlayer.IsDead() && base.xui.playerUI.windowManager.IsHUDEnabled();
		if (removeItemQueue.Count > 0)
		{
			for (int i = 0; i < removeItemQueue.Count; i++)
			{
				RemoveItemStack(removeItemQueue[i]);
			}
			removeItemQueue.Clear();
		}
		if (items.Count <= 0)
		{
			return;
		}
		float time = Time.time;
		for (int j = 0; j < items.Count; j++)
		{
			if (time - items[j].TimeAdded > 5f)
			{
				removeLastEntry(j);
				j--;
			}
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		PrefabItems.gameObject.SetActive(value: false);
	}

	public void AddIconNotification(string iconNotifier, int count = 1, bool _bAddOnlyIfNotExisting = false)
	{
		if (iconNotifier == string.Empty || items == null || count == 0 || (_bAddOnlyIfNotExisting && items.Count > 0 && items[items.Count - 1].uiAtlasIcon.EqualsCaseInsensitive(iconNotifier)))
		{
			return;
		}
		for (int num = items.Count - 1; num >= 0; num--)
		{
			if (items[num].uiAtlasIcon != string.Empty && items[num].uiAtlasIcon.EqualsCaseInsensitive(iconNotifier))
			{
				items[num].count += count;
				Transform transform = items[num].Item.transform;
				if (transform != null)
				{
					UILabel componentInChildren = transform.GetComponentInChildren<UILabel>();
					if (componentInChildren != null)
					{
						componentInChildren.text = ((items[num].count > 1) ? ("+" + items[num].count) : "");
					}
				}
				items[num].TimeAdded = Time.time;
				return;
			}
		}
		GameObject gameObject = base.ViewComponent.UiTransform.gameObject.AddChild(PrefabItems.gameObject);
		if (!(gameObject == null))
		{
			gameObject.SetActive(value: true);
			Transform transform = gameObject.transform.Find("Negative");
			if (transform != null)
			{
				transform.gameObject.SetActive(value: false);
			}
			UILabel componentInChildren = gameObject.transform.GetComponentInChildren<UILabel>();
			if (componentInChildren == null)
			{
				componentInChildren = gameObject.transform.GetComponent<UILabel>();
			}
			if (componentInChildren != null)
			{
				componentInChildren.text = ((count > 0) ? ("+" + count) : (count.ToString() ?? ""));
			}
			UISprite component = gameObject.transform.Find("Icon").GetComponent<UISprite>();
			if (component != null)
			{
				component.atlas = base.xui.GetAtlasByName("UIAtlas", iconNotifier);
				component.spriteName = iconNotifier;
				component.color = Color.white;
			}
			gameObject.transform.localPosition = new Vector3(gameObject.transform.localPosition.x, (float)items.Count * height + (float)yOffset, gameObject.transform.localPosition.z);
			Data data = new Data();
			data.Item = gameObject;
			data.TimeAdded = Time.time;
			data.ItemStack = null;
			data.count = count;
			data.uiAtlasIcon = iconNotifier;
			items.Add(data);
			if (items.Count > 12)
			{
				removeLastEntry();
			}
		}
	}

	public void AddCraftingSkillNotification(ProgressionValue craftingSkill, bool _bAddOnlyIfNotExisting = false)
	{
		if (craftingSkill == null || items == null || (_bAddOnlyIfNotExisting && items.Count > 0 && items[items.Count - 1].CraftingSkill == craftingSkill))
		{
			return;
		}
		for (int num = items.Count - 1; num >= 0; num--)
		{
			if (items[num].CraftingSkill != null && items[num].CraftingSkill == craftingSkill)
			{
				Transform transform = items[num].Item.transform;
				if (transform != null)
				{
					UILabel componentInChildren = transform.GetComponentInChildren<UILabel>();
					if (componentInChildren != null)
					{
						componentInChildren.text = $"{items[num].CraftingSkill.Level}/{items[num].CraftingSkill.ProgressionClass.MaxLevel}";
					}
				}
				items[num].TimeAdded = Time.time;
				return;
			}
		}
		GameObject gameObject = base.ViewComponent.UiTransform.gameObject.AddChild(PrefabItems.gameObject);
		if (!(gameObject == null))
		{
			gameObject.SetActive(value: true);
			Transform transform = gameObject.transform.Find("Negative");
			if (transform != null)
			{
				transform.gameObject.SetActive(value: false);
			}
			UILabel componentInChildren = gameObject.transform.GetComponentInChildren<UILabel>();
			if (componentInChildren == null)
			{
				componentInChildren = gameObject.transform.GetComponent<UILabel>();
			}
			if (componentInChildren != null)
			{
				componentInChildren.text = $"{craftingSkill.Level}/{craftingSkill.ProgressionClass.MaxLevel}";
			}
			UISprite component = gameObject.transform.Find("Icon").GetComponent<UISprite>();
			if (component != null)
			{
				component.atlas = base.xui.GetAtlasByName("UIAtlas", craftingSkill.ProgressionClass.Icon);
				component.spriteName = craftingSkill.ProgressionClass.Icon;
				component.color = Color.white;
			}
			gameObject.transform.localPosition = new Vector3(gameObject.transform.localPosition.x, (float)items.Count * height + (float)yOffset, gameObject.transform.localPosition.z);
			Data data = new Data();
			data.Item = gameObject;
			data.TimeAdded = Time.time;
			data.ItemStack = null;
			data.count = -1;
			data.CraftingSkill = craftingSkill;
			data.uiAtlasIcon = craftingSkill.ProgressionClass.Icon;
			items.Add(data);
			if (items.Count > 12)
			{
				removeLastEntry();
			}
		}
	}

	public void AddItemStack(ItemStack _is, bool _bAddOnlyIfNotExisting = false)
	{
		if (_is == null || _is.itemValue == null || items == null || _is.itemValue.type == 0)
		{
			return;
		}
		if (_is.count == 0)
		{
			Manager.PlayInsidePlayerHead("missingitemtorepair");
		}
		if (_bAddOnlyIfNotExisting && items.Count > 0 && items[items.Count - 1].ItemStack != null && items[items.Count - 1].ItemStack.itemValue.type == _is.itemValue.type && items[items.Count - 1].ItemStack.count == _is.count)
		{
			return;
		}
		bool flag = _is.count < 0;
		for (int num = items.Count - 1; num >= 0; num--)
		{
			if (items[num].ItemStack != null && items[num].ItemStack.itemValue.type == _is.itemValue.type)
			{
				if (items[num].isNegative == flag)
				{
					if (_is.count != 0 && !items[num].isMissingNotifier)
					{
						items[num].ItemStack.count += _is.count;
						Transform transform = items[num].Item.transform;
						if (transform != null)
						{
							UILabel componentInChildren = transform.GetComponentInChildren<UILabel>();
							if (componentInChildren != null)
							{
								componentInChildren.text = string.Format("{0} ({1})", (items[num].ItemStack.count >= 1) ? ("+" + items[num].ItemStack.count) : items[num].ItemStack.count.ToString(), base.xui.PlayerInventory.GetItemCountWithMods(items[num].ItemStack.itemValue));
							}
						}
						items[num].TimeAdded = Time.time;
						return;
					}
					if (_is.count == 0 && items[num].isMissingNotifier)
					{
						items[num].TimeAdded = Time.time;
						return;
					}
				}
				else
				{
					items[num].TimeAdded = 0f;
				}
			}
		}
		GameObject gameObject = base.ViewComponent.UiTransform.gameObject.AddChild(PrefabItems.gameObject);
		if (gameObject == null)
		{
			return;
		}
		gameObject.SetActive(value: true);
		if (_is.count == 0)
		{
			Transform transform2 = gameObject.transform.Find("Negative");
			if (transform2 != null)
			{
				transform2.gameObject.SetActive(value: true);
			}
			UILabel uILabel = gameObject.transform.GetComponentInChildren<UILabel>();
			if (uILabel == null)
			{
				uILabel = gameObject.transform.GetComponent<UILabel>();
			}
			if (uILabel != null)
			{
				uILabel.text = "";
			}
		}
		else
		{
			Transform transform3 = gameObject.transform.Find("Negative");
			if (transform3 != null)
			{
				transform3.gameObject.SetActive(value: false);
			}
			UILabel uILabel2 = gameObject.transform.GetComponentInChildren<UILabel>();
			if (uILabel2 == null)
			{
				uILabel2 = gameObject.transform.GetComponent<UILabel>();
			}
			if (uILabel2 != null)
			{
				uILabel2.text = string.Format("{0} ({1})", (_is.count >= 1) ? ("+" + _is.count) : _is.count.ToString(), base.xui.PlayerInventory.GetItemCountWithMods(_is.itemValue));
			}
		}
		UISprite component = gameObject.transform.Find("Icon").GetComponent<UISprite>();
		ItemClass itemClass = _is.itemValue.ItemClass;
		if (component != null && itemClass != null)
		{
			string propertyOverride = _is.itemValue.GetPropertyOverride("CustomIcon", itemClass.GetIconName());
			propertyOverride = itemClass.GetIconName();
			component.atlas = base.xui.GetAtlasByName("itemIconAtlas", propertyOverride);
			component.spriteName = propertyOverride;
			component.color = itemClass.GetIconTint(_is.itemValue);
		}
		gameObject.transform.localPosition = new Vector3(gameObject.transform.localPosition.x, (float)items.Count * height + (float)yOffset, gameObject.transform.localPosition.z);
		Data data = new Data();
		data.Item = gameObject;
		data.TimeAdded = Time.time;
		data.ItemStack = _is;
		data.uiAtlasIcon = string.Empty;
		data.isMissingNotifier = _is.count == 0;
		data.isNegative = flag;
		items.Add(data);
		if (items.Count > 12)
		{
			removeLastEntry();
		}
	}

	public void RemoveItemStack(ItemStack _is)
	{
		if (_is == null || _is.IsEmpty())
		{
			return;
		}
		_is = _is.Clone();
		if (_is.count > 0)
		{
			_is.count *= -1;
		}
		for (int num = items.Count - 1; num >= 0; num--)
		{
			if (items[num].ItemStack != null && items[num].ItemStack.itemValue.type == _is.itemValue.type)
			{
				if (items[num].isNegative)
				{
					if (_is.count != 0 && !items[num].isMissingNotifier)
					{
						items[num].ItemStack.count += _is.count;
						Transform transform = items[num].Item.transform;
						if (transform != null)
						{
							UILabel componentInChildren = transform.GetComponentInChildren<UILabel>();
							if (componentInChildren != null)
							{
								componentInChildren.text = string.Format("{0} ({1})", (items[num].ItemStack.count >= 1) ? ("+" + items[num].ItemStack.count) : items[num].ItemStack.count.ToString(), base.xui.PlayerInventory.GetItemCountWithMods(items[num].ItemStack.itemValue));
							}
						}
						items[num].TimeAdded = Time.time;
						return;
					}
					if (_is.count == 0 && items[num].isMissingNotifier)
					{
						items[num].TimeAdded = Time.time;
						return;
					}
				}
				else
				{
					items[num].TimeAdded = 0f;
				}
			}
		}
		GameObject gameObject = base.ViewComponent.UiTransform.gameObject.AddChild(PrefabItems.gameObject);
		gameObject.SetActive(value: true);
		UILabel uILabel = gameObject.transform.GetComponentInChildren<UILabel>();
		if (uILabel == null)
		{
			uILabel = gameObject.transform.GetComponent<UILabel>();
		}
		uILabel.text = string.Format("{0} ({1})", (_is.count >= 0) ? ("-" + _is.count) : _is.count.ToString(), base.xui.PlayerInventory.GetItemCountWithMods(_is.itemValue));
		gameObject.transform.Find("Negative").gameObject.SetActive(value: false);
		UISprite component = gameObject.transform.Find("Icon").GetComponent<UISprite>();
		ItemClass itemClassOrMissing = _is.itemValue.ItemClassOrMissing;
		string propertyOverride = _is.itemValue.GetPropertyOverride("CustomIcon", (itemClassOrMissing.CustomIcon == null) ? itemClassOrMissing.GetIconName() : itemClassOrMissing.CustomIcon.Value);
		component.atlas = base.xui.GetAtlasByName(((Object)component.atlas).name, propertyOverride);
		component.spriteName = propertyOverride;
		component.color = itemClassOrMissing.GetIconTint(_is.itemValue);
		gameObject.transform.localPosition = new Vector3(gameObject.transform.localPosition.x, (float)items.Count * height + (float)yOffset, gameObject.transform.localPosition.z);
		Data data = new Data();
		data.Item = gameObject;
		data.ItemStack = _is;
		data.TimeAdded = Time.time;
		data.uiAtlasIcon = string.Empty;
		data.isNegative = true;
		items.Add(data);
		if (items.Count > 12)
		{
			removeLastEntry();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void removeLastEntry(int index = 0)
	{
		GameObject item = items[index].Item;
		if ((bool)item)
		{
			item.GetOrAddComponent<TemporaryObject>().enabled = true;
			TweenColor orAddComponent = item.GetOrAddComponent<TweenColor>();
			orAddComponent.from = Color.white;
			orAddComponent.to = new Color(1f, 1f, 1f, 0f);
			orAddComponent.enabled = true;
			orAddComponent.duration = 0.4f;
			TweenPosition component = item.GetComponent<TweenPosition>();
			if ((bool)component)
			{
				Object.Destroy(component);
			}
			component = item.AddComponent<TweenPosition>();
			component.from = item.transform.localPosition;
			component.to = new Vector3(component.from.x + 300f, component.from.y, component.from.z);
			component.enabled = true;
			component.duration = 0.4f;
		}
		items.RemoveAt(index);
		updateEntries();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateEntries()
	{
		float time = Time.time;
		for (int i = 0; i < items.Count; i++)
		{
			Data data = items[i];
			if (time - data.TimeAdded <= 5f)
			{
				GameObject item = data.Item;
				TweenPosition component = item.GetComponent<TweenPosition>();
				if ((bool)component)
				{
					Object.Destroy(component);
				}
				component = item.AddComponent<TweenPosition>();
				component.from = item.transform.localPosition;
				Vector3 to = component.from;
				to.y = (float)i * height + (float)yOffset;
				component.to = to;
				component.enabled = true;
			}
		}
	}

	public void SetYOffset(int _yOffset)
	{
		if (_yOffset != yOffset)
		{
			yOffset = _yOffset;
			updateEntries();
		}
	}
}
