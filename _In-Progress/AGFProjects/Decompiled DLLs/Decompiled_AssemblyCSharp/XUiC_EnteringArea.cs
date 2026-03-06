using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_EnteringArea : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public List<XUiV_Label> labels = new List<XUiV_Label>();

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite bgSprite;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<XUiV_Sprite> sprites = new List<XUiV_Sprite>();

	[PublicizedFrom(EAccessModifier.Private)]
	public float fadePercent = 0.0001f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float fadePercentTarget;

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
	public string message;

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
	public FastTags<TagGroup.Poi> prefabIgnoreTags = FastTags<TagGroup.Poi>.Parse("part,streettile,navonly,hideui");

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterXuiRgbaColor activeColorFormatter = new CachedStringFormatterXuiRgbaColor();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterXuiRgbaColor inactiveColorFormatter = new CachedStringFormatterXuiRgbaColor();

	public override void Init()
	{
		base.Init();
		GetChildrenByViewType(labels);
		GetChildrenByViewType(sprites);
		XUiController childById = GetChildById("background");
		if (childById != null)
		{
			bgSprite = childById.ViewComponent as XUiV_Sprite;
		}
		IsDirty = true;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (!LocalPlayer)
		{
			if ((bool)base.xui && (bool)base.xui.playerUI)
			{
				LocalPlayer = base.xui.playerUI.entityPlayer;
			}
			if (!LocalPlayer)
			{
				return;
			}
		}
		if (!LocalPlayer.IsAlive() || !base.xui.playerUI.windowManager.IsHUDEnabled())
		{
			base.ViewComponent.IsVisible = false;
			fadePercent = 0f;
			message = null;
			return;
		}
		// If AreaMessage is active, always show it and skip biome/prefab logic
		if (LocalPlayer.AreaMessage != null)
		{
			showTime = LocalPlayer.AreaMessageAlpha;
			fadePercentTarget = LocalPlayer.AreaMessageAlpha;
			if (LocalPlayer.AreaMessage != message)
			{
				message = LocalPlayer.AreaMessage;
				// Force ReferenceFont for AreaMessage (plain, readable font)
				foreach (var label in labels)
				{
					label.UIFont = base.xui.GetUIFontByName("ReferenceFont", _showWarning: false);
					label.FontSize = 16;
				}
				RefreshBindings(_forceAll: true);
			}
			base.ViewComponent.IsVisible = true;
		}
		else
		{
			Prefab prefab = LocalPlayer.enteredPrefab?.prefab;
			if (prefab != null && prefab != this.prefab && prefab != lastPrefab)
			{
				LocalPlayer.enteredPrefab = null;
				if (!prefab.Tags.Test_AnySet(prefabIgnoreTags))
				{
					this.prefab = prefab;
					prefabDiff = this.prefab.DifficultyTier;
					CalcBiomeDifficulty();
					showTime = 3f;
					message = null;
					fadePercentTarget = 1f;
					RefreshBindings(_forceAll: true);
					base.ViewComponent.IsVisible = true;
				}
			}
			if (this.prefab != null && this.prefab != LocalPlayer.prefab?.prefab)
			{
				showTime = Utils.FastMin(2f, showTime);
			}
			if (this.prefab == null)
			{
				BiomeDefinition biomeStandingOn = LocalPlayer.biomeStandingOn;
				if (biomeStandingOn != null && biome != biomeStandingOn && biomeStandingOn != lastBiome)
				{
					if (ignoreFirst)
					{
						lastBiome = biomeStandingOn;
						ignoreFirst = false;
					}
					else
					{
						biome = biomeStandingOn;
						CalcBiomeDifficulty();
						showTime = 3f;
						message = null;
						fadePercentTarget = 1f;
						RefreshBindings(_forceAll: true);
						base.ViewComponent.IsVisible = true;
					}
				}
			}
		}
		showTime -= _dt;
		if (showTime <= 0f)
		{
			if (this.prefab != null)
			{
				lastPrefab = this.prefab;
			if (this.prefab == null && biome == null)
			}
			if (biome != null)
			{
					showTime = LocalPlayer.AreaMessageAlpha;
				biome = null;
			}
			prefabDiff = 0;
			biomeDiff = 0;
			showBiomeHalf = false;
			fadePercentTarget = 0f;
		}
		if (fadePercent != fadePercentTarget)
		{
			FadeUpdate();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CalcBiomeDifficulty()
	{
		float num = (float)(LocalPlayer.biomeStandingOn.Difficulty - 1) * 0.5f;
		biomeDiff = (int)Mathf.Floor(num);
		showBiomeHalf = num - (float)biomeDiff == 0.5f;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void FadeUpdate()
	{
		float num = ((fadePercentTarget > 0f) ? 2f : 0.33f);
		fadePercent = Utils.FastMoveTowards(fadePercent, fadePercentTarget, Time.deltaTime * num);
		for (int i = 0; i < labels.Count; i++)
		{
			XUiV_Label xUiV_Label = labels[i];
			Color color = xUiV_Label.Color;
			color.a = fadePercent;
			xUiV_Label.Color = color;
		}
		for (int j = 0; j < sprites.Count; j++)
		{
			XUiV_Sprite xUiV_Sprite = sprites[j];
			Color color2 = xUiV_Sprite.Color;
			if (xUiV_Sprite == bgSprite)
			{
				color2.a = Utils.FastLerp(0f, 0.5f, fadePercent);
			}
			else
			{
				color2.a = fadePercent;
			}
			xUiV_Sprite.Color = color2;
		}
		if (fadePercent == 0f)
		{
			base.ViewComponent.IsVisible = false;
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		ignoreFirst = true;
		RefreshBindings(_forceAll: true);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		switch (_bindingName)
		{
		case "locationname":
			if (prefab == null && biome == null)
			{
				_value = "";
				return true;
			}
			_value = ((prefab != null) ? prefab.LocalizedName : biome.LocalizedName);
			return true;
		case "messagename":
			if (prefab != null || biome != null || message == null)
			{
				_value = "";
				return true;
			}
			_value = message;
			return true;
		case "color1":
			_value = GetNumberedColor(1);
			return true;
		case "color2":
			_value = GetNumberedColor(2);
			return true;
		case "color3":
			_value = GetNumberedColor(3);
			return true;
		case "color4":
			_value = GetNumberedColor(4);
			return true;
		case "color5":
			_value = GetNumberedColor(5);
			return true;
		case "color6":
			_value = GetNumberedColor(6);
			return true;
		case "color7":
			_value = GetNumberedColor(7);
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
			if (LocalPlayer == null || (prefab == null && biome == null))
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

	[PublicizedFrom(EAccessModifier.Private)]
	public string GetNumberedColor(int _number)
	{
		if (prefabDiff >= _number)
		{
			return activeColorFormatter.Format(difficultyActiveColor);
		}
		if (prefabDiff + biomeDiff >= _number)
		{
			return activeColorFormatter.Format(biomeActiveColor);
		}
		return inactiveColorFormatter.Format(inactiveColor);
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
