namespace Platform;

public enum EBeginUserAuthenticationResult
{
	Ok,
	InvalidTicket,
	DuplicateRequest,
	InvalidVersion,
	GameMismatch,
	ExpiredTicket
}
