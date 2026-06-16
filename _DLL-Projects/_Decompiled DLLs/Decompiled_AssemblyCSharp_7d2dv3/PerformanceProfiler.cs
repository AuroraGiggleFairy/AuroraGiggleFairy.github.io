using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SystemInformation;
using Unity.Profiling;
using UnityEngine;

public static class PerformanceProfiler
{
	public struct FrameInfo
	{
		public long systemTextureMemory;

		public long systemTextureMemoryDelta;

		public float texStreamCurrentMB;

		public float texStreamTargetMB;

		public float texStreamDesiredMB;

		public float texStreamBudgetMB;

		public float gcHeapMemoryMB;

		public float rssMemoryMB;

		public int chunkCount;

		public int displayedChunkObjects;

		public int enemyCount;

		public int entityCount;

		public int entityInstanceCount;

		public int itemCount;

		public float timeSinceLevelLoadMin;
	}

	public struct SpikeEvent
	{
		public int startFrame;

		public int durationFrames;

		public float totalDurationMs;

		public float peakFrameTimeMs;

		public float baselineFrameTimeMs;

		public float peakMultiple;

		public bool isGcAttributed;

		public float gcHeapDropMB;
	}

	public struct PerformanceMetrics
	{
		public float averageFrameTime;

		public float averageFPS;

		public float onePercentLow;

		public float pointOnePercentLow;

		public float fivePercentLow;

		public float maxFrameTime;

		public float minFrameTime;

		public float standardDeviation;

		public float coefficientOfVariation;

		public int framePacingViolations20;

		public int framePacingViolations50;

		public float framePacingViolationPercent20;

		public float framePacingViolationPercent50;

		public int maxConsecutiveBadFrames;

		public int spikeEventCount;

		public int gcAttributedSpikeCount;

		public int nonGcAttributedSpikeCount;

		public float gcAttributedSpikePercent;

		public float meanSpikeDurationMs;

		public float maxSpikeDurationMs;

		public float meanTimeBetweenSpikesSec;

		public float meanGcSpikeMs;

		public float meanNonGcSpikeMs;

		public int estimatedGcCollectionCount;

		public float meanGcIntervalSec;

		public int totalFramesCaptured;

		public float captureDurationSeconds;
	}

	public struct AggregateMetrics
	{
		public int runCount;

		public float meanAverageFrameTime;

		public float meanAverageFPS;

		public float stdDevAverageFPS;

		public float meanOnePercentLow;

		public float stdDevOnePercentLow;

		public float meanPointOnePercentLow;

		public float meanFivePercentLow;

		public float meanFramePacingViolationPercent20;

		public float meanFramePacingViolationPercent50;

		public float meanMaxConsecutiveBadFrames;

		public float meanSpikeEventCount;

		public float meanGcAttributedSpikePercent;

		public float meanTimeBetweenSpikesSec;

		public float meanEstimatedGcCollectionCount;

		public float meanGcIntervalSec;

		public float bestRunAverageFPS;

		public float worstRunAverageFPS;

		public int totalFramesCaptured;

		public float totalCaptureDurationSeconds;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public class FrameTimeCapture : MonoBehaviour
	{
		[NonSerialized]
		[PublicizedFrom(EAccessModifier.Private)]
		public int lateUpdatesSeen;

		[PublicizedFrom(EAccessModifier.Private)]
		public void LateUpdate()
		{
			if (!isCapturing)
			{
				UnityEngine.Object.Destroy(base.gameObject);
				return;
			}
			if (memorySnapshotStart == null)
			{
				lateUpdatesSeen++;
				if (lateUpdatesSeen == 2 && memoryProfiler != null)
				{
					memorySnapshotStart = memoryProfiler.GetLastValueCsv();
				}
			}
			CaptureFrameTime();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const int SpikeBaselineHalfWindow = 30;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float SpikeThresholdMultiplier = 2f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int SpikeMergeGapFrames = 3;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float GcAttributionHeapDropMB = 0.5f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int GcAttributionWindowFrames = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool isCapturing = false;

	[PublicizedFrom(EAccessModifier.Private)]
	public static string profilePrefix = null;

	[PublicizedFrom(EAccessModifier.Private)]
	public static List<float> frameTimeData = new List<float>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static List<FrameInfo> frameExtraData = new List<FrameInfo>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static ProfilerRecorder frameTimeRecorder;

	[PublicizedFrom(EAccessModifier.Private)]
	public static DateTime captureStartTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public static int targetFramerate = 30;

	[PublicizedFrom(EAccessModifier.Private)]
	public static float lastFrameTime = 0f;

	[PublicizedFrom(EAccessModifier.Private)]
	public static FrameTimeCapture captureComponent;

	[PublicizedFrom(EAccessModifier.Private)]
	public static string captureDirectory;

	[PublicizedFrom(EAccessModifier.Private)]
	public static ProfilingMetricCapture memoryProfiler;

	[PublicizedFrom(EAccessModifier.Private)]
	public static string memorySnapshotStart;

	[PublicizedFrom(EAccessModifier.Private)]
	public static List<SpikeEvent> lastRunSpikeEvents = new List<SpikeEvent>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool isInSession = false;

	[PublicizedFrom(EAccessModifier.Private)]
	public static int sessionRunTarget = 0;

	[PublicizedFrom(EAccessModifier.Private)]
	public static int sessionRunsCompleted = 0;

	[PublicizedFrom(EAccessModifier.Private)]
	public static List<PerformanceMetrics> sessionRunMetrics = new List<PerformanceMetrics>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static string sessionDirectory = null;

	[PublicizedFrom(EAccessModifier.Private)]
	public static string sessionPrefix = null;

	[PublicizedFrom(EAccessModifier.Private)]
	public static long lastSystemTextureMemory = 0L;

	public static event Action<int, int, PerformanceMetrics> OnRunComplete;

	public static event Action<AggregateMetrics> OnSessionComplete;

	public static void BeginSession(string directory, int runCount, string prefix = null)
	{
		if (isInSession)
		{
			Debug.LogWarning("[PerformanceProfiler] Session already in progress.");
			return;
		}
		if (runCount < 1)
		{
			Debug.LogWarning("[PerformanceProfiler] runCount must be >= 1.");
			return;
		}
		isInSession = true;
		sessionRunTarget = runCount;
		sessionRunsCompleted = 0;
		sessionRunMetrics.Clear();
		sessionDirectory = directory ?? DateTime.Now.ToString("yyyyMMdd_HHmmss");
		sessionPrefix = prefix;
		Debug.Log($"[PerformanceProfiler] Session '{sessionDirectory}' begun — {runCount} run(s) planned.");
	}

	public static void AbortSession()
	{
		if (isInSession)
		{
			if (isCapturing)
			{
				StopCapture(saveOutput: false);
			}
			isInSession = false;
			sessionPrefix = null;
			sessionRunMetrics.Clear();
			Debug.Log("[PerformanceProfiler] Session aborted.");
		}
	}

	public static void StartCapture(string directory = null, string prefix = null, int targetFps = 30)
	{
		if (isCapturing)
		{
			Debug.LogWarning("[PerformanceProfiler] Capture already in progress.");
			return;
		}
		frameTimeData.Clear();
		frameTimeData.Capacity = 5000;
		frameExtraData.Clear();
		frameExtraData.Capacity = 5000;
		lastRunSpikeEvents.Clear();
		lastRunSpikeEvents.Capacity = 1000;
		profilePrefix = prefix;
		targetFramerate = targetFps;
		captureStartTime = DateTime.Now;
		captureDirectory = (isInSession ? sessionDirectory : (directory ?? DateTime.Now.ToString("yyyyMMdd_HHmmss")));
		if (!TryCreateProfilerRecorder())
		{
			Debug.LogWarning("[PerformanceProfiler] ProfilerRecorder not available — using Time.deltaTime.");
		}
		memoryProfiler = ProfilerCaptureUtils.CreateMemoryProfiler();
		memorySnapshotStart = null;
		isCapturing = true;
		GameObject obj = new GameObject("PerformanceProfilerCapture")
		{
			hideFlags = HideFlags.HideAndDontSave
		};
		captureComponent = obj.AddComponent<FrameTimeCapture>();
		UnityEngine.Object.DontDestroyOnLoad(obj);
		string arg = ((prefix != null) ? (" [" + prefix + "]") : "");
		Debug.Log($"[PerformanceProfiler] Capture started{arg}. Target: {targetFramerate} FPS.");
	}

	public static PerformanceMetrics StopCapture(bool saveOutput = true)
	{
		if (!isCapturing)
		{
			Debug.LogWarning("[PerformanceProfiler] No capture in progress.");
			return default(PerformanceMetrics);
		}
		isCapturing = false;
		if (captureComponent != null)
		{
			UnityEngine.Object.Destroy(captureComponent.gameObject);
			captureComponent = null;
		}
		if (frameTimeRecorder.Valid)
		{
			frameTimeRecorder.Dispose();
		}
		float num = (float)(DateTime.Now - captureStartTime).TotalSeconds;
		if (frameTimeData.Count == 0)
		{
			Debug.LogError("[PerformanceProfiler] No frame data captured.");
			return default(PerformanceMetrics);
		}
		Debug.Log($"[PerformanceProfiler] Capture stopped — {frameTimeData.Count} frames over {num:F2}s.");
		PerformanceMetrics performanceMetrics = CalculateMetrics(num);
		if (saveOutput)
		{
			string text = BuildBasePath(captureDirectory);
			SaveRawData(Path.Combine(text, "perf_raw_" + profilePrefix + ".csv"));
			SaveAnalysis(Path.Combine(text, "perf_analysis_" + profilePrefix + ".csv"), performanceMetrics);
			SaveSpikeEvents(Path.Combine(text, "perf_spikes_" + profilePrefix + ".csv"), lastRunSpikeEvents);
			SaveMemorySnapshot(Path.Combine(text, "perf_memory_" + profilePrefix + ".csv"));
			Debug.Log("Saved performance output to " + text + ".");
		}
		if (memoryProfiler != null)
		{
			memoryProfiler.Cleanup();
			memoryProfiler = null;
		}
		LogSummary(performanceMetrics);
		if (isInSession)
		{
			sessionRunMetrics.Add(performanceMetrics);
			int arg = sessionRunsCompleted++;
			PerformanceProfiler.OnRunComplete?.Invoke(arg, sessionRunTarget, performanceMetrics);
			Debug.Log($"[PerformanceProfiler] Session run {sessionRunsCompleted}/{sessionRunTarget} complete.");
			if (sessionRunsCompleted >= sessionRunTarget)
			{
				FinalizeSession();
			}
		}
		else
		{
			PerformanceProfiler.OnRunComplete?.Invoke(0, 1, performanceMetrics);
		}
		return performanceMetrics;
	}

	public static IReadOnlyList<SpikeEvent> GetLastRunSpikeEvents()
	{
		return lastRunSpikeEvents;
	}

	public static bool IsCapturing()
	{
		return isCapturing;
	}

	public static int GetCurrentFrameCount()
	{
		return frameTimeData.Count;
	}

	public static float GetLastFrameTime()
	{
		return lastFrameTime;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void FinalizeSession()
	{
		AggregateMetrics aggregateMetrics = CalculateAggregateMetrics(sessionRunMetrics);
		string path = BuildBasePath(sessionDirectory);
		string path2 = (string.IsNullOrEmpty(sessionPrefix) ? "perf_aggregate.csv" : ("perf_aggregate_" + sessionPrefix + ".csv"));
		SaveAggregateAnalysis(Path.Combine(path, path2), aggregateMetrics);
		LogAggregateSummary(aggregateMetrics);
		PerformanceProfiler.OnSessionComplete?.Invoke(aggregateMetrics);
		isInSession = false;
		sessionPrefix = null;
		sessionRunMetrics.Clear();
		Debug.Log("[PerformanceProfiler] Session '" + sessionDirectory + "' complete.");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void CaptureFrameTime()
	{
		if (isCapturing)
		{
			float num = 0f;
			if (frameTimeRecorder.Valid)
			{
				num = (float)frameTimeRecorder.LastValue / 1000000f;
			}
			if (num <= 0f)
			{
				num = Time.deltaTime * 1000f;
			}
			if (!(num <= 0f) && !(num >= 1000f))
			{
				long currentTextureMemory = (long)Texture.currentTextureMemory;
				World world = GameManager.Instance.World;
				frameTimeData.Add(num);
				frameExtraData.Add(new FrameInfo
				{
					systemTextureMemory = currentTextureMemory,
					systemTextureMemoryDelta = currentTextureMemory - lastSystemTextureMemory,
					texStreamCurrentMB = (float)Texture.currentTextureMemory / 1048576f,
					texStreamTargetMB = (float)Texture.targetTextureMemory / 1048576f,
					texStreamDesiredMB = (float)Texture.desiredTextureMemory / 1048576f,
					texStreamBudgetMB = (QualitySettings.streamingMipmapsActive ? QualitySettings.streamingMipmapsMemoryBudget : (-1f)),
					gcHeapMemoryMB = (float)GC.GetTotalMemory(forceFullCollection: false) / 1048576f,
					rssMemoryMB = (float)GetRSS.GetCurrentRSS() / 1024f / 1024f,
					chunkCount = Chunk.InstanceCount,
					displayedChunkObjects = (world?.m_ChunkManager.GetDisplayedChunkGameObjectsCount() ?? 0),
					enemyCount = GameStats.GetInt(EnumGameStats.EnemyCount),
					entityCount = (world?.Entities.Count ?? 0),
					entityInstanceCount = Entity.InstanceCount,
					itemCount = EntityItem.ItemInstanceCount,
					timeSinceLevelLoadMin = Time.timeSinceLevelLoad / 60f
				});
				lastSystemTextureMemory = currentTextureMemory;
				lastFrameTime = num;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static float[] CalculateRollingBaseline()
	{
		int count = frameTimeData.Count;
		float[] array = new float[count];
		List<float> list = new List<float>(61);
		for (int i = 0; i < count; i++)
		{
			int num = Math.Max(0, i - 30);
			int num2 = Math.Min(count - 1, i + 30);
			list.Clear();
			for (int j = num; j <= num2; j++)
			{
				if (j != i)
				{
					list.Add(frameTimeData[j]);
				}
			}
			list.Sort();
			array[i] = ((list.Count == 0) ? frameTimeData[i] : list[list.Count / 2]);
		}
		return array;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static List<SpikeEvent> ExtractSpikeEvents(float[] baseline)
	{
		int count = frameTimeData.Count;
		bool[] array = new bool[count];
		for (int i = 0; i < count; i++)
		{
			array[i] = baseline[i] > 0f && frameTimeData[i] > baseline[i] * 2f;
		}
		List<(int, int)> list = new List<(int, int)>();
		int j = 0;
		while (j < count)
		{
			if (!array[j])
			{
				j++;
				continue;
			}
			int item = j;
			for (; j < count && array[j]; j++)
			{
			}
			list.Add((item, j - 1));
		}
		for (int num = list.Count - 1; num > 0; num--)
		{
			if (list[num].Item1 - list[num - 1].Item2 - 1 < 3)
			{
				list[num - 1] = (list[num - 1].Item1, list[num].Item2);
				list.RemoveAt(num);
			}
		}
		List<SpikeEvent> list2 = new List<SpikeEvent>(list.Count);
		foreach (var item4 in list)
		{
			int item2 = item4.Item1;
			int item3 = item4.Item2;
			float num2 = 0f;
			float num3 = 0f;
			for (int k = item2; k <= item3; k++)
			{
				num2 += frameTimeData[k];
				if (frameTimeData[k] > num3)
				{
					num3 = frameTimeData[k];
				}
			}
			float num4 = 0f;
			int num5 = Math.Max(1, item2 - 1);
			int num6 = Math.Min(count - 1, item3 + 1);
			for (int l = num5; l <= num6; l++)
			{
				float num7 = frameExtraData[l - 1].gcHeapMemoryMB - frameExtraData[l].gcHeapMemoryMB;
				if (num7 > num4)
				{
					num4 = num7;
				}
			}
			list2.Add(new SpikeEvent
			{
				startFrame = item2,
				durationFrames = item3 - item2 + 1,
				totalDurationMs = num2,
				peakFrameTimeMs = num3,
				baselineFrameTimeMs = baseline[item2],
				peakMultiple = ((baseline[item2] > 0f) ? (num3 / baseline[item2]) : 0f),
				isGcAttributed = (num4 >= 0.5f),
				gcHeapDropMB = ((num4 >= 0.5f) ? num4 : 0f)
			});
		}
		return list2;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static List<int> DetectGcCollections()
	{
		List<int> list = new List<int>();
		for (int i = 1; i < frameExtraData.Count; i++)
		{
			if (frameExtraData[i - 1].gcHeapMemoryMB - frameExtraData[i].gcHeapMemoryMB >= 0.5f)
			{
				list.Add(i);
			}
		}
		return list;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static PerformanceMetrics CalculateMetrics(float captureDuration)
	{
		PerformanceMetrics m = default(PerformanceMetrics);
		if (frameTimeData.Count == 0)
		{
			return m;
		}
		List<float> list = new List<float>(frameTimeData);
		list.Sort();
		m.totalFramesCaptured = frameTimeData.Count;
		m.captureDurationSeconds = captureDuration;
		m.averageFrameTime = frameTimeData.Average();
		m.averageFPS = 1000f / m.averageFrameTime;
		m.minFrameTime = list[0];
		m.maxFrameTime = list[list.Count - 1];
		m.fivePercentLow = GetPercentile(list, 95f);
		m.onePercentLow = GetPercentile(list, 99f);
		m.pointOnePercentLow = GetPercentile(list, 99.9f);
		float num = frameTimeData.Sum([PublicizedFrom(EAccessModifier.Internal)] (float f) => (f - m.averageFrameTime) * (f - m.averageFrameTime));
		m.standardDeviation = Mathf.Sqrt(num / (float)m.totalFramesCaptured);
		m.coefficientOfVariation = m.standardDeviation / m.averageFrameTime;
		float num2 = 1000f / (float)targetFramerate;
		float num3 = num2 * 1.2f;
		float num4 = num2 * 1.5f;
		int num5 = 0;
		foreach (float frameTimeDatum in frameTimeData)
		{
			if (frameTimeDatum > num3)
			{
				m.framePacingViolations20++;
				num5++;
			}
			else
			{
				num5 = 0;
			}
			if (frameTimeDatum > num4)
			{
				m.framePacingViolations50++;
			}
			m.maxConsecutiveBadFrames = Mathf.Max(m.maxConsecutiveBadFrames, num5);
		}
		m.framePacingViolationPercent20 = (float)m.framePacingViolations20 / (float)m.totalFramesCaptured * 100f;
		m.framePacingViolationPercent50 = (float)m.framePacingViolations50 / (float)m.totalFramesCaptured * 100f;
		List<SpikeEvent> list2 = (lastRunSpikeEvents = ExtractSpikeEvents(CalculateRollingBaseline()));
		m.spikeEventCount = list2.Count;
		m.gcAttributedSpikeCount = list2.Count([PublicizedFrom(EAccessModifier.Internal)] (SpikeEvent s) => s.isGcAttributed);
		m.nonGcAttributedSpikeCount = list2.Count([PublicizedFrom(EAccessModifier.Internal)] (SpikeEvent s) => !s.isGcAttributed);
		m.gcAttributedSpikePercent = ((list2.Count > 0) ? ((float)m.gcAttributedSpikeCount / (float)list2.Count * 100f) : 0f);
		if (list2.Count > 0)
		{
			m.meanSpikeDurationMs = list2.Average([PublicizedFrom(EAccessModifier.Internal)] (SpikeEvent s) => s.totalDurationMs);
			m.maxSpikeDurationMs = list2.Max([PublicizedFrom(EAccessModifier.Internal)] (SpikeEvent s) => s.totalDurationMs);
			List<SpikeEvent> list3 = list2.Where([PublicizedFrom(EAccessModifier.Internal)] (SpikeEvent s) => s.isGcAttributed).ToList();
			List<SpikeEvent> list4 = list2.Where([PublicizedFrom(EAccessModifier.Internal)] (SpikeEvent s) => !s.isGcAttributed).ToList();
			m.meanGcSpikeMs = ((list3.Count > 0) ? list3.Average([PublicizedFrom(EAccessModifier.Internal)] (SpikeEvent s) => s.totalDurationMs) : 0f);
			m.meanNonGcSpikeMs = ((list4.Count > 0) ? list4.Average([PublicizedFrom(EAccessModifier.Internal)] (SpikeEvent s) => s.totalDurationMs) : 0f);
		}
		if (list2.Count >= 2)
		{
			float num6 = 0f;
			for (int num7 = 1; num7 < list2.Count; num7++)
			{
				num6 += (float)(list2[num7].startFrame - list2[num7 - 1].startFrame);
			}
			float num8 = num6 / (float)(list2.Count - 1);
			m.meanTimeBetweenSpikesSec = num8 * (captureDuration / (float)m.totalFramesCaptured);
		}
		List<int> list5 = DetectGcCollections();
		m.estimatedGcCollectionCount = list5.Count;
		if (list5.Count >= 2)
		{
			float num9 = 0f;
			for (int num10 = 1; num10 < list5.Count; num10++)
			{
				num9 += (float)(list5[num10] - list5[num10 - 1]);
			}
			float num11 = num9 / (float)(list5.Count - 1);
			m.meanGcIntervalSec = num11 * (captureDuration / (float)m.totalFramesCaptured);
		}
		return m;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static AggregateMetrics CalculateAggregateMetrics(List<PerformanceMetrics> runs)
	{
		AggregateMetrics agg = default(AggregateMetrics);
		if (runs.Count == 0)
		{
			return agg;
		}
		agg.runCount = runs.Count;
		agg.meanAverageFrameTime = runs.Average([PublicizedFrom(EAccessModifier.Internal)] (PerformanceMetrics r) => r.averageFrameTime);
		agg.meanAverageFPS = runs.Average([PublicizedFrom(EAccessModifier.Internal)] (PerformanceMetrics r) => r.averageFPS);
		agg.meanFivePercentLow = runs.Average([PublicizedFrom(EAccessModifier.Internal)] (PerformanceMetrics r) => r.fivePercentLow);
		agg.meanOnePercentLow = runs.Average([PublicizedFrom(EAccessModifier.Internal)] (PerformanceMetrics r) => r.onePercentLow);
		agg.meanPointOnePercentLow = runs.Average([PublicizedFrom(EAccessModifier.Internal)] (PerformanceMetrics r) => r.pointOnePercentLow);
		agg.meanFramePacingViolationPercent20 = runs.Average([PublicizedFrom(EAccessModifier.Internal)] (PerformanceMetrics r) => r.framePacingViolationPercent20);
		agg.meanFramePacingViolationPercent50 = runs.Average([PublicizedFrom(EAccessModifier.Internal)] (PerformanceMetrics r) => r.framePacingViolationPercent50);
		agg.meanMaxConsecutiveBadFrames = (float)runs.Average([PublicizedFrom(EAccessModifier.Internal)] (PerformanceMetrics r) => r.maxConsecutiveBadFrames);
		agg.meanSpikeEventCount = (float)runs.Average([PublicizedFrom(EAccessModifier.Internal)] (PerformanceMetrics r) => r.spikeEventCount);
		agg.meanGcAttributedSpikePercent = runs.Average([PublicizedFrom(EAccessModifier.Internal)] (PerformanceMetrics r) => r.gcAttributedSpikePercent);
		agg.meanTimeBetweenSpikesSec = runs.Average([PublicizedFrom(EAccessModifier.Internal)] (PerformanceMetrics r) => r.meanTimeBetweenSpikesSec);
		agg.meanEstimatedGcCollectionCount = (float)runs.Average([PublicizedFrom(EAccessModifier.Internal)] (PerformanceMetrics r) => r.estimatedGcCollectionCount);
		agg.meanGcIntervalSec = runs.Average([PublicizedFrom(EAccessModifier.Internal)] (PerformanceMetrics r) => r.meanGcIntervalSec);
		agg.totalFramesCaptured = runs.Sum([PublicizedFrom(EAccessModifier.Internal)] (PerformanceMetrics r) => r.totalFramesCaptured);
		agg.totalCaptureDurationSeconds = runs.Sum([PublicizedFrom(EAccessModifier.Internal)] (PerformanceMetrics r) => r.captureDurationSeconds);
		agg.bestRunAverageFPS = runs.Max([PublicizedFrom(EAccessModifier.Internal)] (PerformanceMetrics r) => r.averageFPS);
		agg.worstRunAverageFPS = runs.Min([PublicizedFrom(EAccessModifier.Internal)] (PerformanceMetrics r) => r.averageFPS);
		float num = runs.Sum([PublicizedFrom(EAccessModifier.Internal)] (PerformanceMetrics r) => (r.averageFPS - agg.meanAverageFPS) * (r.averageFPS - agg.meanAverageFPS));
		float num2 = runs.Sum([PublicizedFrom(EAccessModifier.Internal)] (PerformanceMetrics r) => (r.onePercentLow - agg.meanOnePercentLow) * (r.onePercentLow - agg.meanOnePercentLow));
		agg.stdDevAverageFPS = Mathf.Sqrt(num / (float)runs.Count);
		agg.stdDevOnePercentLow = Mathf.Sqrt(num2 / (float)runs.Count);
		return agg;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static float GetPercentile(List<float> sorted, float percentile)
	{
		if (sorted.Count == 0)
		{
			return 0f;
		}
		if (sorted.Count == 1)
		{
			return sorted[0];
		}
		float num = percentile / 100f * (float)(sorted.Count - 1);
		int num2 = (int)num;
		float num3 = num - (float)num2;
		if (num2 + 1 >= sorted.Count)
		{
			return sorted[num2];
		}
		return sorted[num2] * (1f - num3) + sorted[num2 + 1] * num3;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string BuildBasePath(string directory)
	{
		string postTerminationAccessiblePath = GameIO.GetPostTerminationAccessiblePath();
		postTerminationAccessiblePath = postTerminationAccessiblePath + "PerformanceCaptures/" + directory + "/";
		if (!Directory.Exists(postTerminationAccessiblePath))
		{
			Directory.CreateDirectory(postTerminationAccessiblePath);
		}
		return postTerminationAccessiblePath;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool TryCreateProfilerRecorder()
	{
		string[] array = new string[4] { "Main Thread", "CPU Frame Time", "Frame Time", "CPU Usage" };
		foreach (string text in array)
		{
			try
			{
				frameTimeRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Internal, text, 15);
				if (frameTimeRecorder.Valid)
				{
					Debug.Log("[PerformanceProfiler] Using recorder: " + text);
					return true;
				}
				frameTimeRecorder.Dispose();
			}
			catch
			{
			}
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void SaveRawData(string filepath)
	{
		try
		{
			using StreamWriter streamWriter = new StreamWriter(filepath);
			streamWriter.WriteLine("Frame,FrameTime(ms),SystemTextureMem,SystemTextureMemDelta,TexStreamCurrent(MB),TexStreamTarget(MB),TexStreamDesired(MB),TexStreamBudget(MB),GCHeap(MB),RSS(MB),Chunks,DisplayedCGOs,Enemies,Entities,EntityInstances,Items,TimeSinceLevelLoad(min)");
			for (int i = 0; i < frameTimeData.Count; i++)
			{
				FrameInfo frameInfo = frameExtraData[i];
				streamWriter.WriteLine($"{i + 1},{frameTimeData[i]:F3}," + $"{frameInfo.systemTextureMemory},{frameInfo.systemTextureMemoryDelta}," + $"{frameInfo.texStreamCurrentMB:F2},{frameInfo.texStreamTargetMB:F2},{frameInfo.texStreamDesiredMB:F2},{frameInfo.texStreamBudgetMB:F2}," + $"{frameInfo.gcHeapMemoryMB:F2},{frameInfo.rssMemoryMB:F2}," + $"{frameInfo.chunkCount},{frameInfo.displayedChunkObjects}," + $"{frameInfo.enemyCount},{frameInfo.entityCount},{frameInfo.entityInstanceCount},{frameInfo.itemCount}," + $"{frameInfo.timeSinceLevelLoadMin:F4}");
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("[PerformanceProfiler] Failed to save raw data: " + ex.Message);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void SaveSpikeEvents(string filepath, List<SpikeEvent> spikes)
	{
		try
		{
			using StreamWriter streamWriter = new StreamWriter(filepath);
			streamWriter.WriteLine("SpikeIndex,StartFrame,DurationFrames,TotalDurationMs,PeakFrameTimeMs,BaselineFrameTimeMs,PeakMultiple,IsGcAttributed,GcHeapDropMB");
			for (int i = 0; i < spikes.Count; i++)
			{
				SpikeEvent spikeEvent = spikes[i];
				streamWriter.WriteLine($"{i + 1},{spikeEvent.startFrame},{spikeEvent.durationFrames},{spikeEvent.totalDurationMs:F3}," + $"{spikeEvent.peakFrameTimeMs:F3},{spikeEvent.baselineFrameTimeMs:F3},{spikeEvent.peakMultiple:F2}," + $"{spikeEvent.isGcAttributed},{spikeEvent.gcHeapDropMB:F3}");
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("[PerformanceProfiler] Failed to save spike data: " + ex.Message);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void SaveMemorySnapshot(string filepath)
	{
		if (memoryProfiler == null)
		{
			return;
		}
		try
		{
			string lastValueCsv = memoryProfiler.GetLastValueCsv();
			using StreamWriter streamWriter = new StreamWriter(filepath);
			streamWriter.WriteLine("Snapshot," + memoryProfiler.GetCsvHeader());
			if (memorySnapshotStart != null)
			{
				streamWriter.WriteLine("Before," + memorySnapshotStart);
			}
			streamWriter.WriteLine("After," + lastValueCsv);
		}
		catch (Exception ex)
		{
			Debug.LogError("[PerformanceProfiler] Failed to save memory snapshot: " + ex.Message);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void SaveAnalysis(string filepath, PerformanceMetrics m)
	{
		try
		{
			using StreamWriter streamWriter = new StreamWriter(filepath);
			streamWriter.WriteLine("Metric,Value");
			streamWriter.WriteLine($"AverageFrameTime(ms),{m.averageFrameTime:F3}");
			streamWriter.WriteLine($"AverageFPS,{m.averageFPS:F2}");
			streamWriter.WriteLine($"MinFrameTime(ms),{m.minFrameTime:F3}");
			streamWriter.WriteLine($"MaxFrameTime(ms),{m.maxFrameTime:F3}");
			streamWriter.WriteLine($"5%Low(ms),{m.fivePercentLow:F3}");
			streamWriter.WriteLine($"1%Low(ms),{m.onePercentLow:F3}");
			streamWriter.WriteLine($"0.1%Low(ms),{m.pointOnePercentLow:F3}");
			streamWriter.WriteLine($"StandardDeviation(ms),{m.standardDeviation:F3}");
			streamWriter.WriteLine($"CoefficientOfVariation,{m.coefficientOfVariation:F4}");
			streamWriter.WriteLine($"FramePacingViolations(>20%),{m.framePacingViolations20}");
			streamWriter.WriteLine($"FramePacingViolations(>50%),{m.framePacingViolations50}");
			streamWriter.WriteLine($"FramePacingViolationPercent(>20%),{m.framePacingViolationPercent20:F2}");
			streamWriter.WriteLine($"FramePacingViolationPercent(>50%),{m.framePacingViolationPercent50:F2}");
			streamWriter.WriteLine($"MaxConsecutiveBadFrames,{m.maxConsecutiveBadFrames}");
			streamWriter.WriteLine($"SpikeEventCount,{m.spikeEventCount}");
			streamWriter.WriteLine($"GcAttributedSpikeCount,{m.gcAttributedSpikeCount}");
			streamWriter.WriteLine($"NonGcAttributedSpikeCount,{m.nonGcAttributedSpikeCount}");
			streamWriter.WriteLine($"GcAttributedSpikePercent,{m.gcAttributedSpikePercent:F1}");
			streamWriter.WriteLine($"MeanSpikeDurationMs,{m.meanSpikeDurationMs:F3}");
			streamWriter.WriteLine($"MaxSpikeDurationMs,{m.maxSpikeDurationMs:F3}");
			streamWriter.WriteLine($"MeanTimeBetweenSpikesSec,{m.meanTimeBetweenSpikesSec:F2}");
			streamWriter.WriteLine($"MeanGcSpikeMs,{m.meanGcSpikeMs:F3}");
			streamWriter.WriteLine($"MeanNonGcSpikeMs,{m.meanNonGcSpikeMs:F3}");
			streamWriter.WriteLine($"EstimatedGcCollectionCount,{m.estimatedGcCollectionCount}");
			streamWriter.WriteLine($"MeanGcIntervalSec,{m.meanGcIntervalSec:F2}");
			streamWriter.WriteLine($"TargetFramerate,{targetFramerate}");
			streamWriter.WriteLine($"TotalFramesCaptured,{m.totalFramesCaptured}");
			streamWriter.WriteLine($"CaptureDuration(s),{m.captureDurationSeconds:F2}");
			streamWriter.WriteLine($"[Config]SpikeBaselineHalfWindow,{30}");
			streamWriter.WriteLine($"[Config]SpikeThresholdMultiplier,{2f}");
			streamWriter.WriteLine($"[Config]SpikeMergeGapFrames,{3}");
			streamWriter.WriteLine($"[Config]GcAttributionHeapDropMB,{0.5f}");
		}
		catch (Exception ex)
		{
			Debug.LogError("[PerformanceProfiler] Failed to save analysis: " + ex.Message);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void SaveAggregateAnalysis(string filepath, AggregateMetrics agg)
	{
		try
		{
			using StreamWriter streamWriter = new StreamWriter(filepath);
			streamWriter.WriteLine("Metric,Value");
			streamWriter.WriteLine($"RunCount,{agg.runCount}");
			streamWriter.WriteLine($"MeanAverageFrameTime(ms),{agg.meanAverageFrameTime:F3}");
			streamWriter.WriteLine($"MeanAverageFPS,{agg.meanAverageFPS:F2}");
			streamWriter.WriteLine($"StdDevAverageFPS,{agg.stdDevAverageFPS:F3}");
			streamWriter.WriteLine($"MeanFivePercentLow(ms),{agg.meanFivePercentLow:F3}");
			streamWriter.WriteLine($"MeanOnePercentLow(ms),{agg.meanOnePercentLow:F3}");
			streamWriter.WriteLine($"StdDevOnePercentLow(ms),{agg.stdDevOnePercentLow:F3}");
			streamWriter.WriteLine($"MeanPointOnePercentLow(ms),{agg.meanPointOnePercentLow:F3}");
			streamWriter.WriteLine($"BestRunAverageFPS,{agg.bestRunAverageFPS:F2}");
			streamWriter.WriteLine($"WorstRunAverageFPS,{agg.worstRunAverageFPS:F2}");
			streamWriter.WriteLine($"MeanFramePacingViolationPercent(>20%),{agg.meanFramePacingViolationPercent20:F2}");
			streamWriter.WriteLine($"MeanFramePacingViolationPercent(>50%),{agg.meanFramePacingViolationPercent50:F2}");
			streamWriter.WriteLine($"MeanMaxConsecutiveBadFrames,{agg.meanMaxConsecutiveBadFrames:F1}");
			streamWriter.WriteLine($"MeanSpikeEventCount,{agg.meanSpikeEventCount:F1}");
			streamWriter.WriteLine($"MeanGcAttributedSpikePercent,{agg.meanGcAttributedSpikePercent:F1}");
			streamWriter.WriteLine($"MeanTimeBetweenSpikesSec,{agg.meanTimeBetweenSpikesSec:F2}");
			streamWriter.WriteLine($"MeanEstimatedGcCollectionCount,{agg.meanEstimatedGcCollectionCount:F1}");
			streamWriter.WriteLine($"MeanGcIntervalSec,{agg.meanGcIntervalSec:F2}");
			streamWriter.WriteLine($"TotalFramesCaptured,{agg.totalFramesCaptured}");
			streamWriter.WriteLine($"TotalCaptureDuration(s),{agg.totalCaptureDurationSeconds:F2}");
		}
		catch (Exception ex)
		{
			Debug.LogError("[PerformanceProfiler] Failed to save aggregate: " + ex.Message);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void LogSummary(PerformanceMetrics m)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("=== Performance Run Summary ===");
		stringBuilder.AppendLine($"  Average    : {m.averageFrameTime:F2}ms  ({m.averageFPS:F1} FPS)");
		stringBuilder.AppendLine($"  5% Low     : {m.fivePercentLow:F2}ms  ({1000f / m.fivePercentLow:F1} FPS)");
		stringBuilder.AppendLine($"  1% Low     : {m.onePercentLow:F2}ms  ({1000f / m.onePercentLow:F1} FPS)");
		stringBuilder.AppendLine($"  0.1% Low   : {m.pointOnePercentLow:F2}ms  ({1000f / m.pointOnePercentLow:F1} FPS)");
		stringBuilder.AppendLine($"  Range      : {m.minFrameTime:F2}ms – {m.maxFrameTime:F2}ms");
		stringBuilder.AppendLine($"  Std Dev    : {m.standardDeviation:F2}ms  (CV: {m.coefficientOfVariation:F3})");
		stringBuilder.AppendLine($"  Pacing     : {m.framePacingViolationPercent20:F1}% frames >20% over target, max {m.maxConsecutiveBadFrames} consecutive");
		stringBuilder.AppendLine($"  Spikes     : {m.spikeEventCount} events  |  GC-attributed: {m.gcAttributedSpikeCount} ({m.gcAttributedSpikePercent:F0}%)  |  other: {m.nonGcAttributedSpikeCount}");
		if (m.spikeEventCount > 0)
		{
			stringBuilder.AppendLine($"  Spike dur  : mean {m.meanSpikeDurationMs:F1}ms  max {m.maxSpikeDurationMs:F1}ms" + ((m.spikeEventCount >= 2) ? $"  every {m.meanTimeBetweenSpikesSec:F1}s" : ""));
			if (m.gcAttributedSpikeCount > 0)
			{
				stringBuilder.AppendLine($"  GC spikes  : mean {m.meanGcSpikeMs:F1}ms  interval {m.meanGcIntervalSec:F1}s  ({m.estimatedGcCollectionCount} est. collections)");
			}
			if (m.nonGcAttributedSpikeCount > 0)
			{
				stringBuilder.AppendLine($"  Other spike: mean {m.meanNonGcSpikeMs:F1}ms");
			}
		}
		Debug.Log(stringBuilder.ToString());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void LogAggregateSummary(AggregateMetrics agg)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine($"=== Session Aggregate Summary ({agg.runCount} runs) ===");
		stringBuilder.AppendLine($"  Mean FPS          : {agg.meanAverageFPS:F1}  (±{agg.stdDevAverageFPS:F2})");
		stringBuilder.AppendLine($"  Mean 1% Low       : {agg.meanOnePercentLow:F2}ms  (±{agg.stdDevOnePercentLow:F2})");
		stringBuilder.AppendLine($"  Best / Worst      : {agg.bestRunAverageFPS:F1} FPS / {agg.worstRunAverageFPS:F1} FPS");
		stringBuilder.AppendLine($"  Mean spikes/run   : {agg.meanSpikeEventCount:F1}  |  GC-attributed: {agg.meanGcAttributedSpikePercent:F0}%");
		stringBuilder.AppendLine($"  Mean spike freq   : every {agg.meanTimeBetweenSpikesSec:F1}s");
		stringBuilder.AppendLine($"  Mean GC interval  : {agg.meanGcIntervalSec:F1}s  ({agg.meanEstimatedGcCollectionCount:F1} collections/run)");
		stringBuilder.AppendLine($"  Mean pacing       : {agg.meanFramePacingViolationPercent20:F1}% frames >20% over target");
		stringBuilder.AppendLine($"  Total frames      : {agg.totalFramesCaptured}  over {agg.totalCaptureDurationSeconds:F1}s");
		Debug.Log(stringBuilder.ToString());
	}
}
