namespace PrefabVolumes;

public class PrefabInfoVolume : PrefabVolumeAbs<PrefabInfoVolume>
{
	public override EVolumeType VolumeType => EVolumeType.Info;

	public override int SerializedSize => 25;
}
