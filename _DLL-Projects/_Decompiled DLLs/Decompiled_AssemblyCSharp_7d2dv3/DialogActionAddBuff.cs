using UnityEngine.Scripting;

[Preserve]
public class DialogActionAddBuff : BaseDialogAction
{
	[PublicizedFrom(EAccessModifier.Private)]
	public string name = "";

	public override ActionTypes ActionType => ActionTypes.AddBuff;

	public override void PerformAction(EntityPlayer player)
	{
		EntityPlayer primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
		if (primaryPlayer != null)
		{
			switch (primaryPlayer.Buffs.AddBuff(base.ID))
			{
			case EntityBuffs.BuffStatus.FailedInvalidName:
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Buff failed: buff \"" + base.ID + "\" unknown");
				break;
			case EntityBuffs.BuffStatus.FailedImmune:
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Buff failed: entity is immune to \"" + base.ID);
				break;
			case EntityBuffs.BuffStatus.FailedFriendlyFire:
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Buff failed: entity is friendly");
				break;
			case EntityBuffs.BuffStatus.FailedEditor:
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Buff failed: buff " + base.ID + " not allowed in editor.");
				break;
			case EntityBuffs.BuffStatus.FailedGameStat:
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Buff failed: missing required game stat.");
				break;
			}
		}
	}
}
