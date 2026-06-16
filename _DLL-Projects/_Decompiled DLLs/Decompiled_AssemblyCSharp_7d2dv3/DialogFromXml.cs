using System;
using System.Collections;
using System.Xml.Linq;

public class DialogFromXml
{
	public static IEnumerator Load(XmlFile _xmlFile)
	{
		Dialog.DialogList.Clear();
		CreateDialogs(_xmlFile);
		yield break;
	}

	public static void Reload()
	{
	}

	public static bool CreateDialogs(XmlFile xmlFile)
	{
		Dialog.DialogList.Clear();
		XElement root = xmlFile.XmlDoc.Root;
		if (!root.HasElements)
		{
			throw new Exception("No element <dialogs> found!");
		}
		ParseNode(root);
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseNode(XElement root)
	{
		foreach (XElement item in root.Elements("dialog"))
		{
			Dialog dialog = ParseDialog(item);
			Dialog.DialogList.Add(dialog.ID, dialog);
		}
	}

	public static Dialog ParseDialog(XElement e)
	{
		if (!e.HasAttribute("id"))
		{
			throw new Exception("quest must have an id attribute");
		}
		string attribute = e.GetAttribute("id");
		if (Dialog.DialogList.ContainsKey(attribute))
		{
			throw new Exception("Duplicate dialog entry with id " + attribute);
		}
		Dialog dialog = new Dialog(attribute);
		if (e.HasAttribute("startstatementid"))
		{
			dialog.StartStatementID = e.GetAttribute("startstatementid");
		}
		if (e.HasAttribute("startresponseid"))
		{
			dialog.StartResponseID = e.GetAttribute("startresponseid");
		}
		foreach (XElement item in e.Elements())
		{
			if (item.Name == "phase")
			{
				ParsePhase(dialog, item);
				continue;
			}
			if (item.Name == "statement")
			{
				ParseStatement(dialog, item);
				continue;
			}
			if (item.Name == "response")
			{
				ParseResponse(dialog, item);
				continue;
			}
			throw new Exception("Unrecognized xml element " + item.Name);
		}
		return dialog;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParsePhase(Dialog dialog, XElement e)
	{
		DialogPhase dialogPhase = null;
		dialogPhase = new DialogPhase(e.GetAttribute("id"));
		dialog.Phases.Add(dialogPhase);
		dialogPhase.OwnerDialog = dialog;
		if (e.HasAttribute("startstatementid"))
		{
			dialogPhase.StartStatementID = e.GetAttribute("startstatementid");
		}
		if (e.HasAttribute("startresponseid"))
		{
			dialogPhase.StartResponseID = e.GetAttribute("startresponseid");
		}
		foreach (XElement item in e.Elements("requirement"))
		{
			BaseDialogRequirement baseDialogRequirement = ParseRequirement(item);
			if (baseDialogRequirement != null)
			{
				dialogPhase.AddRequirement(baseDialogRequirement);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseStatement(Dialog dialog, XElement e)
	{
		DialogStatement dialogStatement = null;
		if (e.HasAttribute("id"))
		{
			dialogStatement = new DialogStatement(e.GetAttribute("id"));
			dialog.Statements.Add(dialogStatement);
			dialogStatement.OwnerDialog = dialog;
		}
		if (e.HasAttribute("nextstatementid"))
		{
			dialogStatement.NextStatementID = e.GetAttribute("nextstatementid");
		}
		if (e.HasAttribute("text"))
		{
			dialogStatement.Text = Localization.Get(e.GetAttribute("text"));
		}
		foreach (XElement item in e.Elements())
		{
			if (item.Name == "response_entry")
			{
				DialogResponseEntry dialogResponseEntry = null;
				if (item.HasAttribute("id"))
				{
					dialogResponseEntry = new DialogResponseEntry(item.GetAttribute("id"));
					dialogStatement.ResponseEntries.Add(dialogResponseEntry);
				}
				if (dialogResponseEntry != null && item.HasAttribute("uniqueid"))
				{
					dialogResponseEntry.UniqueID = item.GetAttribute("uniqueid");
				}
			}
			else if (item.Name == "quest_entry")
			{
				string questID = "";
				string returnStatementID = "";
				string type = "";
				int listIndex = -1;
				int tier = -1;
				if (item.HasAttribute("id"))
				{
					questID = item.GetAttribute("id");
				}
				if (item.HasAttribute("type"))
				{
					type = item.GetAttribute("type");
				}
				if (item.HasAttribute("nextstatementid"))
				{
					returnStatementID = item.GetAttribute("nextstatementid");
				}
				if (item.HasAttribute("returnstatementid"))
				{
					returnStatementID = item.GetAttribute("returnstatementid");
				}
				if (item.HasAttribute("tier"))
				{
					tier = StringParsers.ParseSInt32(item.GetAttribute("tier"));
				}
				if (item.HasAttribute("listindex"))
				{
					listIndex = Convert.ToInt32(item.GetAttribute("listindex"));
				}
				dialogStatement.ResponseEntries.Add(new DialogQuestResponseEntry(questID, type, returnStatementID, listIndex, tier));
			}
			else if (item.Name == "action")
			{
				BaseDialogAction baseDialogAction = ParseAction(item);
				if (baseDialogAction != null)
				{
					dialogStatement.AddAction(baseDialogAction);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseResponse(Dialog dialog, XElement e)
	{
		DialogResponse dialogResponse = null;
		if (e.HasAttribute("id"))
		{
			dialogResponse = new DialogResponse(e.GetAttribute("id"));
			dialog.Responses.Add(dialogResponse);
			dialogResponse.OwnerDialog = dialog;
		}
		if (e.HasAttribute("nextstatementid"))
		{
			dialogResponse.NextStatementID = e.GetAttribute("nextstatementid");
		}
		if (e.HasAttribute("text"))
		{
			dialogResponse.Text = Localization.Get(e.GetAttribute("text"));
		}
		if (e.HasAttribute("returnstatementid"))
		{
			dialogResponse.ReturnStatementID = e.GetAttribute("returnstatementid");
		}
		foreach (XElement item in e.Elements())
		{
			if (item.Name == "requirement")
			{
				BaseDialogRequirement baseDialogRequirement = ParseRequirement(item);
				if (baseDialogRequirement != null)
				{
					dialogResponse.AddRequirement(baseDialogRequirement);
				}
			}
			if (item.Name == "action")
			{
				BaseDialogAction baseDialogAction = ParseAction(item);
				if (baseDialogAction != null)
				{
					dialogResponse.AddAction(baseDialogAction);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static BaseDialogAction ParseAction(XElement e)
	{
		if (!e.HasAttribute("type"))
		{
			throw new Exception("Dialog Action must have a type!");
		}
		BaseDialogAction baseDialogAction = null;
		string attribute = e.GetAttribute("type");
		try
		{
			baseDialogAction = (BaseDialogAction)Activator.CreateInstance(ReflectionHelpers.GetTypeWithPrefix("DialogAction", attribute));
		}
		catch (Exception)
		{
			throw new Exception("No action class '" + attribute + " found!");
		}
		if (e.HasAttribute("id"))
		{
			baseDialogAction.ID = e.GetAttribute("id");
		}
		if (e.HasAttribute("value"))
		{
			baseDialogAction.Value = e.GetAttribute("value");
		}
		return baseDialogAction;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static BaseDialogRequirement ParseRequirement(XElement e)
	{
		if (!e.HasAttribute("type"))
		{
			throw new Exception("Dialog Action must have a type!");
		}
		BaseDialogRequirement baseDialogRequirement = null;
		string attribute = e.GetAttribute("type");
		try
		{
			baseDialogRequirement = (BaseDialogRequirement)Activator.CreateInstance(ReflectionHelpers.GetTypeWithPrefix("DialogRequirement", attribute));
		}
		catch (Exception)
		{
			throw new Exception("No action class '" + attribute + " found!");
		}
		if (e.HasAttribute("id"))
		{
			baseDialogRequirement.ID = e.GetAttribute("id");
		}
		if (e.HasAttribute("value"))
		{
			baseDialogRequirement.Value = e.GetAttribute("value");
		}
		if (e.HasAttribute("tag"))
		{
			baseDialogRequirement.Tag = e.GetAttribute("tag");
		}
		else
		{
			baseDialogRequirement.Tag = "";
		}
		if (e.HasAttribute("requirementtype"))
		{
			baseDialogRequirement.RequirementVisibilityType = EnumUtils.Parse<BaseDialogRequirement.RequirementVisibilityTypes>(e.GetAttribute("requirementtype"));
		}
		return baseDialogRequirement;
	}
}
