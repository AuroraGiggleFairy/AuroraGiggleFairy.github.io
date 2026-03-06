using System.Runtime.CompilerServices;

namespace Newtonsoft.Json.Linq;

internal class JsonCloneSettings
{
	[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(1)]
	internal static readonly JsonCloneSettings SkipCopyAnnotations = new JsonCloneSettings
	{
		CopyAnnotations = false
	};

	public bool CopyAnnotations { get; set; }

	public JsonCloneSettings()
	{
		CopyAnnotations = true;
	}
}
