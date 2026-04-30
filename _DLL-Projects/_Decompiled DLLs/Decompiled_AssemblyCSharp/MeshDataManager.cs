using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Rendering;

public class MeshDataManager
{
	[PublicizedFrom(EAccessModifier.Private)]
	public sealed class JobBatch : IDisposable
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public Mesh.MeshDataArray m_meshDataArray;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly JobState[] m_states;

		[PublicizedFrom(EAccessModifier.Private)]
		public Task m_statesAllComplete;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool m_disposed;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool m_disposedMeshData;

		public Mesh.MeshDataArray MeshDataArray => m_meshDataArray;

		public JobBatch(IEnumerable<JobState> stateSupplier)
		{
			m_states = stateSupplier.ToArray();
			m_meshDataArray = Mesh.AllocateWritableMeshData(m_states.Length);
		}

		public void Start()
		{
			Task[] array = new Task[m_states.Length];
			for (int i = 0; i < m_states.Length; i++)
			{
				array[i] = m_states[i].Start(m_meshDataArray[i]);
			}
			m_statesAllComplete = Task.WhenAll(array);
		}

		public void ApplyAndDispose()
		{
			m_statesAllComplete.Wait();
			Mesh[] meshes = m_states.Select([PublicizedFrom(EAccessModifier.Internal)] (JobState s) => s.Mesh).ToArray();
			m_disposedMeshData = true;
			Mesh.ApplyAndDisposeWritableMeshData(m_meshDataArray, meshes, MeshUpdateFlags.DontRecalculateBounds);
			m_meshDataArray = default(Mesh.MeshDataArray);
			JobState[] states = m_states;
			for (int num = 0; num < states.Length; num++)
			{
				states[num].PostApply();
			}
			Dispose();
		}

		public void Dispose()
		{
			if (!m_disposed)
			{
				m_disposed = true;
				JobState[] states = m_states;
				for (int i = 0; i < states.Length; i++)
				{
					states[i]?.Dispose();
				}
				if (!m_disposedMeshData)
				{
					m_meshDataArray.Dispose();
					m_meshDataArray = default(Mesh.MeshDataArray);
					m_disposedMeshData = true;
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public sealed class JobState : IDisposable
	{
		public readonly Mesh Mesh;

		[PublicizedFrom(EAccessModifier.Private)]
		public PinnedBuffer<Vector3> Vertices;

		[PublicizedFrom(EAccessModifier.Private)]
		public PinnedBuffer<int> Indices;

		[PublicizedFrom(EAccessModifier.Private)]
		public PinnedBuffer<Vector3> Normals;

		[PublicizedFrom(EAccessModifier.Private)]
		public PinnedBuffer<Vector4> Tangents;

		[PublicizedFrom(EAccessModifier.Private)]
		public PinnedBuffer<Color> Colors;

		[PublicizedFrom(EAccessModifier.Private)]
		public PinnedBuffer<Vector2> TexCoord0s;

		[PublicizedFrom(EAccessModifier.Private)]
		public PinnedBuffer<Vector2> TexCoord1s;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly bool GenerateNormals;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly bool GenerateTangents;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly bool RecalculateUvDistributionMetrics;

		[PublicizedFrom(EAccessModifier.Private)]
		public Action OnJobComplete;

		[PublicizedFrom(EAccessModifier.Private)]
		public CancellationTokenSource CopyTaskCancellation;

		[PublicizedFrom(EAccessModifier.Private)]
		public Task CopyTask;

		[PublicizedFrom(EAccessModifier.Private)]
		public PinnedBuffer<VertexAttributeDescriptor> VertexAttributes;

		[PublicizedFrom(EAccessModifier.Private)]
		public Bounds Bounds;

		public JobState(Mesh mesh, PinnedBuffer<Vector3> vertices, PinnedBuffer<int> indices, PinnedBuffer<Vector3> normals, PinnedBuffer<Vector4> tangents, PinnedBuffer<Color> colors, PinnedBuffer<Vector2> texCoord0s, PinnedBuffer<Vector2> texCoord1s, bool generateNormals, bool generateTangents, bool recalculateUvDistributionMetrics, Action onJobComplete)
		{
			Mesh = mesh;
			Vertices = vertices;
			Indices = indices;
			Normals = normals;
			Tangents = tangents;
			Colors = colors;
			TexCoord0s = texCoord0s;
			TexCoord1s = texCoord1s;
			GenerateNormals = generateNormals;
			GenerateTangents = generateTangents;
			RecalculateUvDistributionMetrics = recalculateUvDistributionMetrics;
			OnJobComplete = onJobComplete;
		}

		public void Dispose()
		{
			CopyTaskCancellation?.Cancel();
			CopyTask?.Wait();
			CopyTask?.Dispose();
			CopyTask = null;
			CopyTaskCancellation?.Dispose();
			CopyTaskCancellation = null;
			VertexAttributes?.Dispose();
			VertexAttributes = null;
			Vertices?.Dispose();
			Vertices = null;
			Indices?.Dispose();
			Indices = null;
			TexCoord0s?.Dispose();
			TexCoord0s = null;
			TexCoord1s?.Dispose();
			TexCoord1s = null;
			Normals?.Dispose();
			Normals = null;
			Tangents?.Dispose();
			Tangents = null;
			Colors?.Dispose();
			Colors = null;
			OnJobComplete?.Invoke();
			OnJobComplete = null;
		}

		public Task Start(Mesh.MeshData meshData)
		{
			CopyTaskCancellation = new CancellationTokenSource();
			return CopyTask = Task.Run([PublicizedFrom(EAccessModifier.Internal)] () => CopyToMeshData(meshData, CopyTaskCancellation.Token), CopyTaskCancellation.Token);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public async Task CopyToMeshData(Mesh.MeshData meshData, CancellationToken cancellationToken)
		{
			MeshDataLayout layout;
			using (NativeList<VertexAttributeDescriptor> vertexAttributesOut = new NativeList<VertexAttributeDescriptor>(Allocator.Persistent))
			{
				layout = MeshDataUtils.SetAttributes(meshData, Vertices, Indices, Normals, Tangents, Colors, TexCoord0s, TexCoord1s, vertexAttributesOut);
				VertexAttributes = new PinnedBuffer<VertexAttributeDescriptor>(vertexAttributesOut.Length);
				vertexAttributesOut.AsArray().AsReadOnlySpan().CopyTo(VertexAttributes);
			}
			List<Task> list = new List<Task>(3);
			Task task = Task.Run([PublicizedFrom(EAccessModifier.Internal)] () => Bounds = MeshDataUtils.RecalculateBounds(Vertices), cancellationToken);
			Task task2 = null;
			if (GenerateNormals)
			{
				task2 = Task.Run([PublicizedFrom(EAccessModifier.Internal)] () =>
				{
					MeshDataUtils.CalculateNormals(Vertices, Indices, Normals);
				}, cancellationToken);
			}
			Task task3 = null;
			if (GenerateTangents)
			{
				task3 = task2?.ContinueWith([PublicizedFrom(EAccessModifier.Internal)] (Task _) => GenerateTask(), cancellationToken) ?? GenerateTask();
			}
			Task task4 = Task.Run([PublicizedFrom(EAccessModifier.Internal)] () =>
			{
				Indices.AsReadOnlySpan().CopyTo(meshData.GetIndexData<int>());
			}, cancellationToken);
			Task item = Task.WhenAll(task4, task).ContinueWith([PublicizedFrom(EAccessModifier.Internal)] (Task _) =>
			{
				MeshDataUtils.SetSubmesh(meshData, Vertices.Length, Indices.Length, Bounds);
			}, cancellationToken);
			list.Add(item);
			List<Task> list2 = new List<Task>(6);
			NativeArray<byte> vertexData = meshData.GetVertexData<byte>();
			list2.Add(Task.Run([PublicizedFrom(EAccessModifier.Internal)] () =>
			{
				MeshDataUtils.CopyInterleaved(layout.PositionSize, layout.PositionOffset, layout.Stride, Vertices.AsBytes(), vertexData);
			}, cancellationToken));
			if (Normals.Length > 0)
			{
				list2.Add(task2?.ContinueWith([PublicizedFrom(EAccessModifier.Internal)] (Task _) => CopyTask(), cancellationToken) ?? CopyTask());
			}
			if (Tangents.Length > 0)
			{
				list2.Add(task3?.ContinueWith([PublicizedFrom(EAccessModifier.Internal)] (Task _) => CopyTask2(), cancellationToken) ?? CopyTask2());
			}
			if (Colors.Length > 0)
			{
				list2.Add(Task.Run([PublicizedFrom(EAccessModifier.Internal)] () =>
				{
					MeshDataUtils.CopyInterleaved(layout.ColorSize, layout.ColorOffset, layout.Stride, Colors.AsBytes(), vertexData);
				}, cancellationToken));
			}
			if (TexCoord0s.Length > 0)
			{
				list2.Add(Task.Run([PublicizedFrom(EAccessModifier.Internal)] () =>
				{
					MeshDataUtils.CopyInterleaved(layout.TexCoord0Size, layout.TexCoord0Offset, layout.Stride, TexCoord0s.AsBytes(), vertexData);
				}, cancellationToken));
			}
			if (TexCoord1s.Length > 0)
			{
				list2.Add(Task.Run([PublicizedFrom(EAccessModifier.Internal)] () =>
				{
					MeshDataUtils.CopyInterleaved(layout.TexCoord1Size, layout.TexCoord1Offset, layout.Stride, TexCoord1s.AsBytes(), vertexData);
				}, cancellationToken));
			}
			Task item2 = Task.WhenAll(list2.ToArray()).ContinueWith([PublicizedFrom(EAccessModifier.Internal)] (Task _) => Task.Run([PublicizedFrom(EAccessModifier.Internal)] () =>
			{
				NativeArray<VertexAttributeDescriptor> array;
				using (VertexAttributes.CreateNativeArray(out array))
				{
					MeshDataUtils.ApplyMeshDataCompression(meshData, array);
				}
			}, cancellationToken), cancellationToken);
			list.Add(item2);
			await Task.WhenAll(list);
			[PublicizedFrom(EAccessModifier.Internal)]
			Task CopyTask()
			{
				return Task.Run([PublicizedFrom(EAccessModifier.Internal)] () =>
				{
					MeshDataUtils.CopyInterleaved(layout.NormalSize, layout.NormalOffset, layout.Stride, Normals.AsBytes(), vertexData);
				}, cancellationToken);
			}
			[PublicizedFrom(EAccessModifier.Internal)]
			Task CopyTask2()
			{
				return Task.Run([PublicizedFrom(EAccessModifier.Internal)] () =>
				{
					MeshDataUtils.CopyInterleaved(layout.TangentSize, layout.TangentOffset, layout.Stride, Tangents.AsBytes(), vertexData);
				}, cancellationToken);
			}
			[PublicizedFrom(EAccessModifier.Internal)]
			Task GenerateTask()
			{
				return Task.Run([PublicizedFrom(EAccessModifier.Internal)] () =>
				{
					Utils.CalculateMeshTangents(Vertices, Indices, Normals, TexCoord0s, Tangents, TexCoord0s.Length <= 0);
				}, cancellationToken);
			}
		}

		public void PostApply()
		{
			Mesh.bounds = Bounds;
			if (RecalculateUvDistributionMetrics)
			{
				Mesh.RecalculateUVDistributionMetrics();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerMarker s_markerCompleteBatches = new ProfilerMarker("MeshDataManager.CompleteBatches");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerMarker s_markerAdd = new ProfilerMarker("MeshDataManager.Add");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerMarker s_markerStartBatches = new ProfilerMarker("MeshDataManager.StartBatches");

	public static readonly MeshDataManager Instance = new MeshDataManager();

	public static bool Enabled;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<JobState> m_toStartForEndOfFrame = new List<JobState>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<JobState> m_toStartForEndOfNextFrame = new List<JobState>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<JobBatch> m_completeAtEndOfFrame = new List<JobBatch>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<JobBatch> m_completeAtEndOfNextFrame = new List<JobBatch>();

	[PublicizedFrom(EAccessModifier.Private)]
	public MeshDataManager()
	{
	}

	public static void Init()
	{
		Enabled = true;
	}

	public void LateUpdate()
	{
		CheckUnstarted();
		CompleteBatches();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CheckUnstarted()
	{
		if (m_toStartForEndOfFrame.Count > 0 || m_toStartForEndOfNextFrame.Count > 0)
		{
			Log.Error("[MeshDataManager] There were unstarted batches at the end of the frame.");
			StartBatches();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CompleteBatches()
	{
		if (m_completeAtEndOfFrame.Count <= 0 && m_completeAtEndOfNextFrame.Count <= 0)
		{
			return;
		}
		using (s_markerCompleteBatches.Auto())
		{
			foreach (JobBatch item in m_completeAtEndOfFrame)
			{
				try
				{
					item.ApplyAndDispose();
				}
				catch (Exception arg)
				{
					Log.Error($"[MeshDataManager] Failed to apply and dispose job batch: {arg}");
				}
				finally
				{
					try
					{
						item.Dispose();
					}
					catch (Exception arg2)
					{
						Log.Error($"[MeshDataManager] Failed to dispose job batch: {arg2}");
					}
				}
			}
			m_completeAtEndOfFrame.Clear();
			List<JobBatch> completeAtEndOfNextFrame = m_completeAtEndOfNextFrame;
			List<JobBatch> completeAtEndOfFrame = m_completeAtEndOfFrame;
			m_completeAtEndOfFrame = completeAtEndOfNextFrame;
			m_completeAtEndOfNextFrame = completeAtEndOfFrame;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool PreValidateJobData(Mesh mesh, ReadOnlySpan<Vector3> vertices, ReadOnlySpan<int> indices, ReadOnlySpan<Vector3> normals, ReadOnlySpan<Vector4> tangents, ReadOnlySpan<Color> colors, ReadOnlySpan<Vector2> texCoord0s, ReadOnlySpan<Vector2> texCoord1s)
	{
		if (!mesh)
		{
			Log.Error("Mesh is not valid.");
			return false;
		}
		int length = vertices.Length;
		if (length <= 0)
		{
			Log.Error($"Vertices.Length ({length}) <= 0");
			return false;
		}
		int length2 = indices.Length;
		if (length2 <= 0)
		{
			Log.Error($"Indices.Length ({length2}) <= 0");
			return false;
		}
		if (normals != null && normals.Length > 0 && normals.Length != length)
		{
			Log.Error($"Normals.Count ({normals.Length}) != Vertices.Length ({length})");
			return false;
		}
		if (tangents != null && tangents.Length > 0 && tangents.Length != length)
		{
			Log.Error($"Tangents.Length ({tangents.Length}) != Vertices.Length ({length})");
			return false;
		}
		if (colors.Length != length)
		{
			Log.Error($"Colors.Length ({colors.Length}) != Vertices.Length ({length})");
			return false;
		}
		if (texCoord0s != null && texCoord0s.Length > 0 && texCoord0s.Length != length)
		{
			Log.Error($"TexCoord0s.Length ({texCoord0s.Length}) != Vertices.Length ({length})");
			return false;
		}
		if (texCoord1s != null && texCoord1s.Length > 0 && texCoord1s.Length != length)
		{
			Log.Error($"TexCoord1s.Length ({texCoord1s.Length}) != Vertices.Length ({length})");
			return false;
		}
		for (int i = 0; i < length2; i++)
		{
			if (indices[i] < 0)
			{
				Log.Error($"Indices[{i}] ({indices[i]}) < 0");
				return false;
			}
			if (indices[i] > length)
			{
				Log.Error($"Indices[{i}] ({indices[i]}) > Vertices.Length ({length})");
				return false;
			}
		}
		return true;
	}

	public void Add(Mesh mesh, ArrayListMP<Vector3> vertices, ArrayListMP<int> indices, ArrayListMP<Vector3> normals, ArrayListMP<Vector4> tangents, ArrayListMP<Color> colors, ArrayListMP<Vector2> texCoord0s, ArrayListMP<Vector2> texCoord1s, bool cloneData = false, bool generateNormals = true, bool generateTangents = true, bool recalculateUvDistributionMetrics = false, bool sameFrameUpload = false, Action onJobComplete = null)
	{
		using (s_markerAdd.Auto())
		{
			if (!Enabled)
			{
				Log.Warning("[MeshDataManager] MeshDataJob was added while disabled.");
			}
			if (PreValidateJobData(mesh, ToReadOnlySpan(vertices), ToReadOnlySpan(indices), ToReadOnlySpan(normals), ToReadOnlySpan(tangents), ToReadOnlySpan(colors), ToReadOnlySpan(texCoord0s), ToReadOnlySpan(texCoord1s)))
			{
				JobState item = CreateJobState(mesh, vertices, indices, normals, tangents, colors, texCoord0s, texCoord1s, cloneData, generateNormals, generateTangents, recalculateUvDistributionMetrics, onJobComplete);
				if (sameFrameUpload)
				{
					m_toStartForEndOfFrame.Add(item);
				}
				else
				{
					m_toStartForEndOfNextFrame.Add(item);
				}
			}
		}
	}

	public void StartBatches()
	{
		using (s_markerStartBatches.Auto())
		{
			StartBatch(m_toStartForEndOfFrame, m_completeAtEndOfFrame);
			StartBatch(m_toStartForEndOfNextFrame, m_completeAtEndOfNextFrame);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void StartBatch(List<JobState> states, List<JobBatch> batches)
	{
		if (states.Count <= 0)
		{
			return;
		}
		bool flag = false;
		JobBatch jobBatch = null;
		try
		{
			jobBatch = new JobBatch(states);
			states.Clear();
			jobBatch.Start();
			batches.Add(jobBatch);
			flag = true;
		}
		catch (Exception e)
		{
			Log.Exception(e);
		}
		finally
		{
			if (!flag)
			{
				jobBatch?.Dispose();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static ReadOnlySpan<T> ToReadOnlySpan<T>(ArrayListMP<T> list) where T : new()
	{
		if (list != null)
		{
			return list.Items.AsSpan(0, list.Count);
		}
		return ReadOnlySpan<T>.Empty;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public JobState CreateJobState(Mesh mesh, ArrayListMP<Vector3> vertices, ArrayListMP<int> indices, ArrayListMP<Vector3> normals, ArrayListMP<Vector4> tangents, ArrayListMP<Color> colors, ArrayListMP<Vector2> texCoord0s, ArrayListMP<Vector2> texCoord1s, bool cloneData, bool generateNormals, bool generateTangents, bool recalculateUvDistributionMetrics, Action onJobComplete)
	{
		bool flag = false;
		List<PinnedBuffer> list = new List<PinnedBuffer>();
		try
		{
			PinnedBuffer<Vector3> pinnedBuffer = PinnedBuffer.Create(vertices, cloneData);
			list.Add(pinnedBuffer);
			PinnedBuffer<int> pinnedBuffer2 = PinnedBuffer.Create(indices, cloneData);
			list.Add(pinnedBuffer2);
			PinnedBuffer<Vector3> pinnedBuffer3;
			if (generateNormals && (normals == null || normals.Count <= 0))
			{
				pinnedBuffer3 = new PinnedBuffer<Vector3>(MemoryPools.poolVector3, pinnedBuffer.Length);
			}
			else
			{
				pinnedBuffer3 = PinnedBuffer.Create(normals, cloneData);
				generateNormals = false;
			}
			list.Add(pinnedBuffer3);
			PinnedBuffer<Vector4> pinnedBuffer4;
			if (generateTangents && (tangents == null || tangents.Count <= 0) && pinnedBuffer3.Length > 0)
			{
				pinnedBuffer4 = new PinnedBuffer<Vector4>(MemoryPools.poolVector4, pinnedBuffer.Length);
			}
			else
			{
				pinnedBuffer4 = PinnedBuffer.Create(tangents, cloneData);
				generateTangents = false;
			}
			list.Add(pinnedBuffer4);
			PinnedBuffer<Color> pinnedBuffer5 = PinnedBuffer.Create(colors, cloneData);
			list.Add(pinnedBuffer5);
			PinnedBuffer<Vector2> pinnedBuffer6 = PinnedBuffer.Create(texCoord0s, cloneData);
			list.Add(pinnedBuffer6);
			PinnedBuffer<Vector2> pinnedBuffer7 = PinnedBuffer.Create(texCoord1s, cloneData);
			list.Add(pinnedBuffer7);
			JobState result = new JobState(mesh, pinnedBuffer, pinnedBuffer2, pinnedBuffer3, pinnedBuffer4, pinnedBuffer5, pinnedBuffer6, pinnedBuffer7, generateNormals, generateTangents, recalculateUvDistributionMetrics, onJobComplete);
			flag = true;
			return result;
		}
		finally
		{
			if (!flag)
			{
				foreach (PinnedBuffer item in list)
				{
					item.Dispose();
				}
			}
		}
	}
}
