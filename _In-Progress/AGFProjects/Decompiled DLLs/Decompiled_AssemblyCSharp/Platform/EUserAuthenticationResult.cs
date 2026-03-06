namespace Platform;

public enum EUserAuthenticationResult
{
	Ok = 0,
	UserNotConnectedToPlatform = 1,
	NoLicenseOrExpired = 2,
	PlatformBanned = 3,
	LoggedInElseWhere = 4,
	PlatformBanCheckTimedOut = 5,
	AuthTicketCanceled = 6,
	AuthTicketInvalidAlreadyUsed = 7,
	AuthTicketInvalid = 8,
	PublisherIssuedBan = 9,
	EosTicketFailed = 50
}
