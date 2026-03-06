using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Newtonsoft.Json.Serialization;

namespace Newtonsoft.Json.Utilities;

[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(0)]
[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(1)]
internal class ReflectionObject
{
	[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(new byte[] { 2, 1 })]
	[field: _003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(new byte[] { 2, 1 })]
	public ObjectConstructor<object> Creator
	{
		[return: _003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(new byte[] { 2, 1 })]
		get;
	}

	public IDictionary<string, ReflectionMember> Members { get; }

	private ReflectionObject([_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(new byte[] { 2, 1 })] ObjectConstructor<object> creator)
	{
		Members = new Dictionary<string, ReflectionMember>();
		Creator = creator;
	}

	[return: _003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)]
	public object GetValue(object target, string member)
	{
		return Members[member].Getter(target);
	}

	public void SetValue(object target, string member, [_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] object value)
	{
		Members[member].Setter(target, value);
	}

	public Type GetType(string member)
	{
		return Members[member].MemberType;
	}

	public static ReflectionObject Create(Type t, params string[] memberNames)
	{
		return Create(t, null, memberNames);
	}

	public static ReflectionObject Create(Type t, [_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] MethodBase creator, params string[] memberNames)
	{
		ReflectionDelegateFactory reflectionDelegateFactory = JsonTypeReflector.ReflectionDelegateFactory;
		ObjectConstructor<object> creator2 = null;
		if (creator != null)
		{
			creator2 = reflectionDelegateFactory.CreateParameterizedConstructor(creator);
		}
		else if (ReflectionUtils.HasDefaultConstructor(t, nonPublic: false))
		{
			Func<object> ctor = reflectionDelegateFactory.CreateDefaultConstructor<object>(t);
			creator2 = ([_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(new byte[] { 1, 2 })] object[] args) => ctor();
		}
		ReflectionObject reflectionObject = new ReflectionObject(creator2);
		foreach (string text in memberNames)
		{
			MemberInfo[] member = t.GetMember(text, BindingFlags.Instance | BindingFlags.Public);
			if (member.Length != 1)
			{
				throw new ArgumentException("Expected a single member with the name '{0}'.".FormatWith(CultureInfo.InvariantCulture, text));
			}
			MemberInfo memberInfo = member.Single();
			ReflectionMember reflectionMember = new ReflectionMember();
			switch (memberInfo.MemberType())
			{
			case MemberTypes.Field:
			case MemberTypes.Property:
				if (ReflectionUtils.CanReadMemberValue(memberInfo, nonPublic: false))
				{
					reflectionMember.Getter = reflectionDelegateFactory.CreateGet<object>(memberInfo);
				}
				if (ReflectionUtils.CanSetMemberValue(memberInfo, nonPublic: false, canSetReadOnly: false))
				{
					reflectionMember.Setter = reflectionDelegateFactory.CreateSet<object>(memberInfo);
				}
				break;
			case MemberTypes.Method:
			{
				MethodInfo methodInfo = (MethodInfo)memberInfo;
				if (!methodInfo.IsPublic)
				{
					break;
				}
				ParameterInfo[] parameters = methodInfo.GetParameters();
				if (parameters.Length == 0 && methodInfo.ReturnType != typeof(void))
				{
					MethodCall<object, object> call = reflectionDelegateFactory.CreateMethodCall<object>(methodInfo);
					reflectionMember.Getter = [return: _003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] (object target) => call(target);
				}
				else if (parameters.Length == 1 && methodInfo.ReturnType == typeof(void))
				{
					MethodCall<object, object> call2 = reflectionDelegateFactory.CreateMethodCall<object>(methodInfo);
					reflectionMember.Setter = (object target, [_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] object arg) =>
					{
						call2(target, arg);
					};
				}
				break;
			}
			default:
				throw new ArgumentException("Unexpected member type '{0}' for member '{1}'.".FormatWith(CultureInfo.InvariantCulture, memberInfo.MemberType(), memberInfo.Name));
			}
			reflectionMember.MemberType = ReflectionUtils.GetMemberUnderlyingType(memberInfo);
			reflectionObject.Members[text] = reflectionMember;
		}
		return reflectionObject;
	}
}
