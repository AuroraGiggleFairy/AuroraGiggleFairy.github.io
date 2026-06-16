using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_Credits : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const int ScrollSpeed = 200;

	public static string ID = "";

	[XuiBindComponent("btnBack", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnBack;

	[XuiBindComponent("creditsGrid", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiView creditsGrid;

	[XuiBindComponent(true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiV_Panel creditsPanel;

	[PublicizedFrom(EAccessModifier.Private)]
	public Transform creditsGridTrans;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2i initialPosition;

	[PublicizedFrom(EAccessModifier.Private)]
	public string defaultCategoryTemplate;

	[PublicizedFrom(EAccessModifier.Private)]
	public string defaultCreditTemplate;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<string, GameObject> categoryTemplates = new CaseInsensitiveStringDictionary<GameObject>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<string, GameObject> creditTemplates = new CaseInsensitiveStringDictionary<GameObject>();

	[PublicizedFrom(EAccessModifier.Private)]
	public bool firstUpdate;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool loaded;

	[XuiXmlAttribute("file", false)]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public string CreditsFile
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public override void Init()
	{
		base.Init();
		ID = base.WindowGroup.Id;
		btnBack.OnPress += BtnBack_OnPressed;
		creditsGridTrans = creditsGrid.UiTransform;
		initialPosition = creditsPanel.Position;
		getTemplates("categoryTemplates", categoryTemplates, ref defaultCategoryTemplate);
		getTemplates("creditTemplates", creditTemplates, ref defaultCreditTemplate);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void getTemplates(string _parentRectName, Dictionary<string, GameObject> _templatesDict, ref string _defaultTemplateField)
	{
		XUiController childById = GetChildById(_parentRectName);
		if (childById == null)
		{
			return;
		}
		for (int i = 0; i < childById.Children.Count; i++)
		{
			XUiView xUiView = childById.Children[i].ViewComponent;
			string iD = xUiView.ID;
			if (_defaultTemplateField == null)
			{
				_defaultTemplateField = iD;
			}
			_templatesDict[iD] = xUiView.UiTransform.gameObject;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnBack_OnPressed(XUiController _sender, int _mouseButton)
	{
		xui.playerUI.windowManager.Close(windowGroup);
		xui.playerUI.windowManager.Open(XUiC_MainMenu.ID, _bModal: true);
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (windowGroup.isModal)
		{
			windowGroup.openWindowOnEsc = XUiC_MainMenu.ID;
		}
		string key;
		GameObject value;
		foreach (KeyValuePair<string, GameObject> categoryTemplate in categoryTemplates)
		{
			categoryTemplate.Deconstruct(out key, out value);
			value.SetActive(value: false);
		}
		foreach (KeyValuePair<string, GameObject> creditTemplate in creditTemplates)
		{
			creditTemplate.Deconstruct(out key, out value);
			value.SetActive(value: false);
		}
		loadCredits();
		firstUpdate = true;
		creditsPanel.StaticWidgets = true;
		creditsPanel.Position = initialPosition;
		creditsPanel.ClipOffset = Vector2.zero;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (firstUpdate)
		{
			firstUpdate = false;
			return;
		}
		Vector2 clipOffset = creditsPanel.ClipOffset - new Vector2(0f, 200f * _dt);
		creditsPanel.Position = new Vector2i(initialPosition.x, initialPosition.y - Mathf.RoundToInt(clipOffset.y));
		creditsPanel.ClipOffset = clipOffset;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XmlFile loadXml()
	{
		if (CreditsFile == null)
		{
			return new XmlFile(Resources.Load("Data/Credits/Credits") as TextAsset);
		}
		string text = ModManager.PatchModPathString(CreditsFile);
		if (text != null)
		{
			return new XmlFile(Path.GetDirectoryName(text), Path.GetFileName(text));
		}
		return null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject getTemplate(string _name, string _defaultTemplateName, Dictionary<string, GameObject> _templatesDict)
	{
		if (string.IsNullOrEmpty(_name))
		{
			_name = _defaultTemplateName;
		}
		return _templatesDict[_name];
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void loadCredits()
	{
		if (loaded || defaultCategoryTemplate == null || defaultCreditTemplate == null)
		{
			return;
		}
		loaded = true;
		XElement xElement = loadXml()?.XmlDoc.Root;
		if (xElement == null)
		{
			Log.Error("Credits.xml not found or no XML root");
			return;
		}
		int num = 10000;
		foreach (XElement item in xElement.Elements("category"))
		{
			string text = item.Attribute("name")?.Value ?? "";
			string name = item.Attribute("template")?.Value ?? defaultCategoryTemplate;
			GameObject gameObject = Object.Instantiate(getTemplate(name, defaultCategoryTemplate, categoryTemplates), creditsGridTrans.transform);
			gameObject.name = num++ + gameObject.name;
			gameObject.transform.localScale = Vector3.one;
			gameObject.transform.localPosition = Vector3.zero;
			gameObject.transform.Find("caption").GetComponent<UILabel>().text = text;
			gameObject.SetActive(value: true);
			foreach (XElement item2 in item.Elements())
			{
				string text2 = item2.Attribute("name")?.Value ?? "";
				string text3 = item2.Attribute("center_text")?.Value ?? "";
				string text4 = item2.Attribute("contribution")?.Value ?? "";
				name = item2.Attribute("template")?.Value ?? defaultCreditTemplate;
				GameObject gameObject2 = Object.Instantiate(getTemplate(name, defaultCreditTemplate, creditTemplates), creditsGridTrans.transform);
				gameObject2.name = num++ + gameObject2.name;
				gameObject2.transform.localScale = Vector3.one;
				gameObject2.transform.localPosition = Vector3.zero;
				gameObject2.transform.Find("name").GetComponent<UILabel>().text = text2;
				gameObject2.transform.Find("contribution").GetComponent<UILabel>().text = text4;
				gameObject2.transform.Find("centertext").GetComponent<UILabel>().text = text3;
				gameObject2.transform.Find("line").gameObject.SetActive(text2.Length > 0 && text4.Length > 0);
				gameObject2.SetActive(value: true);
			}
			GameObject gameObject3 = Object.Instantiate(getTemplate("", defaultCreditTemplate, creditTemplates), creditsGridTrans.transform);
			gameObject3.name = num++ + gameObject3.name;
			gameObject3.transform.localScale = Vector3.one;
			gameObject3.transform.localPosition = Vector3.zero;
			gameObject3.transform.Find("name").GetComponent<UILabel>().text = "";
			gameObject3.transform.Find("contribution").GetComponent<UILabel>().text = "";
			gameObject3.transform.Find("centertext").GetComponent<UILabel>().text = "";
			gameObject3.transform.Find("line").gameObject.SetActive(value: false);
			gameObject3.SetActive(value: true);
		}
		creditsGridTrans.GetComponent<UIGrid>()?.Reposition();
		creditsGridTrans.GetComponent<UITable>()?.Reposition();
	}
}
