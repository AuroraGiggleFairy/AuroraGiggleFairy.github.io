namespace GameSparks.Platforms;

public interface IControlledWebSocket : IGameSparksWebSocket
{
	int SocketId { get; }

	void TriggerOnClose();

	void TriggerOnOpen();

	void TriggerOnError(string message);

	void TriggerOnMessage(string message);

	bool Update();
}
