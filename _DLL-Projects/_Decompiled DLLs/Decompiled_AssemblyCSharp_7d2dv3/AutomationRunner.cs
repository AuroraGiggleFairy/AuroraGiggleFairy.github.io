using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Platform;
using UnityEngine;

public class AutomationRunner
{
	public static readonly AutomationRunner Instance = new AutomationRunner();

	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsAwaitingShutdown;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool _abortRequested;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool _gameStartDone;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool IsRunning
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public AutomationScript CurrentScript
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	} = new AutomationScript();

	[PublicizedFrom(EAccessModifier.Private)]
	public AutomationRunner()
	{
	}

	public bool LoadScript(AutomationScript script)
	{
		Log.Error("[AutomationRunner] Disabled for this build type.");
		return false;
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public void LoadScriptUnchecked(AutomationScript script)
	{
		Log.Error("[AutomationRunner] Disabled for this build type.");
	}

	public void StartRuns()
	{
		Log.Error("[AutomationRunner] Disabled for this build type.");
	}

	public void Abort()
	{
		if (!IsRunning)
		{
			Log.Out("[AutomationRunner] Nothing to abort.");
			return;
		}
		_abortRequested = true;
		Log.Out("[AutomationRunner] Abort requested — finishing current step.");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator RunScript(AutomationScript script)
	{
		int stepIndex = 0;
		while (stepIndex < script.steps.Count && !_abortRequested)
		{
			AutomationStep automationStep = script.steps[stepIndex];
			if (automationStep.type == AutomationStep.StepType.StartPerfSession)
			{
				int endIndex = FindMatchingEnd(script, stepIndex);
				yield return ExecutePerfSession(script, stepIndex, endIndex);
				stepIndex = endIndex + 1;
			}
			else
			{
				yield return ExecuteStep(automationStep, 0, script.ResolveSessionDir());
				stepIndex++;
			}
		}
		IsRunning = false;
		Log.Out("[AutomationRunner] Script finished.");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator ExecutePerfSession(AutomationScript script, int startIndex, int endIndex)
	{
		string sessionDir = script.ResolveSessionDir();
		AutomationStep automationStep = script.steps[startIndex];
		int runs = automationStep.runCount;
		int num = 0;
		string text = null;
		for (int i = startIndex + 1; i < endIndex; i++)
		{
			if (script.steps[i].type == AutomationStep.StepType.StopPerfCapture)
			{
				num++;
			}
			if (script.steps[i].type == AutomationStep.StepType.StartPerfCapture && text == null)
			{
				text = script.steps[i].capturePrefix;
			}
		}
		int num2 = runs * num;
		if (num2 > 0)
		{
			PerformanceProfiler.BeginSession(sessionDir, num2, text);
		}
		Log.Out($"[AutomationRunner] ── PerfSession: {runs} run(s) × {num} capture(s)/run ──");
		for (int run = 0; run < runs; run++)
		{
			if (_abortRequested)
			{
				Log.Out($"[AutomationRunner] PerfSession aborted before run {run + 1}/{runs}.");
				PerformanceProfiler.AbortSession();
				yield break;
			}
			Log.Out($"[AutomationRunner] PerfSession run {run + 1}/{runs}");
			for (int j = startIndex + 1; j < endIndex; j++)
			{
				if (_abortRequested)
				{
					PerformanceProfiler.AbortSession();
					yield break;
				}
				yield return ExecuteStep(script.steps[j], run, sessionDir);
			}
		}
		Log.Out("[AutomationRunner] ── PerfSession complete ──");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static int FindMatchingEnd(AutomationScript script, int startIndex)
	{
		for (int i = startIndex + 1; i < script.steps.Count; i++)
		{
			if (script.steps[i].type == AutomationStep.StepType.StopPerfSession)
			{
				return i;
			}
		}
		Log.Error("Performance Capture session start has no matching stop.");
		return script.steps.Count;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static EntityPlayerLocal GetPlayer()
	{
		return GameManager.Instance.World?.GetPrimaryPlayer();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator ExecuteStep(AutomationStep step, int runIndex, string sessionDir)
	{
		switch (step.type)
		{
		case AutomationStep.StepType.LoadGame:
		{
			if (GameManager.Instance.World != null)
			{
				Log.Out("[AutomationRunner] LoadGame: world already loaded — skipping.");
				break;
			}
			GamePrefs.Set(EnumGamePrefs.GameWorld, step.world);
			GamePrefs.Set(EnumGamePrefs.GameName, step.gameName);
			GamePrefs.Set(EnumGamePrefs.GameSaveStorageType, (int)PlatformManager.MultiPlatform.UserDataRoaming.DefaultSaveStorage);
			GamePrefs.Instance.Load(GameIO.GetSaveGameDir() + "/gameOptions.sdf");
			Log.Out("[AutomationRunner] LoadGame: starting '" + step.world + "' / '" + step.gameName + "'...");
			_gameStartDone = false;
			ModEvents.GameStartDone.RegisterHandler(OnGameStartDone);
			NetworkConnectionError networkConnectionError = SingletonMonoBehaviour<ConnectionManager>.Instance.StartServers(GamePrefs.GetString(EnumGamePrefs.ServerPassword), _offline: false);
			if (networkConnectionError != NetworkConnectionError.NoError)
			{
				Log.Error($"[AutomationRunner] LoadGame failed: {networkConnectionError}");
				ModEvents.GameStartDone.UnregisterHandler(OnGameStartDone);
				_abortRequested = true;
			}
			else
			{
				while (!_gameStartDone)
				{
					yield return null;
				}
				Log.Out("[AutomationRunner] LoadGame: world loaded and ready.");
			}
			break;
		}
		case AutomationStep.StepType.PreparePlayer:
		{
			EntityPlayerLocal player2 = GetPlayer();
			if (player2 == null)
			{
				Log.Error("[AutomationRunner] PreparePlayer: no player.");
				break;
			}
			player2.IsGodMode.Value = true;
			player2.IsNoCollisionMode.Value = true;
			player2.IsFlyMode.Value = true;
			player2.IsSpectator = true;
			player2.Buffs.AddBuff("god");
			GameManager.Instance.World.SetTimeJump(12000uL, _isSeek: true);
			List<Entity> list = new List<Entity>(GameManager.Instance.World.Entities.list);
			for (int num = 0; num < list.Count; num++)
			{
				Entity entity = list[num];
				if (entity != null && !(entity is EntityPlayer))
				{
					entity.DamageEntity(new DamageSource(EnumDamageSource.Internal, EnumDamageTypes.Suicide), 99999, _criticalHit: false);
				}
			}
			GameStats.Set(EnumGameStats.TimeOfDayIncPerSec, 0);
			break;
		}
		case AutomationStep.StepType.Teleport:
		{
			EntityPlayerLocal player = GetPlayer();
			if (player == null)
			{
				Log.Error("[AutomationRunner] Teleport: no player.");
				break;
			}
			player.TeleportToPosition(step.position, _onlyIfNotFlying: false, step.lookDir);
			while (!player.Spawned)
			{
				yield return null;
			}
			yield return null;
			yield return null;
			Log.Out($"[AutomationRunner] Teleport: settled at {step.position}.");
			break;
		}
		case AutomationStep.StepType.Wait:
			yield return new WaitForSeconds(step.duration);
			break;
		case AutomationStep.StepType.WaitForChunksLoaded:
		{
			EntityPlayerLocal player5 = GetPlayer();
			if (player5 == null)
			{
				Log.Error("[AutomationRunner] WaitForChunksLoaded: no player.");
				break;
			}
			ChunkManager.ChunkObserver chunkObserver = player5.ChunkObserver;
			if (chunkObserver == null)
			{
				Log.Error("[AutomationRunner] WaitForChunksLoaded: no chunk observer.");
				break;
			}
			Log.Out("[AutomationRunner] WaitForChunksLoaded: waiting for chunks to load...");
			yield return ProfilerGameUtils.WaitForChunksAroundObserverToLoad(chunkObserver, ChunkConditions.Displayed);
			Log.Out("[AutomationRunner] WaitForChunksLoaded: chunks loaded.");
			break;
		}
		case AutomationStep.StepType.MoveLine:
		{
			EntityPlayerLocal player4 = GetPlayer();
			if (player4 == null)
			{
				Log.Error("[AutomationRunner] MoveLine: no player.");
				break;
			}
			if (step.hasStart)
			{
				player4.SetPosition(step.positionB);
			}
			bool done3 = false;
			player4.EnableAutoMove(_enable: true).StartLine(step.duration, 0, step.position, [PublicizedFrom(EAccessModifier.Internal)] () =>
			{
				done3 = true;
			});
			while (!done3)
			{
				yield return null;
			}
			break;
		}
		case AutomationStep.StepType.MovePingPong:
		{
			EntityPlayerLocal player = GetPlayer();
			if (player == null)
			{
				Log.Error("[AutomationRunner] MovePingPong: no player.");
				break;
			}
			EntityPlayerLocal.AutoMove am = player.EnableAutoMove(_enable: true);
			int segments = 2 * Mathf.Max(1, step.pingPongCount);
			for (int i = 0; i < segments; i++)
			{
				if (_abortRequested)
				{
					break;
				}
				Vector3 pos = ((i % 2 == 0) ? step.position : step.positionB);
				Vector3 vector = ((i % 2 == 0) ? step.positionB : step.position);
				player.SetPosition(pos);
				am.SetLookAt(vector);
				bool done = false;
				am.StartLine(step.duration, 0, vector, [PublicizedFrom(EAccessModifier.Internal)] () =>
				{
					done = true;
				});
				while (!done)
				{
					yield return null;
				}
			}
			break;
		}
		case AutomationStep.StepType.Orbit:
		{
			EntityPlayerLocal player3 = GetPlayer();
			if (player3 == null)
			{
				Log.Error("[AutomationRunner] Orbit: no player.");
				break;
			}
			bool done2 = false;
			player3.EnableAutoMove(_enable: true).StartOrbit(step.duration, 0, step.position, step.lookForward, step.isFlipped, [PublicizedFrom(EAccessModifier.Internal)] () =>
			{
				done2 = true;
			});
			while (!done2)
			{
				yield return null;
			}
			break;
		}
		case AutomationStep.StepType.StartPerfCapture:
		{
			string prefix = (string.IsNullOrEmpty(step.capturePrefix) ? $"run_{runIndex + 1:D2}" : $"{step.capturePrefix}_{runIndex + 1:D2}");
			PerformanceProfiler.StartCapture(sessionDir, prefix, step.targetFps);
			break;
		}
		case AutomationStep.StepType.StopPerfCapture:
			if (PerformanceProfiler.IsCapturing())
			{
				PerformanceProfiler.StopCapture();
			}
			else
			{
				Log.Warning("[AutomationRunner] StopPerfCapture step: no capture in progress.");
			}
			break;
		case AutomationStep.StepType.ExitToMenu:
			Log.Out("[AutomationRunner] ExitToMenu: disconnecting and returning to main menu.");
			GameManager.Instance.Disconnect();
			break;
		case AutomationStep.StepType.Cleanup:
			ModEvents.GameStartDone.UnregisterHandler(OnGameStartDone);
			Log.Out("[AutomationRunner] Cleanup: event handlers unregistered.");
			break;
		case AutomationStep.StepType.AutomationComplete:
		{
			if (string.IsNullOrEmpty(step.url))
			{
				Log.Warning("[AutomationRunner] AutomationComplete: no url configured — skipping callback.");
				break;
			}
			IsAwaitingShutdown = true;
			string callbackUrl = step.url;
			Log.Out("[AutomationRunner] AutomationComplete: POSTing to " + callbackUrl);
			bool postDone = false;
			bool postOk = false;
			Task.Run([PublicizedFrom(EAccessModifier.Internal)] async () =>
			{
				try
				{
					using HttpClient client = new HttpClient();
					client.Timeout = TimeSpan.FromSeconds(10.0);
					StringContent content = new StringContent($"{{\"script\":\"{CurrentScript.name}\",\"timestamp\":\"{DateTime.UtcNow:O}\",\"steps\":{CurrentScript.steps.Count}}}", Encoding.UTF8, "application/json");
					HttpResponseMessage httpResponseMessage = await client.PostAsync(callbackUrl, content);
					postOk = httpResponseMessage.IsSuccessStatusCode;
					if (!postOk)
					{
						Log.Error($"[AutomationRunner] Callback POST returned {(int)httpResponseMessage.StatusCode}");
					}
				}
				catch (Exception ex)
				{
					Log.Error("[AutomationRunner] Callback POST failed: " + ex.Message);
				}
				finally
				{
					postDone = true;
				}
			});
			float waited = 0f;
			while (!postDone && waited < 15f)
			{
				yield return new WaitForSeconds(0.25f);
				waited += 0.25f;
			}
			if (postOk)
			{
				Log.Out("[AutomationRunner] AutomationComplete: callback sent successfully.");
			}
			else if (!postDone)
			{
				Log.Error("[AutomationRunner] AutomationComplete: callback timed out.");
			}
			break;
		}
		case AutomationStep.StepType.LogError:
			Log.Error(step.text);
			break;
		case AutomationStep.StepType.DeleteSave:
		{
			if (GameManager.Instance.World != null)
			{
				Log.Out("[AutomationRunner] DeleteSave: Can only be run while not in a game.");
				break;
			}
			string saveGameDir = GameIO.GetSaveGameDir(step.world, step.gameName, PlatformManager.MultiPlatform.UserDataRoaming.DefaultSaveStorage);
			if (SdDirectory.Exists(saveGameDir))
			{
				SdDirectory.Delete(saveGameDir, recursive: true);
				SaveDataUtils.SaveDataManager.CommitSync();
				Log.Out("[AutomationRunner] DeleteSave: Save " + step.world + "/" + step.gameName + " deleted.");
			}
			else
			{
				Log.Out("[AutomationRunner] DeleteSave: Save not found.");
			}
			break;
		}
		case AutomationStep.StepType.ConsoleCmd:
			GUIWindowConsole.AddLines(SingletonMonoBehaviour<SdtdConsole>.Instance.ExecuteSync(step.text, null));
			break;
		default:
			Log.Warning($"[AutomationRunner] Unknown step type '{step.type}' — skipping.");
			break;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnGameStartDone(ref ModEvents.SGameStartDoneData _data)
	{
		_gameStartDone = true;
	}

	public bool RestartAllowed()
	{
		if (!IsRunning)
		{
			return !IsAwaitingShutdown;
		}
		return false;
	}

	public static string GetAutomationDataPath()
	{
		return GameIO.GetPostTerminationAccessiblePath() + "Automation/";
	}

	public static void InitialiseLogging()
	{
		string text = GetAutomationDataPath() + "Logs/";
		if (!Directory.Exists(text))
		{
			Directory.CreateDirectory(text);
		}
		string text2 = DateTime.UtcNow.ToString("yyyy-MM-dd_HH-mm-ss.fff");
		Log.AddOutputPath(Path.Join(text, "Player-" + text2 + ".log"));
		string[] files = Directory.GetFiles(text, "Player*.log");
		if (files.Length > 3)
		{
			Array.Sort(files, StringComparer.InvariantCulture);
			for (int i = 0; i < files.Length - 3; i++)
			{
				File.Delete(files[i]);
				Log.Out("[AUTOMATION] Deleted old log: " + files[i]);
			}
		}
	}
}
