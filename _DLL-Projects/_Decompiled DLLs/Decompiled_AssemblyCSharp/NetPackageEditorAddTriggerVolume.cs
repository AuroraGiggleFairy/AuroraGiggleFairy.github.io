using UnityEngine.Scripting;

[Preserve]
public class NetPackageEditorAddTriggerVolume : NetPackageEditorAddSleeperVolume
{
	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world != null && !_world.IsRemote())
		{
			PrefabTriggerVolumeManager.Instance.AddTriggerVolumeServer(startPos, size);
		}
	}
}
