using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdVisitPois : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Private)]
	public bool m_isRunning;

	[PublicizedFrom(EAccessModifier.Private)]
	public Coroutine m_coroutine;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Vector3 MinPoiSize = new Vector3i(5, 5, 5);

	public override bool IsExecuteOnClient => true;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[2] { "vpois", "visitpois" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "<s[tart] [pois per auto-pause]|p[ause]|r[eset]>";
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
		default:
			return false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void MacroStart(int _poisPerAutoPause)
	{
		if (m_coroutine == null)
		{
			Coroutine coroutine = ThreadManager.StartCoroutine(CoroutineVisit(_poisPerAutoPause));
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
	public IEnumerator CoroutineVisit(int _poisPerAutoPause)
	{
		World world = GameManager.Instance.World;
		if (world == null || !ProfilerGameUtils.TryGetFlyingPlayer(out var player))
		{
			yield break;
		}
		m_isRunning = true;
		int poisVisited = 0;
		PrefabInstance[] array = GameManager.Instance.GetDynamicPrefabDecorator().allPrefabs.ToArray();
		foreach (PrefabInstance prefabInstance in array)
		{
			while (!m_isRunning)
			{
				yield return new WaitForSeconds(1f);
			}
			Vector3 size = prefabInstance.GetAABB().size;
			if (!(size.x <= MinPoiSize.x) && !(size.y <= MinPoiSize.y) && !(size.z <= MinPoiSize.z))
			{
				if (world != GameManager.Instance.World || player != world.GetPrimaryPlayer())
				{
					MacroReset();
					yield break;
				}
				Bounds aABB = prefabInstance.GetAABB();
				Vector3 center = aABB.center;
				Log.Out($"Visit Pois: {prefabInstance.name} ({center.x}, {center.y}, {center.z}) {aABB}");
				player.SetPosition(center);
				yield return ProfilerGameUtils.WaitForChunksAroundObserverToLoad(player.ChunkObserver, ChunkConditions.Displayed);
				yield return null;
				yield return null;
				yield return null;
				if ((_poisPerAutoPause > 0 && poisVisited % _poisPerAutoPause == 0) || poisVisited == 0)
				{
					Log.Out($"Visit Pois: #{poisVisited} (PAUSED)");
					MacroPause();
				}
				poisVisited++;
			}
		}
		MacroReset();
	}
}
