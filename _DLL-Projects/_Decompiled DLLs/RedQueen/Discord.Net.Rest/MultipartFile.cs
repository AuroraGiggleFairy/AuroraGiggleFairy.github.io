using System.IO;
using System.Runtime.CompilerServices;

namespace Discord.Net.Rest;

internal struct MultipartFile
{
	public Stream Stream
	{
		[_003C93e5c5cd_002Dbc90_002D4083_002D83bc_002Df9a3bc2f6df9_003EIsReadOnly]
		get;
	}

	public string Filename
	{
		[_003C93e5c5cd_002Dbc90_002D4083_002D83bc_002Df9a3bc2f6df9_003EIsReadOnly]
		get;
	}

	public string ContentType
	{
		[_003C93e5c5cd_002Dbc90_002D4083_002D83bc_002Df9a3bc2f6df9_003EIsReadOnly]
		get;
	}

	public MultipartFile(Stream stream, string filename, string contentType = null)
	{
		Stream = stream;
		Filename = filename;
		ContentType = contentType;
	}
}
