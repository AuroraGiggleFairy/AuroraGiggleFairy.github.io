using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_Location : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public Prefab lastPrefab;

	[PublicizedFrom(EAccessModifier.Private)]
	public PrefabInstance lastPrefabInstance;

	[PublicizedFrom(EAccessModifier.Private)]
	public BiomeDefinition lastBiome;

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayerLocal LocalPlayer;

	[PublicizedFrom(EAccessModifier.Private)]
	public int prefabDiff;

	[PublicizedFrom(EAccessModifier.Private)]
	public int biomeDiff;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool showBiomeHalf;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color difficultyActiveColor = Color.red;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color biomeActiveColor = new Color(1f, 0.5f, 0f);

	[PublicizedFrom(EAccessModifier.Private)]
	public Color inactiveColor = Color.grey;

	[PublicizedFrom(EAccessModifier.Private)]
	public FastTags<TagGroup.Poi> partTag = FastTags<TagGroup.Poi>.Parse("part");

	[PublicizedFrom(EAccessModifier.Private)]
	public FastTags<TagGroup.Poi> streetTileTag = FastTags<TagGroup.Poi>.Parse("streettile");

	[PublicizedFrom(EAccessModifier.Private)]
	public FastTags<TagGroup.Poi> navOnlyTileTag = FastTags<TagGroup.Poi>.Parse("navonly");

	[PublicizedFrom(EAccessModifier.Private)]
	public FastTags<TagGroup.Poi> hideUITag = FastTags<TagGroup.Poi>.Parse("hideui");

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterXuiRgbaColor activeColorFormatter = new CachedStringFormatterXuiRgbaColor();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterXuiRgbaColor inactiveColorFormatter = new CachedStringFormatterXuiRgbaColor();

	public override void Init()
	{
		base.Init();
		IsDirty = true;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (LocalPlayer == null && base.xui != null && base.xui.playerUI != null && base.xui.playerUI.entityPlayer != null)
		{
			LocalPlayer = base.xui.playerUI.entityPlayer;
		}
		if (LocalPlayer == null)
		{
			return;
		}
		GUIWindowManager windowManager = base.xui.playerUI.windowManager;
		if (windowManager.IsHUDEnabled() || (base.xui.dragAndDrop.InMenu && windowManager.IsHUDPartialHidden()))
		{
			if (base.ViewComponent.IsVisible && LocalPlayer.IsDead())
			{
				base.ViewComponent.IsVisible = false;
			}
			else if (!base.ViewComponent.IsVisible && !LocalPlayer.IsDead())
			{
				base.ViewComponent.IsVisible = true;
			}
		}
		else
		{
			base.ViewComponent.IsVisible = false;
		}
		if (!LocalPlayer.IsAlive())
		{
			base.ViewComponent.IsVisible = false;
			return;
		}
		PrefabInstance prefab = LocalPlayer.prefab;
		if (prefab == lastPrefabInstance && LocalPlayer.biomeStandingOn == lastBiome)
		{
			return;
		}
		if (prefab != null && prefab.IsWithinInfoArea(LocalPlayer.position))
		{
			if (prefab.prefab.Tags.Test_AnySet(partTag) || prefab.prefab.Tags.Test_AnySet(streetTileTag) || prefab.prefab.Tags.Test_AnySet(navOnlyTileTag) || prefab.prefab.Tags.Test_AnySet(hideUITag))
			{
				lastPrefabInstance = null;
			}
			else
			{
				lastPrefabInstance = prefab;
			}
		}
		else
		{
			lastPrefabInstance = null;
		}
		lastPrefab = ((lastPrefabInstance != null) ? lastPrefabInstance.prefab : null);
		if (lastPrefab != null)
		{
			prefabDiff = lastPrefab.DifficultyTier;
		}
		else
		{
			prefabDiff = 0;
		}
		lastBiome = LocalPlayer.biomeStandingOn;
		if (lastBiome != null)
		{
			float num = (float)(lastBiome.Difficulty - 1) * 0.5f;
			biomeDiff = (int)Mathf.Floor(num);
			showBiomeHalf = num - (float)biomeDiff == 0.5f;
		}
		else
		{
			biomeDiff = 0;
			showBiomeHalf = false;
		}
		RefreshBindings(_forceAll: true);
	}

	public override void OnOpen()
	{
		base.OnOpen();
		RefreshBindings(_forceAll: true);
	}

	public override void OnClose()
	{
		base.OnClose();
		RefreshBindings(_forceAll: true);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		switch (_bindingName)
		{
		case "locationname":
			if (lastPrefab != null)
			{
				_value = lastPrefab.LocalizedName;
			}
			else if (lastBiome != null)
			{
				_value = lastBiome.LocalizedName;
			}
			else
			{
				_value = "";
			}
			return true;
		case "difficultycolor1":
			_value = inactiveColorFormatter.Format(inactiveColor);
			if (prefabDiff >= 1)
			{
				_value = activeColorFormatter.Format(difficultyActiveColor);
			}
			else if (prefabDiff + biomeDiff >= 1)
			{
				_value = activeColorFormatter.Format(biomeActiveColor);
			}
			return true;
		case "difficultycolor2":
			_value = inactiveColorFormatter.Format(inactiveColor);
			if (prefabDiff >= 2)
			{
				_value = activeColorFormatter.Format(difficultyActiveColor);
			}
			else if (prefabDiff + biomeDiff >= 2)
			{
				_value = activeColorFormatter.Format(biomeActiveColor);
			}
			return true;
		case "difficultycolor3":
			_value = inactiveColorFormatter.Format(inactiveColor);
			if (prefabDiff >= 3)
			{
				_value = activeColorFormatter.Format(difficultyActiveColor);
			}
			else if (prefabDiff + biomeDiff >= 3)
			{
				_value = activeColorFormatter.Format(biomeActiveColor);
			}
			return true;
		case "difficultycolor4":
			_value = inactiveColorFormatter.Format(inactiveColor);
			if (prefabDiff >= 4)
			{
				_value = activeColorFormatter.Format(difficultyActiveColor);
			}
			else if (prefabDiff + biomeDiff >= 4)
			{
				_value = activeColorFormatter.Format(biomeActiveColor);
			}
			return true;
		case "difficultycolor5":
			_value = inactiveColorFormatter.Format(inactiveColor);
			if (prefabDiff >= 5)
			{
				_value = activeColorFormatter.Format(difficultyActiveColor);
			}
			else if (prefabDiff + biomeDiff >= 5)
			{
				_value = activeColorFormatter.Format(biomeActiveColor);
			}
			return true;
		case "difficultycolor6":
			_value = inactiveColorFormatter.Format(inactiveColor);
			if (prefabDiff >= 6)
			{
				_value = activeColorFormatter.Format(difficultyActiveColor);
			}
			else if (prefabDiff + biomeDiff >= 6)
			{
				_value = activeColorFormatter.Format(biomeActiveColor);
			}
			return true;
		case "difficultycolor7":
			_value = inactiveColorFormatter.Format(inactiveColor);
			if (prefabDiff >= 7)
			{
				_value = activeColorFormatter.Format(difficultyActiveColor);
			}
			else if (prefabDiff + biomeDiff >= 7)
			{
				_value = activeColorFormatter.Format(biomeActiveColor);
			}
			return true;
		case "visible1":
			_value = (prefabDiff + biomeDiff >= 1).ToString();
			return true;
		case "visible2":
			_value = (prefabDiff + biomeDiff >= 2).ToString();
			return true;
		case "visible3":
			_value = (prefabDiff + biomeDiff >= 3).ToString();
			return true;
		case "visible4":
			_value = (prefabDiff + biomeDiff >= 4).ToString();
			return true;
		case "visible5":
			_value = (prefabDiff + biomeDiff >= 5).ToString();
			return true;
		case "visible6":
			_value = (prefabDiff + biomeDiff >= 6).ToString();
			return true;
		case "visible7":
			_value = (prefabDiff + biomeDiff >= 7).ToString();
			return true;
		case "visible_half":
			_value = showBiomeHalf.ToString();
			return true;
		case "visible_loot_max":
			if (LocalPlayer == null)
			{
				_value = "false";
				return true;
			}
			_value = LocalPlayer.LootAtMax.ToString();
			return true;
		default:
			return false;
		}
	}

	public override bool ParseAttribute(string name, string value, XUiController _parent)
	{
		bool flag = base.ParseAttribute(name, value, _parent);
		if (!flag)
		{
			if (!(name == "active_color"))
			{
				if (!(name == "inactive_color"))
				{
					return false;
				}
				inactiveColor = StringParsers.ParseColor32(value);
			}
			else
			{
				difficultyActiveColor = StringParsers.ParseColor32(value);
			}
			return true;
		}
		return flag;
	}
}
