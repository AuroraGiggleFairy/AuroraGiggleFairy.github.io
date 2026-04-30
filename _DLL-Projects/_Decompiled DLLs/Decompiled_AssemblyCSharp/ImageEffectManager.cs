using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
[AddComponentMenu("Image Effects/Rendering/Colorize")]
public class ImageEffectManager : MonoBehaviour
{
	[Serializable]
	public class Effect
	{
		public string name;

		[NonSerialized]
		[PublicizedFrom(EAccessModifier.Private)]
		public bool needToCheckMaterial = true;

		public Material material;

		public Material instantiatedMtrl;

		[NonSerialized]
		[PublicizedFrom(EAccessModifier.Private)]
		public GameObject materialHolder;

		public Dictionary<string, bool> hasProperty;

		public Dictionary<string, float> floatPropertyUpdates;

		public Effect()
		{
			instantiatedMtrl = null;
			hasProperty = new Dictionary<string, bool>();
			floatPropertyUpdates = new Dictionary<string, float>();
		}

		public void UpdateMaterial()
		{
			if (material == null)
			{
				return;
			}
			foreach (KeyValuePair<string, float> floatPropertyUpdate in floatPropertyUpdates)
			{
				if (needToCheckMaterial)
				{
					if (GetMaterial().HasProperty(floatPropertyUpdate.Key))
					{
						GetMaterial().SetFloat(floatPropertyUpdate.Key, floatPropertyUpdate.Value);
					}
				}
				else
				{
					GetMaterial().SetFloat(floatPropertyUpdate.Key, floatPropertyUpdate.Value);
				}
			}
			floatPropertyUpdates.Clear();
			needToCheckMaterial = false;
		}

		public void SetFloat(string _propertyName, float _value)
		{
			bool value = false;
			if (hasProperty.TryGetValue(_propertyName, out value))
			{
				if (value)
				{
					float value2 = 0f;
					if (!floatPropertyUpdates.TryGetValue(_propertyName, out value2))
					{
						floatPropertyUpdates.Add(_propertyName, _value);
					}
					else
					{
						floatPropertyUpdates[_propertyName] = _value;
					}
				}
			}
			else if (material != null)
			{
				bool flag = GetMaterial().HasProperty(_propertyName);
				hasProperty.Add(_propertyName, flag);
				if (flag)
				{
					floatPropertyUpdates.Add(_propertyName, _value);
				}
			}
			else
			{
				needToCheckMaterial = true;
				float value3 = 0f;
				if (!floatPropertyUpdates.TryGetValue(_propertyName, out value3))
				{
					floatPropertyUpdates.Add(_propertyName, _value);
				}
				else
				{
					floatPropertyUpdates[_propertyName] = _value;
				}
			}
		}

		public Material GetMaterial()
		{
			if (instantiatedMtrl == null)
			{
				instantiatedMtrl = UnityEngine.Object.Instantiate(material);
			}
			return instantiatedMtrl;
		}
	}

	[Serializable]
	public class EffectGroup
	{
		public string name;

		public Effect[] effects;

		public int GetNumEffects()
		{
			if (effects == null)
			{
				return 0;
			}
			return effects.Length;
		}

		public string GetEffectName(int _index)
		{
			if (effects == null)
			{
				return "";
			}
			if (_index < 0)
			{
				return "";
			}
			if (_index >= effects.Length)
			{
				return "";
			}
			return effects[_index].name;
		}
	}

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static GameObject staticGameObject;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static ImageEffectManager staticClass;

	public static Dictionary<string, EffectGroup> staticEffectGroups;

	public EffectGroup[] effectGroups;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<string, Dictionary<int, float>> enabledEffects;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int numEnabledEffects;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static bool validated;

	public int GetNumEffects(string _effectGroupName)
	{
		ValidateStaticClassEffects();
		if (staticEffectGroups == null)
		{
			return 0;
		}
		if (staticEffectGroups.TryGetValue(_effectGroupName, out var value))
		{
			return value.GetNumEffects();
		}
		return 0;
	}

	public string GetEffectName(string _effectGroupName, int _index)
	{
		if (_index <= 0)
		{
			return "Off";
		}
		ValidateStaticClassEffects();
		if (staticEffectGroups == null)
		{
			return "";
		}
		if (staticEffectGroups.TryGetValue(_effectGroupName, out var value))
		{
			return value.GetEffectName(_index);
		}
		return "";
	}

	public void DisableEffectGroup(string _effectGroupName)
	{
		if (enabledEffects != null && enabledEffects.TryGetValue(_effectGroupName, out var value))
		{
			numEnabledEffects -= value.Count;
			value.Clear();
		}
	}

	public bool SetEffect_Slow(string _effectGroupName, string _effectName, float _newIntensity = 1f)
	{
		if (!ValidateEffects())
		{
			return false;
		}
		if (staticEffectGroups.TryGetValue(_effectGroupName, out var value))
		{
			int num = 0;
			Effect[] effects = value.effects;
			for (int i = 0; i < effects.Length; i++)
			{
				if (effects[i].name.Equals(_effectName))
				{
					return SetEffect(_effectGroupName, num, _newIntensity);
				}
				num++;
			}
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool ValidateEffects()
	{
		ValidateStaticClassEffects();
		if (staticClass == null)
		{
			return false;
		}
		if (staticEffectGroups == null)
		{
			return false;
		}
		if (enabledEffects == null)
		{
			enabledEffects = new Dictionary<string, Dictionary<int, float>>();
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetEffectIntenal(string _effectGroupName, int _index, float _newIntensity = 1f)
	{
		if (enabledEffects.TryGetValue(_effectGroupName, out var value))
		{
			float value2 = 0f;
			if (value.TryGetValue(_index, out value2))
			{
				if (_newIntensity == 0f)
				{
					value.Remove(_index);
					if (value.Count == 0)
					{
						enabledEffects.Remove(_effectGroupName);
					}
					numEnabledEffects--;
				}
				else
				{
					value[_index] = _newIntensity;
				}
				return;
			}
			if (_newIntensity > 0f)
			{
				value.Add(_index, _newIntensity);
				numEnabledEffects++;
			}
		}
		else if (_newIntensity > 0f)
		{
			Dictionary<int, float> dictionary = new Dictionary<int, float>();
			dictionary.Add(_index, _newIntensity);
			enabledEffects.Add(_effectGroupName, dictionary);
			numEnabledEffects++;
		}
		base.enabled = numEnabledEffects > 0;
	}

	public bool SetEffect(string _effectGroupName, float _newIntensity = 1f)
	{
		return SetEffect(_effectGroupName, 0, _newIntensity);
	}

	public bool SetEffect(string _effectGroupName, string _effectName, float _newIntensity = 1f)
	{
		ValidateStaticClassEffects();
		if (staticEffectGroups.TryGetValue(_effectGroupName, out var value))
		{
			int num = 0;
			Effect[] effects = value.effects;
			for (int i = 0; i < effects.Length; i++)
			{
				if (effects[i].name.Equals(_effectName))
				{
					return SetEffect(_effectGroupName, num, _newIntensity);
				}
				num++;
			}
		}
		return false;
	}

	public bool SetEffect(string _effectGroupName, int _index, float _newIntensity = 1f)
	{
		if (_index < 0)
		{
			return false;
		}
		if (!ValidateEffects())
		{
			return false;
		}
		SetEffectIntenal(_effectGroupName, _index, _newIntensity);
		return true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Start()
	{
		ValidateStaticClassEffects();
		_ = staticClass == null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ValidateStaticClassEffects()
	{
		if (validated)
		{
			return;
		}
		if (staticGameObject == null)
		{
			staticGameObject = Resources.Load<GameObject>("Prefabs/ImageEffectsPrefab");
			if (staticGameObject != null)
			{
				staticClass = staticGameObject.GetComponent<ImageEffectManager>();
			}
		}
		if (staticClass == null)
		{
			return;
		}
		if (staticEffectGroups == null)
		{
			staticEffectGroups = new Dictionary<string, EffectGroup>();
		}
		if (staticClass.effectGroups != null)
		{
			EffectGroup[] array = staticClass.effectGroups;
			foreach (EffectGroup effectGroup in array)
			{
				staticEffectGroups.Add(effectGroup.name, effectGroup);
			}
		}
		validated = true;
	}

	public void SetFloat_Slow(string _effectGroup, string _effectName, string _propertyName, float _value)
	{
		ValidateStaticClassEffects();
		EffectGroup value = null;
		if (!staticEffectGroups.TryGetValue(_effectGroup, out value))
		{
			return;
		}
		for (int i = 0; i < value.effects.Length; i++)
		{
			if (value.effects[i].name.EqualsCaseInsensitive(_effectName))
			{
				SetFloat(value, i, _propertyName, _value);
				break;
			}
		}
	}

	public void SetFloat(string _effectGroup, int _effectIndex, string _propertyName, float _value)
	{
		ValidateStaticClassEffects();
		if (_effectIndex >= 0)
		{
			EffectGroup value = null;
			if (staticEffectGroups.TryGetValue(_effectGroup, out value))
			{
				SetFloat(value, _effectIndex, _propertyName, _value);
			}
		}
	}

	public void SetFloat(EffectGroup _effectGroup, int _effectIndex, string _propertyName, float _value)
	{
		ValidateStaticClassEffects();
		if (_effectGroup.effects.Length > _effectIndex)
		{
			_effectGroup.effects[_effectIndex].SetFloat(_propertyName, _value);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (source == null)
		{
			return;
		}
		if (enabledEffects == null || enabledEffects.Count == 0)
		{
			base.enabled = false;
			return;
		}
		ValidateStaticClassEffects();
		RenderTexture renderTexture = null;
		RenderTexture renderTexture2 = null;
		if (enabledEffects.Count > 1)
		{
			renderTexture = RenderTexture.GetTemporary(source.width, source.height);
		}
		if (enabledEffects.Count > 2)
		{
			renderTexture2 = RenderTexture.GetTemporary(source.width, source.height);
		}
		bool flag = true;
		int num = 0;
		RenderTexture renderTexture3 = source;
		foreach (KeyValuePair<string, Dictionary<int, float>> enabledEffect in enabledEffects)
		{
			foreach (KeyValuePair<int, float> item in enabledEffect.Value)
			{
				if (!staticEffectGroups.ContainsKey(enabledEffect.Key))
				{
					continue;
				}
				Effect effect = staticEffectGroups[enabledEffect.Key].effects[item.Key];
				if (effect.material == null)
				{
					continue;
				}
				effect.GetMaterial().SetFloat("Intensity", Mathf.Clamp01(item.Value));
				Material material = effect.GetMaterial();
				BlendMode blendMode = BlendMode.OneMinusDstAlpha;
				bool value = false;
				if (effect.hasProperty.TryGetValue("BlendSrc", out value) && value)
				{
					blendMode = (BlendMode)material.GetInt("BlendSrc");
				}
				effect.UpdateMaterial();
				if (renderTexture == null)
				{
					Graphics.Blit(renderTexture3, destination, effect.GetMaterial());
				}
				else if (flag)
				{
					if (blendMode == BlendMode.Zero)
					{
						Graphics.Blit(renderTexture3, renderTexture, effect.GetMaterial());
						renderTexture3 = renderTexture;
					}
					else
					{
						Graphics.Blit(renderTexture3, renderTexture3, effect.GetMaterial());
					}
					flag = false;
				}
				else if (num == numEnabledEffects - 1)
				{
					Graphics.Blit(renderTexture3, destination, effect.GetMaterial());
				}
				else if (blendMode == BlendMode.Zero)
				{
					RenderTexture renderTexture4 = ((renderTexture3 == renderTexture) ? renderTexture2 : renderTexture);
					Graphics.Blit(renderTexture3, renderTexture4, effect.GetMaterial());
					renderTexture3 = renderTexture4;
				}
				else
				{
					Graphics.Blit(renderTexture3, renderTexture3, effect.GetMaterial());
				}
				num++;
			}
		}
		if (renderTexture != null)
		{
			RenderTexture.ReleaseTemporary(renderTexture);
		}
		if (renderTexture2 != null)
		{
			RenderTexture.ReleaseTemporary(renderTexture2);
		}
	}
}
