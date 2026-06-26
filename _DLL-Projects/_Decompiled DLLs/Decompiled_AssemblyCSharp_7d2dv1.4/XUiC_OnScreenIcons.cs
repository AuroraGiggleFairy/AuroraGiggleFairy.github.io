using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_OnScreenIcons : XUiController
{
	public class OnScreenIcon
	{
		public XUiC_OnScreenIcons Owner;

		public NavObject NavObject;

		public UISprite Sprite;

		public UISprite FillSprite;

		public UISprite SubSprite;

		public UILabel Label;

		public Transform Transform;

		public bool ReadyForUnload;

		public NavObjectScreenSettings ScreenSettings;

		public void Init()
		{
			if (Label != null)
			{
				ScreenSettings = NavObject.CurrentScreenSettings;
				Label.transform.localPosition = new Vector2(0f, 0f - (ScreenSettings.SpriteSize * 0.5f + 8f));
			}
		}

		public void Update(Vector3 offset, Vector3 playerPosition, Vector3 cameraForward, XUi xui, ref int depth)
		{
			if (!NavObject.IsValid())
			{
				return;
			}
			UISprite sprite = Sprite;
			bool enabled = (Label.enabled = !NavObject.hiddenOnCompass);
			sprite.enabled = enabled;
			depth = UpdateDepth(depth) + 1;
			EntityPlayerLocal entityPlayer = xui.playerUI.entityPlayer;
			if (!NavObject.HasRequirements)
			{
				Transform.localPosition = NavObject.InvalidPos;
				return;
			}
			if (entityPlayer.IsDead())
			{
				Transform.localPosition = NavObject.InvalidPos;
				return;
			}
			ScreenSettings = NavObject.CurrentScreenSettings;
			if (ScreenSettings == null)
			{
				Transform.localPosition = NavObject.InvalidPos;
				return;
			}
			Vector3 vector = NavObject.GetPosition() + ScreenSettings.Offset;
			if (ScreenSettings.UseHeadOffset && NavObject.TrackType == NavObject.TrackTypes.Entity)
			{
				vector += new Vector3(0f, NavObject.TrackedEntity.GetEyeHeight(), 0f) + NavObject.TrackedEntity.EntityClass.NavObjectHeadOffset;
			}
			string spriteName = NavObject.GetSpriteName(ScreenSettings);
			Color color = (NavObject.UseOverrideColor ? NavObject.OverrideColor : ScreenSettings.Color);
			float num = 1f;
			if (Sprite.spriteName != spriteName)
			{
				Sprite.atlas = xui.GetAtlasByName("UIAtlas", spriteName);
				Sprite.spriteName = spriteName;
			}
			Sprite.color = color;
			float num2 = Vector3.Distance(NavObject.GetPosition(), entityPlayer.position - Origin.position);
			float maxDistance = NavObject.GetMaxDistance(ScreenSettings, entityPlayer);
			if (maxDistance != -1f && num2 > maxDistance)
			{
				Transform.localPosition = NavObject.InvalidPos;
				return;
			}
			if (ScreenSettings.MinDistance > 0f && num2 < ScreenSettings.MinDistance)
			{
				Transform.localPosition = NavObject.InvalidPos;
				return;
			}
			Vector3 lhs = vector - Owner.Camera.transform.position;
			lhs.Normalize();
			Vector3 screenSpaceVector = entityPlayer.finalCamera.WorldToScreenPoint(vector);
			if (ScreenSettings.ShowOffScreen)
			{
				if (screenSpaceVector.x < 30f)
				{
					screenSpaceVector.x = 30f;
				}
				if (screenSpaceVector.y < 30f + ((ScreenSettings.ShowTextType != NavObjectScreenSettings.ShowTextTypes.None) ? 30f : 0f))
				{
					screenSpaceVector.y = 30f + ((ScreenSettings.ShowTextType != NavObjectScreenSettings.ShowTextTypes.None) ? 30f : 0f);
				}
				if (screenSpaceVector.x > (float)(Screen.width - 30))
				{
					screenSpaceVector.x = Screen.width - 30;
				}
				if (screenSpaceVector.y > (float)(Screen.height - 30))
				{
					screenSpaceVector.y = Screen.height - 30;
				}
				if (Vector3.Dot(lhs, cameraForward) < 0f)
				{
					if (screenSpaceVector.y < (float)(Screen.height / 2))
					{
						screenSpaceVector.y = Screen.height - 30;
					}
					else
					{
						screenSpaceVector.y = 30f + ((ScreenSettings.ShowTextType != NavObjectScreenSettings.ShowTextTypes.None) ? 30f : 0f);
					}
					screenSpaceVector.x = (float)Screen.width - screenSpaceVector.x - 30f;
				}
			}
			else if (Vector3.Dot(lhs, cameraForward) < 0f)
			{
				Transform.localPosition = NavObject.InvalidPos;
				return;
			}
			Vector3 localPosition = xui.TranslateScreenVectorToXuiVector(screenSpaceVector);
			localPosition.z = 0f;
			Transform.localPosition = localPosition;
			if (maxDistance != -1f)
			{
				float num3 = 1f;
				if (num2 >= maxDistance)
				{
					num3 = 0f;
				}
				else if (num2 >= ScreenSettings.FadeEndDistance)
				{
					num3 = 1f - Mathf.Clamp01((num2 - ScreenSettings.FadeEndDistance) / (maxDistance - ScreenSettings.FadeEndDistance));
					if (num3 < 1f)
					{
						Sprite.color = color;
					}
				}
				Sprite.alpha = num3;
			}
			else
			{
				Sprite.alpha = 1f;
			}
			if (num2 <= maxDistance - ScreenSettings.FadeEndDistance && ScreenSettings.HasPulse)
			{
				float num4 = Mathf.PingPong(Time.time, 0.5f);
				Sprite.color = Color.Lerp(Color.grey, color, num4 * 4f);
				if (num4 > 0.25f)
				{
					num += num4 - 0.25f;
				}
			}
			Sprite.SetDimensions((int)(num * ScreenSettings.SpriteSize), (int)(num * ScreenSettings.SpriteSize));
			if (ScreenSettings.SpriteFillType != NavObjectScreenSettings.SpriteFillTypes.None && NavObject.TrackedEntity != null)
			{
				if (FillSprite == null)
				{
					SetupFillSprite();
				}
				FillSprite.color = ScreenSettings.SpriteFillColor;
				FillSprite.alpha = Sprite.alpha;
				FillSprite.spriteName = ScreenSettings.SpriteFillName;
				FillSprite.SetDimensions((int)(num * ScreenSettings.SpriteSize), (int)(num * ScreenSettings.SpriteSize));
				if (ScreenSettings.SpriteFillType == NavObjectScreenSettings.SpriteFillTypes.Health)
				{
					FillSprite.fillAmount = ((EntityAlive)NavObject.TrackedEntity).Stats.Health.ValuePercent;
				}
			}
			else
			{
				RemoveFillSprite();
			}
			if (ScreenSettings.SubSpriteName != "")
			{
				if (SubSprite == null)
				{
					SetupSubSprite();
				}
				int num5 = (int)ScreenSettings.SubSpriteSize;
				SubSprite.transform.localPosition = ScreenSettings.SubSpriteOffset;
				SubSprite.SetDimensions(num5, num5);
				SubSprite.spriteName = ScreenSettings.SubSpriteName;
			}
			else
			{
				RemoveSubSprite();
			}
			if (!(Label != null))
			{
				return;
			}
			if (ScreenSettings.ShowTextType != NavObjectScreenSettings.ShowTextTypes.None)
			{
				Label.alpha = Sprite.alpha;
				Label.fontSize = ScreenSettings.FontSize;
				Label.color = (NavObject.UseOverrideFontColor ? NavObject.OverrideColor : ScreenSettings.FontColor);
				if (ScreenSettings.ShowTextType == NavObjectScreenSettings.ShowTextTypes.Distance)
				{
					string arg = "m";
					if (num2 >= 1000f)
					{
						num2 /= 1000f;
						arg = "km";
					}
					Label.text = string.Format("{0} {1}", num2.ToCultureInvariantString("0.0"), arg);
				}
				else if (ScreenSettings.ShowTextType == NavObjectScreenSettings.ShowTextTypes.Name)
				{
					Label.text = NavObject.DisplayName;
				}
				else
				{
					Label.text = ((NavObject.TrackedEntity != null) ? NavObject.TrackedEntity.spawnByName : "");
				}
			}
			else
			{
				Label.text = "";
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void SetupFillSprite()
		{
			GameObject gameObject = new GameObject("FilledSprite");
			gameObject.transform.SetParent(Transform);
			gameObject.transform.localPosition = Vector3.zero;
			UISprite uISprite = gameObject.AddComponent<UISprite>();
			uISprite.atlas = Owner.xui.GetAtlasByName("UIAtlas", "menu_empty");
			uISprite.transform.localScale = Vector3.one;
			uISprite.spriteName = "menu_empty";
			uISprite.SetDimensions(50, 50);
			uISprite.color = Color.clear;
			uISprite.pivot = UIWidget.Pivot.Center;
			uISprite.fillDirection = UIBasicSprite.FillDirection.Radial360;
			uISprite.type = UIBasicSprite.Type.Filled;
			uISprite.depth = 300;
			uISprite.gameObject.layer = 12;
			uISprite.color = new Color(1f, 1f, 1f, 1f);
			FillSprite = uISprite;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void SetupSubSprite()
		{
			GameObject gameObject = new GameObject("SubSprite");
			gameObject.transform.SetParent(Transform);
			int num = (int)ScreenSettings.SubSpriteSize;
			gameObject.transform.localPosition = ScreenSettings.SubSpriteOffset;
			UISprite uISprite = gameObject.AddComponent<UISprite>();
			uISprite.atlas = Owner.xui.GetAtlasByName("UIAtlas", "menu_empty");
			uISprite.transform.localScale = Vector3.one;
			uISprite.spriteName = "menu_empty";
			uISprite.SetDimensions(num, num);
			uISprite.color = Color.clear;
			uISprite.pivot = UIWidget.Pivot.Center;
			uISprite.depth = 300;
			uISprite.gameObject.layer = 12;
			uISprite.color = new Color(1f, 1f, 1f, 1f);
			SubSprite = uISprite;
		}

		public void RemoveFillSprite()
		{
			if (FillSprite != null)
			{
				Object.Destroy(FillSprite.gameObject);
				FillSprite = null;
			}
		}

		public void RemoveSubSprite()
		{
			if (SubSprite != null)
			{
				Object.Destroy(SubSprite.gameObject);
				SubSprite = null;
			}
		}

		public int UpdateDepth(int depth)
		{
			Sprite.depth = depth;
			Label.depth = depth;
			if (FillSprite != null)
			{
				FillSprite.depth = ++depth;
			}
			if (SubSprite != null)
			{
				SubSprite.depth = ++depth;
			}
			return depth;
		}
	}

	public List<OnScreenIcon> screenIconList = new List<OnScreenIcon>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<OnScreenIcon> disabledIcons = new List<OnScreenIcon>();

	public Camera Camera;

	public override void Init()
	{
		base.Init();
		NavObjectManager.Instance.OnNavObjectAdded += Instance_OnNavObjectAdded;
		NavObjectManager.Instance.OnNavObjectRemoved += Instance_OnNavObjectRemoved;
	}

	public override void Cleanup()
	{
		base.Cleanup();
		screenIconList.Clear();
		disabledIcons.Clear();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Instance_OnNavObjectRemoved(NavObject newNavObject)
	{
		if (newNavObject.HasOnScreen)
		{
			UnRegisterIcon(newNavObject);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Instance_OnNavObjectAdded(NavObject newNavObject)
	{
		if (newNavObject.HasOnScreen)
		{
			RegisterIcon(newNavObject);
		}
	}

	public override void Update(float _dt)
	{
		if (screenIconList.Count == 0)
		{
			return;
		}
		Vector3 offset = new Vector3((float)(-base.ViewComponent.Size.x) * 0.5f, (float)(-base.ViewComponent.Size.y) * 0.5f, 0f);
		Transform transform = base.xui.playerUI.entityPlayer.playerCamera.transform;
		Vector3 position = base.xui.playerUI.entityPlayer.GetPosition();
		int depth = 300;
		for (int num = screenIconList.Count - 1; num >= 0; num--)
		{
			screenIconList[num].Update(offset, position, transform.forward, base.xui, ref depth);
			if (screenIconList[num].ReadyForUnload)
			{
				disabledIcons.Add(screenIconList[num]);
				screenIconList[num].Transform.gameObject.SetActive(value: false);
				screenIconList.RemoveAt(num);
			}
		}
		base.Update(_dt);
	}

	public override void OnOpen()
	{
		base.OnOpen();
		Camera = base.xui.playerUI.entityPlayer.playerCamera;
	}

	public override void OnClose()
	{
		base.OnClose();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RegisterIcon(NavObject newNavObject)
	{
		for (int i = 0; i < screenIconList.Count; i++)
		{
			if (screenIconList[i].NavObject == newNavObject)
			{
				return;
			}
		}
		OnScreenIcon onScreenIcon = CreateIcon();
		onScreenIcon.Owner = this;
		onScreenIcon.NavObject = newNavObject;
		onScreenIcon.Init();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public OnScreenIcon CreateIcon()
	{
		if (disabledIcons.Count > 0)
		{
			OnScreenIcon onScreenIcon = disabledIcons[0];
			disabledIcons.RemoveAt(0);
			screenIconList.Add(onScreenIcon);
			onScreenIcon.ReadyForUnload = false;
			onScreenIcon.Transform.gameObject.SetActive(value: true);
			onScreenIcon.Sprite.color = Color.clear;
			onScreenIcon.Sprite.spriteName = "";
			return onScreenIcon;
		}
		GameObject gameObject = new GameObject("ScreenIcon");
		gameObject.transform.parent = base.ViewComponent.UiTransform;
		gameObject.transform.localScale = Vector3.one;
		gameObject.layer = 12;
		screenIconList.Add(new OnScreenIcon());
		screenIconList[screenIconList.Count - 1].Transform = gameObject.transform;
		GameObject gameObject2 = new GameObject("Sprite");
		gameObject2.transform.parent = gameObject.transform;
		UISprite uISprite = gameObject2.AddComponent<UISprite>();
		uISprite.atlas = base.xui.GetAtlasByName("UIAtlas", "menu_empty");
		uISprite.transform.localScale = Vector3.one;
		uISprite.spriteName = "menu_empty";
		uISprite.SetDimensions(50, 50);
		uISprite.color = Color.clear;
		uISprite.pivot = UIWidget.Pivot.Center;
		uISprite.depth = 300;
		uISprite.gameObject.layer = 12;
		screenIconList[screenIconList.Count - 1].Sprite = uISprite;
		GameObject gameObject3 = new GameObject("Label");
		gameObject3.transform.parent = gameObject.transform;
		UILabel uILabel = gameObject3.AddComponent<UILabel>();
		uILabel.transform.localScale = Vector3.one;
		uILabel.font = base.xui.GetUIFontByName("ReferenceFont");
		uILabel.fontSize = 24;
		uILabel.pivot = UIWidget.Pivot.Center;
		uILabel.overflowMethod = UILabel.Overflow.ResizeFreely;
		uILabel.alignment = NGUIText.Alignment.Center;
		uILabel.transform.localPosition = new Vector2(-50f, -30f);
		uILabel.effectStyle = UILabel.Effect.Outline;
		uILabel.effectColor = new Color32(0, 0, 0, byte.MaxValue);
		uILabel.effectDistance = new Vector2(2f, 2f);
		uILabel.color = Color.white;
		uILabel.text = "";
		uILabel.gameObject.layer = 12;
		uILabel.depth = 300;
		uILabel.width = 200;
		screenIconList[screenIconList.Count - 1].Label = uILabel;
		return screenIconList[screenIconList.Count - 1];
	}

	public void UnRegisterIcon(NavObject navObject)
	{
		for (int num = screenIconList.Count - 1; num >= 0; num--)
		{
			if (screenIconList[num].NavObject == navObject)
			{
				if (!disabledIcons.Contains(screenIconList[num]))
				{
					disabledIcons.Add(screenIconList[num]);
					screenIconList[num].Transform.gameObject.SetActive(value: false);
				}
				screenIconList.RemoveAt(num);
			}
		}
	}
}
