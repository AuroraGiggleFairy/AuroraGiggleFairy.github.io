using System;
using UnityEngine;

namespace WaterClippingTool;

[Serializable]
public class ShapeSettings : IEquatable<ShapeSettings>
{
	public string shapeName;

	public GameObject shapeModel;

	public Vector3 modelOffset;

	public Vector4 plane;

	public BlockFaceFlag waterFlowMask = BlockFaceFlag.All;

	public bool hasPlane => plane != WaterClippingPlanePlacer.DisabledPlaneVec;

	public ShapeSettings()
	{
		ResetToDefault();
	}

	public void ResetToDefault()
	{
		shapeName = string.Empty;
		shapeModel = null;
		modelOffset = WaterClippingPlanePlacer.DefaultModelOffset;
		plane = WaterClippingPlanePlacer.DisabledPlaneVec;
		waterFlowMask = BlockFaceFlag.All;
	}

	public void CopyFrom(ShapeSettings other)
	{
		shapeName = other.shapeName;
		shapeModel = other.shapeModel;
		modelOffset = other.modelOffset;
		plane = other.plane;
		waterFlowMask = other.waterFlowMask;
	}

	public bool Equals(ShapeSettings other)
	{
		if (plane != other.plane)
		{
			return false;
		}
		if (shapeName != other.shapeName)
		{
			return false;
		}
		if (shapeModel != other.shapeModel)
		{
			return false;
		}
		if (modelOffset != other.modelOffset)
		{
			return false;
		}
		if (waterFlowMask != other.waterFlowMask)
		{
			return false;
		}
		return true;
	}
}
