using MusicUtils.Enums;

namespace DynamicMusic.Legacy.ObjectModel;

public class ThreatLevel : EnumDictionary<LayerType, Layer>
{
	public readonly double Tempo;

	public readonly double SignatureHi;

	public readonly double SignatureLo;

	public ThreatLevel(double _tempo, double _sigHi, double _sigLo)
	{
		Tempo = _tempo;
		SignatureHi = _sigHi;
		SignatureLo = _sigLo;
	}
}
