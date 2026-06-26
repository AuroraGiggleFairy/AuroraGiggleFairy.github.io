using System;
using System.Collections.Generic;

public class TileEntityCompositeData
{
	public class MemStringEqualityComparer : IEqualityComparer<ReadOnlyMemory<char>>
	{
		public static readonly MemStringEqualityComparer Instance = new MemStringEqualityComparer();

		[PublicizedFrom(EAccessModifier.Private)]
		public MemStringEqualityComparer()
		{
		}

		public int GetHashCode(ReadOnlyMemory<char> _obj)
		{
			return _obj.Span.GetStableHashCode();
		}

		public bool Equals(ReadOnlyMemory<char> _x, ReadOnlyMemory<char> _y)
		{
			return _x.Span.Equals(_y.Span, StringComparison.Ordinal);
		}
	}

	public static readonly Dictionary<BlockCompositeTileEntity, TileEntityCompositeData> FeaturesByBlock = new Dictionary<BlockCompositeTileEntity, TileEntityCompositeData>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Dictionary<string, Type> knownFeatures = new Dictionary<string, Type>();

	public readonly BlockCompositeTileEntity Block;

	public readonly DynamicProperties CompositeProps;

	public readonly List<TileEntityFeatureData> Features = new List<TileEntityFeatureData>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<Type, int> featureIndexByType = new Dictionary<Type, int>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<ReadOnlyMemory<char>, int> featureIndexByName = new Dictionary<ReadOnlyMemory<char>, int>(MemStringEqualityComparer.Instance);

	[PublicizedFrom(EAccessModifier.Private)]
	public static void Init()
	{
		if (knownFeatures.Count > 0)
		{
			return;
		}
		ReflectionHelpers.FindTypesImplementingBase(typeof(ITileEntityFeature), [PublicizedFrom(EAccessModifier.Internal)] (Type _type) =>
		{
			string name = _type.Name;
			if (knownFeatures.TryGetValue(name, out var value))
			{
				Log.Warning("Redeclaration of CompositeTileEntity feature " + name + ": " + value.FullName + " vs " + _type.FullName);
			}
			else if (_type.GetConstructor(Type.EmptyTypes) == null)
			{
				Log.Warning("CompositeTileEntity feature " + name + " has no parameterless constructor!");
			}
			else
			{
				knownFeatures[name] = _type;
			}
		});
	}

	public static void Cleanup()
	{
		FeaturesByBlock.Clear();
	}

	public static TileEntityCompositeData ParseBlock(BlockCompositeTileEntity _block)
	{
		if (!_block.Properties.Classes.TryGetValue("CompositeFeatures", out var _value))
		{
			throw new ArgumentException("Block " + _block.GetBlockName() + " uses class BlockCompositeTileEntity but has no CompositeFeatures property");
		}
		return new TileEntityCompositeData(_block, _value);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public TileEntityCompositeData(BlockCompositeTileEntity _block, DynamicProperties _compositeProps)
	{
		Init();
		Block = _block;
		CompositeProps = _compositeProps;
		int num = 0;
		foreach (var (text2, props) in _compositeProps.Classes.Dict)
		{
			if (!knownFeatures.TryGetValue(text2, out var value))
			{
				throw new ArgumentException("Block \"" + _block.GetBlockName() + "\": CompositeFeature class \"" + text2 + "\" not found!");
			}
			TileEntityFeatureData item = new TileEntityFeatureData(this, text2, num, value, props);
			Features.Add(item);
			num++;
		}
		if (Features.Count == 0)
		{
			throw new ArgumentException("Block \"" + _block.GetBlockName() + "\": No CompositeFeatures specified!");
		}
		Features.Sort(TileEntityFeatureData.FeatureDataSorterByName.Instance);
		for (int i = 0; i < Features.Count; i++)
		{
			featureIndexByName[Features[i].Name.AsMemory()] = i;
		}
		FeaturesByBlock[_block] = this;
	}

	public int GetFeatureIndex(ReadOnlyMemory<char> _featureName)
	{
		return featureIndexByName.GetValueOrDefault(_featureName, -1);
	}

	public int GetFeatureIndex<T>() where T : class
	{
		Type typeFromHandle = typeof(T);
		if (featureIndexByType.TryGetValue(typeFromHandle, out var value))
		{
			return value;
		}
		for (int i = 0; i < Features.Count; i++)
		{
			TileEntityFeatureData tileEntityFeatureData = Features[i];
			if (typeFromHandle.IsAssignableFrom(tileEntityFeatureData.Type))
			{
				featureIndexByType[typeFromHandle] = i;
				return i;
			}
		}
		featureIndexByType[typeFromHandle] = -1;
		return -1;
	}

	public bool HasFeature(ReadOnlyMemory<char> _featureName)
	{
		return GetFeatureIndex(_featureName) >= 0;
	}

	public bool HasFeature<T>() where T : class
	{
		return GetFeatureIndex<T>() >= 0;
	}

	public void PrintConfig()
	{
		Log.Out("Composite block: " + Block.GetBlockName() + ":");
		string value;
		string key;
		foreach (KeyValuePair<string, string> item in CompositeProps.Values.Dict)
		{
			item.Deconstruct(out value, out key);
			string text = value;
			string text2 = key;
			Log.Out("    " + text + "=" + text2);
		}
		foreach (TileEntityFeatureData feature in Features)
		{
			Log.Out($"  Feature: {feature.Name} (class {feature.Type.FullName} in assembly ({feature.Type.Assembly.FullName})), manual order = {feature.CustomOrder}:");
			foreach (KeyValuePair<string, string> item2 in feature.Props.Values.Dict)
			{
				item2.Deconstruct(out key, out value);
				string text3 = key;
				string text4 = value;
				Log.Out("    " + text3 + "=" + text4);
			}
		}
	}
}
