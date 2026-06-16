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
	}

	[Preserve]
	[PublicizedFrom(EAccessModifier.Protected)]
	public class DlcListEntryController : XUiC_ListEntry
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public const float HoverDuration = 0.25f;

		[PublicizedFrom(EAccessModifier.Private)]
		public const float HoverScale = 1.15f;

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

		[XuiXmlBinding("name")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingName()
		{
			return entryData?.Name ?? "";
		}

		[XuiXmlBinding("texture_uri")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingTextureUri()
		{
			return entryData?.ImageResourceUri ?? "";
		}

		[XuiXmlBinding("owned")]
		[PublicizedFrom(EAccessModifier.Private)]
		public bool bindingOwned()
		{
			if (entryData != null)
			{
				return EntitlementManager.Instance.HasEntitlement(entryData.DlcSet);
			}
			return false;
		}
	}

	public static readonly List<DlcEntry> DlcEntries = new List<DlcEntry>
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

	[XuiXmlBinding("dlc_count")]
	public int DlcCount
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return allEntries.Count;
		}
	}

	[XuiXmlBinding("paging_active")]
	public bool PagingRequired
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return allEntries.Count > base.PageLength;
		}
	}

	public override void RebuildList(bool _resetFilter = false)
	{
		allEntries.Clear();
		foreach (DlcEntry dlcEntry in DlcEntries)
		{
			if (EntitlementManager.Instance.IsAvailableOnPlatform(dlcEntry.DlcSet) && EntitlementManager.Instance.IsEntitlementPurchasable(dlcEntry.DlcSet))
			{
				allEntries.Add(dlcEntry);
			}
		}
		base.RebuildList(_resetFilter);
	}

	public override void OnOpen()
	{
		base.OnOpen();
		RebuildList();
	}
}
