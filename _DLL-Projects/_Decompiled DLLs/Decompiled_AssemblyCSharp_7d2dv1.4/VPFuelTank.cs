using UnityEngine.Scripting;

[Preserve]
public class VPFuelTank : VehiclePart
{
	[PublicizedFrom(EAccessModifier.Private)]
	public float fuelCapacity;

	[PublicizedFrom(EAccessModifier.Private)]
	public float fuelLevel;

	public VPFuelTank()
	{
		fuelLevel = 0f;
	}

	public override void SetProperties(DynamicProperties _properties)
	{
		base.SetProperties(_properties);
		StringParsers.TryParseFloat(GetProperty("capacity"), out fuelCapacity);
		if (fuelCapacity < 1f)
		{
			fuelCapacity = 1f;
		}
		fuelLevel = fuelCapacity;
	}

	public override void HandleEvent(Event _event, VehiclePart _part, float _amount)
	{
		if (_event == Event.FuelRemove)
		{
			AddFuel(0f - _amount);
		}
	}

	public override bool IsBroken()
	{
		return false;
	}

	public float GetFuelLevel()
	{
		if (IsBroken())
		{
			return 0f;
		}
		return fuelLevel;
	}

	public float GetMaxFuelLevel()
	{
		if (IsBroken())
		{
			return 0f;
		}
		return fuelCapacity * vehicle.EffectFuelMaxPer;
	}

	public float GetFuelLevelPercent()
	{
		if (IsBroken())
		{
			return 0f;
		}
		float num = fuelLevel / (fuelCapacity * vehicle.EffectFuelMaxPer);
		if (num > 1f)
		{
			num = 1f;
		}
		return num;
	}

	public void SetFuelLevel(float _fuelLevel)
	{
		if (_fuelLevel <= 0f)
		{
			fuelLevel = 0f;
			vehicle.FireEvent(Event.FuelEmpty, this, 0f);
			return;
		}
		float num = fuelCapacity * vehicle.EffectFuelMaxPer;
		if (_fuelLevel > num)
		{
			_fuelLevel = num;
		}
		fuelLevel = _fuelLevel;
	}

	public void AddFuel(float _amount)
	{
		SetFuelLevel(fuelLevel + _amount);
	}
}
