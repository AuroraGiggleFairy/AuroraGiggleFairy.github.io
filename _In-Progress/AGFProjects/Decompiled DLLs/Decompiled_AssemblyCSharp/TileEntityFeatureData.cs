using System;
using System.Collections.Generic;

public class TileEntityFeatureData
{
	public class FeatureDataSorterByName : IComparer<TileEntityFeatureData>
	{
		public static readonly FeatureDataSorterByName Instance = new FeatureDataSorterByName();

		[PublicizedFrom(EAccessModifier.Private)]
		public FeatureDataSorterByName()
		{
		}

		public int Compare(TileEntityFeatureData _x, TileEntityFeatureData _y)
		{
			if (_x == _y)
			{
				return 0;
			}
			if (_y == null)
			{
				return 1;
			}
			if (_x == null)
			{
				return -1;
			}
			return string.Compare(_x.Name, _y.Name, StringComparison.Ordinal);
		}
	}

	public readonly TileEntityCompositeData Parent;

	public readonly string Name;

	public readonly int NameHash;

	public readonly int CustomOrder;

	public readonly Type Type;

	public readonly DynamicProperties Props;

	public TileEntityFeatureData(TileEntityCompositeData _parent, string _name, int _customOrder, Type _type, DynamicProperties _props)
	{
		Parent = _parent;
		Name = _name;
		NameHash = Name.GetStableHashCode();
		CustomOrder = _customOrder;
		Type = _type;
		Props = _props;
	}

	public ITileEntityFeature InstantiateModule()
	{
		return ReflectionHelpers.Instantiate<ITileEntityFeature>(Type);
	}
}
