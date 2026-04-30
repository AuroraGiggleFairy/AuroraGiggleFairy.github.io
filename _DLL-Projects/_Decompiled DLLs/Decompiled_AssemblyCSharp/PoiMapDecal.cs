public class PoiMapDecal
{
	public int textureIndex;

	public BlockFace face;

	public float m_Prob;

	public PoiMapDecal(int _texIndex, BlockFace _face, float _prob)
	{
		textureIndex = _texIndex;
		face = _face;
		m_Prob = _prob;
	}
}
