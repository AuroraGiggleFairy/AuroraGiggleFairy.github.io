using UnityEngine.Networking;

public interface INetworkClient
{
	void Update();

	void LateUpdate();

	void Connect(GameServerInfo _gsi);

	void Disconnect();

	NetworkError SendData(int _channel, ArrayListMP<byte> _data);

	void SetLatencySimulation(bool _enable, int _min, int _max);

	void SetPacketLossSimulation(bool _enable, int _chance);

	void EnableStatistics();

	void DisableStatistics();

	string PrintNetworkStatistics();

	void ResetNetworkStatistics();
}
