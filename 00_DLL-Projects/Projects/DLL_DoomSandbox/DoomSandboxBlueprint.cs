using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace DoomSandbox
{
	/// <summary>
	/// Presets and option choices compiled into this DLL.
	/// Mods-folder copies of the text files are not read.
	/// </summary>
	public static class DoomSandboxBlueprint
	{
		const string OptionsResource = "DoomSandbox.Blueprint.sandbox_options.txt";
		const string DifficultiesResource = "DoomSandbox.Blueprint.sandbox_difficulties.txt";

		static string _stamp;

		public static string Stamp
		{
			get
			{
				if (_stamp != null)
					return _stamp;
				using (var sha = SHA256.Create())
				{
					byte[] options = ReadBytes(OptionsResource);
					byte[] difficulties = ReadBytes(DifficultiesResource);
					sha.TransformBlock(options, 0, options.Length, null, 0);
					sha.TransformFinalBlock(difficulties, 0, difficulties.Length);
					_stamp = BitConverter.ToString(sha.Hash).Replace("-", "");
				}
				return _stamp;
			}
		}

		public static string[] ReadOptionsLines()
		{
			return ReadLines(OptionsResource);
		}

		public static string[] ReadDifficultiesLines()
		{
			return ReadLines(DifficultiesResource);
		}

		/// <summary>
		/// Localization turns the two characters \n into a line break while loading.
		/// Worksheet display text is not a CSV, so do that same conversion here.
		/// </summary>
		public static string AsDisplayText(string text)
		{
			if (string.IsNullOrEmpty(text) || text.IndexOf('\\') < 0)
				return text ?? "";
			return text.Replace("\\n", "\n");
		}

		static string[] ReadLines(string resourceName)
		{
			byte[] bytes = ReadBytes(resourceName);
			var lines = new List<string>();
			using (var reader = new StreamReader(new MemoryStream(bytes), Encoding.UTF8, true))
			{
				string line;
				while ((line = reader.ReadLine()) != null)
					lines.Add(line);
			}
			return lines.ToArray();
		}

		static byte[] ReadBytes(string resourceName)
		{
			var asm = typeof(DoomSandboxBlueprint).Assembly;
			using (var stream = asm.GetManifestResourceStream(resourceName))
			{
				if (stream == null)
					throw new InvalidOperationException("Missing compiled blueprint: " + resourceName);
				using (var ms = new MemoryStream())
				{
					stream.CopyTo(ms);
					return ms.ToArray();
				}
			}
		}
	}
}
