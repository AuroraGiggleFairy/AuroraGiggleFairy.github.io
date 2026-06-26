using LibNoise;
using LibNoise.Modifiers;

public class TGMHillsAndFlat : TGMAbstract
{
	[PublicizedFrom(EAccessModifier.Private)]
	public IModule outputModule;

	public TGMHillsAndFlat()
	{
		Select obj = new Select(source1: new ScaleBiasOutput(new FastBillow
		{
			Frequency = 4.0
		})
		{
			Scale = 0.4,
			Bias = 1.0
		}, source2: new ScaleBiasOutput(new FastTurbulence(new ScaleBiasOutput(new FastRidgedMultifractal(0)
		{
			Frequency = 5.0
		})
		{
			Scale = 1.2,
			Bias = 4.0
		})
		{
			Power = 0.45,
			Frequency = 3.0,
			Roughness = 3
		})
		{
			Scale = 0.800000011920929,
			Bias = 9.0
		}, control: new FastNoise(0 + 1)
		{
			Frequency = 3.0
		});
		obj.SetBounds(0.0, 0.5);
		obj.EdgeFalloff = 0.5;
		outputModule = obj;
		IsSeedSet = true;
	}

	public override void SetSeed(int _seed)
	{
	}

	public override float GetValue(float _x, float _z, float _biomeIntens)
	{
		return 1f * _biomeIntens + (float)outputModule.GetValue(_x, 0.0, _z) * _biomeIntens;
	}
}
