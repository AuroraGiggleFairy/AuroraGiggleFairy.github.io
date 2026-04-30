using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

namespace UAI;

[Preserve]
public class UAIConsiderationBase
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public CurveType curveType;

	[PublicizedFrom(EAccessModifier.Private)]
	public float xIntercept;

	[PublicizedFrom(EAccessModifier.Private)]
	public float yIntercept;

	[PublicizedFrom(EAccessModifier.Private)]
	public float slopeIntercept = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float exponent = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool flipY;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool flipX;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public string Name { get; set; }

	public virtual void Init(Dictionary<string, string> parameters)
	{
		if (parameters.ContainsKey("curve"))
		{
			curveType = EnumUtils.Parse<CurveType>(parameters["curve"], _ignoreCase: true);
		}
		else
		{
			curveType = CurveType.Linear;
		}
		if (parameters.ContainsKey("flip_x"))
		{
			flipX = StringParsers.ParseBool(parameters["flip_x"]);
		}
		else
		{
			flipX = false;
		}
		if (parameters.ContainsKey("flip_y"))
		{
			flipY = StringParsers.ParseBool(parameters["flip_y"]);
		}
		else
		{
			flipY = false;
		}
		if (parameters.ContainsKey("x_intercept"))
		{
			xIntercept = StringParsers.ParseFloat(parameters["x_intercept"]);
		}
		if (parameters.ContainsKey("y_intercept"))
		{
			yIntercept = StringParsers.ParseFloat(parameters["y_intercept"]);
		}
		if (parameters.ContainsKey("slope_intercept"))
		{
			slopeIntercept = StringParsers.ParseFloat(parameters["slope_intercept"]);
		}
		if (parameters.ContainsKey("exponent"))
		{
			exponent = StringParsers.ParseFloat(parameters["exponent"]);
		}
	}

	public virtual float GetScore(Context _context, object currentTargetConsideration)
	{
		return 1f;
	}

	public float ComputeResponseCurve(float x)
	{
		if (flipX)
		{
			x = 1f - x;
		}
		float num = 0f;
		switch (curveType)
		{
		case CurveType.Constant:
			num = yIntercept;
			break;
		case CurveType.Linear:
			num = slopeIntercept * (x - xIntercept) + yIntercept;
			break;
		case CurveType.Quadratic:
			num = slopeIntercept * x * Mathf.Pow(Mathf.Abs(x + xIntercept), exponent) + yIntercept;
			break;
		case CurveType.Logistic:
			num = exponent * (1f / (1f + Mathf.Pow(Mathf.Abs(1000f * slopeIntercept), -1f * x + xIntercept + 0.5f))) + yIntercept;
			break;
		case CurveType.Logit:
			num = (0f - Mathf.Log(1f / Mathf.Pow(Mathf.Abs(x - xIntercept), exponent) - 1f)) * 0.05f * slopeIntercept + (0.5f + yIntercept);
			break;
		case CurveType.Threshold:
			num = ((x > xIntercept) ? (1f - yIntercept) : (0f - (1f - slopeIntercept)));
			break;
		case CurveType.Sine:
			num = Mathf.Sin(slopeIntercept * Mathf.Pow(x + xIntercept, exponent)) * 0.5f + 0.5f + yIntercept;
			break;
		case CurveType.Parabolic:
			num = Mathf.Pow(slopeIntercept * (x + xIntercept), 2f) + exponent * (x + xIntercept) + yIntercept;
			break;
		case CurveType.NormalDistribution:
			num = exponent / Mathf.Sqrt(6.283192f) * Mathf.Pow(2f, (0f - 1f / (Mathf.Abs(slopeIntercept) * 0.01f)) * Mathf.Pow(x - (xIntercept + 0.5f), 2f)) + yIntercept;
			break;
		case CurveType.Bounce:
			num = Mathf.Abs(Mathf.Sin(6.28f * exponent * (x + xIntercept + 1f) * (x + xIntercept + 1f)) * (1f - x) * slopeIntercept) + yIntercept;
			break;
		}
		if (flipY)
		{
			num = 1f - num;
		}
		return Mathf.Clamp01(num);
	}
}
