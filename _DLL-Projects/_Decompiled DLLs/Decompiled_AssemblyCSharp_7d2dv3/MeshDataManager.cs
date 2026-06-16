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
		public readonly bool m_sameFrameUpload;

		[PublicizedFrom(EAccessModifier.Private)]
		public Task m_statesAllComplete;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool m_disposed;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool m_disposedMeshData;

		public IReadOnlyList<JobState> States => m_states;

		public JobBatch(IEnumerable<JobState> stateSupplier)
		{
			m_states = stateSupplier.ToArray();
			if (m_states.Length != 0)
			{
				m_meshDataArray = Mesh.AllocateWritableMeshData(m_states.Length);
				m_sameFrameUpload = m_states.Any([PublicizedFrom(EAccessModifier.Internal)] (JobState state) => state.SameFrameUpload);
			}
			else
			{
				m_disposed = true;
				m_disposedMeshData = true;
			}
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

		public bool TryApplyAndDispose()
		{
			if (m_disposedMeshData || m_disposed)
			{
				return true;
			}
			if (!m_sameFrameUpload && !m_statesAllComplete.IsCompleted)
			{
				return false;
			}
			try
			{
				m_statesAllComplete.Wait();
			}
			catch (Exception e)
			{
				Log.Exception(e);
			}
			Mesh[] meshes = m_states.Select([PublicizedFrom(EAccessModifier.Internal)] (JobState s) => s.Mesh).ToArray();
			m_disposedMeshData = true;
			Mesh.ApplyAndDisposeWritableMeshData(m_meshDataArray, meshes, MeshUpdateFlags.DontRecalculateBounds);
			m_meshDataArray = default(Mesh.MeshDataArray);
			JobState[] states = m_states;
			foreach (JobState jobState in states)
			{
				try
				{
					jobState.PostApply();
				}
				catch (Exception e2)
				{
					Log.Exception(e2);
				}
			}
			Dispose();
			return true;
		}

		public void Dispose()
		{
			if (m_disposed)
			{
				return;
			}
			m_disposed = true;
			JobState[] states = m_states;
			foreach (JobState jobState in states)
			{
				try
				{
					jobState?.Dispose();
				}
				catch (Exception e)
				{
					Log.Exception(e);
				}
			}
			if (!m_disposedMeshData)
			{
				m_meshDataArray.Dispose();
				m_meshDataArray = default(Mesh.MeshDataArray);
				m_disposedMeshData = true;
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

		public readonly bool SameFrameUpload;

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

		public JobState(Mesh mesh, PinnedBuffer<Vector3> vertices, PinnedBuffer<int> indices, PinnedBuffer<Vector3> normals, PinnedBuffer<Vector4> tangents, PinnedBuffer<Color> colors, PinnedBuffer<Vector2> texCoord0s, PinnedBuffer<Vector2> texCoord1s, bool generateNormals, bool generateTangents, bool recalculateUvDistributionMetrics, bool sameFrameUpload, Action onJobComplete)
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
			SameFrameUpload = sameFrameUpload;
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
			try
			{
				OnJobComplete?.Invoke();
			}
			catch (Exception e)
			{
				Log.Exception(e);
			}
			finally
			{
				OnJobComplete = null;
			}
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
			Task recalculateBoundsTask = Task.Run((Action)RecalculateBounds, cancellationToken);
			Task generateNormalsTask;
			if (this.GenerateNormals)
			{
				generateNormalsTask = Task.Run((Action)GenerateNormals, cancellationToken);
			}
			else
			{
				generateNormalsTask = Task.CompletedTask;
			}
			Task generateTangentsTask;
			if (this.GenerateTangents)
			{
				generateTangentsTask = Task.Run((Func<Task>)GenerateTangents, cancellationToken);
			}
			else
			{
				generateTangentsTask = Task.CompletedTask;
			}
			Task copyIndicesTask = Task.Run((Action)CopyIndices, cancellationToken);
			Task item = SetSubmesh();
			list.Add(item);
			List<Task> vertexCopyTasks = new List<Task>(6);
			NativeArray<byte> vertexData = meshData.GetVertexData<byte>();
			vertexCopyTasks.Add(Task.Run((Action)CopyPosition, cancellationToken));
			if (Normals.Length > 0)
			{
				vertexCopyTasks.Add(Task.Run((Func<Task>)CopyNormals, cancellationToken));
			}
			if (Tangents.Length > 0)
			{
				vertexCopyTasks.Add(Task.Run((Func<Task>)CopyTangents, cancellationToken));
			}
			if (Colors.Length > 0)
			{
				vertexCopyTasks.Add(Task.Run((Action)CopyColors, cancellationToken));
			}
			if (TexCoord0s.Length > 0)
			{
				vertexCopyTasks.Add(Task.Run((Action)CopyTexCoord0s, cancellationToken));
			}
			if (TexCoord1s.Length > 0)
			{
				vertexCopyTasks.Add(Task.Run((Action)CopyTexCoord1s, cancellationToken));
			}
			list.Add(Task.Run((Func<Task>)CompressMesh, cancellationToken));
			await Task.WhenAll(list);
			[PublicizedFrom(EAccessModifier.Internal)]
			async Task CompressMesh()
			{
				await Task.WhenAll(vertexCopyTasks);
				NativeArray<VertexAttributeDescriptor> array;
				using (VertexAttributes.CreateNativeArray(out array))
				{
					MeshDataUtils.ApplyMeshDataCompression(meshData, array);
				}
			}
			[PublicizedFrom(EAccessModifier.Internal)]
			void CopyColors()
			{
				MeshDataUtils.CopyInterleaved(layout.ColorSize, layout.ColorOffset, layout.Stride, Colors.AsBytes(), vertexData);
			}
			[PublicizedFrom(EAccessModifier.Internal)]
			void CopyIndices()
			{
				Indices.AsReadOnlySpan().CopyTo(meshData.GetIndexData<int>());
			}
			[PublicizedFrom(EAccessModifier.Internal)]
			async Task CopyNormals()
			{
				await generateNormalsTask;
				MeshDataUtils.CopyInterleaved(layout.NormalSize, layout.NormalOffset, layout.Stride, Normals.AsBytes(), vertexData);
			}
			[PublicizedFrom(EAccessModifier.Internal)]
			void CopyPosition()
			{
				MeshDataUtils.CopyInterleaved(layout.PositionSize, layout.PositionOffset, layout.Stride, Vertices.AsBytes(), vertexData);
			}
			[PublicizedFrom(EAccessModifier.Internal)]
			async Task CopyTangents()
			{
				await generateTangentsTask;
				MeshDataUtils.CopyInterleaved(layout.TangentSize, layout.TangentOffset, layout.Stride, Tangents.AsBytes(), vertexData);
			}
			[PublicizedFrom(EAccessModifier.Internal)]
			void CopyTexCoord0s()
			{
				MeshDataUtils.CopyInterleaved(layout.TexCoord0Size, layout.TexCoord0Offset, layout.Stride, TexCoord0s.AsBytes(), vertexData);
			}
			[PublicizedFrom(EAccessModifier.Internal)]
			void CopyTexCoord1s()
			{
				MeshDataUtils.CopyInterleaved(layout.TexCoord1Size, layout.TexCoord1Offset, layout.Stride, TexCoord1s.AsBytes(), vertexData);
			}
			[PublicizedFrom(EAccessModifier.Internal)]
			void GenerateNormals()
			{
				MeshDataUtils.CalculateNormals(Vertices, Indices, Normals);
			}
			[PublicizedFrom(EAccessModifier.Internal)]
			async Task GenerateTangents()
			{
				await generateNormalsTask;
				Utils.CalculateMeshTangents(Vertices, Indices, Normals, TexCoord0s, Tangents, TexCoord0s.Length <= 0);
			}
			[PublicizedFrom(EAccessModifier.Internal)]
			void RecalculateBounds()
			{
				Bounds = MeshDataUtils.RecalculateBounds(Vertices);
			}
			[PublicizedFrom(EAccessModifier.Internal)]
			async Task SetSubmesh()
			{
				await Task.WhenAll(copyIndicesTask, recalculateBoundsTask);
				MeshDataUtils.SetSubmesh(meshData, Vertices.Length, Indices.Length, Bounds);
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
	public readonly List<JobState> m_toStart = new List<JobState>();

	[PublicizedFrom(EAccessModifier.Private)]
	public LinkedList<JobBatch> m_running = new LinkedList<JobBatch>();

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
		if (m_toStart.Count > 0)
		{
			Log.Error("[MeshDataManager] There were unstarted batches at the end of the frame.");
			StartBatches();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CompleteBatches()
	{
		if (m_running.Count <= 0)
		{
			return;
		}
		using (s_markerCompleteBatches.Auto())
		{
			LinkedListNode<JobBatch> linkedListNode = m_running.First;
			while (linkedListNode != null)
			{
				JobBatch value = linkedListNode.Value;
				bool flag = true;
				try
				{
					flag = value.TryApplyAndDispose();
				}
				catch (Exception arg)
				{
					Log.Error($"[MeshDataManager] Failed to apply and dispose job batch: {arg}");
				}
				finally
				{
					LinkedListNode<JobBatch> next = linkedListNode.Next;
					try
					{
						if (flag)
						{
							m_running.Remove(linkedListNode);
							value.Dispose();
						}
					}
					catch (Exception arg2)
					{
						Log.Error($"[MeshDataManager] Failed to remove or dispose job batch: {arg2}");
					}
					finally
					{
						linkedListNode = next;
					}
				}
			}
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
		if (colors != null && colors.Length > 0 && colors.Length != length)
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

	public void Add(Mesh mesh, ArrayListMP<Vector3> vertices, ArrayListMP<int> indices, ArrayListMP<Vector3> normals, ArrayListMP<Vector4> tangents, ArrayListMP<Color> colors, ArrayListMP<Vector2> texCoord0s, ArrayListMP<Vector2> texCoord1s, PinnedBuffer.Ownership ownership, bool generateNormals = false, bool generateTangents = false, bool recalculateUvDistributionMetrics = false, bool sameFrameUpload = false, Action onJobComplete = null)
	{
		using (s_markerAdd.Auto())
		{
			if (!Enabled)
			{
				Log.Warning("[MeshDataManager] MeshDataJob was added while disabled.");
			}
			if (PreValidateJobData(mesh, ToReadOnlySpan(vertices), ToReadOnlySpan(indices), ToReadOnlySpan(normals), ToReadOnlySpan(tangents), ToReadOnlySpan(colors), ToReadOnlySpan(texCoord0s), ToReadOnlySpan(texCoord1s)))
			{
				JobState item = CreateJobState(mesh, vertices, indices, normals, tangents, colors, texCoord0s, texCoord1s, ownership, generateNormals, generateTangents, recalculateUvDistributionMetrics, sameFrameUpload, onJobComplete);
				m_toStart.Add(item);
			}
		}
	}

	public void StartBatches()
	{
		using (s_markerStartBatches.Auto())
		{
			if (m_toStart.Count <= 0)
			{
				return;
			}
			try
			{
				StartBatch(m_toStart.Where([PublicizedFrom(EAccessModifier.Internal)] (JobState state) => state.SameFrameUpload));
				StartBatch(m_toStart.Where([PublicizedFrom(EAccessModifier.Internal)] (JobState state) => !state.SameFrameUpload));
			}
			finally
			{
				m_toStart.Clear();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void StartBatch(IEnumerable<JobState> stateSupplier)
	{
		bool flag = false;
		JobBatch jobBatch = null;
		try
		{
			jobBatch = new JobBatch(stateSupplier);
			if (jobBatch.States.Count > 0)
			{
				jobBatch.Start();
				m_running.AddLast(jobBatch);
				flag = true;
			}
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
	public JobState CreateJobState(Mesh mesh, ArrayListMP<Vector3> vertices, ArrayListMP<int> indices, ArrayListMP<Vector3> normals, ArrayListMP<Vector4> tangents, ArrayListMP<Color> colors, ArrayListMP<Vector2> texCoord0s, ArrayListMP<Vector2> texCoord1s, PinnedBuffer.Ownership ownership, bool generateNormals, bool generateTangents, bool recalculateUvDistributionMetrics, bool sameFrameUpload, Action onJobComplete)
	{
		bool flag = false;
		List<PinnedBuffer> list = new List<PinnedBuffer>();
		try
		{
			PinnedBuffer<Vector3> pinnedBuffer = PinnedBuffer.Create(vertices, ownership);
			list.Add(pinnedBuffer);
			PinnedBuffer<int> pinnedBuffer2 = PinnedBuffer.Create(indices, ownership);
			list.Add(pinnedBuffer2);
			PinnedBuffer<Vector3> pinnedBuffer3;
			if (generateNormals && (normals == null || normals.Count <= 0))
			{
				pinnedBuffer3 = new PinnedBuffer<Vector3>(MemoryPools.poolVector3, pinnedBuffer.Length);
			}
			else
			{
				pinnedBuffer3 = PinnedBuffer.Create(normals, ownership);
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
				pinnedBuffer4 = PinnedBuffer.Create(tangents, ownership);
				generateTangents = false;
			}
			list.Add(pinnedBuffer4);
			PinnedBuffer<Color> pinnedBuffer5 = PinnedBuffer.Create(colors, ownership);
			list.Add(pinnedBuffer5);
			PinnedBuffer<Vector2> pinnedBuffer6 = PinnedBuffer.Create(texCoord0s, ownership);
			list.Add(pinnedBuffer6);
			PinnedBuffer<Vector2> pinnedBuffer7 = PinnedBuffer.Create(texCoord1s, ownership);
			list.Add(pinnedBuffer7);
			JobState result = new JobState(mesh, pinnedBuffer, pinnedBuffer2, pinnedBuffer3, pinnedBuffer4, pinnedBuffer5, pinnedBuffer6, pinnedBuffer7, generateNormals, generateTangents, recalculateUvDistributionMetrics, sameFrameUpload, onJobComplete);
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
