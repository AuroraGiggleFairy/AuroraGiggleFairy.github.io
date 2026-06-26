using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;

public abstract class ModEventAbs<TDelegate>
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public class Receiver
	{
		public readonly Mod Mod;

		public readonly TDelegate DelegateFunc;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly bool coreGame;

		public string ModName
		{
			get
			{
				if (Mod != null)
				{
					return Mod.Name;
				}
				if (coreGame)
				{
					return "-GameCore-";
				}
				return "-UnknownMod-";
			}
		}

		public Receiver(Mod _mod, TDelegate _handler, bool _coreGame = false)
		{
			Mod = _mod;
			DelegateFunc = _handler;
			coreGame = _coreGame;
		}
	}

	public string eventName;

	[PublicizedFrom(EAccessModifier.Protected)]
	public readonly List<Receiver> receivers = new List<Receiver>();

	[MethodImpl(MethodImplOptions.NoInlining)]
	public void RegisterHandler(TDelegate _handlerFunc)
	{
		Assembly callingAssembly = Assembly.GetCallingAssembly();
		Assembly assembly = typeof(ModEvents).Assembly;
		bool coreGame = false;
		Mod mod = null;
		if (callingAssembly.Equals(assembly))
		{
			coreGame = true;
		}
		else
		{
			mod = ModManager.GetModForAssembly(callingAssembly);
			if (mod == null)
			{
				Log.Warning("[MODS] Could not find mod that tries to register a handler for event " + eventName);
			}
		}
		receivers.Add(new Receiver(mod, _handlerFunc, coreGame));
	}

	public void UnregisterHandler(TDelegate _handlerFunc)
	{
		for (int i = 0; i < receivers.Count; i++)
		{
			if (receivers[i].DelegateFunc.Equals(_handlerFunc))
			{
				receivers.RemoveAt(i);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void LogError(Exception _e, Receiver _currentMod)
	{
		Log.Error("[MODS] Error while executing " + eventName + " on mod \"" + _currentMod.ModName + "\"");
		Log.Exception(_e);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public ModEventAbs()
	{
	}
}
