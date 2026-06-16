using System;
using System.Collections;
using System.Xml.Linq;
using SandboxOptions;

public class SandboxOverridesFromXml
{
	public static IEnumerator CreateOverrides(XmlFile xmlFile)
	{
		XElement root = xmlFile.XmlDoc.Root;
		SandboxOptionManager sandboxManager = SandboxOptionManager.Current;
		sandboxManager.RemoveOverrides();
		MicroStopwatch msw = new MicroStopwatch(_bStart: true);
		foreach (XElement item in root.Elements())
		{
			if (item.Name == "preset")
			{
				SandboxOptionManager.Current.LoadPresetFromXml(item, "Modded", isModded: true);
			}
			else
			{
				if (!(item.Name == "sandbox_override"))
				{
					throw new Exception("Unrecognized xml element " + item.Name);
				}
				if (item.HasAttribute("option"))
				{
					global::SandboxOptions.SandboxOptions result = global::SandboxOptions.SandboxOptions.Max;
					if (Enum.TryParse<global::SandboxOptions.SandboxOptions>(item.GetAttribute("option"), out result))
					{
						sandboxManager.AddOverride(result);
					}
				}
			}
			if (msw.ElapsedMilliseconds > Constants.cMaxLoadTimePerFrameMillis)
			{
				yield return null;
				msw.ResetAndRestart();
			}
		}
		_ = root;
	}

	public static void Reload(XmlFile xmlFile)
	{
		ThreadManager.RunCoroutineSync(CreateOverrides(xmlFile));
	}
}
