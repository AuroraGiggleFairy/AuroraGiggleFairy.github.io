using UnityEngine.Scripting;

[Preserve]
public class XUiC_SignDebugPanel : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label lblGroupDepth;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label lblComplexity;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label lblDescCount;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label lblWarpCount;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label lblDrawCount;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label lblBgnCompDepth;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label lblMaxCompDepth;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label lblEndCompDepth;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label lblMaxUVDepth;

	public override void Init()
	{
		base.Init();
		lblGroupDepth = (XUiV_Label)GetChildById("lblGroupDepth").GetChildById("value").ViewComponent;
		lblComplexity = (XUiV_Label)GetChildById("lblComplexity").GetChildById("value").ViewComponent;
		lblDescCount = (XUiV_Label)GetChildById("lblDescCount").GetChildById("value").ViewComponent;
		lblWarpCount = (XUiV_Label)GetChildById("lblWarpCount").GetChildById("value").ViewComponent;
		lblDrawCount = (XUiV_Label)GetChildById("lblDrawCount").GetChildById("value").ViewComponent;
		lblBgnCompDepth = (XUiV_Label)GetChildById("lblBgnCompDepth").GetChildById("value").ViewComponent;
		lblMaxCompDepth = (XUiV_Label)GetChildById("lblMaxCompDepth").GetChildById("value").ViewComponent;
		lblEndCompDepth = (XUiV_Label)GetChildById("lblEndCompDepth").GetChildById("value").ViewComponent;
		lblMaxUVDepth = (XUiV_Label)GetChildById("lblMaxUVDepth").GetChildById("value").ViewComponent;
	}

	public void Clear()
	{
		SetAll("-");
	}

	public void Populate(in LayerComplexityInfo layerComplexityInfo)
	{
		XUiV_Label xUiV_Label = lblGroupDepth;
		int groupDepth = layerComplexityInfo.GroupDepth;
		xUiV_Label.Text = groupDepth.ToString();
		XUiV_Label xUiV_Label2 = lblComplexity;
		float complexity = layerComplexityInfo.Complexity;
		xUiV_Label2.Text = complexity.ToString();
		XUiV_Label xUiV_Label3 = lblDescCount;
		groupDepth = layerComplexityInfo.DescriptorCount;
		xUiV_Label3.Text = groupDepth.ToString();
		XUiV_Label xUiV_Label4 = lblWarpCount;
		groupDepth = layerComplexityInfo.WarpCount;
		xUiV_Label4.Text = groupDepth.ToString();
		XUiV_Label xUiV_Label5 = lblDrawCount;
		groupDepth = layerComplexityInfo.DrawCount;
		xUiV_Label5.Text = groupDepth.ToString();
		XUiV_Label xUiV_Label6 = lblBgnCompDepth;
		groupDepth = layerComplexityInfo.StartCompStackIndex;
		xUiV_Label6.Text = groupDepth.ToString();
		XUiV_Label xUiV_Label7 = lblMaxCompDepth;
		groupDepth = layerComplexityInfo.MaxCompStackIndex;
		xUiV_Label7.Text = groupDepth.ToString();
		XUiV_Label xUiV_Label8 = lblEndCompDepth;
		groupDepth = layerComplexityInfo.EndCompStackIndex;
		xUiV_Label8.Text = groupDepth.ToString();
		XUiV_Label xUiV_Label9 = lblMaxUVDepth;
		groupDepth = layerComplexityInfo.MaxUVStackIndex;
		xUiV_Label9.Text = groupDepth.ToString();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetAll(string value)
	{
		lblGroupDepth.Text = value;
		lblComplexity.Text = value;
		lblDescCount.Text = value;
		lblWarpCount.Text = value;
		lblDrawCount.Text = value;
		lblBgnCompDepth.Text = value;
		lblMaxCompDepth.Text = value;
		lblEndCompDepth.Text = value;
		lblMaxUVDepth.Text = value;
	}
}
