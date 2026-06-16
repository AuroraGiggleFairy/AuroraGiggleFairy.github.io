using System;
using System.Text;
using UnityEngine;

[RequireComponent(typeof(TextMesh))]
public class SmartTextMesh : MonoBehaviour
{
	public TextMesh TheMesh;

	[TextArea]
	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public string unwrappedText;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly StringBuilder wrappedText = new StringBuilder();

	public float MaxWidth;

	public int MaxLines;

	public bool NeedsLayout = true;

	public bool ConvertNewLines;

	public bool SeperatedLinesMode;

	public string UnwrappedText
	{
		get
		{
			return unwrappedText;
		}
		set
		{
			if (value != unwrappedText)
			{
				unwrappedText = value ?? "";
				NeedsLayout = true;
			}
		}
	}

	public float MaxWidthReal
	{
		get
		{
			return MaxWidth * 2f;
		}
		set
		{
			MaxWidth = value / 2f;
		}
	}

	public void Start()
	{
		TheMesh = GetComponent<TextMesh>();
		if (ConvertNewLines && UnwrappedText != null)
		{
			UnwrappedText = UnwrappedText.Replace("\\n", "\n");
		}
		if (UnwrappedText == null)
		{
			string text = (UnwrappedText = "");
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void LateUpdate()
	{
		if (NeedsLayout)
		{
			NeedsLayout = false;
			if (MaxWidth == 0f)
			{
				TheMesh.text = UnwrappedText;
			}
			else if (SeperatedLinesMode)
			{
				FormatSeparateLines();
			}
			else
			{
				WrapTextToWidth();
			}
		}
	}

	public bool CanRenderString(string _text)
	{
		string text = _text.Trim();
		foreach (char c in text)
		{
			if (c != '\n' && !TheMesh.font.HasCharacter(c))
			{
				return false;
			}
		}
		return true;
	}

	public void WrapTextToWidth()
	{
		TheMesh.font.RequestCharactersInTexture(unwrappedText, TheMesh.fontSize, TheMesh.fontStyle);
		float textWidth = GetTextWidth(TheMesh, " ");
		ReadOnlySpan<char> span = unwrappedText;
		span = span.Trim();
		int num = 0;
		int num2 = 1;
		int num3 = 0;
		float num4 = 0f;
		wrappedText.Clear();
		while (num < span.Length && num2 <= MaxLines)
		{
			int num5 = span.Slice(num).IndexOfAny(' ', '\n');
			num5 = ((num5 >= 0) ? (num5 + num) : span.Length);
			ReadOnlySpan<char> s = span.Slice(num, num5 - num);
			float textWidth2 = GetTextWidth(TheMesh, s);
			float num6 = ((num4 > 0f) ? (num4 + textWidth + textWidth2) : textWidth2);
			if (num6 > MaxWidthReal)
			{
				if (num4 > 0f)
				{
					wrappedText.Append(span.Slice(num3, num - 1 - num3));
					wrappedText.Append('\n');
					num = (num3 = num);
					num2++;
					num4 = 0f;
				}
				else
				{
					float num7 = 1.2f * textWidth2 / MaxWidthReal;
					int length = (int)((float)s.Length / num7);
					wrappedText.Append(s.Slice(0, length));
					wrappedText.Append('…');
					wrappedText.Append('\n');
					num = (num3 = num5 + 1);
					num2++;
					num4 = 0f;
				}
			}
			else if (num5 >= span.Length || span[num5] == '\n')
			{
				wrappedText.Append(span.Slice(num3, num5 - num3));
				wrappedText.Append('\n');
				num2++;
				num = (num3 = num5 + 1);
				num4 = 0f;
			}
			else
			{
				num4 = num6;
				num = num5 + 1;
			}
		}
		if (wrappedText.Length > 0 && wrappedText[wrappedText.Length - 1] == '\n')
		{
			num2--;
			wrappedText.Length--;
			if (num < span.Length)
			{
				wrappedText.Append('…');
			}
		}
		TheMesh.text = wrappedText.ToString();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void FormatSeparateLines()
	{
		TheMesh.font.RequestCharactersInTexture(unwrappedText, TheMesh.fontSize, TheMesh.fontStyle);
		float textWidth = GetTextWidth(TheMesh, " ");
		ReadOnlySpan<char> span = unwrappedText;
		span = span.Trim();
		wrappedText.Clear();
		string[] array = span.ToString().Split('\n');
		for (int i = 0; i < array.Length; i++)
		{
			string[] array2 = array[i].Split(' ');
			float num = 0f;
			for (int j = 0; j < array2.Length; j++)
			{
				ReadOnlySpan<char> readOnlySpan = array2[j];
				float textWidth2 = GetTextWidth(TheMesh, readOnlySpan);
				float num2 = ((num > 0f) ? (num + textWidth + textWidth2) : textWidth2);
				if (num2 > MaxWidthReal)
				{
					wrappedText.Append('…');
					break;
				}
				if (j != 0)
				{
					wrappedText.Append(' ');
				}
				wrappedText.Append(readOnlySpan);
				num = num2;
			}
			if (i < array.Length - 1)
			{
				wrappedText.Append('\n');
			}
		}
		TheMesh.text = wrappedText.ToString();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float GetTextWidth(TextMesh _textMesh, ReadOnlySpan<char> _s)
	{
		Font font = _textMesh.font;
		int fontSize = _textMesh.fontSize;
		FontStyle fontStyle = _textMesh.fontStyle;
		int num = 0;
		ReadOnlySpan<char> readOnlySpan = _s;
		for (int i = 0; i < readOnlySpan.Length; i++)
		{
			char c = readOnlySpan[i];
			if (font.GetCharacterInfo(c, out var info, fontSize, fontStyle))
			{
				num += info.advance;
			}
			else
			{
				Log.Warning($"No character info for symbol '{c}'");
			}
		}
		return (float)num * _textMesh.characterSize * Mathf.Abs(_textMesh.transform.lossyScale.x) * 0.1f;
	}
}
