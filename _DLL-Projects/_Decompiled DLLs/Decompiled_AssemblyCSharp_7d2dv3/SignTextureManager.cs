using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Rendering;

public sealed class SignTextureManager
{
	public enum SignTextureQuality
	{
		Lowest,
		Low,
		Medium,
		High,
		Ultra,
		Infinite
	}

	public readonly struct TierSpec
	{
		public readonly int Resolution;

		public readonly int ActiveCount;

		public readonly int BufferCount;

		public int TotalCount => ActiveCount + BufferCount;

		public TierSpec(int resolution, int activeCount, int bufferCount)
		{
			Resolution = resolution;
			ActiveCount = activeCount;
			BufferCount = bufferCount;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const string kShaderName = "Game/SignTech/UI";

	public static readonly ProfilerMarker s_SignTextureManagerInitialize = new ProfilerMarker("SignTextureManager.Initialize");

	public static readonly ProfilerMarker s_SignTextureManagerCleanup = new ProfilerMarker("SignTextureManager.Cleanup");

	public static readonly ProfilerMarker s_SignTextureManagerInvalidate = new ProfilerMarker("SignTextureManager.Invalidate");

	public static readonly ProfilerMarker s_SignTextureManagerMainThreadUpdate = new ProfilerMarker("SignTextureManager.MainThreadUpdate");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerCounterValue<int> s_RegisteredSigns = new ProfilerCounterValue<int>(ProfilerCategory.Scripts, "STM Registered Signs", ProfilerMarkerDataUnit.Count, ProfilerCounterOptions.FlushOnEndOfFrame);

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly SignTextureManager m_instance = new SignTextureManager();

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly TierSpec[] s_tiersLowest = new TierSpec[3]
	{
		new TierSpec(64, 32, 32),
		new TierSpec(128, 32, 16),
		new TierSpec(512, 8, 8)
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly TierSpec[] s_tiersLow = new TierSpec[3]
	{
		new TierSpec(128, 32, 32),
		new TierSpec(512, 8, 8),
		new TierSpec(1024, 3, 4)
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly TierSpec[] s_tiersMedium = new TierSpec[3]
	{
		new TierSpec(256, 32, 16),
		new TierSpec(512, 8, 4),
		new TierSpec(1024, 3, 4)
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly TierSpec[] s_tiersHigh = new TierSpec[3]
	{
		new TierSpec(256, 32, 16),
		new TierSpec(1024, 10, 6),
		new TierSpec(2048, 1, 1)
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly TierSpec[] s_tiersUltra = new TierSpec[3]
	{
		new TierSpec(512, 32, 16),
		new TierSpec(1024, 10, 6),
		new TierSpec(2048, 2, 2)
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly HashSet<SignCanvas> _canvases = new HashSet<SignCanvas>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly SignPrioritizer _prioritizer;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly SignTextureStore _store;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly BakeScheduler _scheduler;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<SignBakeRequest> _tmpRequests = new List<SignBakeRequest>(256);

	[PublicizedFrom(EAccessModifier.Private)]
	public Shader _shader;

	[PublicizedFrom(EAccessModifier.Private)]
	public Material _material;

	[PublicizedFrom(EAccessModifier.Private)]
	public CommandBuffer _cmd;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool _initialised;

	[PublicizedFrom(EAccessModifier.Private)]
	public SignTextureQuality _qualityLevel = SignTextureQuality.Medium;

	[PublicizedFrom(EAccessModifier.Private)]
	public TierSpec[] _activeTiers = s_tiersMedium;

	[PublicizedFrom(EAccessModifier.Private)]
	public int _tileSize = 256;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool _requestsDirty = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 _lastUpdatePosition = Vector3.zero;

	[PublicizedFrom(EAccessModifier.Private)]
	public uint _framesSinceRefreshed;

	[PublicizedFrom(EAccessModifier.Private)]
	public const uint kMinFramesSinceRefreshed = 10u;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float kMinSqrDistanceSinceRefreshed = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool _showProgress;

	[PublicizedFrom(EAccessModifier.Private)]
	public StringBuilder sb = new StringBuilder();

	public static SignTextureManager Instance => m_instance;

	public bool IsEnabled => _initialised;

	public SignTextureQuality CurrentQuality => _qualityLevel;

	[PublicizedFrom(EAccessModifier.Private)]
	public SignTextureManager()
	{
		_prioritizer = new SignPrioritizer();
		_store = new SignTextureStore();
		_scheduler = new BakeScheduler();
	}

	public void SetQuality(SignTextureQuality quality)
	{
		if (_qualityLevel == quality)
		{
			return;
		}
		_qualityLevel = quality;
		SelectActiveTiers();
		if (!_initialised)
		{
			return;
		}
		if (_qualityLevel == SignTextureQuality.Infinite)
		{
			foreach (SignCanvas canvase in _canvases)
			{
				canvase?.SetTexture(null);
			}
		}
		_store.RebuildPools(_activeTiers);
		_scheduler.Clear();
		_requestsDirty = true;
	}

	public void SetTileSize(int tileSize)
	{
		if (tileSize > 0)
		{
			_tileSize = tileSize;
		}
	}

	public void SetTileSizeForCurrentQuality()
	{
		if (_activeTiers != null && _activeTiers.Length != 0)
		{
			_tileSize = _activeTiers[0].Resolution;
		}
	}

	public void SetShowProgress(bool showProgress)
	{
		_showProgress = showProgress;
	}

	public bool Initialize()
	{
		using (s_SignTextureManagerInitialize.Auto())
		{
			_initialised = false;
			if (GameManager.IsDedicatedServer)
			{
				return false;
			}
			_shader = GlobalAssets.FindShader("Game/SignTech/UI");
			if (_shader == null)
			{
				Log.Error("Failed to initialize SignTextureManager. Shader not found: Game/SignTech/UI");
				return false;
			}
			if (_material == null)
			{
				_material = new Material(_shader)
				{
					hideFlags = HideFlags.HideAndDontSave
				};
			}
			if (_cmd == null)
			{
				_cmd = new CommandBuffer
				{
					name = "SignTech_Bake"
				};
			}
			SelectActiveTiers();
			_store.RebuildPools(_activeTiers);
			_initialised = true;
			return true;
		}
	}

	public void Cleanup(bool preserveRegisteredCanvases = false)
	{
		using (s_SignTextureManagerCleanup.Auto())
		{
			foreach (SignCanvas canvase in _canvases)
			{
				canvase?.SetTexture(null);
			}
			if (!preserveRegisteredCanvases)
			{
				_canvases.Clear();
				s_RegisteredSigns.Value = _canvases.Count;
			}
			_scheduler.Clear();
			_prioritizer.Clear();
			_store.Clear();
			if (_material != null)
			{
				UnityEngine.Object.DestroyImmediate(_material);
				_material = null;
			}
			if (_cmd != null)
			{
				_cmd.Release();
				_cmd = null;
			}
			_shader = null;
			_initialised = false;
			_requestsDirty = true;
		}
	}

	public void Register(SignCanvas canvas)
	{
		if (_initialised && !(canvas == null))
		{
			_canvases.Add(canvas);
			_requestsDirty = true;
			s_RegisteredSigns.Value = _canvases.Count;
		}
	}

	public void Deregister(SignCanvas canvas)
	{
		if (_initialised && !(canvas == null))
		{
			_canvases.Remove(canvas);
			s_RegisteredSigns.Value = _canvases.Count;
		}
	}

	public void Invalidate(SignCanvas canvas)
	{
		using (s_SignTextureManagerInvalidate.Auto())
		{
			if (!_initialised || canvas == null || !canvas.DisplaySignId.IsValid)
			{
				return;
			}
			GlobalSignId displaySignId = canvas.DisplaySignId;
			foreach (SignCanvas canvase in _canvases)
			{
				if (canvase != null && canvase.DisplaySignId.Equals(displaySignId))
				{
					canvase.SetTexture(null);
				}
			}
			_store.InvalidateSign(displaySignId);
			_scheduler.Clear();
			_requestsDirty = true;
		}
	}

	public void MainThreadUpdate()
	{
		Vector3 playerPos;
		using (s_SignTextureManagerMainThreadUpdate.Auto())
		{
			if (_initialised && !(_material == null) && _cmd != null && _qualityLevel != SignTextureQuality.Infinite)
			{
				_framesSinceRefreshed++;
				playerPos = GetPlayerPos();
				float sqrMagnitude = (playerPos - _lastUpdatePosition).sqrMagnitude;
				_requestsDirty |= _framesSinceRefreshed > 10 && sqrMagnitude > 1f;
				if (_requestsDirty && _scheduler.CurrentState == BakeScheduler.BakeJobState.None)
				{
					RefreshRequestQueue();
				}
				else
				{
					_scheduler.TickOnce(_tileSize, _material, _cmd, _store, _prioritizer, _showProgress);
				}
			}
		}
		[PublicizedFrom(EAccessModifier.Private)]
		void RefreshRequestQueue()
		{
			_store.BeginFrameMarkAllNotInUse();
			_tmpRequests.Clear();
			_prioritizer.RebuildRequests(_canvases, playerPos, _activeTiers, _store, _tmpRequests);
			_store.EvictUnused();
			_scheduler.ResetRequests(_tmpRequests);
			_lastUpdatePosition = playerPos;
			_framesSinceRefreshed = 0u;
			_requestsDirty = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Vector3 GetPlayerPos()
	{
		return GameManager.Instance.World.GetPrimaryPlayer().position + new Vector3(0f, 1.5f, 0f);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SelectActiveTiers()
	{
		_activeTiers = _qualityLevel switch
		{
			SignTextureQuality.Infinite => Array.Empty<TierSpec>(), 
			SignTextureQuality.Ultra => s_tiersUltra, 
			SignTextureQuality.High => s_tiersHigh, 
			SignTextureQuality.Medium => s_tiersMedium, 
			SignTextureQuality.Low => s_tiersLow, 
			_ => s_tiersLowest, 
		};
	}

	public void LogDebugInfo(bool saveToDisk)
	{
		sb.Clear();
		sb.AppendLine("[STM] LogDebugInfo");
		bool flag = true;
		if (!_initialised)
		{
			flag = false;
			sb.AppendLine("Cannot log sign group info: SignTextureManager is not initialized.");
		}
		else if (_material == null || _cmd == null)
		{
			flag = false;
			sb.AppendLine("Cannot log sign group info: SignTextureManager has null material or command buffer.");
		}
		else if (_qualityLevel == SignTextureQuality.Infinite)
		{
			flag = false;
			sb.AppendLine("Cannot log sign group info: SignTextureManager is in Infinite quality mode.");
		}
		if (flag)
		{
			Vector3 playerPos = GetPlayerPos();
			float sqrMagnitude = (playerPos - _lastUpdatePosition).sqrMagnitude;
			sb.AppendLine();
			sb.AppendLine("**************************************************");
			sb.AppendLine("Manager (Enabled)");
			sb.AppendLine("**************************************************");
			sb.AppendLine();
			sb.AppendLine($"Registered Canvases: {_canvases.Count}");
			sb.AppendLine();
			sb.AppendLine($"Bake Job State: {_scheduler.CurrentState}");
			sb.AppendLine($"Player Pos: {playerPos}");
			sb.AppendLine($"Last Update Pos: {_lastUpdatePosition}");
			sb.AppendLine(string.Format("Sq Dist Since Update: {0} (min: {1}, ready: {2})", sqrMagnitude.ToString("00.000"), 1f, sqrMagnitude > 1f));
			sb.AppendLine($"Frames Since Update: {_framesSinceRefreshed} (min: {10u}, ready: {_framesSinceRefreshed > 10})");
			sb.AppendLine($"Requests Dirty: {_requestsDirty}");
			sb.AppendLine();
			sb.AppendLine($"Tile Size: {_tileSize}");
			sb.AppendLine($"Quality: {_qualityLevel}");
			if (_activeTiers == null)
			{
				sb.AppendLine("Active Tiers: NULL");
			}
			else if (_activeTiers.Length == 0)
			{
				sb.AppendLine("Active Tiers: NONE");
			}
			else
			{
				sb.AppendLine("Active Tiers:");
				for (int i = 0; i < _activeTiers.Length; i++)
				{
					TierSpec tierSpec = _activeTiers[i];
					sb.AppendLine($"\t{i}: {tierSpec.Resolution}px, {tierSpec.ActiveCount} active, {tierSpec.BufferCount} buffer");
				}
			}
			sb.AppendLine();
			_store.WriteTextureStoreInfo(sb);
			sb.AppendLine();
			_prioritizer.WriteGroupInfo(sb, playerPos, _activeTiers);
			Log.Out(sb.ToString());
			if (saveToDisk)
			{
				string path = $"STM_Dump_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
				string text = Path.Combine(Application.dataPath + "/../", path);
				File.WriteAllText(text, sb.ToString());
				Log.Out("[STM] Full dump saved to " + text);
			}
		}
		else
		{
			Log.Warning(sb.ToString());
		}
		sb.Clear();
	}

	[Conditional("SIGN_TEXTURE_DEBUG_LOG_INIT")]
	public static void DebugLog(string message)
	{
		Log.Out("[STM] " + message);
	}
}
