using System;
using System.Collections.Generic;
using System.Diagnostics;

public class EntityAsyncManager
{
	public class EntityCreateHandle
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public readonly EntityAsyncManager manager;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly EntityFactory.CreateEntityOperation op;

		[PublicizedFrom(EAccessModifier.Private)]
		public Action<EntityCreateHandle> onComplete;

		[field: PublicizedFrom(EAccessModifier.Private)]
		public bool IsCompleted
		{
			get; [PublicizedFrom(EAccessModifier.Private)]
			set;
		}

		public int Id => op.EntityId;

		public string DebugInfo => op.DebugEntityInfo;

		public Entity Entity => op.entity;

		public EntityCreateHandle(EntityAsyncManager manager, EntityFactory.CreateEntityOperation op, Action<EntityCreateHandle> onComplete)
		{
			this.manager = manager;
			this.op = op;
			this.onComplete = onComplete;
		}

		public bool TryComplete()
		{
			if (IsCompleted)
			{
				return true;
			}
			if (!op.IsLoadingComplete)
			{
				return false;
			}
			try
			{
				IsCompleted = true;
				op.CompleteEntity();
				onComplete?.Invoke(this);
				onComplete = null;
			}
			finally
			{
				manager.OnCreateEntityRequestFinalized(Id);
			}
			return true;
		}

		public Entity WaitForComplete()
		{
			op.WaitForLoadingComplete();
			if (!TryComplete())
			{
				return null;
			}
			return op.entity;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Queue<EntityCreateHandle> requestQueue = new Queue<EntityCreateHandle>(64);

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<int, EntityCreateHandle> requestIdMap = new Dictionary<int, EntityCreateHandle>(64);

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly NetEntityPackageQueue netEntityPackageQueue;

	public EntityAsyncManager(NetEntityPackageQueue netEntityPackageQueue)
	{
		this.netEntityPackageQueue = netEntityPackageQueue;
	}

	public EntityCreateHandle StartCreateEntity(EntityCreationData _ecd, Action<EntityCreateHandle> _onComplete = null)
	{
		EntityFactory.CreateEntityOperation op = EntityFactory.CreateEntityAsync(_ecd);
		EntityCreateHandle entityCreateHandle = new EntityCreateHandle(this, op, _onComplete);
		if (!requestIdMap.TryAdd(entityCreateHandle.Id, entityCreateHandle))
		{
			throw new Exception($"Request already exists for entity id {entityCreateHandle.Id}. EntityCreationData: {_ecd}");
		}
		requestQueue.Enqueue(entityCreateHandle);
		return entityCreateHandle;
	}

	public void Update()
	{
		EntityCreateHandle result;
		while (requestQueue.TryPeek(out result))
		{
			if (result.IsCompleted)
			{
				requestQueue.Dequeue();
				continue;
			}
			if (result.TryComplete())
			{
				requestQueue.Dequeue();
				continue;
			}
			break;
		}
	}

	public bool IsEntityPending(int _entityId)
	{
		if (requestIdMap.TryGetValue(_entityId, out var value))
		{
			return !value.IsCompleted;
		}
		return false;
	}

	public void EnsureEntity(int _entityId)
	{
		if (requestIdMap.TryGetValue(_entityId, out var value))
		{
			LogWarning("force spawning pending entity " + value.DebugInfo);
			value.WaitForComplete();
			requestIdMap.Remove(value.Id);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnCreateEntityRequestFinalized(int id)
	{
		requestIdMap.Remove(id);
		netEntityPackageQueue.ProcessPackagesForEntity(id);
	}

	public void CompletePendingCreateTasks()
	{
		if (requestQueue.Count != 0)
		{
			LogInfo($"completing {requestQueue.Count} pending create tasks");
			EntityCreateHandle result;
			while (requestQueue.TryDequeue(out result))
			{
				result.WaitForComplete();
			}
			requestIdMap.Clear();
		}
	}

	public void Cleanup()
	{
		CompletePendingCreateTasks();
	}

	[Conditional("DEBUG_ENTITY_ASYNC")]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void LogDebug(string message)
	{
		Log.Out("[EntityAsync] " + message);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void LogInfo(string message)
	{
		Log.Out("[EntityAsync] " + message);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void LogWarning(string message)
	{
		Log.Warning("[EntityAsync] " + message);
	}
}
