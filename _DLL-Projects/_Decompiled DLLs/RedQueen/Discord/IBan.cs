namespace Discord;

internal interface IBan
{
	IUser User { get; }

	string Reason { get; }
}
