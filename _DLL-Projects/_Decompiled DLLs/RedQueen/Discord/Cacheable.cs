using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Discord;

internal struct Cacheable<TEntity, TId> where TEntity : IEntity<TId> where TId : IEquatable<TId>
{
	public bool HasValue
	{
		[_003C565f4ed8_002D6c7c_002D496e_002D81c6_002D6ecd6b2b714c_003EIsReadOnly]
		get;
	}

	public TId Id
	{
		[_003C565f4ed8_002D6c7c_002D496e_002D81c6_002D6ecd6b2b714c_003EIsReadOnly]
		get;
	}

	public TEntity Value
	{
		[_003C565f4ed8_002D6c7c_002D496e_002D81c6_002D6ecd6b2b714c_003EIsReadOnly]
		get;
	}

	private Func<Task<TEntity>> DownloadFunc
	{
		[_003C565f4ed8_002D6c7c_002D496e_002D81c6_002D6ecd6b2b714c_003EIsReadOnly]
		get;
	}

	internal Cacheable(TEntity value, TId id, bool hasValue, Func<Task<TEntity>> downloadFunc)
	{
		Value = value;
		Id = id;
		HasValue = hasValue;
		DownloadFunc = downloadFunc;
	}

	public async Task<TEntity> DownloadAsync()
	{
		return await DownloadFunc().ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<TEntity> GetOrDownloadAsync()
	{
		return (!HasValue) ? (await DownloadAsync().ConfigureAwait(continueOnCapturedContext: false)) : Value;
	}
}
internal struct Cacheable<TCachedEntity, TDownloadableEntity, TRelationship, TId> where TCachedEntity : IEntity<TId>, TRelationship where TDownloadableEntity : IEntity<TId>, TRelationship where TId : IEquatable<TId>
{
	public bool HasValue
	{
		[_003C565f4ed8_002D6c7c_002D496e_002D81c6_002D6ecd6b2b714c_003EIsReadOnly]
		get;
	}

	public TId Id
	{
		[_003C565f4ed8_002D6c7c_002D496e_002D81c6_002D6ecd6b2b714c_003EIsReadOnly]
		get;
	}

	public TCachedEntity Value
	{
		[_003C565f4ed8_002D6c7c_002D496e_002D81c6_002D6ecd6b2b714c_003EIsReadOnly]
		get;
	}

	private Func<Task<TDownloadableEntity>> DownloadFunc
	{
		[_003C565f4ed8_002D6c7c_002D496e_002D81c6_002D6ecd6b2b714c_003EIsReadOnly]
		get;
	}

	internal Cacheable(TCachedEntity value, TId id, bool hasValue, Func<Task<TDownloadableEntity>> downloadFunc)
	{
		Value = value;
		Id = id;
		HasValue = hasValue;
		DownloadFunc = downloadFunc;
	}

	public async Task<TDownloadableEntity> DownloadAsync()
	{
		return await DownloadFunc().ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<TRelationship> GetOrDownloadAsync()
	{
		return (!HasValue) ? ((TRelationship)(object)(await DownloadAsync().ConfigureAwait(continueOnCapturedContext: false))) : ((TRelationship)(object)Value);
	}
}
