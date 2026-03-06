using System;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Serialization;

[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(0)]
[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(1)]
internal class DynamicValueProvider : IValueProvider
{
	private readonly MemberInfo _memberInfo;

	[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(new byte[] { 2, 1, 2 })]
	private Func<object, object> _getter;

	[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(new byte[] { 2, 1, 2 })]
	private Action<object, object> _setter;

	public DynamicValueProvider(MemberInfo memberInfo)
	{
		ValidationUtils.ArgumentNotNull(memberInfo, "memberInfo");
		_memberInfo = memberInfo;
	}

	public void SetValue(object target, [_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] object value)
	{
		try
		{
			if (_setter == null)
			{
				_setter = DynamicReflectionDelegateFactory.Instance.CreateSet<object>(_memberInfo);
			}
			_setter(target, value);
		}
		catch (Exception innerException)
		{
			throw new JsonSerializationException("Error setting value to '{0}' on '{1}'.".FormatWith(CultureInfo.InvariantCulture, _memberInfo.Name, target.GetType()), innerException);
		}
	}

	[return: _003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)]
	public object GetValue(object target)
	{
		try
		{
			if (_getter == null)
			{
				_getter = DynamicReflectionDelegateFactory.Instance.CreateGet<object>(_memberInfo);
			}
			return _getter(target);
		}
		catch (Exception innerException)
		{
			throw new JsonSerializationException("Error getting value from '{0}' on '{1}'.".FormatWith(CultureInfo.InvariantCulture, _memberInfo.Name, target.GetType()), innerException);
		}
	}
}
