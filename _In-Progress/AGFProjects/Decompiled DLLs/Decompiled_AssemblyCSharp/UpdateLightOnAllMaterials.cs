public class UpdateLightOnAllMaterials : UpdateLight
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnEnable()
	{
		base.OnEnable();
		if (!GameManager.IsDedicatedServer && GameLightManager.Instance != null)
		{
			GameLightManager.Instance.AddUpdateLight(this);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnDisable()
	{
		base.OnDisable();
		if (!GameManager.IsDedicatedServer && GameLightManager.Instance != null)
		{
			GameLightManager.Instance.RemoveUpdateLight(this);
		}
	}
}
