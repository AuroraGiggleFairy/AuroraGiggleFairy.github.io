using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdSquareSpiral : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Private)]
	public enum WaitMode
	{
		SingleChunkDecorated,
		SurroundingMeshesCopied,
		SurroundingChunksDisplayed
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public enum SpiralSequenceState
	{
		Left,
		Down,
		Right,
		Up
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool m_isRunning;

	[PublicizedFrom(EAccessModifier.Private)]
	public Coroutine m_coroutine;

	[PublicizedFrom(EAccessModifier.Private)]
	public ChunkConditions.Delegate chunkCondition = ChunkConditions.Decorated;

	[PublicizedFrom(EAccessModifier.Private)]
	public WaitMode waitMode;

	public override bool IsExecuteOnClient => true;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[2] { "squarespiral", "sqs" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Move the player chunk by chunk in a square spiral. Will start off paused and required un-pausing. Also gives god mode and flying at the start.";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "<s[tart] [chunks per auto-pause]|p[ause]|r[eset]|waitmode [minimal|meshes|displayed]>";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (!ExecuteInternal(_params, _senderInfo))
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output(GetHelp());
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool ExecuteInternal(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (!GameManager.Instance.World.GetPrimaryPlayer())
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("No local player! (Are you in-game?)");
			return true;
		}
		if (_params.Count == 0)
		{
			return false;
		}
		switch (_params[0].ToLowerInvariant())
		{
		case "s":
		case "start":
		{
			if (_params.Count > 2)
			{
				return false;
			}
			int result = 0;
			if (_params.Count > 1 && !int.TryParse(_params[1], out result))
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Failed to parse as int " + _params[1]);
				return false;
			}
			MacroStart(result);
			return true;
		}
		case "p":
		case "pause":
			MacroPause();
			return true;
		case "r":
		case "reset":
			MacroReset();
			return true;
		case "waitmode":
			if (_params.Count > 1)
			{
				switch (_params[1].ToLowerInvariant())
				{
				case "minimal":
					waitMode = WaitMode.SingleChunkDecorated;
					chunkCondition = ChunkConditions.Decorated;
					return true;
				case "meshes":
					waitMode = WaitMode.SurroundingMeshesCopied;
					chunkCondition = ChunkConditions.MeshesCopied;
					return true;
				case "displayed":
					waitMode = WaitMode.SurroundingChunksDisplayed;
					chunkCondition = ChunkConditions.Displayed;
					return true;
				}
			}
			return false;
		default:
			return false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void MacroStart(int _chunksPerAutoPause)
	{
		if (m_coroutine == null)
		{
			Coroutine coroutine = ThreadManager.StartCoroutine(CoroutineSpiral(_chunksPerAutoPause));
			if (m_isRunning)
			{
				m_coroutine = coroutine;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void MacroPause()
	{
		if (m_isRunning)
		{
			m_isRunning = false;
		}
		else
		{
			m_isRunning = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void MacroReset()
	{
		if (m_coroutine != null)
		{
			m_isRunning = false;
			ThreadManager.StopCoroutine(m_coroutine);
			m_coroutine = null;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator CoroutineSpiral(int _chunksPerAutoPause)
	{
		World world = GameManager.Instance.World;
		if (world == null || !ProfilerGameUtils.TryGetFlyingPlayer(out var player))
		{
			yield break;
		}
		m_isRunning = true;
		IEnumerator<Vector2i> spiralSequence = SpiralSequence();
		float lastY = player.position.y;
		int n = 0;
		DateTime lastPrintTime = DateTime.MinValue;
		while (world == GameManager.Instance.World && !(player != world.GetPrimaryPlayer()))
		{
			if (m_isRunning)
			{
				spiralSequence.MoveNext();
				Vector2i chunkPos = spiralSequence.Current;
				n++;
				DateTime now = DateTime.Now;
				if (now.Subtract(lastPrintTime).TotalSeconds > 10.0)
				{
					Log.Out("Square Spiral: ({0}, {1}) #{2}", chunkPos.x, chunkPos.y, n);
					lastPrintTime = now;
				}
				Vector2i centreOfChunk = new Vector2i(chunkPos.x * 16 + 8, chunkPos.y * 16 + 8);
				Vector3 rotationEuler = Quaternion.FromToRotation(Vector3.forward, new Vector3(-centreOfChunk.x, 0f, -centreOfChunk.y)).eulerAngles;
				Chunk chunk = (Chunk)world.GetChunkSync(chunkPos.x, chunkPos.y);
				if (chunk == null)
				{
					player.SetPosition(new Vector3(centreOfChunk.x, lastY, centreOfChunk.y));
					player.SetRotation(rotationEuler);
					while (chunk == null)
					{
						yield return null;
						chunk = (Chunk)world.GetChunkSync(chunkPos.x, chunkPos.y);
					}
				}
				float y = world.GetHeight(centreOfChunk.x, centreOfChunk.y) + 10;
				player.SetPosition(new Vector3(centreOfChunk.x, y, centreOfChunk.y));
				player.SetRotation(rotationEuler);
				lastY = player.position.y;
				switch (waitMode)
				{
				case WaitMode.SingleChunkDecorated:
					yield return ProfilerGameUtils.WaitForSingleChunkToLoad(chunk, chunkCondition);
					break;
				case WaitMode.SurroundingMeshesCopied:
					yield return ProfilerGameUtils.WaitForChunksAroundObserverToLoad(player.ChunkObserver, chunkCondition);
					break;
				case WaitMode.SurroundingChunksDisplayed:
					yield return ProfilerGameUtils.WaitForChunksAroundObserverToLoad(player.ChunkObserver, chunkCondition);
					break;
				}
				yield return null;
				if ((_chunksPerAutoPause > 0 && n % _chunksPerAutoPause == 0) || n == 1)
				{
					Log.Out("Square Spiral: ({0}, {1}) #{2} (PAUSED)", chunkPos.x, chunkPos.y, n);
					MacroPause();
				}
			}
			else
			{
				yield return new WaitForSeconds(1f);
			}
		}
		MacroReset();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator<Vector2i> SpiralSequence()
	{
		int x = 0;
		int y = 0;
		int rPos = 1;
		int rNeg = -1;
		SpiralSequenceState state = SpiralSequenceState.Left;
		while (true)
		{
			yield return new Vector2i(x, y);
			switch (state)
			{
			case SpiralSequenceState.Left:
				if (x == rNeg)
				{
					state = SpiralSequenceState.Down;
					y--;
				}
				else
				{
					x--;
				}
				break;
			case SpiralSequenceState.Down:
				if (y == rNeg)
				{
					state = SpiralSequenceState.Right;
					x++;
				}
				else
				{
					y--;
				}
				break;
			case SpiralSequenceState.Right:
				if (x == rPos)
				{
					state = SpiralSequenceState.Up;
					y++;
				}
				else
				{
					x++;
				}
				break;
			case SpiralSequenceState.Up:
				if (y == rPos)
				{
					rPos++;
					rNeg--;
					state = SpiralSequenceState.Left;
					x--;
				}
				else
				{
					y++;
				}
				break;
			}
		}
	}
}
