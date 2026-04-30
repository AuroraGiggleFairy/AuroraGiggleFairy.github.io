using System.Runtime.CompilerServices;

namespace Newtonsoft.Json.Utilities;

[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(0)]
[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(2)]
internal class FSharpFunction
{
	private readonly object _instance;

	[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(new byte[] { 1, 2, 1 })]
	private readonly MethodCall<object, object> _invoker;

	public FSharpFunction(object instance, [_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(new byte[] { 1, 2, 1 })] MethodCall<object, object> invoker)
	{
		_instance = instance;
		_invoker = invoker;
	}

	[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(1)]
	public object Invoke(params object[] args)
	{
		return _invoker(_instance, args);
	}
}
