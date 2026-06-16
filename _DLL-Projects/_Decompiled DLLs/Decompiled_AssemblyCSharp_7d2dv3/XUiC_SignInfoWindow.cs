using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_SignInfoWindow : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public GlobalSignId signId = GlobalSignId.InvalidId;

	[PublicizedFrom(EAccessModifier.Private)]
	public SignData signData;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Texture signMaterial;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnEdit;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnDuplicate;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnDelete;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnExport;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SignGalleryWindow galleryWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ConfirmationPrompt confirmationPrompt;

	public override void Init()
	{
		base.Init();
		signMaterial = GetChildById("signMaterial").ViewComponent as XUiV_Texture;
		signMaterial.CreateMaterial("Game/SignTech/UI");
		XUiV_Texture xUiV_Texture = signMaterial;
		xUiV_Texture.OnRenderTexture = (UIDrawCall.OnRenderCallback)Delegate.Combine(xUiV_Texture.OnRenderTexture, new UIDrawCall.OnRenderCallback(OnWillRender));
		signMaterial.Texture = Texture2D.whiteTexture;
		btnEdit = (XUiC_SimpleButton)GetChildById("btnEdit");
		btnDuplicate = (XUiC_SimpleButton)GetChildById("btnDuplicate");
		btnDelete = (XUiC_SimpleButton)GetChildById("btnDelete");
		btnExport = (XUiC_SimpleButton)GetChildById("btnExport");
		btnEdit.OnPressed += BtnEdit_OnPressed;
		btnDuplicate.OnPressed += [PublicizedFrom(EAccessModifier.Private)] (XUiController _, int _) =>
		{
			DuplicateSign();
		};
		btnDelete.OnPressed += [PublicizedFrom(EAccessModifier.Private)] (XUiController _, int _) =>
		{
			DeleteSign();
		};
		btnExport.OnPressed += BtnExport_OnPressed;
		confirmationPrompt = GetChildById("confirmation_prompt_controller") as XUiC_ConfirmationPrompt;
		galleryWindow = windowGroup.Controller.GetChildByType<XUiC_SignGalleryWindow>();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnExport_OnPressed(XUiController _sender, int _mouseButton)
	{
		if (signId.IsValid && signData != null)
		{
			string text = SignTextureExporter.Instance.ExportSignToTexture(signData.name, signId);
			Log.Out("Sign png saved to: " + text);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnEdit_OnPressed(XUiController _sender, int _mouseButton)
	{
		if (galleryWindow.targetEntity != null)
		{
			XUiC_SignEditorWindow.Open(xui.playerUI, galleryWindow.targetEntity);
		}
		else
		{
			XUiC_SignEditorWindow.Open(xui.playerUI, signData, signId);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DuplicateSign()
	{
		if (!PrefabEditModeManager.Instance.IsActive())
		{
			Log.Error("Duplicate function can only be used in the Prefab Editor.");
			return;
		}
		string fileNameNoExtension = PrefabEditModeManager.Instance.LoadedPrefab.FileNameNoExtension;
		SignData sign = SignData.Duplicate(signData, SignDataManager.Instance.GetDuplicateName(fileNameNoExtension, signData.name) ?? "");
		GlobalSignId newSignId = SignDataManager.Instance.AddSignToLibrary(fileNameNoExtension, sign);
		PrefabEditModeManager.Instance.NeedsSaving = true;
		galleryWindow.RefreshAndSelect(newSignId);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DeleteSign()
	{
		if (!PrefabEditModeManager.Instance.IsActive())
		{
			Log.Error("Delete function can only be used in the Prefab Editor.");
		}
		else
		{
			confirmationPrompt.ShowPrompt(Localization.Get("lblSignDeleteTitle"), Localization.Get("lblSignDeleteMessage"), Localization.Get("xuiCancel"), Localization.Get("xuiConfirm"), OnDeletionPromptResult);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDeletionPromptResult(XUiC_ConfirmationPrompt.Result result)
	{
		if (result != XUiC_ConfirmationPrompt.Result.Cancelled && result == XUiC_ConfirmationPrompt.Result.Confirmed)
		{
			if (!SignDataManager.Instance.TryDeleteSign(signId))
			{
				Log.Error($"Failed to delete sign with ID: {signId}");
				return;
			}
			PrefabEditModeManager.Instance.NeedsSaving = true;
			galleryWindow.RefreshAndSelect(SignData.defaultSignDataID);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnWillRender(Material mat)
	{
		SignDataManager.Instance.TryApplyRenderingData(signId, 1f, mat);
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		UpdateInputs();
		if (IsDirty && base.ViewComponent.IsVisible)
		{
			IsDirty = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateInputs()
	{
		if (!confirmationPrompt.IsVisible && signData != null)
		{
			if (InputUtils.ControlKeyPressed && Input.GetKeyDown(KeyCode.D))
			{
				DuplicateSign();
			}
			if (Input.GetKeyDown(KeyCode.Delete))
			{
				DeleteSign();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string value, string bindingName)
	{
		switch (bindingName)
		{
		case "signname":
			if (!signId.IsValid)
			{
				value = Localization.Get("lblSignNone");
			}
			else if (signData == null)
			{
				value = string.Format(Localization.Get("lblSignMissing"), signId.signGuid.ToString());
			}
			else
			{
				value = signData.name;
			}
			return true;
		case "libraryname":
			value = SignDataManager.GetLibraryNiceName(signId);
			return true;
		case "lastmodified":
			value = ((signId.IsValid && signData != null) ? signData.lastModified.ToString("g") : "-");
			return true;
		default:
			return base.GetBindingValueInternal(ref value, bindingName);
		}
	}

	public void SetSignInfo(GlobalSignId signId)
	{
		SignData signData;
		bool flag = SignDataManager.Instance.TryGetSignData(signId, out signData);
		this.signId = signId;
		this.signData = signData;
		signMaterial.IsVisible = signId.IsValid && flag;
		bool enabled = signId.IsValid && flag && signId.libraryId != "[D]";
		btnEdit.Enabled = enabled;
		btnDelete.Enabled = enabled;
		btnDuplicate.Enabled = flag;
		btnExport.Enabled = flag;
		RefreshBindings();
	}
}
