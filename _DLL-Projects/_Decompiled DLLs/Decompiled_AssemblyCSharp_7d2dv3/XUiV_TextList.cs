using UnityEngine;

public class XUiV_TextList : XUiV_LabelBase
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public UITextList textList;

	[PublicizedFrom(EAccessModifier.Protected)]
	public UITextList.Style listStyle;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string firstLinePrefix;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int paragraphHistory = 50;

	public float ScrollValue
	{
		get
		{
			return textList.scrollValue;
		}
		set
		{
			textList.scrollValue = value;
		}
	}

	[XuiXmlAttribute("max_paragraphs", false)]
	public int ParagraphHistory
	{
		get
		{
			return paragraphHistory;
		}
		set
		{
			if (value != paragraphHistory)
			{
				paragraphHistory = value;
				SetDirty();
			}
		}
	}

	[XuiXmlAttribute("list_style", false)]
	public UITextList.Style ListStyle
	{
		get
		{
			return listStyle;
		}
		set
		{
			if (value != listStyle)
			{
				listStyle = value;
				SetDirty();
			}
		}
	}

	[XuiXmlAttribute("prefix_first_line", false)]
	public string FirstLinePrefix
	{
		get
		{
			return firstLinePrefix;
		}
		set
		{
			if (!(firstLinePrefix == value))
			{
				firstLinePrefix = value;
				SetDirty();
			}
		}
	}

	public XUiV_TextList(XUi _xui, string _id)
		: base(_xui, _id)
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void createComponents(GameObject _go)
	{
		base.createComponents(_go);
		_go.AddComponent<UITextList>();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void captureComponents()
	{
		base.captureComponents();
		textList = uiTransform.GetComponent<UITextList>();
	}

	public override void InitView()
	{
		base.EventOnDrag = true;
		base.EventOnScroll = true;
		base.InitView();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void updateData()
	{
		label.width = size.x;
		label.height = size.y;
		textList.paragraphHistory = paragraphHistory;
		textList.style = listStyle;
		base.updateData();
	}

	public override void Cleanup()
	{
		textList.Clear();
		base.Cleanup();
	}

	public void AddLine(string _line)
	{
		if (!string.IsNullOrEmpty(firstLinePrefix))
		{
			_line = firstLinePrefix + _line;
		}
		_line = getFormattedText(_line);
		textList.Add(_line);
	}
}
