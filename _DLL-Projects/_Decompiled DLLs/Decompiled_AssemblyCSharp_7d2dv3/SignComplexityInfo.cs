using System.Collections.Generic;

public struct SignComplexityInfo
{
	public readonly float TotalComplexity;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<SignData.SignLayer, LayerComplexityInfo> ComplexityByLayer;

	public readonly LayerStackInfo StackInfo;

	public bool IsValid => ComplexityByLayer != null;

	public static SignComplexityInfo Invalid => new SignComplexityInfo(0f, null, default(LayerStackInfo));

	public SignComplexityInfo(float totalComplexity, Dictionary<SignData.SignLayer, LayerComplexityInfo> complexityByLayer, LayerStackInfo stackInfo)
	{
		TotalComplexity = totalComplexity;
		ComplexityByLayer = complexityByLayer;
		StackInfo = stackInfo;
	}

	public bool TryGetLayerComplexityInfo(SignData.SignLayer layer, out LayerComplexityInfo layerComplexityInfo)
	{
		layerComplexityInfo = default(LayerComplexityInfo);
		if (layer != null && ComplexityByLayer != null)
		{
			return ComplexityByLayer.TryGetValue(layer, out layerComplexityInfo);
		}
		return false;
	}
}
