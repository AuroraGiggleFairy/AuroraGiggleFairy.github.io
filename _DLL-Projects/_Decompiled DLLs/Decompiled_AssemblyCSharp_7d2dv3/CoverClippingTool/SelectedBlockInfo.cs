using System;
using System.Collections.Generic;
using UnityEngine;

namespace CoverClippingTool;

[Serializable]
public struct SelectedBlockInfo
{
	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public BlockShapeInfo _shapeInfo = null;

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public ShapeReference _savedShapeReference = default(ShapeReference);

	public readonly List<Renderable> renderables = new List<Renderable>();

	public BlockShapeInfo shapeInfo
	{
		get
		{
			return _shapeInfo;
		}
		set
		{
			_shapeInfo = value;
			_savedShapeReference.Name = _shapeInfo?.Name;
			_savedShapeReference.Source = ((_shapeInfo != null) ? _shapeInfo.Source : DataSource.Block);
		}
	}

	public ShapeReference savedShapeReference => _savedShapeReference;

	public SelectedBlockInfo(int thisIsDumb)
	{
	}
}
