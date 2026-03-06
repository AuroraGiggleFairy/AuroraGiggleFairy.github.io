namespace Discord.Net.ED25519.Ed25519Ref10;

internal struct GroupElementPreComp(FieldElement yplusx, FieldElement yminusx, FieldElement xy2d)
{
	public FieldElement yplusx = yplusx;

	public FieldElement yminusx = yminusx;

	public FieldElement xy2d = xy2d;
}
