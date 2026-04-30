using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public abstract class ConsoleCmdTeleportsAbs : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public bool TryParseV3i(List<string> _params, int _startIndex, out Vector3i _result)
	{
		_result = default(Vector3i);
		if (!int.TryParse(_params[_startIndex], out _result.x))
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("x argument is not a valid integer");
			return false;
		}
		if (!int.TryParse(_params[_startIndex + 1], out _result.y))
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("y argument is not a valid integer");
			return false;
		}
		if (!int.TryParse(_params[_startIndex + 2], out _result.z))
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("z argument is not a valid integer");
			return false;
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3? TryParseViewDirection(string _viewDirectionString)
	{
		if (_viewDirectionString.EqualsCaseInsensitive("n") || _viewDirectionString.EqualsCaseInsensitive("north"))
		{
			return new Vector3(0f, 0f, 0f);
		}
		if (_viewDirectionString.EqualsCaseInsensitive("ne") || _viewDirectionString.EqualsCaseInsensitive("northeast"))
		{
			return new Vector3(0f, 45f, 0f);
		}
		if (_viewDirectionString.EqualsCaseInsensitive("e") || _viewDirectionString.EqualsCaseInsensitive("east"))
		{
			return new Vector3(0f, 90f, 0f);
		}
		if (_viewDirectionString.EqualsCaseInsensitive("se") || _viewDirectionString.EqualsCaseInsensitive("southeast"))
		{
			return new Vector3(0f, 135f, 0f);
		}
		if (_viewDirectionString.EqualsCaseInsensitive("s") || _viewDirectionString.EqualsCaseInsensitive("south"))
		{
			return new Vector3(0f, 180f, 0f);
		}
		if (_viewDirectionString.EqualsCaseInsensitive("sw") || _viewDirectionString.EqualsCaseInsensitive("southwest"))
		{
			return new Vector3(0f, 225f, 0f);
		}
		if (_viewDirectionString.EqualsCaseInsensitive("w") || _viewDirectionString.EqualsCaseInsensitive("west"))
		{
			return new Vector3(0f, 270f, 0f);
		}
		if (_viewDirectionString.EqualsCaseInsensitive("nw") || _viewDirectionString.EqualsCaseInsensitive("northwest"))
		{
			return new Vector3(0f, 315f, 0f);
		}
		return null;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public EntityPlayer GetExecutingEntityPlayer(CommandSenderInfo _senderInfo)
	{
		if (_senderInfo.IsLocalGame)
		{
			return GameManager.Instance.World.GetPrimaryPlayer();
		}
		if (_senderInfo.RemoteClientInfo != null)
		{
			return GameManager.Instance.World.Players.dict[_senderInfo.RemoteClientInfo.entityId];
		}
		return null;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool TryGetDestinationFromPlayer(string _targetPlayerString, out Vector3 _destination)
	{
		_destination = default(Vector3);
		ClientInfo clientInfo = ConsoleHelper.ParseParamIdOrName(_targetPlayerString);
		EntityPlayer entityPlayer;
		if (clientInfo == null)
		{
			if (GameManager.IsDedicatedServer || !ConsoleHelper.ParamIsLocalPlayer(_targetPlayerString))
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Target playername or entity/userid id not found.");
				return false;
			}
			entityPlayer = GameManager.Instance.World.GetPrimaryPlayer();
		}
		else
		{
			entityPlayer = GameManager.Instance.World.Players.dict[clientInfo.entityId];
		}
		_destination = entityPlayer.GetPosition();
		_destination.y += 1f;
		_destination.z += 1f;
		return true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void ExecuteTeleport(ClientInfo _player, Vector3 _destPos, Vector3? _viewDirection)
	{
		ThreadManager.StartCoroutine(ExecuteTeleportCo(_player, _destPos, _viewDirection));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator ExecuteTeleportCo(ClientInfo _player, Vector3 _destPos, Vector3? _viewDirection)
	{
		int playerId = _player?.entityId ?? ((GameManager.Instance.GetPersistentLocalPlayer() != null) ? GameManager.Instance.GetPersistentLocalPlayer().EntityId : (-1));
		yield return GameManager.Instance.ResetWindowsAndLocksByPlayer(playerId);
		NetPackageTeleportPlayer netPackageTeleportPlayer = NetPackageManager.GetPackage<NetPackageTeleportPlayer>().Setup(_destPos, _viewDirection);
		if (_player == null)
		{
			netPackageTeleportPlayer.ProcessPackage(GameManager.Instance.World, GameManager.Instance);
		}
		else
		{
			_player.SendPackage(netPackageTeleportPlayer);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public ConsoleCmdTeleportsAbs()
	{
	}
}
