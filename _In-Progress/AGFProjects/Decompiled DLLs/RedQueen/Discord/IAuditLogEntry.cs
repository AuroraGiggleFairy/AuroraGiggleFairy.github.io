namespace Discord;

internal interface IAuditLogEntry : ISnowflakeEntity, IEntity<ulong>
{
	ActionType Action { get; }

	IAuditLogData Data { get; }

	IUser User { get; }

	string Reason { get; }
}
