using System.Collections;
using System.Collections.Generic;
using System.Text;

public sealed class StateHistory<T> : IEnumerable<T>, IEnumerable
{
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly T[] m_states;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly HashSet<T> m_statesSet;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly bool m_preventConsecutiveDuplicates;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly bool m_preventDuplicates;

	[PublicizedFrom(EAccessModifier.Private)]
	public int m_index;

	[PublicizedFrom(EAccessModifier.Private)]
	public int m_size;

	public T Current
	{
		get
		{
			if (m_size <= 0)
			{
				return default(T);
			}
			return m_states[m_index];
		}
	}

	public int Length => m_size;

	public StateHistory(int max, bool preventConsecutiveDuplicates = false, bool preventDuplicates = false)
	{
		m_states = new T[max];
		m_statesSet = (preventDuplicates ? new HashSet<T>() : null);
		m_preventConsecutiveDuplicates = preventConsecutiveDuplicates;
		m_preventDuplicates = preventDuplicates;
		m_index = -1;
		m_size = 0;
	}

	public void Add(T state)
	{
		if ((!m_preventDuplicates || !m_statesSet.Contains(state)) && (!m_preventConsecutiveDuplicates || m_size <= 0 || !EqualityComparer<T>.Default.Equals(m_states[m_index], state)))
		{
			if (m_size < m_states.Length)
			{
				m_size++;
			}
			m_index++;
			if (m_index >= m_size)
			{
				m_index = 0;
			}
			m_statesSet?.Remove(m_states[m_index]);
			m_states[m_index] = state;
			m_statesSet?.Add(state);
		}
	}

	public void Clear()
	{
		for (int i = 0; i < m_states.Length; i++)
		{
			m_states[i] = default(T);
		}
		m_statesSet?.Clear();
		m_index = -1;
		m_size = 0;
	}

	public IEnumerator<T> GetEnumerator()
	{
		for (int n = 0; n < m_size; n++)
		{
			int num = m_index - n;
			if (num < 0)
			{
				num += m_size;
			}
			yield return m_states[num];
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public override string ToString()
	{
		if (m_size <= 0)
		{
			return "No State History";
		}
		StringBuilder stringBuilder = new StringBuilder();
		using (IEnumerator<T> enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				T current = enumerator.Current;
				if (stringBuilder.Length > 0)
				{
					stringBuilder.Append(" < ");
				}
				stringBuilder.Append(current);
			}
		}
		return stringBuilder.ToString();
	}
}
