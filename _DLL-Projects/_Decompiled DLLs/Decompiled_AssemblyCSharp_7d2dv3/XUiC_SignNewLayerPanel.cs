using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_SignNewLayerPanel : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SignLayerType layerTypeText;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SignLayerType layerTypePolygon;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SignLayerType layerTypeNoise;

	public Action<SignData.SignLayer> OnLayerTypeSelected;

	public override void Init()
	{
		base.Init();
		layerTypeText = GetChildById("layerTypeText").GetChildByType<XUiC_SignLayerType>();
		layerTypePolygon = GetChildById("layerTypePolygon").GetChildByType<XUiC_SignLayerType>();
		layerTypeNoise = GetChildById("layerTypeNoise").GetChildByType<XUiC_SignLayerType>();
		XUiC_SignLayerType xUiC_SignLayerType = layerTypeText;
		xUiC_SignLayerType.OnBecameSelected = (Action<XUiC_SignGridEntry>)Delegate.Combine(xUiC_SignLayerType.OnBecameSelected, new Action<XUiC_SignGridEntry>(LayerTypeText_OnBecameSelected));
		XUiC_SignLayerType xUiC_SignLayerType2 = layerTypePolygon;
		xUiC_SignLayerType2.OnBecameSelected = (Action<XUiC_SignGridEntry>)Delegate.Combine(xUiC_SignLayerType2.OnBecameSelected, new Action<XUiC_SignGridEntry>(LayerTypePolygon_OnBecameSelected));
		XUiC_SignLayerType xUiC_SignLayerType3 = layerTypeNoise;
		xUiC_SignLayerType3.OnBecameSelected = (Action<XUiC_SignGridEntry>)Delegate.Combine(xUiC_SignLayerType3.OnBecameSelected, new Action<XUiC_SignGridEntry>(LayerTypeNoise_OnBecameSelected));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void LayerTypeText_OnBecameSelected(XUiC_SignGridEntry gridEntry)
	{
		SignData.SignLayer obj = new SignData.TextSignLayer();
		OnLayerTypeSelected?.Invoke(obj);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void LayerTypePolygon_OnBecameSelected(XUiC_SignGridEntry gridEntry)
	{
		SignData.SignLayer obj = new SignData.PolygonSignLayer();
		OnLayerTypeSelected?.Invoke(obj);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void LayerTypeNoise_OnBecameSelected(XUiC_SignGridEntry gridEntry)
	{
		SignData.SignLayer obj = new SignData.NoiseSignLayer();
		OnLayerTypeSelected?.Invoke(obj);
	}
}
