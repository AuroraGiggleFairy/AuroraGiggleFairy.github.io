using System;
using UnityEngine.Scripting;

[Preserve]
public abstract class XUiC_SignWarpSettings : XUiC_SignLayerSettings
{
	public Action OnWarpRemoved;

	[PublicizedFrom(EAccessModifier.Protected)]
	public SignData.SignLayer currentLayer;

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiC_SimpleButton btnRemove;

	public abstract SignData.SignWarp CurrentWarp
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get;
	}

	public override void Init()
	{
		base.Init();
		btnRemove = (XUiC_SimpleButton)GetChildById("btnRemove");
		btnRemove.OnPressed += BtnRemove_OnPressed;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnRemove_OnPressed(XUiController _sender, int _mouseButton)
	{
		if (currentLayer == null)
		{
			Log.Error("Failed to remove warp. Layer reference is null.");
			return;
		}
		if (CurrentWarp == null)
		{
			Log.Error("Failed to remove warp. Warp reference is null.");
			return;
		}
		string name = CurrentWarp.GetType().Name;
		if (!currentLayer.warps.Contains(CurrentWarp))
		{
			Log.Error("Failed to remove " + name + ". Layer warp list does not contain referenced warp.");
			return;
		}
		OnPreLayerSettingsChanged?.Invoke("Remove " + name, arg2: true);
		currentLayer.warps.Remove(CurrentWarp);
		OnLayerSettingsChanged?.Invoke();
		OnWarpRemoved?.Invoke();
	}

	public abstract void SetWarp(SignData.SignWarp warp);

	public override void SetLayer(SignData.SignLayer layer)
	{
		currentLayer = layer;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiC_SignWarpSettings()
	{
	}
}
