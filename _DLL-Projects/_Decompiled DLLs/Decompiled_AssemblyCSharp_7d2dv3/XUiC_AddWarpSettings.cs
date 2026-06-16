using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_AddWarpSettings : XUiC_SignLayerSettings
{
	public Action<string> OnWarpAdded;

	[PublicizedFrom(EAccessModifier.Private)]
	public SignData.SignLayer currentLayer;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SignLayerType warpTypeSkew;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SignLayerType warpTypeBulge;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SignLayerType warpTypeTwirl;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SignLayerType warpTypeKaleido;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SignLayerType warpTypePerspective;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SignLayerType warpTypeArc;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SignLayerType warpTypeStretch;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SignLayerType warpTypeGrid;

	public override void Init()
	{
		base.Init();
		warpTypeSkew = GetChildById("warpTypeSkew").GetChildByType<XUiC_SignLayerType>();
		XUiC_SignLayerType xUiC_SignLayerType = warpTypeSkew;
		xUiC_SignLayerType.OnBecameSelected = (Action<XUiC_SignGridEntry>)Delegate.Combine(xUiC_SignLayerType.OnBecameSelected, (Action<XUiC_SignGridEntry>)([PublicizedFrom(EAccessModifier.Private)] (XUiC_SignGridEntry _) =>
		{
			AddWarp(new SignData.SkewWarp());
		}));
		warpTypeBulge = GetChildById("warpTypeBulge").GetChildByType<XUiC_SignLayerType>();
		XUiC_SignLayerType xUiC_SignLayerType2 = warpTypeBulge;
		xUiC_SignLayerType2.OnBecameSelected = (Action<XUiC_SignGridEntry>)Delegate.Combine(xUiC_SignLayerType2.OnBecameSelected, (Action<XUiC_SignGridEntry>)([PublicizedFrom(EAccessModifier.Private)] (XUiC_SignGridEntry _) =>
		{
			AddWarp(new SignData.BulgeWarp());
		}));
		warpTypeTwirl = GetChildById("warpTypeTwirl").GetChildByType<XUiC_SignLayerType>();
		XUiC_SignLayerType xUiC_SignLayerType3 = warpTypeTwirl;
		xUiC_SignLayerType3.OnBecameSelected = (Action<XUiC_SignGridEntry>)Delegate.Combine(xUiC_SignLayerType3.OnBecameSelected, (Action<XUiC_SignGridEntry>)([PublicizedFrom(EAccessModifier.Private)] (XUiC_SignGridEntry _) =>
		{
			AddWarp(new SignData.TwirlWarp());
		}));
		warpTypeKaleido = GetChildById("warpTypeKaleido").GetChildByType<XUiC_SignLayerType>();
		XUiC_SignLayerType xUiC_SignLayerType4 = warpTypeKaleido;
		xUiC_SignLayerType4.OnBecameSelected = (Action<XUiC_SignGridEntry>)Delegate.Combine(xUiC_SignLayerType4.OnBecameSelected, (Action<XUiC_SignGridEntry>)([PublicizedFrom(EAccessModifier.Private)] (XUiC_SignGridEntry _) =>
		{
			AddWarp(new SignData.KaleidoWarp());
		}));
		warpTypePerspective = GetChildById("warpTypePerspective").GetChildByType<XUiC_SignLayerType>();
		XUiC_SignLayerType xUiC_SignLayerType5 = warpTypePerspective;
		xUiC_SignLayerType5.OnBecameSelected = (Action<XUiC_SignGridEntry>)Delegate.Combine(xUiC_SignLayerType5.OnBecameSelected, (Action<XUiC_SignGridEntry>)([PublicizedFrom(EAccessModifier.Private)] (XUiC_SignGridEntry _) =>
		{
			AddWarp(new SignData.PerspectiveWarp());
		}));
		warpTypeArc = GetChildById("warpTypeArc").GetChildByType<XUiC_SignLayerType>();
		XUiC_SignLayerType xUiC_SignLayerType6 = warpTypeArc;
		xUiC_SignLayerType6.OnBecameSelected = (Action<XUiC_SignGridEntry>)Delegate.Combine(xUiC_SignLayerType6.OnBecameSelected, (Action<XUiC_SignGridEntry>)([PublicizedFrom(EAccessModifier.Private)] (XUiC_SignGridEntry _) =>
		{
			AddWarp(new SignData.ArcWarp());
		}));
		warpTypeStretch = GetChildById("warpTypeStretch").GetChildByType<XUiC_SignLayerType>();
		XUiC_SignLayerType xUiC_SignLayerType7 = warpTypeStretch;
		xUiC_SignLayerType7.OnBecameSelected = (Action<XUiC_SignGridEntry>)Delegate.Combine(xUiC_SignLayerType7.OnBecameSelected, (Action<XUiC_SignGridEntry>)([PublicizedFrom(EAccessModifier.Private)] (XUiC_SignGridEntry _) =>
		{
			AddWarp(new SignData.StretchWarp());
		}));
		warpTypeGrid = GetChildById("warpTypeGrid").GetChildByType<XUiC_SignLayerType>();
		XUiC_SignLayerType xUiC_SignLayerType8 = warpTypeGrid;
		xUiC_SignLayerType8.OnBecameSelected = (Action<XUiC_SignGridEntry>)Delegate.Combine(xUiC_SignLayerType8.OnBecameSelected, (Action<XUiC_SignGridEntry>)([PublicizedFrom(EAccessModifier.Private)] (XUiC_SignGridEntry _) =>
		{
			AddWarp(new SignData.GridWarp());
		}));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void AddWarp<T>(T warp) where T : SignData.SignWarp
	{
		if (currentLayer == null)
		{
			Log.Error("Failed to add warp: layer reference is null.");
			return;
		}
		if (currentLayer.warps == null)
		{
			Log.Error("Failed to add warp: layer warps list is null.");
			return;
		}
		string name = typeof(T).Name;
		OnPreLayerSettingsChanged?.Invoke("Add " + name, arg2: true);
		currentLayer.warps.Add(warp);
		OnLayerSettingsChanged?.Invoke();
		OnWarpAdded?.Invoke(name);
	}

	public override void SetLayer(SignData.SignLayer layer)
	{
		currentLayer = layer;
	}
}
