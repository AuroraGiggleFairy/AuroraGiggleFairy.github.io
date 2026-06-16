public interface IProtocolManagerProtocolInterface
{
	bool IsServer { get; }

	bool IsClient { get; }

	void InvalidPasswordEv();

	void ConnectionFailedEv(string _msg);

	void DisconnectedFromServerEv(string _msg);
}
