using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Newtonsoft.Json.Linq;

[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(1)]
[_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(0)]
internal class JRaw : JValue
{
	public static async Task<JRaw> CreateAsync(JsonReader reader, CancellationToken cancellationToken = default(CancellationToken))
	{
		using StringWriter sw = new StringWriter(CultureInfo.InvariantCulture);
		using JsonTextWriter jsonWriter = new JsonTextWriter(sw);
		await jsonWriter.WriteTokenSyncReadingAsync(reader, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		return new JRaw(sw.ToString());
	}

	public JRaw(JRaw other)
		: base(other, null)
	{
	}

	internal JRaw(JRaw other, [_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] JsonCloneSettings settings)
		: base(other, settings)
	{
	}

	[_003C464f7c58_002D4ec4_002D4694_002Da30c_002D0ded4d74fb4d_003ENullableContext(2)]
	public JRaw(object rawJson)
		: base(rawJson, JTokenType.Raw)
	{
	}

	public static JRaw Create(JsonReader reader)
	{
		using StringWriter stringWriter = new StringWriter(CultureInfo.InvariantCulture);
		using JsonTextWriter jsonTextWriter = new JsonTextWriter(stringWriter);
		jsonTextWriter.WriteToken(reader);
		return new JRaw(stringWriter.ToString());
	}

	internal override JToken CloneToken([_003C9200ff2c_002Dc7a6_002D43ef_002Da776_002D3d61f59ba112_003ENullable(2)] JsonCloneSettings settings)
	{
		return new JRaw(this, settings);
	}
}
