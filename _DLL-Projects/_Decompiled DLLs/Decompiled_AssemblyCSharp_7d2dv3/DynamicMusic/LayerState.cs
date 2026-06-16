using System;
using MusicUtils.Enums;

namespace DynamicMusic;

public class LayerState : ICountable
{
	public readonly Func<float, LayerStateType> Get;

	public int Count => 1;

	public LayerState(Func<float, LayerStateType> _getFunc)
	{
		Get = _getFunc;
	}
}
