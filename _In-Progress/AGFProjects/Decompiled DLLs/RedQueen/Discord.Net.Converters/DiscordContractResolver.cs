using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Discord.API;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Discord.Net.Converters;

internal class DiscordContractResolver : DefaultContractResolver
{
	private static readonly TypeInfo _ienumerable = typeof(IEnumerable<ulong[]>).GetTypeInfo();

	private static readonly MethodInfo _shouldSerialize = typeof(DiscordContractResolver).GetTypeInfo().GetDeclaredMethod("ShouldSerialize");

	protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
	{
		JsonProperty jsonProperty = base.CreateProperty(member, memberSerialization);
		if (jsonProperty.Ignored)
		{
			return jsonProperty;
		}
		if (member is PropertyInfo propertyInfo)
		{
			JsonConverter converter = GetConverter(jsonProperty, propertyInfo, propertyInfo.PropertyType, 0);
			if (converter != null)
			{
				jsonProperty.Converter = converter;
			}
			return jsonProperty;
		}
		throw new InvalidOperationException(member.DeclaringType.FullName + "." + member.Name + " is not a property.");
	}

	private static JsonConverter GetConverter(JsonProperty property, PropertyInfo propInfo, Type type, int depth)
	{
		if (type.IsArray)
		{
			return MakeGenericConverter(property, propInfo, typeof(ArrayConverter<>), type.GetElementType(), depth);
		}
		if (type.IsConstructedGenericType)
		{
			Type genericTypeDefinition = type.GetGenericTypeDefinition();
			if (depth == 0 && genericTypeDefinition == typeof(Optional<>))
			{
				Type declaringType = propInfo.DeclaringType;
				Type type2 = type.GenericTypeArguments[0];
				Type delegateType = typeof(Func<, >).MakeGenericType(declaringType, type);
				Delegate getterDelegate = propInfo.GetMethod.CreateDelegate(delegateType);
				MethodInfo methodInfo = _shouldSerialize.MakeGenericMethod(declaringType, type2);
				Func<object, Delegate, bool> shouldSerializeDelegate = (Func<object, Delegate, bool>)methodInfo.CreateDelegate(typeof(Func<object, Delegate, bool>));
				property.ShouldSerialize = [_003C288c0fe8_002D6a6b_002D477e_002Da575_002De26c8b9edf09_003ENullableContext(1)] (object x) => shouldSerializeDelegate(x, getterDelegate);
				return MakeGenericConverter(property, propInfo, typeof(OptionalConverter<>), type2, depth);
			}
			if (genericTypeDefinition == typeof(Nullable<>))
			{
				return MakeGenericConverter(property, propInfo, typeof(NullableConverter<>), type.GenericTypeArguments[0], depth);
			}
			if (genericTypeDefinition == typeof(EntityOrId<>))
			{
				return MakeGenericConverter(property, propInfo, typeof(UInt64EntityOrIdConverter<>), type.GenericTypeArguments[0], depth);
			}
		}
		if (propInfo.GetCustomAttribute<Int53Attribute>() == null && type == typeof(ulong))
		{
			return UInt64Converter.Instance;
		}
		if (propInfo.GetCustomAttribute<UnixTimestampAttribute>() != null && type == typeof(DateTimeOffset))
		{
			return UnixTimestampConverter.Instance;
		}
		if (type == typeof(UserStatus))
		{
			return UserStatusConverter.Instance;
		}
		if (type == typeof(EmbedType))
		{
			return EmbedTypeConverter.Instance;
		}
		if (type == typeof(Discord.API.Image))
		{
			return ImageConverter.Instance;
		}
		if (typeof(IMessageComponent).IsAssignableFrom(type))
		{
			return MessageComponentConverter.Instance;
		}
		if (type == typeof(Interaction))
		{
			return InteractionConverter.Instance;
		}
		if (type == typeof(Discord.API.DiscordError))
		{
			return DiscordErrorConverter.Instance;
		}
		if (type == typeof(GuildFeatures))
		{
			return GuildFeaturesConverter.Instance;
		}
		TypeInfo typeInfo = type.GetTypeInfo();
		if (typeInfo.ImplementedInterfaces.Any((Type x) => x == typeof(IEntity<ulong>)))
		{
			return UInt64EntityConverter.Instance;
		}
		if (typeInfo.ImplementedInterfaces.Any((Type x) => x == typeof(IEntity<string>)))
		{
			return StringEntityConverter.Instance;
		}
		return null;
	}

	private static bool ShouldSerialize<TOwner, TValue>(object owner, Delegate getter)
	{
		return (getter as Func<TOwner, Optional<TValue>>)((TOwner)owner).IsSpecified;
	}

	private static JsonConverter MakeGenericConverter(JsonProperty property, PropertyInfo propInfo, Type converterType, Type innerType, int depth)
	{
		TypeInfo typeInfo = converterType.MakeGenericType(innerType).GetTypeInfo();
		JsonConverter converter = GetConverter(property, propInfo, innerType, depth + 1);
		return typeInfo.DeclaredConstructors.First().Invoke(new object[1] { converter }) as JsonConverter;
	}
}
