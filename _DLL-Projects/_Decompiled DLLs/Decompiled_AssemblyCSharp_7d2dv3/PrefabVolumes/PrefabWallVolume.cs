namespace PrefabVolumes;

public class PrefabWallVolume : PrefabVolumeAbs<PrefabWallVolume>
{
	public override EVolumeType VolumeType => EVolumeType.Wall;

	public override int SerializedSize => 25;
}
