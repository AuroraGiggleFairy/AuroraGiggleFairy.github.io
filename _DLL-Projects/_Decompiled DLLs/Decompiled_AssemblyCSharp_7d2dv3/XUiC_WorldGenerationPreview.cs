using System;
using System.Collections;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;
using WorldGenerationEngineFinal;

[Preserve]
public class XUiC_WorldGenerationPreview : XUiController
{
	public enum PreviewStep
	{
		Start,
		Biome,
		Terrain,
		Done
	}

	public class PrefabNameHandler : MonoBehaviour
	{
		[NonSerialized]
		[PublicizedFrom(EAccessModifier.Private)]
		public TextMesh textMesh;

		[PublicizedFrom(EAccessModifier.Private)]
		public void Awake()
		{
			textMesh = base.transform.GetComponent<TextMesh>();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void OnMouseOver(bool _isOver)
		{
			if (!(textMesh == null))
			{
				base.gameObject.layer = (_isOver ? 11 : 0);
			}
		}
	}

	[XuiBindParent(true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_WorldGenerationWindow uiWinGroup;

	[PublicizedFrom(EAccessModifier.Private)]
	public RenderTextureSystem renderTextureSystem;

	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject terrainPreviewRootObj;

	[XuiBindComponent(true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiV_Texture uiPreviewTexture;

	[PublicizedFrom(EAccessModifier.Private)]
	public float cameraRotX;

	[PublicizedFrom(EAccessModifier.Private)]
	public float cameraRotY;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float CurrentFlySpeed = 150f;

	public static WorldBuilder WorldBuilder;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Texture2D previewImage;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color32[] previewColors;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isHovered;

	[XuiXmlBinding("ishovered")]
	public bool IsHovered
	{
		get
		{
			return isHovered;
		}
		set
		{
			isHovered = value;
			IsDirty = true;
		}
	}

	[XuiXmlBinding("iscontrolling")]
	public bool Controlling
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return xui.playerUI.windowManager.IsInputLocked;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			if (Controlling != value)
			{
				xui.playerUI.windowManager.IsInputLocked = value;
				xui.playerUI.CursorController.Locked = value;
				xui.playerUI.CursorController.SetCursorHidden(value);
				if (value)
				{
					PlatformManager.NativePlatform.Input.ActionSetManager.Push(xui.playerUI.playerInput);
				}
				else
				{
					PlatformManager.NativePlatform.Input.ActionSetManager.Pop();
				}
				IsDirty = true;
			}
		}
	}

	public override void Init()
	{
		base.Init();
		uiPreviewTexture.Controller.OnPress += worldPreview_OnPress;
		uiPreviewTexture.Controller.OnHover += onHover;
	}

	[XuiBindEvent("OnHover", null)]
	[PublicizedFrom(EAccessModifier.Private)]
	public void onHover(XUiController _sender, bool _isOver)
	{
		IsHovered = _isOver;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void worldPreview_OnPress(XUiController _sender, int _button)
	{
		Controlling = true;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		handleDirtyUpdateDefault();
		if (renderTextureSystem == null || !Controlling)
		{
			return;
		}
		if (xui.playerUI.playerInput.Primary.WasReleased || xui.playerUI.playerInput.Menu.WasReleased)
		{
			ThreadManager.RunTaskAfterFrames([PublicizedFrom(EAccessModifier.Private)] () =>
			{
				Controlling = false;
			});
			return;
		}
		Transform transform = renderTextureSystem.CameraGO.transform;
		if (xui.playerUI.playerInput.Reload.WasReleased || xui.playerUI.playerInput.PermanentActions.Reload.WasReleased)
		{
			resetCamera();
			return;
		}
		Vector3 vector = updateMovementUnified(_dt);
		vector *= 150f * Time.deltaTime;
		Vector3 position = transform.position;
		position += transform.forward * vector.z;
		position += transform.right * vector.x;
		position += transform.up * vector.y;
		position.y = Utils.FastMax(40f, position.y);
		transform.position = position;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 updateMovementUnified(float _dt)
	{
		PlayerActionsLocal playerInput = xui.playerUI.playerInput;
		Vector3 zero = Vector3.zero;
		zero = new Vector3(playerInput.Move.X, playerInput.JumpCrouch.Value - playerInput.ToggleCrouch.Value, playerInput.Move.Y);
		if (playerInput.Run.IsPressed)
		{
			zero *= 10f;
		}
		Transform transform = renderTextureSystem.CameraGO.transform;
		cameraRotX += playerInput.Look.Y * _dt * -165f;
		cameraRotY += playerInput.Look.X * _dt * 165f;
		transform.rotation = Quaternion.Euler(cameraRotX, cameraRotY, 0f);
		return zero;
	}

	public Vector3 GetCameraPosition()
	{
		return renderTextureSystem.CameraGO.transform.position;
	}

	public override void OnOpen()
	{
		renderTextureInit();
		base.OnOpen();
	}

	public override void OnClose()
	{
		Controlling = false;
		renderTextureCleanup();
		previewTextureCleanup();
		TerrainCleanup();
		poiPreviewsCleanup();
		base.OnClose();
	}

	public IEnumerator ShowPreview(PreviewStep _step)
	{
		if (uiWinGroup.PreviewQualityLevel != XUiC_WorldGenerationWindow.PreviewQuality.NoPreview)
		{
			switch (_step)
			{
			case PreviewStep.Biome:
				PreviewInit();
				yield return WorldBuilder.SetMessage(Localization.Get("xuiRwgCreatingPreview"), _logToConsole: true);
				WorldBuilder.PreviewTextureUpdateBiomes(previewColors);
				previewImage.SetPixels32(previewColors);
				previewImage.Apply(updateMipmaps: true);
				WorldPreviewTerrain.SetTexture(previewImage);
				break;
			case PreviewStep.Terrain:
				TerrainUpdate();
				break;
			case PreviewStep.Done:
			{
				MicroStopwatch ms = new MicroStopwatch(_bStart: true);
				yield return WorldBuilder.PreviewTextureUpdateFinal(previewColors);
				Log.Out("CreatePreviewTexture in {0}", (float)ms.ElapsedMilliseconds * 0.001f);
				previewImage.SetPixels32(previewColors);
				previewImage.Apply(updateMipmaps: true);
				WorldPreviewTerrain.SetTexture(previewImage);
				TerrainUpdate();
				break;
			}
			}
		}
	}

	public void PreviewInit()
	{
		previewTextureInit();
		TerrainCleanup();
		if ((bool)terrainPreviewRootObj && WorldBuilder != null)
		{
			WorldPreviewTerrain.Init(uiWinGroup.WorldBuilder, terrainPreviewRootObj.transform);
			TerrainUpdate();
			resetCamera();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void previewTextureInit()
	{
		previewTextureCleanup();
		if (uiWinGroup.PreviewQualityLevel != XUiC_WorldGenerationWindow.PreviewQuality.NoPreview)
		{
			int worldSize = uiWinGroup.WorldSize;
			int num = worldSize * worldSize;
			if (previewColors == null || previewColors.Length != num)
			{
				previewColors = new Color32[num];
			}
			previewImage = new Texture2D(worldSize, worldSize)
			{
				filterMode = FilterMode.Point
			};
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void previewTextureCleanup()
	{
		if ((bool)previewImage)
		{
			UnityEngine.Object.Destroy(previewImage);
			previewImage = null;
		}
	}

	public void PreviewTextureDraw()
	{
		if ((bool)previewImage && WorldBuilder != null)
		{
			previewImage.SetPixels32(previewColors);
			previewImage.Apply(updateMipmaps: true);
			WorldPreviewTerrain.SetTexture(previewImage);
		}
	}

	public void TerrainUpdate()
	{
		if ((bool)terrainPreviewRootObj && WorldBuilder != null)
		{
			WorldPreviewTerrain.GenerateTerrain();
		}
	}

	public void TerrainCleanup()
	{
		WorldPreviewTerrain.Cleanup();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void poiPreviewsCleanup()
	{
		uiWinGroup?.CleanupPreviewManager();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void renderTextureInit()
	{
		if (renderTextureSystem == null)
		{
			renderTextureSystem = new RenderTextureSystem();
			terrainPreviewRootObj = new GameObject("TerrainMesh");
			renderTextureSystem.Create("worldpreview", terrainPreviewRootObj, new Vector3(0f, 0f, 0f), new Vector3(0f, 4000f, 0f), uiPreviewTexture.Size, _isAA: false);
			Camera component = renderTextureSystem.CameraGO.transform.GetComponent<Camera>();
			component.nearClipPlane = 0.1f;
			component.farClipPlane = 20000f;
			resetCamera();
			Transform transform = renderTextureSystem.LightGO.transform;
			transform.localPosition = new Vector3(0f, 2000f, 0f);
			transform.localRotation = Quaternion.Euler(30f, 0f, 0f);
			transform.GetComponent<Light>().type = LightType.Directional;
			transform.GetComponent<Light>().intensity = 1.2f;
			terrainPreviewRootObj.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
			terrainPreviewRootObj.transform.position = new Vector3(-10240f, 0f, -10240f);
			uiPreviewTexture.Texture = renderTextureSystem.RenderTex;
		}
		renderTextureSystem.SetEnabled(_b: true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void renderTextureCleanup()
	{
		renderTextureSystem.SetEnabled(_b: false);
		UnityEngine.Object.Destroy(renderTextureSystem.ParentGO);
		renderTextureSystem = null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void resetCamera()
	{
		if (renderTextureSystem != null && WorldBuilder != null)
		{
			cameraRotX = 90f;
			cameraRotY = 0f;
			Transform transform = renderTextureSystem.CameraGO.transform;
			transform.localPosition = new Vector3(0f, (float)WorldBuilder.WorldSize * 0.8745f, 0f);
			transform.localRotation = Quaternion.Euler(cameraRotX, cameraRotY, 0f);
		}
	}
}
