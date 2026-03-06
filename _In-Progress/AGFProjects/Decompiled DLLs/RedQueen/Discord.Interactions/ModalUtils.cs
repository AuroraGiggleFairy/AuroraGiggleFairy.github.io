using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Discord.Interactions.Builders;

namespace Discord.Interactions;

internal static class ModalUtils
{
	private static readonly ConcurrentDictionary<Type, ModalInfo> _modalInfos = new ConcurrentDictionary<Type, ModalInfo>();

	public static IReadOnlyCollection<ModalInfo> Modals => _modalInfos.Values.ToReadOnlyCollection();

	public static ModalInfo GetOrAdd(Type type, InteractionService interactionService)
	{
		if (!typeof(IModal).IsAssignableFrom(type))
		{
			throw new ArgumentException("Must be an implementation of IModal", "type");
		}
		return _modalInfos.GetOrAdd(type, ModuleClassBuilder.BuildModalInfo(type, interactionService));
	}

	public static ModalInfo GetOrAdd<T>(InteractionService interactionService) where T : class, IModal
	{
		return GetOrAdd(typeof(T), interactionService);
	}

	public static bool TryGet(Type type, out ModalInfo modalInfo)
	{
		if (!typeof(IModal).IsAssignableFrom(type))
		{
			throw new ArgumentException("Must be an implementation of IModal", "type");
		}
		return _modalInfos.TryGetValue(type, out modalInfo);
	}

	public static bool TryGet<T>(out ModalInfo modalInfo) where T : class, IModal
	{
		return TryGet(typeof(T), out modalInfo);
	}

	public static bool TryRemove(Type type, out ModalInfo modalInfo)
	{
		if (!typeof(IModal).IsAssignableFrom(type))
		{
			throw new ArgumentException("Must be an implementation of IModal", "type");
		}
		return _modalInfos.TryRemove(type, out modalInfo);
	}

	public static bool TryRemove<T>(out ModalInfo modalInfo) where T : class, IModal
	{
		return TryRemove(typeof(T), out modalInfo);
	}

	public static void Clear()
	{
		_modalInfos.Clear();
	}

	public static int Count()
	{
		return _modalInfos.Count;
	}
}
