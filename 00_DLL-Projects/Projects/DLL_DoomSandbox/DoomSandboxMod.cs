using System.IO;

namespace DoomSandbox
{
	public static class DoomSandboxMod
	{
		public static Mod ModInstance;

		public static string ModPath =>
			ModInstance != null ? ModInstance.Path : null;

		public static string ConfigPath =>
			ModPath == null ? null : Path.Combine(ModPath, "Config", "sandbox_options.txt");

		public static string DifficultiesPath =>
			ModPath == null ? null : Path.Combine(ModPath, "Config", "sandbox_difficulties.txt");

		/// <summary>XUi texture path for Custom / User sandbox preset art.</summary>
		public static string CustomPresetIconPath =>
			"@modfolder(DoomSandbox):UIAtlases/Doom_CustomSandboxImage.jpg";
	}
}
