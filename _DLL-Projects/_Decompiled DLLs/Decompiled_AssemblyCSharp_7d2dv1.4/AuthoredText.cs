using System.IO;

public class AuthoredText
{
	[field: PublicizedFrom(EAccessModifier.Private)]
	public string Text
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public PlatformUserIdentifierAbs Author
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public AuthoredText()
	{
		Update(string.Empty, null);
	}

	public AuthoredText(string _text, PlatformUserIdentifierAbs _author)
	{
		Update(_text, _author);
	}

	public void Update(string _text, PlatformUserIdentifierAbs _author)
	{
		Author = _author;
		Text = _text;
	}

	public static AuthoredText FromStream(BinaryReader _reader)
	{
		if (_reader == null)
		{
			return null;
		}
		if (!_reader.ReadBoolean())
		{
			return null;
		}
		string text = _reader.ReadString();
		PlatformUserIdentifierAbs author = PlatformUserIdentifierAbs.FromStream(_reader);
		AuthoredText authoredText = new AuthoredText();
		authoredText.Update(text, author);
		return authoredText;
	}

	public static void ToStream(AuthoredText _instance, BinaryWriter _writer)
	{
		if (_writer != null)
		{
			if (_instance == null)
			{
				_writer.Write((byte)0);
				return;
			}
			_writer.Write((byte)1);
			_writer.Write(_instance.Text);
			_instance.Author.ToStream(_writer);
		}
	}

	public static AuthoredText Clone(AuthoredText _cloneFrom)
	{
		if (_cloneFrom == null)
		{
			return null;
		}
		return new AuthoredText(_cloneFrom.Text, _cloneFrom.Author);
	}
}
