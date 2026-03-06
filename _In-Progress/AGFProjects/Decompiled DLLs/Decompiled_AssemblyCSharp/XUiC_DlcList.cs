using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_DlcList : XUiC_List<XUiC_DlcList.DlcEntry>
{
	[Preserve]
	public class DlcEntry : XUiListEntry<DlcEntry>
	{
		public readonly string Name;

		public readonly string ImageResourceUri;

		public readonly EntitlementSetEnum DlcSet;

		public DlcEntry(string _nameKey, string _imageResourceUri, EntitlementSetEnum _dlcSet)
		{
			Name = Localization.Get(_nameKey);
			ImageResourceUri = _imageResourceUri;
			DlcSet = _dlcSet;
		}

		public override int CompareTo(DlcEntry _otherEntry)
		{
			if (_otherEntry == null)
			{
				return -1;
			}
			return string.Compare(Name, _otherEntry.Name, StringComparison.OrdinalIgnoreCase);
		}

		public override bool GetBindingValue(ref string _value, string _bindingName)
		{
			switch (_bindingName)
			{
			case "name":
				_value = Name ?? "";
				return true;
			case "texture_uri":
				_value = ImageResourceUri ?? "";
				return true;
			case "owned":
				_value = EntitlementManager.Instance.HasEntitlement(DlcSet).ToString();
				return true;
			default:
				return false;
			}
		}

		public override bool MatchesSearch(string _searchString)
		{
			return true;
		}

		[Preserve]
		public static bool GetNullBindingValues(ref string _value, string _bindingName)
		{
			switch (_bindingName)
			{
			case "name":
				_value = string.Empty;
				return true;
			case "texture_uri":
				_value = string.Empty;
				return true;
			case "owned":
				_value = "False";
				return true;
			default:
				return false;
			}
		}
	}

	[Preserve]
	public class DlcListEntryController : XUiC_ListEntry<DlcEntry>
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public const float hoverDuration = 0.25f;

		[PublicizedFrom(EAccessModifier.Private)]
		public const float hoverScale = 1.15f;

		[PublicizedFrom(EAccessModifier.Private)]
		public TweenScale textureScaler;

		public override void Init()
		{
			base.Init();
			if (GetChildById("dlcImage")?.ViewComponent is XUiV_Texture xUiV_Texture)
			{
				textureScaler = xUiV_Texture.UiTransform.gameObject.AddComponent<TweenScale>();
				textureScaler.enabled = false;
				textureScaler.from = Vector3.one;
				textureScaler.to = Vector3.one * 1.15f;
				textureScaler.duration = 0.25f;
				textureScaler.ResetToBeginning();
			}
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void OnHovered(bool _isOver)
		{
			base.OnHovered(_isOver);
			if (!(textureScaler == null))
			{
				if (_isOver)
				{
					textureScaler.PlayForward();
				}
				else
				{
					textureScaler.PlayReverse();
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void OnPressed(int _mouseButton)
		{
			base.OnPressed(_mouseButton);
			DlcEntry entry = GetEntry();
			if (entry != null)
			{
				EntitlementManager.Instance.OpenStore(entry.DlcSet, [PublicizedFrom(EAccessModifier.Private)] (EntitlementSetEnum _) =>
				{
					List.RebuildList();
				});
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public List<DlcEntry> dlcEntries = new List<DlcEntry>
	{
		new DlcEntry("armorClassicSurvivorSet", "Data/DlcInfo/banner_classicsurvivor_outfit", EntitlementSetEnum.ClassicSurvivorCosmetic),
		new DlcEntry("armorHolidayHatsSet", "Data/DlcInfo/banner_holidayhats", EntitlementSetEnum.ChristmasCosmetics),
		new DlcEntry("armorButcherSet", "Data/DlcInfo/banner_butcher_outfit", EntitlementSetEnum.ButcherCosmetic),
		new DlcEntry("armorHellreaverSet", "Data/DlcInfo/banner_hellreaver_outfit", EntitlementSetEnum.HellreaverCosmetic),
		new DlcEntry("armorPirateSet", "Data/DlcInfo/banner_pirate_outfit", EntitlementSetEnum.PirateCosmetic),
		new DlcEntry("armorHoarderSet", "Data/DlcInfo/banner_hoarder_outfit", EntitlementSetEnum.HoarderCosmetic),
		new DlcEntry("armorMarauderSet", "Data/DlcInfo/banner_marauder_outfit", EntitlementSetEnum.MarauderCosmetic),
		new DlcEntry("armorDesertSet", "Data/DlcInfo/banner_desert_outfit", EntitlementSetEnum.DesertCosmetic)
	};

	public override void RebuildList(bool _resetFilter = false)
	{
		allEntries.Clear();
		foreach (DlcEntry dlcEntry in dlcEntries)
		{
			if (EntitlementManager.Instance.IsAvailableOnPlatform(dlcEntry.DlcSet) && EntitlementManager.Instance.IsEntitlementPurchasable(dlcEntry.DlcSet))
			{
				allEntries.Add(dlcEntry);
			}
		}
		base.RebuildList(_resetFilter);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		if (!(_bindingName == "dlc_count"))
		{
			if (_bindingName == "paging_active")
			{
				_value = (allEntries.Count > base.PageLength).ToString();
				return true;
			}
			return base.GetBindingValueInternal(ref _value, _bindingName);
		}
		_value = allEntries.Count.ToString();
		return true;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		RebuildList();
	}

	public void PageUp()
	{
		GetChildByType<XUiC_Paging>().PageUp();
	}

	public void PageDown()
	{
		GetChildByType<XUiC_Paging>().PageDown();
	}
}
