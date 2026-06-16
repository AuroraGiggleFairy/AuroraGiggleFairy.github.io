using System.Collections.Generic;
using System.IO;
using UnityEngine.Scripting;

[Preserve]
public class AIDirectorChunkData
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const int cVersion = 2;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cCooldownDelay = 240f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cCooldownLongDelay = 1320f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cCooldownNeighborDelay = 180f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cCooldownNeighborLongDelay = 720f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float activityLevel;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<AIDirectorChunkEvent> events = new List<AIDirectorChunkEvent>();

	public float cooldownDelay;

	public float ActivityLevel => activityLevel;

	public bool IsReady => cooldownDelay <= 0f;

	public int EventCount => events.Count;

	public void StartNeighborCooldown(bool _isLong)
	{
		float v = (_isLong ? 720f : 180f);
		cooldownDelay = Utils.FastMax(cooldownDelay, v);
	}

	public AIDirectorChunkEvent GetEvent(int _index)
	{
		return events[_index];
	}

	public void Write(BinaryWriter _stream)
	{
		_stream.Write(2);
		_stream.Write(activityLevel);
		_stream.Write(events.Count);
		for (int i = 0; i < events.Count; i++)
		{
			events[i].Write(_stream);
		}
		_stream.Write(cooldownDelay);
	}

	public void Read(BinaryReader _stream, int outerVersion)
	{
		int num = _stream.ReadInt32();
		activityLevel = _stream.ReadSingle();
		events.Clear();
		int num2 = _stream.ReadInt32();
		for (int i = 0; i < num2; i++)
		{
			events.Add(AIDirectorChunkEvent.Read(_stream));
		}
		if (num >= 2)
		{
			cooldownDelay = _stream.ReadSingle();
		}
	}

	public void AddEvent(AIDirectorChunkEvent _chunkEvent)
	{
		int num = events.FindIndex([PublicizedFrom(EAccessModifier.Internal)] (AIDirectorChunkEvent e) => e.Position == _chunkEvent.Position && e.EventType == _chunkEvent.EventType);
		if (num < 0)
		{
			events.Add(_chunkEvent);
		}
		else
		{
			AIDirectorChunkEvent aIDirectorChunkEvent = events[num];
			aIDirectorChunkEvent.Value += _chunkEvent.Value;
			aIDirectorChunkEvent.Duration = _chunkEvent.Duration;
		}
		activityLevel += _chunkEvent.Value;
	}

	public bool Tick(float _elapsed)
	{
		if (cooldownDelay > 0f)
		{
			cooldownDelay -= _elapsed;
			return true;
		}
		DecayEvents(_elapsed);
		if (EventCount > 0)
		{
			return true;
		}
		return false;
	}

	public void DecayEvents(float _elapsed)
	{
		activityLevel = 0f;
		int num = 0;
		while (num < events.Count)
		{
			AIDirectorChunkEvent aIDirectorChunkEvent = events[num];
			float num2 = _elapsed / aIDirectorChunkEvent.Duration;
			aIDirectorChunkEvent.Value -= aIDirectorChunkEvent.Value * num2;
			aIDirectorChunkEvent.Duration -= _elapsed;
			if (aIDirectorChunkEvent.Duration > 0f && aIDirectorChunkEvent.Value > 0f)
			{
				activityLevel += aIDirectorChunkEvent.Value;
				num++;
			}
			else
			{
				events.RemoveAt(num);
			}
		}
	}

	public AIDirectorChunkEvent FindBestEventAndReset()
	{
		AIDirectorChunkEvent aIDirectorChunkEvent = null;
		if (events.Count > 0)
		{
			aIDirectorChunkEvent = events[0];
			for (int i = 1; i < events.Count; i++)
			{
				if (events[i].Value > aIDirectorChunkEvent.Value)
				{
					aIDirectorChunkEvent = events[i];
				}
			}
			cooldownDelay = 240f;
		}
		ClearEvents();
		return aIDirectorChunkEvent;
	}

	public void SetLongDelay()
	{
		cooldownDelay = 1320f;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ClearEvents()
	{
		activityLevel = 0f;
		events.Clear();
	}
}
