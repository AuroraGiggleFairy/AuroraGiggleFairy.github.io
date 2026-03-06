using System;
using System.Collections.Generic;
using System.Linq;

namespace DynamicMusic.Legacy.ObjectModel;

public class Layer : Dictionary<int, InstrumentID>
{
	[PublicizedFrom(EAccessModifier.Private)]
	public Queue<InstrumentID> idQ;

	[PublicizedFrom(EAccessModifier.Private)]
	public static GameRandom Random;

	public Layer()
	{
		if (Random == null)
		{
			Random = GameRandomManager.Instance.CreateGameRandom();
		}
	}

	public InstrumentID GetInstrumentID()
	{
		if (idQ == null || idQ.Count < 3)
		{
			PopulateQueue();
		}
		LayerReserve.AddLoad(idQ.ElementAt(1));
		return idQ.Dequeue();
	}

	public void PopulateQueue()
	{
		if (idQ == null)
		{
			idQ = new Queue<InstrumentID>(base.Values.OrderBy([PublicizedFrom(EAccessModifier.Internal)] (InstrumentID e) => Random.RandomRange(int.MaxValue)));
			LayerReserve.AddLoad(idQ.Peek());
		}
		else
		{
			RefillQueue();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RefillQueue()
	{
		Array.ForEach(base.Values.OrderBy([PublicizedFrom(EAccessModifier.Private)] (InstrumentID e) => (e.Name.Equals(idQ.Peek().Name) || e.Name.Equals(idQ.ElementAt(1).Name)) ? int.MaxValue : Random.RandomRange(int.MaxValue)).ToArray(), idQ.Enqueue);
	}
}
