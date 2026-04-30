using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Serialization;

[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(2)]
[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(0)]
internal class JsonArrayContract : JsonContainerContract
{
	private readonly Type _genericCollectionDefinitionType;

	private Type _genericWrapperType;

	[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(new byte[] { 2, 1 })]
	private ObjectConstructor<object> _genericWrapperCreator;

	[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(new byte[] { 2, 1 })]
	private Func<object> _genericTemporaryCollectionCreator;

	private readonly ConstructorInfo _parameterizedConstructor;

	[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(new byte[] { 2, 1 })]
	private ObjectConstructor<object> _parameterizedCreator;

	[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(new byte[] { 2, 1 })]
	private ObjectConstructor<object> _overrideCreator;

	public Type CollectionItemType { get; }

	public bool IsMultidimensionalArray { get; }

	internal bool IsArray { get; }

	internal bool ShouldCreateWrapper { get; }

	internal bool CanDeserialize { get; private set; }

	[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(new byte[] { 2, 1 })]
	internal ObjectConstructor<object> ParameterizedCreator
	{
		[return: _003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(new byte[] { 2, 1 })]
		get
		{
			if (_parameterizedCreator == null && _parameterizedConstructor != null)
			{
				_parameterizedCreator = JsonTypeReflector.ReflectionDelegateFactory.CreateParameterizedConstructor(_parameterizedConstructor);
			}
			return _parameterizedCreator;
		}
	}

	[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(new byte[] { 2, 1 })]
	public ObjectConstructor<object> OverrideCreator
	{
		[return: _003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(new byte[] { 2, 1 })]
		get
		{
			return _overrideCreator;
		}
		[param: _003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(new byte[] { 2, 1 })]
		set
		{
			_overrideCreator = value;
			CanDeserialize = true;
		}
	}

	public bool HasParameterizedCreator { get; set; }

	internal bool HasParameterizedCreatorInternal
	{
		get
		{
			if (!HasParameterizedCreator && _parameterizedCreator == null)
			{
				return _parameterizedConstructor != null;
			}
			return true;
		}
	}

	[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(1)]
	public JsonArrayContract(Type underlyingType)
		: base(underlyingType)
	{
		ContractType = JsonContractType.Array;
		IsArray = base.CreatedType.IsArray || (NonNullableUnderlyingType.IsGenericType() && NonNullableUnderlyingType.GetGenericTypeDefinition().FullName == "System.Linq.EmptyPartition`1");
		bool canDeserialize;
		Type implementingType;
		if (IsArray)
		{
			CollectionItemType = ReflectionUtils.GetCollectionItemType(base.UnderlyingType);
			IsReadOnlyOrFixedSize = true;
			_genericCollectionDefinitionType = typeof(List<>).MakeGenericType(CollectionItemType);
			canDeserialize = true;
			IsMultidimensionalArray = base.CreatedType.IsArray && base.UnderlyingType.GetArrayRank() > 1;
		}
		else if (typeof(IList).IsAssignableFrom(NonNullableUnderlyingType))
		{
			if (ReflectionUtils.ImplementsGenericDefinition(NonNullableUnderlyingType, typeof(ICollection<>), out _genericCollectionDefinitionType))
			{
				CollectionItemType = _genericCollectionDefinitionType.GetGenericArguments()[0];
			}
			else
			{
				CollectionItemType = ReflectionUtils.GetCollectionItemType(NonNullableUnderlyingType);
			}
			if (NonNullableUnderlyingType == typeof(IList))
			{
				base.CreatedType = typeof(List<object>);
			}
			if (CollectionItemType != null)
			{
				_parameterizedConstructor = CollectionUtils.ResolveEnumerableCollectionConstructor(NonNullableUnderlyingType, CollectionItemType);
			}
			IsReadOnlyOrFixedSize = ReflectionUtils.InheritsGenericDefinition(NonNullableUnderlyingType, typeof(ReadOnlyCollection<>));
			canDeserialize = true;
		}
		else if (ReflectionUtils.ImplementsGenericDefinition(NonNullableUnderlyingType, typeof(ICollection<>), out _genericCollectionDefinitionType))
		{
			CollectionItemType = _genericCollectionDefinitionType.GetGenericArguments()[0];
			if (ReflectionUtils.IsGenericDefinition(NonNullableUnderlyingType, typeof(ICollection<>)) || ReflectionUtils.IsGenericDefinition(NonNullableUnderlyingType, typeof(IList<>)))
			{
				base.CreatedType = typeof(List<>).MakeGenericType(CollectionItemType);
			}
			if (ReflectionUtils.IsGenericDefinition(NonNullableUnderlyingType, typeof(ISet<>)))
			{
				base.CreatedType = typeof(HashSet<>).MakeGenericType(CollectionItemType);
			}
			_parameterizedConstructor = CollectionUtils.ResolveEnumerableCollectionConstructor(NonNullableUnderlyingType, CollectionItemType);
			canDeserialize = true;
			ShouldCreateWrapper = true;
		}
		else if (ReflectionUtils.ImplementsGenericDefinition(NonNullableUnderlyingType, typeof(IReadOnlyCollection<>), out implementingType))
		{
			CollectionItemType = implementingType.GetGenericArguments()[0];
			if (ReflectionUtils.IsGenericDefinition(NonNullableUnderlyingType, typeof(IReadOnlyCollection<>)) || ReflectionUtils.IsGenericDefinition(NonNullableUnderlyingType, typeof(IReadOnlyList<>)))
			{
				base.CreatedType = typeof(ReadOnlyCollection<>).MakeGenericType(CollectionItemType);
			}
			_genericCollectionDefinitionType = typeof(List<>).MakeGenericType(CollectionItemType);
			_parameterizedConstructor = CollectionUtils.ResolveEnumerableCollectionConstructor(base.CreatedType, CollectionItemType);
			StoreFSharpListCreatorIfNecessary(NonNullableUnderlyingType);
			IsReadOnlyOrFixedSize = true;
			canDeserialize = HasParameterizedCreatorInternal;
		}
		else if (ReflectionUtils.ImplementsGenericDefinition(NonNullableUnderlyingType, typeof(IEnumerable<>), out implementingType))
		{
			CollectionItemType = implementingType.GetGenericArguments()[0];
			if (ReflectionUtils.IsGenericDefinition(base.UnderlyingType, typeof(IEnumerable<>)))
			{
				base.CreatedType = typeof(List<>).MakeGenericType(CollectionItemType);
			}
			_parameterizedConstructor = CollectionUtils.ResolveEnumerableCollectionConstructor(NonNullableUnderlyingType, CollectionItemType);
			StoreFSharpListCreatorIfNecessary(NonNullableUnderlyingType);
			if (NonNullableUnderlyingType.IsGenericType() && NonNullableUnderlyingType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
			{
				_genericCollectionDefinitionType = implementingType;
				IsReadOnlyOrFixedSize = false;
				ShouldCreateWrapper = false;
				canDeserialize = true;
			}
			else
			{
				_genericCollectionDefinitionType = typeof(List<>).MakeGenericType(CollectionItemType);
				IsReadOnlyOrFixedSize = true;
				ShouldCreateWrapper = true;
				canDeserialize = HasParameterizedCreatorInternal;
			}
		}
		else
		{
			canDeserialize = false;
			ShouldCreateWrapper = true;
		}
		CanDeserialize = canDeserialize;
		if (CollectionItemType != null && ImmutableCollectionsUtils.TryBuildImmutableForArrayContract(NonNullableUnderlyingType, CollectionItemType, out var createdType, out var parameterizedCreator))
		{
			base.CreatedType = createdType;
			_parameterizedCreator = parameterizedCreator;
			IsReadOnlyOrFixedSize = true;
			CanDeserialize = true;
		}
	}

	[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(1)]
	internal IWrappedCollection CreateWrapper(object list)
	{
		if (_genericWrapperCreator == null)
		{
			_genericWrapperType = typeof(CollectionWrapper<>).MakeGenericType(CollectionItemType);
			Type type = ((!ReflectionUtils.InheritsGenericDefinition(_genericCollectionDefinitionType, typeof(List<>)) && !(_genericCollectionDefinitionType.GetGenericTypeDefinition() == typeof(IEnumerable<>))) ? _genericCollectionDefinitionType : typeof(ICollection<>).MakeGenericType(CollectionItemType));
			ConstructorInfo constructor = _genericWrapperType.GetConstructor(new Type[1] { type });
			_genericWrapperCreator = JsonTypeReflector.ReflectionDelegateFactory.CreateParameterizedConstructor(constructor);
		}
		return (IWrappedCollection)_genericWrapperCreator(list);
	}

	[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(1)]
	internal IList CreateTemporaryCollection()
	{
		if (_genericTemporaryCollectionCreator == null)
		{
			Type type = ((IsMultidimensionalArray || CollectionItemType == null) ? typeof(object) : CollectionItemType);
			Type type2 = typeof(List<>).MakeGenericType(type);
			_genericTemporaryCollectionCreator = JsonTypeReflector.ReflectionDelegateFactory.CreateDefaultConstructor<object>(type2);
		}
		return (IList)_genericTemporaryCollectionCreator();
	}

	[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(1)]
	private void StoreFSharpListCreatorIfNecessary(Type underlyingType)
	{
		if (!HasParameterizedCreatorInternal && underlyingType.Name == "FSharpList`1")
		{
			FSharpUtils.EnsureInitialized(underlyingType.Assembly());
			_parameterizedCreator = FSharpUtils.Instance.CreateSeq(CollectionItemType);
		}
	}
}
