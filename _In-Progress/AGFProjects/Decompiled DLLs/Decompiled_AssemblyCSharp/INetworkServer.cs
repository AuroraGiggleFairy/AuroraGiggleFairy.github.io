using UnityEngine.Networking;

public interface INetworkServer
{
	void Update();

	void LateUpdate();

	NetworkConnectionError StartServer(int _basePort, string _password);

	void SetServerPassword(string _password);

	void StopServer();

	void DropClient(ClientInfo _clientInfo, bool _clientDisconnect);

	NetworkError SendData(ClientInfo _clientInfo, int _channel, ArrayListMP<byte> _data, bool reliableDelivery = true);

	string GetIP(ClientInfo _cInfo);

	int GetPing(ClientInfo _cInfo);

	string GetServerPorts(int _basePort);

	void SetLatencySimulation(bool _enable, int _min, int _max);

	void SetPacketLossSimulation(bool _enable, int _chance);

	void EnableStatistics();

	void DisableStatistics();

	string PrintNetworkStatistics();

	void ResetNetworkStatistics();

	int GetMaximumPacketSize(ClientInfo _cInfo, bool reliable = false);

	int GetBadPacketCount(ClientInfo _cInfo);
}
