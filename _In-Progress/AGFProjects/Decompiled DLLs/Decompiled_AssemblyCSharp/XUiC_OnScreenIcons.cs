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

		public Transform Transform;

		public UISprite Sprite;

		public UISprite FillSprite;

		public UISprite SubSprite;

		public UILabel Label;

		public bool ReadyForUnload;

		public NavObjectScreenSettings ScreenSettings;

		public void Init()
		{
		}

		public void Update(Vector3 offset, Vector3 playerPosition, Vector3 cameraForward, XUi xui, ref int depth)
		{
			if (!NavObject.IsValid())
			{
				return;
			}
			if ((bool)Sprite)
			{
				UISprite sprite = Sprite;
				bool enabled = (Label.enabled = !NavObject.hiddenOnCompass);
				sprite.enabled = enabled;
				depth = UpdateDepth(depth) + 1;
			}
			ScreenSettings = NavObject.CurrentScreenSettings;
			if (ScreenSettings == null)
			{
				HideObjects();
				return;
			}
			EntityPlayerLocal entityPlayer = xui.playerUI.entityPlayer;
			if (entityPlayer.IsDead())
			{
				HideObjects();
				return;
			}
			float num = Vector3.Distance(NavObject.GetPosition(), entityPlayer.position - Origin.position);
			float maxDistance = NavObject.GetMaxDistance(ScreenSettings, entityPlayer);
			if (maxDistance != -1f && num > maxDistance)
			{
				HideObjects();
				return;
			}
			if (num < ScreenSettings.MinDistance)
			{
				HideObjects();
				return;
			}
			CreateObjects();
			string spriteName = NavObject.GetSpriteName(ScreenSettings);
			if (Sprite.spriteName != spriteName)
			{
				Sprite.atlas = xui.GetAtlasByName("UIAtlas", spriteName);
				Sprite.spriteName = spriteName;
			}
			Color color = (NavObject.UseOverrideColor ? NavObject.OverrideColor : ScreenSettings.Color);
			Sprite.color = color;
			Vector3 vector = NavObject.GetPosition() + ScreenSettings.Offset;
			if (ScreenSettings.UseHeadOffset && NavObject.TrackType == NavObject.TrackTypes.Entity)
			{
				vector.y += NavObject.TrackedEntity.GetEyeHeight();
				vector += NavObject.TrackedEntity.EntityClass.NavObjectHeadOffset;
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
				float num2 = ((ScreenSettings.ShowTextType != NavObjectScreenSettings.ShowTextTypes.None) ? 30f : 0f);
				if (screenSpaceVector.y < 30f + num2)
				{
					screenSpaceVector.y = 30f + num2;
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
						screenSpaceVector.y = 30f + num2;
					}
					screenSpaceVector.x = (float)Screen.width - screenSpaceVector.x - 30f;
				}
			}
			else if (Vector3.Dot(lhs, cameraForward) < 0f)
			{
				HideObjects();
				return;
			}
			Vector3 localPosition = xui.TranslateScreenVectorToXuiVector(screenSpaceVector);
			localPosition.z = 0f;
			Transform.localPosition = localPosition;
			Transform.gameObject.SetActive(value: true);
			if (maxDistance != -1f)
			{
				float num3 = 1f;
				if (num >= maxDistance)
				{
					num3 = 0f;
				}
				else if (num >= ScreenSettings.FadeEndDistance)
				{
					num3 = 1f - Utils.FastClamp01((num - ScreenSettings.FadeEndDistance) / (maxDistance - ScreenSettings.FadeEndDistance));
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
			float num4 = 1f;
			if (num <= maxDistance - ScreenSettings.FadeEndDistance && ScreenSettings.HasPulse)
			{
				float num5 = Mathf.PingPong(Time.time, 0.5f);
				Sprite.color = Color.Lerp(Color.grey, color, num5 * 4f);
				if (num5 > 0.25f)
				{
					num4 += num5 - 0.25f;
				}
			}
			int num6 = (int)(num4 * ScreenSettings.SpriteSize);
			Sprite.SetDimensions(num6, num6);
			if (ScreenSettings.SpriteFillType != NavObjectScreenSettings.SpriteFillTypes.None && NavObject.TrackedEntity != null)
			{
				if (FillSprite == null)
				{
					SetupFillSprite();
				}
				FillSprite.color = ScreenSettings.SpriteFillColor;
				FillSprite.alpha = Sprite.alpha;
				FillSprite.spriteName = ScreenSettings.SpriteFillName;
				FillSprite.SetDimensions(num6, num6);
				if (ScreenSettings.SpriteFillType == NavObjectScreenSettings.SpriteFillTypes.Health)
				{
					FillSprite.fillAmount = ((EntityAlive)NavObject.TrackedEntity).Stats.Health.ValuePercent;
				}
			}
			else
			{
				RemoveFillSprite();
			}
			if (ScreenSettings.SubSpriteName != null)
			{
				if (SubSprite == null)
				{
					SetupSubSprite();
				}
				int num7 = (int)ScreenSettings.SubSpriteSize;
				SubSprite.transform.localPosition = ScreenSettings.SubSpriteOffset;
				SubSprite.SetDimensions(num7, num7);
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
					if (num >= 1000f)
					{
						num /= 1000f;
						arg = "km";
					}
					Label.text = string.Format("{0} {1}", num.ToCultureInvariantString("0.0"), arg);
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

		public void CreateObjects()
		{
			if (!Sprite)
			{
				GameObject gameObject = new GameObject("ScreenIcon");
				Transform transform = (Transform = gameObject.transform);
				transform.SetParent(Owner.ViewComponent.UiTransform, worldPositionStays: false);
				gameObject.layer = 12;
				GameObject gameObject2 = new GameObject("Sprite");
				gameObject2.transform.SetParent(transform, worldPositionStays: false);
				gameObject2.layer = 12;
				UISprite uISprite = (Sprite = gameObject2.AddComponent<UISprite>());
				uISprite.atlas = Owner.xui.GetAtlasByName("UIAtlas", "menu_empty");
				uISprite.spriteName = "menu_empty";
				uISprite.SetDimensions(50, 50);
				uISprite.color = Color.clear;
				uISprite.pivot = UIWidget.Pivot.Center;
				uISprite.depth = 300;
				GameObject gameObject3 = new GameObject("Label");
				gameObject3.transform.SetParent(transform, worldPositionStays: false);
				gameObject3.layer = 12;
				UILabel uILabel = (Label = gameObject3.AddComponent<UILabel>());
				uILabel.font = Owner.xui.GetUIFontByName("ReferenceFont");
				uILabel.fontSize = 24;
				uILabel.pivot = UIWidget.Pivot.Center;
				uILabel.overflowMethod = UILabel.Overflow.ResizeFreely;
				uILabel.alignment = NGUIText.Alignment.Center;
				uILabel.effectStyle = UILabel.Effect.Outline;
				uILabel.effectColor = new Color32(0, 0, 0, byte.MaxValue);
				uILabel.effectDistance = new Vector2(2f, 2f);
				uILabel.color = Color.white;
				uILabel.text = "";
				uILabel.depth = 300;
				uILabel.width = 200;
				ScreenSettings = NavObject.CurrentScreenSettings;
				uILabel.transform.localPosition = new Vector2(0f, 0f - (ScreenSettings.SpriteSize * 0.5f + 8f));
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void HideObjects()
		{
			if ((bool)Transform)
			{
				Transform.localPosition = NavObject.InvalidPos;
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void SetupFillSprite()
		{
			GameObject gameObject = new GameObject("FilledSprite");
			gameObject.layer = 12;
			gameObject.transform.SetParent(Transform, worldPositionStays: false);
			UISprite uISprite = (FillSprite = gameObject.AddComponent<UISprite>());
			uISprite.atlas = Owner.xui.GetAtlasByName("UIAtlas", "menu_empty");
			uISprite.spriteName = "menu_empty";
			uISprite.SetDimensions(50, 50);
			uISprite.color = Color.clear;
			uISprite.pivot = UIWidget.Pivot.Center;
			uISprite.fillDirection = UIBasicSprite.FillDirection.Radial360;
			uISprite.type = UIBasicSprite.Type.Filled;
			uISprite.depth = 300;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void SetupSubSprite()
		{
			GameObject obj = new GameObject("SubSprite")
			{
				layer = 12
			};
			Transform transform = obj.transform;
			transform.SetParent(Transform, worldPositionStays: false);
			transform.localPosition = ScreenSettings.SubSpriteOffset;
			UISprite uISprite = (SubSprite = obj.AddComponent<UISprite>());
			uISprite.atlas = Owner.xui.GetAtlasByName("UIAtlas", "menu_empty");
			uISprite.spriteName = "menu_empty";
			int num = (int)ScreenSettings.SubSpriteSize;
			uISprite.SetDimensions(num, num);
			uISprite.color = Color.clear;
			uISprite.pivot = UIWidget.Pivot.Center;
			uISprite.depth = 300;
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
		EntityPlayerLocal entityPlayer = base.xui.playerUI.entityPlayer;
		Vector3 forward = entityPlayer.cameraTransform.forward;
		Vector3 position = entityPlayer.GetPosition();
		int depth = 300;
		for (int num = screenIconList.Count - 1; num >= 0; num--)
		{
			OnScreenIcon onScreenIcon = screenIconList[num];
			onScreenIcon.Update(offset, position, forward, base.xui, ref depth);
			if (onScreenIcon.ReadyForUnload)
			{
				screenIconList.RemoveAt(num);
				disabledIcons.Add(onScreenIcon);
				onScreenIcon.Transform.gameObject.SetActive(value: false);
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
		OnScreenIcon onScreenIcon;
		if (disabledIcons.Count > 0)
		{
			onScreenIcon = disabledIcons[0];
			disabledIcons.RemoveAt(0);
			onScreenIcon.ReadyForUnload = false;
			if ((bool)onScreenIcon.Transform)
			{
				onScreenIcon.Sprite.color = Color.clear;
				onScreenIcon.Sprite.spriteName = "";
			}
		}
		else
		{
			onScreenIcon = new OnScreenIcon();
		}
		screenIconList.Add(onScreenIcon);
		return onScreenIcon;
	}

	public void UnRegisterIcon(NavObject navObject)
	{
		for (int num = screenIconList.Count - 1; num >= 0; num--)
		{
			OnScreenIcon onScreenIcon = screenIconList[num];
			if (onScreenIcon.NavObject == navObject)
			{
				if (!disabledIcons.Contains(onScreenIcon))
				{
					disabledIcons.Add(onScreenIcon);
					if ((bool)onScreenIcon.Transform)
					{
						onScreenIcon.Transform.gameObject.SetActive(value: false);
					}
				}
				screenIconList.RemoveAt(num);
				break;
			}
		}
	}
}
