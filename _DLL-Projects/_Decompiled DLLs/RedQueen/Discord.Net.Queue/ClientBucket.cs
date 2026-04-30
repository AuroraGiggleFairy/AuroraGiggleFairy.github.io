using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace Discord.Net.Queue;

internal struct ClientBucket
{
	private static readonly ImmutableDictionary<ClientBucketType, ClientBucket> DefsByType;

	private static readonly ImmutableDictionary<BucketId, ClientBucket> DefsById;

	public ClientBucketType Type
	{
		[_003C93e5c5cd_002Dbc90_002D4083_002D83bc_002Df9a3bc2f6df9_003EIsReadOnly]
		get;
	}

	public BucketId Id
	{
		[_003C93e5c5cd_002Dbc90_002D4083_002D83bc_002Df9a3bc2f6df9_003EIsReadOnly]
		get;
	}

	public int WindowCount
	{
		[_003C93e5c5cd_002Dbc90_002D4083_002D83bc_002Df9a3bc2f6df9_003EIsReadOnly]
		get;
	}

	public int WindowSeconds
	{
		[_003C93e5c5cd_002Dbc90_002D4083_002D83bc_002Df9a3bc2f6df9_003EIsReadOnly]
		get;
	}

	static ClientBucket()
	{
		ClientBucket[] array = new ClientBucket[2]
		{
			new ClientBucket(ClientBucketType.Unbucketed, BucketId.Create(null, "<unbucketed>", null), 10, 10),
			new ClientBucket(ClientBucketType.SendEdit, BucketId.Create(null, "<send_edit>", null), 10, 10)
		};
		ImmutableDictionary<ClientBucketType, ClientBucket>.Builder builder = ImmutableDictionary.CreateBuilder<ClientBucketType, ClientBucket>();
		ClientBucket[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			ClientBucket value = array2[i];
			builder.Add(value.Type, value);
		}
		DefsByType = builder.ToImmutable();
		ImmutableDictionary<BucketId, ClientBucket>.Builder builder2 = ImmutableDictionary.CreateBuilder<BucketId, ClientBucket>();
		array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			ClientBucket value2 = array2[i];
			builder2.Add(value2.Id, value2);
		}
		DefsById = builder2.ToImmutable();
	}

	public static ClientBucket Get(ClientBucketType type)
	{
		return DefsByType[type];
	}

	public static ClientBucket Get(BucketId id)
	{
		return DefsById[id];
	}

	public ClientBucket(ClientBucketType type, BucketId id, int count, int seconds)
	{
		Type = type;
		Id = id;
		WindowCount = count;
		WindowSeconds = seconds;
	}
}
