using System.Collections.Generic;

public class DynamicObjectPool<T> where T : new()
{
	[PublicizedFrom(EAccessModifier.Private)]
	public int m_numAllocatedObjects;

	[PublicizedFrom(EAccessModifier.Private)]
	public int m_numUsedObjects;

	[PublicizedFrom(EAccessModifier.Private)]
	public int m_numFreeObjects;

	[PublicizedFrom(EAccessModifier.Private)]
	public int m_numObjectsPerBlock;

	[PublicizedFrom(EAccessModifier.Private)]
	public int m_maxObjects;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<T> m_freeObjects;

	public int NumAllocatedObjects => m_numAllocatedObjects;

	public int NumUsedObjects => m_numUsedObjects;

	public int NumFreeObjects => m_numFreeObjects;

	public int MaxObjects => m_maxObjects;

	public DynamicObjectPool(int objectsPerBlock)
	{
		Init(objectsPerBlock, int.MaxValue);
	}

	public DynamicObjectPool(int objectsPerBlock, int maxObjects)
	{
		Init(objectsPerBlock, maxObjects);
	}

	public void AllocateBlock(int numObjects)
	{
		for (int i = 0; i < numObjects; i++)
		{
			push(new T());
		}
		m_numAllocatedObjects += numObjects;
	}

	public T Allocate()
	{
		if (m_numFreeObjects < 1)
		{
			AllocateBlock(m_numObjectsPerBlock);
		}
		m_numUsedObjects++;
		return pop();
	}

	public T[] Allocate(int numToAllocate)
	{
		if (numToAllocate < 1)
		{
			return null;
		}
		T[] array = new T[numToAllocate];
		while (m_numFreeObjects < numToAllocate)
		{
			AllocateBlock(m_numObjectsPerBlock);
		}
		for (int i = 0; i < numToAllocate; i++)
		{
			array[i] = Allocate();
		}
		return array;
	}

	public void Free(T obj)
	{
		push(obj);
		m_numUsedObjects--;
	}

	public void Free(T[] array)
	{
		foreach (T obj in array)
		{
			Free(obj);
		}
	}

	public void Compact()
	{
		m_numFreeObjects = 0;
		m_numAllocatedObjects -= m_freeObjects.Count;
		m_freeObjects = new List<T>(m_numObjectsPerBlock);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Init(int objectsPerBlock, int maxObjects)
	{
		m_numObjectsPerBlock = objectsPerBlock;
		m_maxObjects = maxObjects;
		m_freeObjects = new List<T>(objectsPerBlock);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void push(T t)
	{
		if (m_numFreeObjects >= m_freeObjects.Count)
		{
			m_freeObjects.Add(t);
		}
		else
		{
			m_freeObjects[m_numFreeObjects] = t;
		}
		m_numFreeObjects++;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public T pop()
	{
		if (m_numFreeObjects < 1)
		{
			return default(T);
		}
		return m_freeObjects[--m_numFreeObjects];
	}
}
