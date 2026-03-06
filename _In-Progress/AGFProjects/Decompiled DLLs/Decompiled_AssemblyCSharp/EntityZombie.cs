using UnityEngine.Scripting;

[Preserve]
public class EntityZombie : EntityHuman
	// Patch: Register real screamer entityId with ScreamerAlertManager when spawned
	public override void Awake()
	{
		base.Awake();
		try {
			if (this.EntityClass != null && this.EntityClass.entityClassName != null && this.EntityClass.entityClassName.ToLower().Contains("screamer")) {
				Debug.Log($"[EntityZombie] Awake: Registering screamer entityId={this.entityId} at {this.position}");
				if (ScreamerAlertManager.Instance != null) {
					ScreamerAlertManager.Instance.AddScoutScreamer(this.position, this.entityId);
				} else {
					Debug.LogWarning("[EntityZombie] ScreamerAlertManager.Instance is null, cannot register screamer entityId");
				}
			}
		} catch (System.Exception ex) {
			Debug.LogError($"[EntityZombie] Exception in Awake: {ex}");
		}
	}
{
	public override bool AimingGun
	{
		get
		{
			return false;
		}
		set
		{
		}
	}
}
