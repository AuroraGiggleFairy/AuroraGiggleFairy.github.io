using UnityEngine.Scripting;

[Preserve]
public class XUiC_LoginPS5 : XUiC_LoginBase
{
	public override string MsgTitleKey
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return "xuiPSNLogin";
		}
	}
}
