using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEquippedSlots : MonoBehaviour
{
	[Serializable]
	public class PartInfo
	{
		public string name;

		public string slot;

		public string rule;

		public bool IsInSlot(string slotReference)
		{
			return RefMatchesSlot(slotReference, slot);
		}

		public static bool RefMatchesSlot(string slotReference, string slotName)
		{
			int num = slotReference.IndexOf("*");
			if (num == -1)
			{
				return slotReference.Equals(slotName);
			}
			return string.Compare(slotReference, 0, slotName, 0, num) == 0;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public class EquippedPart
	{
		public string name;

		public PartInfo partInfo;

		public bool wasShowing;

		public bool isShowing;
	}

	public List<PartInfo> parts;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public List<EquippedPart> equippedParts = new List<EquippedPart>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Transform outfitXF;

	public void Init(Transform outfit)
	{
		outfitXF = outfit;
		if (outfitXF != null)
		{
			_DisableAllNASubmeshes();
		}
	}

	public void ListParts()
	{
		int count = parts.Count;
		for (int i = 0; i < count; i++)
		{
			Log.Warning(parts[i].name);
		}
	}

	public void ListEquipment()
	{
		int count = equippedParts.Count;
		for (int i = 0; i < count; i++)
		{
			Log.Warning(equippedParts[i].name);
		}
	}

	public bool IsEquipped(string partName)
	{
		int count = equippedParts.Count;
		for (int i = 0; i < count; i++)
		{
			if (equippedParts[i].name == partName)
			{
				return true;
			}
		}
		return false;
	}

	public bool Equip(string partName)
	{
		if (IsEquipped(partName))
		{
			return false;
		}
		int count = parts.Count;
		for (int i = 0; i < count; i++)
		{
			PartInfo partInfo = parts[i];
			if (partInfo.name == partName)
			{
				EquippedPart equippedPart = new EquippedPart();
				equippedPart.name = partName;
				equippedPart.partInfo = partInfo;
				equippedParts.Add(equippedPart);
				_RunRules();
				return true;
			}
		}
		Log.Warning("Part '{0}' not equipped.", partName);
		return false;
	}

	public bool UnEquip(string partName)
	{
		int count = equippedParts.Count;
		for (int i = 0; i < count; i++)
		{
			if (equippedParts[i].name == partName)
			{
				_EnableNASubmesh(partName, enable: false);
				equippedParts.RemoveAt(i);
				_RunRules();
				return true;
			}
		}
		Log.Warning("Part '{0}' not unequipped.", partName);
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void _RunRules()
	{
		int count = equippedParts.Count;
		for (int i = 0; i < count; i++)
		{
			EquippedPart equippedPart = equippedParts[i];
			equippedPart.wasShowing = equippedPart.isShowing;
			equippedPart.isShowing = true;
		}
		for (int j = 0; j < count; j++)
		{
			EquippedPart equippedPart2 = equippedParts[j];
			if (!equippedPart2.isShowing)
			{
				continue;
			}
			PartInfo partInfo = equippedPart2.partInfo;
			if (string.IsNullOrEmpty(partInfo.rule))
			{
				continue;
			}
			string rule = partInfo.rule;
			for (int k = 0; k < count; k++)
			{
				if (k == j)
				{
					continue;
				}
				EquippedPart equippedPart3 = equippedParts[k];
				if (equippedPart3.isShowing)
				{
					PartInfo partInfo2 = equippedPart3.partInfo;
					if (partInfo2.IsInSlot(rule))
					{
						Log.Warning(" Note: Part {0} hides part {1} with rule {2}.", partInfo.name, partInfo2.name, rule);
						equippedPart3.isShowing = false;
					}
				}
			}
		}
		for (int l = 0; l < count; l++)
		{
			EquippedPart equippedPart4 = equippedParts[l];
			if (equippedPart4.isShowing && !equippedPart4.wasShowing)
			{
				_EnableNASubmesh(equippedPart4.partInfo.name, enable: true);
			}
			else if (!equippedPart4.isShowing && equippedPart4.wasShowing)
			{
				_EnableNASubmesh(equippedPart4.partInfo.name, enable: false);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Transform _GetNAOutfit()
	{
		return outfitXF;
	}

	public void _EnableNASubmesh(string submeshName, bool enable)
	{
		Transform transform = _GetNAOutfit();
		if (transform == null)
		{
			return;
		}
		int childCount = transform.childCount;
		for (int i = 0; i < childCount; i++)
		{
			Transform child = transform.GetChild(i);
			if (!(child.name == "Origin"))
			{
				GameObject gameObject = child.gameObject;
				if (gameObject.name == submeshName)
				{
					gameObject.SetActive(enable);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void _DisableAllNASubmeshes()
	{
		Transform transform = _GetNAOutfit();
		if (transform == null)
		{
			return;
		}
		int childCount = transform.childCount;
		for (int i = 0; i < childCount; i++)
		{
			Transform child = transform.GetChild(i);
			if (!(child.name == "Origin"))
			{
				child.gameObject.SetActive(value: false);
			}
		}
	}
}
