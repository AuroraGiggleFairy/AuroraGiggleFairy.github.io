namespace Discord;

internal enum LoginState : byte
{
	LoggedOut,
	LoggingIn,
	LoggedIn,
	LoggingOut
}
