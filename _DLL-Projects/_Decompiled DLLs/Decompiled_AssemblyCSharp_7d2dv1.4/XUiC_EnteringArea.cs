using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_EnteringArea : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public float showTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public Prefab lastPrefab;

	[PublicizedFrom(EAccessModifier.Private)]
	public Prefab prefab;

	[PublicizedFrom(EAccessModifier.Private)]
	public BiomeDefinition lastBiome;

	[PublicizedFrom(EAccessModifier.Private)]
	public BiomeDefinition biome;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool ignoreFirst;

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

	public Prefab Prefab
	{
		get
		{
			return prefab;
		}
		set
		{
			prefab = value;
			if (prefab != null)
			{
				showTime = 5f;
			}
		}
	}

	public BiomeDefinition Biome
	{
		get
		{
			return biome;
		}
		set
		{
			biome = value;
			if (biome != null)
			{
				showTime = 5f;
			}
		}
	}

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
		if (!LocalPlayer.IsAlive())
		{
			base.ViewComponent.IsVisible = false;
			return;
		}
		if (!base.xui.playerUI.windowManager.IsHUDEnabled())
		{
			base.ViewComponent.IsVisible = false;
			return;
		}
		if (LocalPlayer.enteredPrefab != null && prefab != LocalPlayer.enteredPrefab.prefab && LocalPlayer.enteredPrefab.prefab != lastPrefab)
		{
			Prefab = LocalPlayer.enteredPrefab.prefab;
			LocalPlayer.enteredPrefab = null;
			if (Prefab != null)
			{
				if (prefab.Tags.Test_AnySet(partTag) || prefab.Tags.Test_AnySet(streetTileTag) || prefab.Tags.Test_AnySet(navOnlyTileTag) || prefab.Tags.Test_AnySet(hideUITag))
				{
					prefabDiff = 0;
					Prefab = null;
				}
				else
				{
					prefabDiff = Prefab.DifficultyTier;
					HandleBiomeDifficulty(LocalPlayer.biomeStandingOn);
				}
			}
			else
			{
				prefabDiff = 0;
			}
			RefreshBindings(_forceAll: true);
			return;
		}
		BiomeDefinition biomeStandingOn = LocalPlayer.biomeStandingOn;
		if (LocalPlayer.prefab == null && biomeStandingOn != null && biome != biomeStandingOn && biomeStandingOn != lastBiome)
		{
			if (ignoreFirst)
			{
				lastBiome = biomeStandingOn;
				ignoreFirst = false;
				return;
			}
			Biome = biomeStandingOn;
			HandleBiomeDifficulty(Biome);
			Prefab = null;
			prefabDiff = 0;
			RefreshBindings(_forceAll: true);
		}
		else
		{
			if (prefab == null && biome == null)
			{
				return;
			}
			if (LocalPlayer.prefab != null && prefab != LocalPlayer.prefab.prefab)
			{
				showTime = 0f;
			}
			showTime -= _dt;
			if (showTime <= 0f)
			{
				if (prefab != null)
				{
					lastPrefab = prefab;
					prefab = null;
				}
				if (biome != null)
				{
					lastBiome = biome;
					biome = null;
				}
				RefreshBindings(_forceAll: true);
			}
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		ignoreFirst = true;
		RefreshBindings(_forceAll: true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleBiomeDifficulty(BiomeDefinition biome)
	{
		if (biome != null)
		{
			float num = (float)(biome.Difficulty - 1) * 0.5f;
			biomeDiff = (int)Mathf.Floor(num);
			showBiomeHalf = num - (float)biomeDiff == 0.5f;
		}
		else
		{
			biomeDiff = 0;
			showBiomeHalf = false;
		}
	}

	public override bool GetBindingValue(ref string _value, string _bindingName)
	{
		switch (_bindingName)
		{
		case "locationname":
			if (Prefab == null && Biome == null)
			{
				_value = "";
				return true;
			}
			_value = ((Prefab != null) ? Prefab.LocalizedName : Biome.LocalizedName);
			return true;
		case "color1":
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
		case "color2":
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
		case "color3":
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
		case "color4":
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
		case "color5":
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
		case "color6":
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
		case "color7":
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
		case "visible":
			if (LocalPlayer == null)
			{
				_value = "false";
				return true;
			}
			if (!LocalPlayer.IsAlive())
			{
				_value = "false";
				return true;
			}
			if (Prefab == null && Biome == null)
			{
				_value = "false";
				return true;
			}
			_value = "true";
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
