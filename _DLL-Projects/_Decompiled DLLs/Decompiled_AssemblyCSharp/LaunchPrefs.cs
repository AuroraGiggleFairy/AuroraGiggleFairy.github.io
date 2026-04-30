using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Platform;

public static class LaunchPrefs
{
	[PublicizedFrom(EAccessModifier.Private)]
	public delegate bool LaunchPrefParser<T>(string stringRepresentation, out T value);

	[PublicizedFrom(EAccessModifier.Private)]
	public static class Parsers
	{
		public static readonly LaunchPrefParser<int> INT = [PublicizedFrom(EAccessModifier.Internal)] (string s, out int value) => int.TryParse(s, out value);

		public static readonly LaunchPrefParser<long> LONG = [PublicizedFrom(EAccessModifier.Internal)] (string s, out long value) => long.TryParse(s, out value);

		public static readonly LaunchPrefParser<ulong> ULONG = [PublicizedFrom(EAccessModifier.Internal)] (string s, out ulong value) => ulong.TryParse(s, out value);

		public static readonly LaunchPrefParser<bool> BOOL = [PublicizedFrom(EAccessModifier.Internal)] (string s, out bool value) => bool.TryParse(s, out value);

		public static readonly LaunchPrefParser<string> STRING = [PublicizedFrom(EAccessModifier.Internal)] (string s, out string value) =>
		{
			value = s;
			return true;
		};
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static class EnumParsers<TEnum> where TEnum : struct, IConvertible
	{
		public static readonly LaunchPrefParser<TEnum> CASE_SENSITIVE = [PublicizedFrom(EAccessModifier.Internal)] (string s, out TEnum value) => EnumUtils.TryParse<TEnum>(s, out value);

		public static readonly LaunchPrefParser<TEnum> CASE_INSENSITIVE = [PublicizedFrom(EAccessModifier.Internal)] (string s, out TEnum value) => EnumUtils.TryParse<TEnum>(s, out value, _ignoreCase: true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public abstract class LaunchPref : ILaunchPref
	{
		[field: PublicizedFrom(EAccessModifier.Private)]
		public string Name { get; }

		[PublicizedFrom(EAccessModifier.Protected)]
		public LaunchPref(string name)
		{
			if (s_initializing)
			{
				throw new InvalidOperationException("LaunchPref should be instantiated before LaunchPrefs initialization begins.");
			}
			if (string.IsNullOrEmpty(name))
			{
				throw new ArgumentException("LaunchPref requires a name", "name");
			}
			if (!s_launchPrefs.TryAdd(name, this))
			{
				throw new InvalidOperationException("There is already a LaunchPref with the name '" + name + "'");
			}
			Name = name;
		}

		public abstract bool TrySet(string stringRepresentation);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public sealed class LaunchPref<T> : LaunchPref, ILaunchPref<T>, ILaunchPref
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public T m_value;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly LaunchPrefParser<T> m_parser;

		public T Value
		{
			get
			{
				if (!s_initialized)
				{
					throw new InvalidOperationException("LaunchPref can only be read after LaunchPrefs has finished initializing.");
				}
				return m_value;
			}
		}

		public LaunchPref(string name, T defaultValue, LaunchPrefParser<T> parser)
			: base(name)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			m_value = defaultValue;
			m_parser = parser;
		}

		public override bool TrySet(string stringRepresentation)
		{
			if (!s_initializing || s_initialized)
			{
				throw new InvalidOperationException("LaunchPref can only be set during LaunchPrefs initialization.");
			}
			if (!m_parser(stringRepresentation, out var value))
			{
				return false;
			}
			m_value = value;
			return true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly object s_initializationLock = new object();

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool s_initializing;

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool s_initialized;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Dictionary<string, ILaunchPref> s_launchPrefs = new Dictionary<string, ILaunchPref>(StringComparer.OrdinalIgnoreCase);

	public static readonly ILaunchPref<bool> SkipNewsScreen = Create(defaultValue: false, Parsers.BOOL, "SkipNewsScreen");

	public static readonly ILaunchPref<string> UserDataFolder = Create(GameIO.GetDefaultUserGameDataDir(), Parsers.STRING.ThenTransform([PublicizedFrom(EAccessModifier.Internal)] (string path) => (!(path != GameIO.GetDefaultUserGameDataDir())) ? path : GameIO.MakeAbsolutePath(path)), "UserDataFolder");

	public static readonly ILaunchPref<bool> PlayerPrefsFile = Create((DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX).IsCurrent(), Parsers.BOOL, "PlayerPrefsFile");

	public static readonly ILaunchPref<bool> AllowCrossplay = Create(defaultValue: true, Parsers.BOOL, "AllowCrossplay");

	public static readonly ILaunchPref<MapChunkDatabaseType> MapChunkDatabase = Create(MapChunkDatabaseType.Region, EnumParsers<MapChunkDatabaseType>.CASE_INSENSITIVE, "MapChunkDatabase");

	public static readonly ILaunchPref<bool> LoadSaveGame = Create(defaultValue: false, Parsers.BOOL, "LoadSaveGame");

	public static readonly ILaunchPref<bool> AllowJoinConfigModded = Create((DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX).IsCurrent(), Parsers.BOOL, "AllowJoinConfigModded");

	public static readonly ILaunchPref<int> MaxWorldSizeHost = Create(PlatformOptimizations.DefaultMaxWorldSizeHost, Parsers.INT, "MaxWorldSizeHost");

	public static readonly ILaunchPref<int> MaxWorldSizeClient = Create(-1, Parsers.INT, "MaxWorldSizeClient");

	public static readonly ILaunchPref<string> SessionInvite = Create(string.Empty, Parsers.STRING, "SessionInvite");

	public static IReadOnlyDictionary<string, ILaunchPref> All => s_launchPrefs;

	[PublicizedFrom(EAccessModifier.Private)]
	public static ILaunchPref<T> Create<T>(T defaultValue, LaunchPrefParser<T> parser, [CallerMemberName] string name = null)
	{
		if (parser == null)
		{
			throw new ArgumentNullException("parser");
		}
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		return new LaunchPref<T>(name, defaultValue, parser);
	}

	public static void InitStart()
	{
		lock (s_initializationLock)
		{
			if (s_initializing)
			{
				throw new InvalidOperationException("LaunchPrefs.InitStart has already been called.");
			}
			s_initializing = true;
		}
	}

	public static void InitEnd()
	{
		lock (s_initializationLock)
		{
			if (!s_initializing)
			{
				throw new InvalidOperationException("LaunchPrefs.InitStart has not been called yet.");
			}
			if (s_initialized)
			{
				throw new InvalidOperationException("LaunchPrefs.InitEnd has already been called.");
			}
			s_initialized = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static LaunchPrefParser<OUT> ThenTransform<IN, OUT>(this LaunchPrefParser<IN> parser, Func<IN, OUT> transform)
	{
		return [PublicizedFrom(EAccessModifier.Internal)] (string representation, out OUT value) =>
		{
			if (!parser(representation, out var value2))
			{
				value = default(OUT);
				return false;
			}
			value = transform(value2);
			return true;
		};
	}
}
