using System.Runtime.CompilerServices;

namespace Discord.API;

internal struct EntityOrId<T>
{
	public ulong Id
	{
		[_003C93e5c5cd_002Dbc90_002D4083_002D83bc_002Df9a3bc2f6df9_003EIsReadOnly]
		get;
	}

	public T Object
	{
		[_003C93e5c5cd_002Dbc90_002D4083_002D83bc_002Df9a3bc2f6df9_003EIsReadOnly]
		get;
	}

	public EntityOrId(ulong id)
	{
		Id = id;
		Object = default(T);
	}

	public EntityOrId(T obj)
	{
		Id = 0uL;
		Object = obj;
	}
}
