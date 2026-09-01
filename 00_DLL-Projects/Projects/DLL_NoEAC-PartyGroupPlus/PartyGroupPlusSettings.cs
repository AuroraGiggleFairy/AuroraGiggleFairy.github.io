using System;
using System.IO;

namespace PartyGroupPlus
{
	internal enum ForcePartyMode
	{
		Off,
		Join,
		Enforce
	}

	internal static class PartyGroupPlusSettings
	{
		public static ForcePartyMode Mode { get; private set; } = ForcePartyMode.Off;
		public static int ServerPartyId { get; private set; } = -1;
		public static string Filename { get; private set; }
		public static string ModPath { get; private set; }

		public static void Load(string modPath)
		{
			ModPath = modPath ?? "";
			Filename = string.IsNullOrEmpty(ModPath) ? "" : Path.Combine(ModPath, "PartyGroupPlus.json");
			Mode = ForcePartyMode.Off;
			ServerPartyId = -1;
			if (string.IsNullOrEmpty(Filename) || !File.Exists(Filename))
			{
				Save();
				return;
			}

			try
			{
				string text = File.ReadAllText(Filename);
				Mode = ParseMode(text);
				ServerPartyId = ReadInt(text, "ServerPartyId", -1);
				if (text.IndexOf("Do not edit", StringComparison.OrdinalIgnoreCase) < 0)
				{
					Save();
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("PartyGroupPlus: settings load failed: " + ex.Message);
				Mode = ForcePartyMode.Off;
				ServerPartyId = -1;
			}
		}

		public static ForcePartyMode SetMode(ForcePartyMode mode)
		{
			Mode = mode;
			Save();
			PartyGroupPlusForceParty.Apply(mode);
			return Mode;
		}

		public static void SetServerPartyId(int partyId)
		{
			if (ServerPartyId == partyId)
			{
				return;
			}

			ServerPartyId = partyId;
			Save();
		}

		public static string ModeToken(ForcePartyMode mode)
		{
			switch (mode)
			{
				case ForcePartyMode.Join:
					return "join";
				case ForcePartyMode.Enforce:
					return "enforce";
				default:
					return "off";
			}
		}

		public static bool TryParseMode(string token, out ForcePartyMode mode)
		{
			mode = ForcePartyMode.Off;
			if (string.IsNullOrEmpty(token))
			{
				return false;
			}

			switch (token.Trim().ToLowerInvariant())
			{
				case "off":
					mode = ForcePartyMode.Off;
					return true;
				case "join":
					mode = ForcePartyMode.Join;
					return true;
				case "enforce":
				case "on":
					mode = ForcePartyMode.Enforce;
					return true;
				default:
					return false;
			}
		}

		public static string AsString()
		{
			return "[PartyGroupPlus] settings"
				+ "\n  forceparty=" + ModeToken(Mode)
				+ "\n  serverparty=" + PartyGroupPlusForceParty.DescribeServerParty()
				+ "\n  file=" + Filename;
		}

		private static void Save()
		{
			if (string.IsNullOrEmpty(Filename))
			{
				return;
			}

			try
			{
				File.WriteAllText(
					Filename,
					"{\n  \"ForcePartyMode\": \"" + ModeToken(Mode) + "\",\n  \"ServerPartyId\": " + ServerPartyId + "  // Do not edit.\n}\n");
			}
			catch (Exception ex)
			{
				Console.WriteLine("PartyGroupPlus: settings save failed: " + ex.Message);
			}
		}

		private static ForcePartyMode ParseMode(string text)
		{
			string token = ReadJsonString(text, "ForcePartyMode");
			if (TryParseMode(token, out ForcePartyMode mode))
			{
				return mode;
			}

			if (HasToken(text, "ForceOneParty", "true"))
			{
				return ForcePartyMode.Enforce;
			}

			return ForcePartyMode.Off;
		}

		private static string ReadJsonString(string text, string key)
		{
			if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(key))
			{
				return null;
			}

			string quotedKey = "\"" + key + "\"";
			int keyAt = text.IndexOf(quotedKey, StringComparison.OrdinalIgnoreCase);
			if (keyAt < 0)
			{
				keyAt = text.IndexOf(key, StringComparison.OrdinalIgnoreCase);
			}

			if (keyAt < 0)
			{
				return null;
			}

			int colon = text.IndexOf(':', keyAt);
			if (colon < 0)
			{
				return null;
			}

			int quote = text.IndexOf('"', colon + 1);
			if (quote < 0)
			{
				return null;
			}

			int end = text.IndexOf('"', quote + 1);
			if (end < 0)
			{
				return null;
			}

			return text.Substring(quote + 1, end - quote - 1);
		}

		private static bool HasToken(string text, string key, string value)
		{
			int keyAt = text.IndexOf(key, StringComparison.OrdinalIgnoreCase);
			if (keyAt < 0)
			{
				return false;
			}

			int valueAt = text.IndexOf(value, keyAt, StringComparison.OrdinalIgnoreCase);
			return valueAt >= 0;
		}

		private static int ReadInt(string text, string key, int fallback)
		{
			int keyAt = text.IndexOf(key, StringComparison.OrdinalIgnoreCase);
			if (keyAt < 0)
			{
				return fallback;
			}

			int colon = text.IndexOf(':', keyAt);
			if (colon < 0)
			{
				return fallback;
			}

			int i = colon + 1;
			while (i < text.Length && (text[i] == ' ' || text[i] == '\t'))
			{
				i++;
			}

			int sign = 1;
			if (i < text.Length && text[i] == '-')
			{
				sign = -1;
				i++;
			}

			int value = 0;
			bool any = false;
			while (i < text.Length && text[i] >= '0' && text[i] <= '9')
			{
				value = (value * 10) + (text[i] - '0');
				any = true;
				i++;
			}

			return any ? sign * value : fallback;
		}
	}
}
