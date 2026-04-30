using System.IO;
using System.Runtime.CompilerServices;

namespace Discord.API;

internal struct Image
{
	public Stream Stream
	{
		[_003C93e5c5cd_002Dbc90_002D4083_002D83bc_002Df9a3bc2f6df9_003EIsReadOnly]
		get;
	}

	public string Hash
	{
		[_003C93e5c5cd_002Dbc90_002D4083_002D83bc_002Df9a3bc2f6df9_003EIsReadOnly]
		get;
	}

	public Image(Stream stream)
	{
		Stream = stream;
		Hash = null;
	}

	public Image(string hash)
	{
		Stream = null;
		Hash = hash;
	}
}
