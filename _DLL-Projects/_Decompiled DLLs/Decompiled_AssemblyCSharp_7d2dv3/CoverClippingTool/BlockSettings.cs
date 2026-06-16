using System;
using UnityEngine;

namespace CoverClippingTool;

[Serializable]
public class BlockSettings
{
	public GameObject shapeModel;

	public BlockShapeInfo savedShapeInfo;

	public BlockSettings()
	{
		ResetToDefault();
	}

	public void ResetToDefault()
	{
		shapeModel = null;
		savedShapeInfo = null;
	}
}
