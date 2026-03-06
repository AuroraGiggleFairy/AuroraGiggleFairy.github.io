using System;

namespace Discord;

internal interface IEntity<TId> where TId : IEquatable<TId>
{
	TId Id { get; }
}
