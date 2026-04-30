namespace Platform.XBL;

public interface XblUser : IUserClient
{
	SocialManagerXbl SocialManager { get; }

	MultiplayerActivityQueryManager MultiplayerActivityQueryManager { get; }
}
