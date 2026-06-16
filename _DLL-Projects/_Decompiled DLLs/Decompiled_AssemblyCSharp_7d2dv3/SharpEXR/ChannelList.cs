using System.Collections;
using System.Collections.Generic;

namespace SharpEXR;

public class ChannelList : IEnumerable<Channel>, IEnumerable
{
	[field: PublicizedFrom(EAccessModifier.Private)]
	public List<Channel> Channels { get; set; }

	public Channel this[int index]
	{
		get
		{
			return Channels[index];
		}
		set
		{
			Channels[index] = value;
		}
	}

	public ChannelList()
	{
		Channels = new List<Channel>();
	}

	public void Read(EXRFile file, IEXRReader reader, int size)
	{
		int num = 0;
		Channel channel;
		int bytesRead;
		while (ReadChannel(file, reader, out channel, out bytesRead))
		{
			Channels.Add(channel);
			num += bytesRead;
			if (num > size)
			{
				throw new EXRFormatException("Read " + num + " bytes but Size was " + size + ".");
			}
		}
		num += bytesRead;
		if (num != size)
		{
			throw new EXRFormatException("Read " + num + " bytes but Size was " + size + ".");
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool ReadChannel(EXRFile file, IEXRReader reader, out Channel channel, out int bytesRead)
	{
		int position = reader.Position;
		string text = reader.ReadNullTerminatedString(255);
		if (text == "")
		{
			channel = null;
			bytesRead = reader.Position - position;
			return false;
		}
		channel = new Channel(text, (PixelType)reader.ReadInt32(), reader.ReadByte() != 0, reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadInt32(), reader.ReadInt32());
		bytesRead = reader.Position - position;
		return true;
	}

	public IEnumerator<Channel> GetEnumerator()
	{
		return Channels.GetEnumerator();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
