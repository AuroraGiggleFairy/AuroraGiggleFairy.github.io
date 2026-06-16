using System.Collections.Generic;

namespace Platform;

public interface IServerListInterface
{
	public class ServerFilter
	{
		public enum EServerFilterType
		{
			Any,
			BoolValue,
			IntValue,
			IntNotValue,
			IntMin,
			IntMax,
			IntRange,
			StringValue,
			StringContains
		}

		public readonly string Name;

		public readonly EServerFilterType Type;

		public readonly int IntMinValue;

		public readonly int IntMaxValue;

		public readonly bool BoolValue;

		public readonly string StringNeedle;

		public ServerFilter(string _name, EServerFilterType _type = EServerFilterType.Any, int _intMinValue = 0, int _intMaxValue = 0, bool _boolValue = false, string _stringNeedle = null)
		{
			Name = _name;
			Type = _type;
			IntMinValue = _intMinValue;
			IntMaxValue = _intMaxValue;
			BoolValue = _boolValue;
			StringNeedle = _stringNeedle;
		}
	}

	bool IsPrefiltered { get; }

	bool IsRefreshing { get; }

	void Init(IPlatform _owner);

	void RegisterGameServerFoundCallback(GameServerFoundCallback _serverFound, MaxResultsReachedCallback _maxResultsCallback, ServerSearchErrorCallback _sessionSearchErrorCallback);

	void StartSearch(IList<ServerFilter> _activeFilters);

	void StopSearch();

	void Disconnect();

	void GetSingleServerDetails(GameServerInfo _serverInfo, EServerRelationType _relation, GameServerFoundCallback _callback);
}
