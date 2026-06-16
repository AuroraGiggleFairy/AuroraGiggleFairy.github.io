using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using UnityEngine;

public class SignData
{
	public struct SignTransform : IEquatable<SignTransform>
	{
		public Vector2 position;

		public float rotation;

		public Vector2 scale;

		public static readonly SignTransform Defaults = new SignTransform(Vector2.zero, 0f, new Vector2(0.3f, 0.3f));

		public static readonly SignTransform Identity = new SignTransform(Vector2.zero, 0f, Vector2.one);

		public bool HasUniformScale => Mathf.Approximately(scale.x, scale.y);

		public SignTransform(Vector2 position, float rotation, Vector2 scale)
		{
			this.position = position;
			this.rotation = rotation;
			this.scale = scale;
		}

		public static SignTransform operator *(SignTransform parent, SignTransform child)
		{
			float f = parent.rotation * (MathF.PI / 180f);
			Vector2 vector = Vector2.Scale(child.position, parent.scale);
			Vector2 vector2 = new Vector2(vector.x * Mathf.Cos(f) - vector.y * Mathf.Sin(f), vector.x * Mathf.Sin(f) + vector.y * Mathf.Cos(f));
			Vector2 vector3 = parent.position + vector2;
			float num = parent.rotation + child.rotation;
			Vector2 vector4 = Vector2.Scale(parent.scale, child.scale);
			return new SignTransform(vector3, num, vector4);
		}

		public bool Equals(SignTransform other)
		{
			if (position == other.position && Mathf.Approximately(rotation, other.rotation))
			{
				return scale == other.scale;
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj is SignTransform other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return ((17 * 23 + position.GetHashCode()) * 23 + rotation.GetHashCode()) * 23 + scale.GetHashCode();
		}

		public static bool operator ==(SignTransform left, SignTransform right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(SignTransform left, SignTransform right)
		{
			return !left.Equals(right);
		}

		public void WriteXml(XmlElement element)
		{
			element.SetAttrib("pos", $"{position.x}, {position.y}");
			element.SetAttrib("rot", rotation.ToCultureInvariantString());
			element.SetAttrib("scale", $"{scale.x}, {scale.y}");
		}

		public void ReadXml(XElement element)
		{
			position = StringParsers.ParseVector2(element.GetAttribute("pos"));
			rotation = float.Parse(element.GetAttribute("rot"), Utils.StandardCulture);
			scale = StringParsers.ParseVector2(element.GetAttribute("scale"));
		}

		public void Write(BinaryWriter bw)
		{
			StreamUtils.Write(bw, position);
			bw.Write(rotation);
			StreamUtils.Write(bw, scale);
		}

		public static SignTransform Read(BinaryReader br)
		{
			Vector2 vector = StreamUtils.ReadVector2(br);
			float num = br.ReadSingle();
			Vector2 vector2 = StreamUtils.ReadVector2(br);
			return new SignTransform(vector, num, vector2);
		}
	}

	public struct SignRenderSettings(Color color, SignRenderSettings.Mode mode = SignRenderSettings.Mode.ColorOnly)
	{
		public enum Mode : byte
		{
			ColorOnly,
			ColorAndMask,
			MaskOnly,
			PunchOut
		}

		public Color color = color;

		public Mode mode = mode;

		public void WriteXml(XmlElement element)
		{
			element.SetAttrib("color", "#" + ColorUtility.ToHtmlStringRGBA(color));
			element.SetAttrib("mode", mode.ToString());
		}

		public void ReadXml(XElement element)
		{
			string text = (element.HasAttribute("tint") ? element.GetAttribute("tint") : element.GetAttribute("color"));
			if (!ColorUtility.TryParseHtmlString(text, out color))
			{
				Log.Error("Invalid color string: " + text);
				color = Color.white;
			}
			if (element.HasAttribute("mode"))
			{
				string attribute = element.GetAttribute("mode");
				if (!Enum.TryParse<Mode>(attribute, ignoreCase: true, out mode))
				{
					Log.Error("Failed to parse SignRenderSettings mode from string: \"" + attribute + "\"");
					mode = Mode.ColorAndMask;
				}
			}
		}

		public void Write(BinaryWriter bw)
		{
			StreamUtils.Write(bw, color);
			bw.Write((byte)mode);
		}

		public static SignRenderSettings Read(BinaryReader br)
		{
			Color obj = StreamUtils.ReadColor(br);
			Mode mode = (Mode)br.ReadByte();
			return new SignRenderSettings(obj, mode);
		}
	}

	public abstract class SignWarp
	{
		public enum WarpType : byte
		{
			Skew,
			Bulge,
			Twirl,
			Kaleido,
			Perspective,
			Arc,
			Stretch,
			Grid
		}

		public abstract WarpType TypeId
		{
			[PublicizedFrom(EAccessModifier.Protected)]
			get;
		}

		public abstract SignWarp Clone();

		[PublicizedFrom(EAccessModifier.Protected)]
		public abstract void ReadXml(XElement element);

		public abstract void WriteXml(XmlElement xmlElement);

		[PublicizedFrom(EAccessModifier.Protected)]
		public abstract void InternalRead(BinaryReader br);

		[PublicizedFrom(EAccessModifier.Protected)]
		public abstract void InternalWrite(BinaryWriter bw);

		public void Write(BinaryWriter bw)
		{
			bw.Write((byte)TypeId);
			InternalWrite(bw);
		}

		public static SignWarp Read(BinaryReader br)
		{
			WarpType warpType = (WarpType)br.ReadByte();
			object obj = warpType switch
			{
				WarpType.Skew => new SkewWarp(), 
				WarpType.Bulge => new BulgeWarp(), 
				WarpType.Twirl => new TwirlWarp(), 
				WarpType.Kaleido => new KaleidoWarp(), 
				WarpType.Perspective => new PerspectiveWarp(), 
				WarpType.Arc => new ArcWarp(), 
				WarpType.Stretch => new StretchWarp(), 
				WarpType.Grid => new GridWarp(), 
				_ => throw new Exception($"Unknown warp type id: {(byte)warpType}"), 
			};
			((SignWarp)obj).InternalRead(br);
			return (SignWarp)obj;
		}

		public static SignWarp WarpFromXml(XElement element)
		{
			string attribute = element.GetAttribute("type");
			object obj = attribute switch
			{
				"SkewWarp" => new SkewWarp(), 
				"BulgeWarp" => new BulgeWarp(), 
				"TwirlWarp" => new TwirlWarp(), 
				"KaleidoWarp" => new KaleidoWarp(), 
				"PerspectiveWarp" => new PerspectiveWarp(), 
				"ArcWarp" => new ArcWarp(), 
				"StretchWarp" => new StretchWarp(), 
				"GridWarp" => new GridWarp(), 
				_ => throw new Exception("Unknown warp type: " + attribute), 
			};
			((SignWarp)obj).ReadXml(element);
			return (SignWarp)obj;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public SignWarp()
		{
		}
	}

	public class SkewWarp : SignWarp
	{
		public Vector2 amount;

		public float rotation;

		public override WarpType TypeId
		{
			[PublicizedFrom(EAccessModifier.Protected)]
			get
			{
				return WarpType.Skew;
			}
		}

		public SkewWarp()
		{
			amount = new Vector2(0f, 0.1f);
			rotation = 0f;
		}

		public SkewWarp(Vector2 amount, float rotation)
		{
			this.amount = amount;
			this.rotation = rotation;
		}

		public override SignWarp Clone()
		{
			return new SkewWarp(amount, rotation);
		}

		public override void WriteXml(XmlElement element)
		{
			element.SetAttrib("type", GetType().Name);
			element.SetAttrib("amount", $"{amount.x}, {amount.y}");
			element.SetAttrib("rotation", rotation.ToCultureInvariantString() ?? "");
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void ReadXml(XElement element)
		{
			amount = StringParsers.ParseVector2(element.GetAttribute("amount"));
			rotation = float.Parse(element.GetAttribute("rotation"), Utils.StandardCulture);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void InternalWrite(BinaryWriter bw)
		{
			StreamUtils.Write(bw, amount);
			bw.Write(rotation);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void InternalRead(BinaryReader br)
		{
			amount = StreamUtils.ReadVector2(br);
			rotation = br.ReadSingle();
		}
	}

	public class BulgeWarp : SignWarp
	{
		public Vector2 offset;

		public float amount;

		public override WarpType TypeId
		{
			[PublicizedFrom(EAccessModifier.Protected)]
			get
			{
				return WarpType.Bulge;
			}
		}

		public BulgeWarp()
		{
			offset = Vector2.zero;
			amount = 2f;
		}

		public BulgeWarp(Vector2 offset, float amount)
		{
			this.offset = offset;
			this.amount = amount;
		}

		public override SignWarp Clone()
		{
			return new BulgeWarp(offset, amount);
		}

		public override void WriteXml(XmlElement element)
		{
			element.SetAttrib("type", GetType().Name);
			element.SetAttrib("offset", $"{offset.x}, {offset.y}");
			element.SetAttrib("amount", amount.ToCultureInvariantString() ?? "");
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void ReadXml(XElement element)
		{
			offset = StringParsers.ParseVector2(element.GetAttribute("offset"));
			amount = float.Parse(element.GetAttribute("amount"), Utils.StandardCulture);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void InternalWrite(BinaryWriter bw)
		{
			StreamUtils.Write(bw, offset);
			bw.Write(amount);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void InternalRead(BinaryReader br)
		{
			offset = StreamUtils.ReadVector2(br);
			amount = br.ReadSingle();
		}
	}

	public class TwirlWarp : SignWarp
	{
		public Vector2 offset;

		public float amount;

		public float frequency;

		public override WarpType TypeId
		{
			[PublicizedFrom(EAccessModifier.Protected)]
			get
			{
				return WarpType.Twirl;
			}
		}

		public TwirlWarp()
		{
			offset = Vector2.zero;
			amount = 0.2f;
			frequency = 0.2f;
		}

		public TwirlWarp(Vector2 offset, float amount, float frequency)
		{
			this.offset = offset;
			this.amount = amount;
			this.frequency = frequency;
		}

		public override SignWarp Clone()
		{
			return new TwirlWarp(offset, amount, frequency);
		}

		public override void WriteXml(XmlElement element)
		{
			element.SetAttrib("type", GetType().Name);
			element.SetAttrib("offset", $"{offset.x}, {offset.y}");
			element.SetAttrib("amount", amount.ToCultureInvariantString() ?? "");
			element.SetAttrib("frequency", frequency.ToCultureInvariantString() ?? "");
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void ReadXml(XElement element)
		{
			offset = StringParsers.ParseVector2(element.GetAttribute("offset"));
			amount = float.Parse(element.GetAttribute("amount"), Utils.StandardCulture);
			frequency = float.Parse(element.GetAttribute("frequency"), Utils.StandardCulture);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void InternalWrite(BinaryWriter bw)
		{
			StreamUtils.Write(bw, offset);
			bw.Write(amount);
			bw.Write(frequency);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void InternalRead(BinaryReader br)
		{
			offset = StreamUtils.ReadVector2(br);
			amount = br.ReadSingle();
			frequency = br.ReadSingle();
		}
	}

	public class KaleidoWarp : SignWarp
	{
		public Vector2 offset;

		public int sides;

		public float rotation;

		public float offsetScale;

		public override WarpType TypeId
		{
			[PublicizedFrom(EAccessModifier.Protected)]
			get
			{
				return WarpType.Kaleido;
			}
		}

		public KaleidoWarp()
		{
			offset = Vector2.zero;
			sides = 6;
			rotation = 0f;
			offsetScale = 1f;
		}

		public KaleidoWarp(Vector2 offset, int sides, float rotation, float offsetScale)
		{
			this.offset = offset;
			this.sides = sides;
			this.rotation = rotation;
			this.offsetScale = offsetScale;
		}

		public override SignWarp Clone()
		{
			return new KaleidoWarp(offset, sides, rotation, offsetScale);
		}

		public override void WriteXml(XmlElement element)
		{
			element.SetAttrib("type", GetType().Name);
			element.SetAttrib("offset", $"{offset.x}, {offset.y}");
			element.SetAttrib("sides", $"{sides}");
			element.SetAttrib("rotation", rotation.ToCultureInvariantString() ?? "");
			element.SetAttrib("offsetScale", offsetScale.ToCultureInvariantString() ?? "");
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void ReadXml(XElement element)
		{
			offset = StringParsers.ParseVector2(element.GetAttribute("offset"));
			sides = int.Parse(element.GetAttribute("sides"));
			rotation = float.Parse(element.GetAttribute("rotation"), Utils.StandardCulture);
			offsetScale = (element.HasAttribute("offsetScale") ? float.Parse(element.GetAttribute("offsetScale"), Utils.StandardCulture) : 1f);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void InternalWrite(BinaryWriter bw)
		{
			StreamUtils.Write(bw, offset);
			bw.Write(sides);
			bw.Write(rotation);
			bw.Write(offsetScale);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void InternalRead(BinaryReader br)
		{
			offset = StreamUtils.ReadVector2(br);
			sides = br.ReadInt32();
			rotation = br.ReadSingle();
			offsetScale = br.ReadSingle();
		}
	}

	public class PerspectiveWarp : SignWarp
	{
		public Vector3 rotation;

		public float strength;

		public override WarpType TypeId
		{
			[PublicizedFrom(EAccessModifier.Protected)]
			get
			{
				return WarpType.Perspective;
			}
		}

		public PerspectiveWarp()
		{
			rotation = new Vector3(0f, 30f, 0f);
			strength = 0.1f;
		}

		public PerspectiveWarp(Vector3 rotation, float strength)
		{
			this.rotation = rotation;
			this.strength = strength;
		}

		public override SignWarp Clone()
		{
			return new PerspectiveWarp(rotation, strength);
		}

		public override void WriteXml(XmlElement element)
		{
			element.SetAttrib("type", GetType().Name);
			element.SetAttrib("rotation", $"{rotation.x}, {rotation.y}, {rotation.z}");
			element.SetAttrib("strength", strength.ToCultureInvariantString() ?? "");
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void ReadXml(XElement element)
		{
			rotation = StringParsers.ParseVector3(element.GetAttribute("rotation"));
			strength = float.Parse(element.GetAttribute("strength"), Utils.StandardCulture);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void InternalWrite(BinaryWriter bw)
		{
			StreamUtils.Write(bw, rotation);
			bw.Write(strength);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void InternalRead(BinaryReader br)
		{
			rotation = StreamUtils.ReadVector3(br);
			strength = br.ReadSingle();
		}
	}

	public class ArcWarp : SignWarp
	{
		public float rotation;

		public float radius;

		public float width;

		public override WarpType TypeId
		{
			[PublicizedFrom(EAccessModifier.Protected)]
			get
			{
				return WarpType.Arc;
			}
		}

		public ArcWarp()
		{
			rotation = 0f;
			radius = 2f;
			width = 0.1f;
		}

		public ArcWarp(float rotation, float radius, float width)
		{
			this.rotation = rotation;
			this.radius = radius;
			this.width = width;
		}

		public override SignWarp Clone()
		{
			return new ArcWarp(rotation, radius, width);
		}

		public override void WriteXml(XmlElement element)
		{
			element.SetAttrib("type", GetType().Name);
			element.SetAttrib("rotation", rotation.ToCultureInvariantString() ?? "");
			element.SetAttrib("radius", radius.ToCultureInvariantString() ?? "");
			element.SetAttrib("width", width.ToCultureInvariantString() ?? "");
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void ReadXml(XElement element)
		{
			rotation = float.Parse(element.GetAttribute("rotation"), Utils.StandardCulture);
			radius = float.Parse(element.GetAttribute("radius"), Utils.StandardCulture);
			width = float.Parse(element.GetAttribute("width"), Utils.StandardCulture);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void InternalWrite(BinaryWriter bw)
		{
			bw.Write(rotation);
			bw.Write(radius);
			bw.Write(width);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void InternalRead(BinaryReader br)
		{
			rotation = br.ReadSingle();
			radius = br.ReadSingle();
			width = br.ReadSingle();
		}
	}

	public class StretchWarp : SignWarp
	{
		public Vector2 offset;

		public float rotation;

		public float distance;

		public float width;

		public float exponent;

		public override WarpType TypeId
		{
			[PublicizedFrom(EAccessModifier.Protected)]
			get
			{
				return WarpType.Stretch;
			}
		}

		public StretchWarp()
		{
			offset = new Vector2(0f, 0f);
			rotation = 0f;
			distance = 0f;
			width = 0f;
			exponent = 0f;
		}

		public StretchWarp(Vector2 offset, float rotation, float distance, float width, float exponent)
		{
			this.offset = offset;
			this.rotation = rotation;
			this.distance = distance;
			this.width = width;
			this.exponent = exponent;
		}

		public override SignWarp Clone()
		{
			return new StretchWarp(offset, rotation, distance, width, exponent);
		}

		public override void WriteXml(XmlElement element)
		{
			element.SetAttrib("type", GetType().Name);
			element.SetAttrib("offset", $"{offset.x}, {offset.y}");
			element.SetAttrib("rotation", rotation.ToCultureInvariantString() ?? "");
			element.SetAttrib("distance", distance.ToCultureInvariantString() ?? "");
			element.SetAttrib("width", width.ToCultureInvariantString() ?? "");
			element.SetAttrib("exponent", exponent.ToCultureInvariantString() ?? "");
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void ReadXml(XElement element)
		{
			offset = StringParsers.ParseVector2(element.GetAttribute("offset"));
			rotation = float.Parse(element.GetAttribute("rotation"), Utils.StandardCulture);
			distance = float.Parse(element.GetAttribute("distance"), Utils.StandardCulture);
			width = float.Parse(element.GetAttribute("width"), Utils.StandardCulture);
			exponent = float.Parse(element.GetAttribute("exponent"), Utils.StandardCulture);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void InternalWrite(BinaryWriter bw)
		{
			StreamUtils.Write(bw, offset);
			bw.Write(rotation);
			bw.Write(distance);
			bw.Write(width);
			bw.Write(exponent);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void InternalRead(BinaryReader br)
		{
			offset = StreamUtils.ReadVector2(br);
			rotation = br.ReadSingle();
			distance = br.ReadSingle();
			width = br.ReadSingle();
			exponent = br.ReadSingle();
		}
	}

	public class GridWarp : SignWarp
	{
		public enum Mode
		{
			Column,
			Rectangle,
			Hex
		}

		public Mode mode;

		public Vector2 offset;

		public float rotation;

		public float scale;

		public float aspect;

		public float shift;

		public override WarpType TypeId
		{
			[PublicizedFrom(EAccessModifier.Protected)]
			get
			{
				return WarpType.Grid;
			}
		}

		public GridWarp()
		{
			mode = Mode.Rectangle;
			offset = Vector2.zero;
			rotation = 0f;
			scale = 1f;
			aspect = 1f;
			shift = 0f;
		}

		public GridWarp(Mode mode, Vector2 offset, float rotation, float scale, float aspect, float shift)
		{
			this.mode = mode;
			this.offset = offset;
			this.rotation = rotation;
			this.scale = scale;
			this.aspect = aspect;
			this.shift = shift;
		}

		public override SignWarp Clone()
		{
			return new GridWarp(mode, offset, rotation, scale, aspect, shift);
		}

		public override void WriteXml(XmlElement element)
		{
			element.SetAttrib("type", GetType().Name);
			element.SetAttrib("mode", $"{(int)mode}");
			element.SetAttrib("offset", $"{offset.x}, {offset.y}");
			element.SetAttrib("rotation", rotation.ToCultureInvariantString() ?? "");
			element.SetAttrib("scale", scale.ToCultureInvariantString() ?? "");
			element.SetAttrib("aspect", aspect.ToCultureInvariantString() ?? "");
			element.SetAttrib("shift", shift.ToCultureInvariantString() ?? "");
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void ReadXml(XElement element)
		{
			mode = (Mode)int.Parse(element.GetAttribute("mode"));
			offset = StringParsers.ParseVector2(element.GetAttribute("offset"));
			rotation = float.Parse(element.GetAttribute("rotation"), Utils.StandardCulture);
			scale = float.Parse(element.GetAttribute("scale"), Utils.StandardCulture);
			aspect = (element.HasAttribute("aspect") ? float.Parse(element.GetAttribute("aspect"), Utils.StandardCulture) : 1f);
			shift = (element.HasAttribute("shift") ? float.Parse(element.GetAttribute("shift"), Utils.StandardCulture) : 0f);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void InternalWrite(BinaryWriter bw)
		{
			bw.Write((int)mode);
			StreamUtils.Write(bw, offset);
			bw.Write(rotation);
			bw.Write(scale);
			bw.Write(aspect);
			bw.Write(shift);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void InternalRead(BinaryReader br)
		{
			mode = (Mode)br.ReadInt32();
			offset = StreamUtils.ReadVector2(br);
			rotation = br.ReadSingle();
			scale = br.ReadSingle();
			aspect = br.ReadSingle();
			shift = br.ReadSingle();
		}
	}

	public abstract class SignLayer
	{
		public enum LayerType : byte
		{
			Group,
			Text,
			Polygon,
			Noise
		}

		public string name;

		public SignTransform transform;

		public SignRenderSettings renderSettings;

		public List<SignWarp> warps;

		public bool HasWarps
		{
			get
			{
				List<SignWarp> list = warps;
				if (list == null)
				{
					return false;
				}
				return list.Count > 0;
			}
		}

		public bool HasTransform => transform != SignTransform.Identity;

		public bool HasTransformOrWarps
		{
			get
			{
				if (!HasTransform)
				{
					return HasWarps;
				}
				return true;
			}
		}

		public abstract LayerType TypeId
		{
			[PublicizedFrom(EAccessModifier.Protected)]
			get;
		}

		public abstract string defaultName
		{
			[PublicizedFrom(EAccessModifier.Protected)]
			get;
		}

		public abstract SignLayer Clone();

		[PublicizedFrom(EAccessModifier.Protected)]
		public abstract void InternalRead(BinaryReader br);

		[PublicizedFrom(EAccessModifier.Protected)]
		public abstract void InternalWrite(BinaryWriter bw);

		public SignLayer(string name, Vector2 position, float rotation, Vector2 scale, SignRenderSettings renderSettings, List<SignWarp> warps)
		{
			this.name = name;
			transform.position = position;
			transform.rotation = rotation;
			transform.scale = scale;
			this.renderSettings = renderSettings;
			this.warps = warps;
		}

		public SignLayer()
		{
			name = string.Empty;
			transform.position = Vector2.zero;
			transform.rotation = 0f;
			transform.scale = new Vector2(0.3f, 0.3f);
			renderSettings = new SignRenderSettings(Color.white);
			warps = new List<SignWarp>();
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public List<SignWarp> CloneWarps()
		{
			List<SignWarp> list = new List<SignWarp>();
			foreach (SignWarp warp in warps)
			{
				list.Add(warp.Clone());
			}
			return list;
		}

		public void Write(BinaryWriter bw)
		{
			bw.Write((byte)TypeId);
			InternalWrite(bw);
			bw.Write(name ?? "");
			transform.Write(bw);
			renderSettings.Write(bw);
			bw.Write(warps?.Count ?? 0);
			if (warps == null)
			{
				return;
			}
			foreach (SignWarp warp in warps)
			{
				warp.Write(bw);
			}
		}

		public static SignLayer Read(BinaryReader br)
		{
			LayerType layerType = (LayerType)br.ReadByte();
			SignLayer signLayer = layerType switch
			{
				LayerType.Text => new TextSignLayer(), 
				LayerType.Polygon => new PolygonSignLayer(), 
				LayerType.Group => new GroupSignLayer(), 
				LayerType.Noise => new NoiseSignLayer(), 
				_ => throw new Exception($"Unknown layer type id: {(byte)layerType}"), 
			};
			signLayer.InternalRead(br);
			signLayer.name = br.ReadString();
			signLayer.transform = SignTransform.Read(br);
			signLayer.renderSettings = SignRenderSettings.Read(br);
			int num = br.ReadInt32();
			signLayer.warps = new List<SignWarp>(num);
			for (int i = 0; i < num; i++)
			{
				signLayer.warps.Add(SignWarp.Read(br));
			}
			return signLayer;
		}

		public static SignLayer LayerFromXml(XElement element)
		{
			string attribute = element.GetAttribute("type");
			object obj = attribute switch
			{
				"TextSignLayer" => new TextSignLayer(), 
				"PolygonSignLayer" => new PolygonSignLayer(), 
				"GroupSignLayer" => new GroupSignLayer(), 
				"NoiseSignLayer" => new NoiseSignLayer(), 
				_ => throw new Exception("Unknown layer type: " + attribute), 
			};
			((SignLayer)obj).ReadXml(element);
			return (SignLayer)obj;
		}

		public virtual void WriteXml(XmlElement element)
		{
			element.SetAttribute("name", name);
			element.SetAttrib("type", GetType().Name);
			transform.WriteXml(element);
			renderSettings.WriteXml(element);
			foreach (SignWarp warp in warps)
			{
				warp.WriteXml(element.AddXmlElement("warp"));
			}
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public virtual void ReadXml(XElement element)
		{
			element.TryGetAttribute("name", out name);
			transform.ReadXml(element);
			renderSettings.ReadXml(element);
			foreach (XElement item in element.Elements("warp"))
			{
				warps.Add(SignWarp.WarpFromXml(item));
			}
		}

		public void SetDefaultName(int idx)
		{
			name = string.Format(defaultName + " " + idx.ToString("00"));
		}
	}

	public class GroupSignLayer : SignLayer
	{
		public enum OffsetTarget : byte
		{
			All,
			Shapes,
			Text,
			Noise
		}

		public enum ColorMode : byte
		{
			Multiply,
			Blend,
			Override
		}

		public List<SignLayer> layers;

		public OffsetTarget offsetTarget;

		public float softnessOffset;

		public float dilateOffset;

		public ColorMode colorMode;

		public override string defaultName
		{
			[PublicizedFrom(EAccessModifier.Protected)]
			get
			{
				return "Group";
			}
		}

		public override LayerType TypeId
		{
			[PublicizedFrom(EAccessModifier.Protected)]
			get
			{
				return LayerType.Group;
			}
		}

		public GroupSignLayer()
		{
		}

		public GroupSignLayer(string name, Vector2 position, float rotation, Vector2 scale, SignRenderSettings renderSettings, List<SignWarp> warps, List<SignLayer> layers, OffsetTarget offsetTarget = OffsetTarget.All, float softnessOffset = 0f, float dilateOffset = 0f, ColorMode colorMode = ColorMode.Multiply)
			: base(name, position, rotation, scale, renderSettings, warps)
		{
			this.layers = layers;
			this.offsetTarget = offsetTarget;
			this.softnessOffset = softnessOffset;
			this.dilateOffset = dilateOffset;
			this.colorMode = colorMode;
		}

		public override SignLayer Clone()
		{
			List<SignLayer> list = null;
			if (layers != null)
			{
				list = new List<SignLayer>();
				foreach (SignLayer layer in layers)
				{
					list.Add(layer.Clone());
				}
			}
			return new GroupSignLayer(name, transform.position, transform.rotation, transform.scale, renderSettings, CloneWarps(), list, offsetTarget, softnessOffset, dilateOffset, colorMode);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void InternalWrite(BinaryWriter bw)
		{
			bw.Write((byte)offsetTarget);
			bw.Write(softnessOffset);
			bw.Write(dilateOffset);
			bw.Write((byte)colorMode);
			bw.Write(layers?.Count ?? 0);
			if (layers == null)
			{
				return;
			}
			foreach (SignLayer layer in layers)
			{
				layer.Write(bw);
			}
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void InternalRead(BinaryReader br)
		{
			offsetTarget = (OffsetTarget)br.ReadByte();
			softnessOffset = br.ReadSingle();
			dilateOffset = br.ReadSingle();
			colorMode = (ColorMode)br.ReadByte();
			int num = br.ReadInt32();
			layers = new List<SignLayer>(num);
			for (int i = 0; i < num; i++)
			{
				layers.Add(SignLayer.Read(br));
			}
		}

		public override void WriteXml(XmlElement element)
		{
			base.WriteXml(element);
			if (offsetTarget != OffsetTarget.All)
			{
				element.SetAttrib("offsetTarget", offsetTarget.ToString());
			}
			if (softnessOffset != 0f)
			{
				element.SetAttrib("softnessOffset", softnessOffset.ToCultureInvariantString());
			}
			if (dilateOffset != 0f)
			{
				element.SetAttrib("dilateOffset", dilateOffset.ToCultureInvariantString());
			}
			if (colorMode != ColorMode.Multiply)
			{
				element.SetAttrib("colorMode", colorMode.ToString());
			}
			foreach (SignLayer layer in layers)
			{
				layer.WriteXml(element.AddXmlElement("layer"));
			}
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void ReadXml(XElement element)
		{
			base.ReadXml(element);
			if (element.TryGetAttribute("offsetTarget", out var _result) && Enum.TryParse<OffsetTarget>(_result, out var result))
			{
				offsetTarget = result;
			}
			else
			{
				offsetTarget = OffsetTarget.All;
			}
			if (element.TryGetAttribute("softnessOffset", out var _result2))
			{
				softnessOffset = float.Parse(_result2, Utils.StandardCulture);
			}
			if (element.TryGetAttribute("dilateOffset", out var _result3))
			{
				dilateOffset = float.Parse(_result3, Utils.StandardCulture);
			}
			if (element.TryGetAttribute("colorMode", out var _result4) && Enum.TryParse<ColorMode>(_result4, out var result2))
			{
				colorMode = result2;
			}
			else
			{
				colorMode = ColorMode.Multiply;
			}
			layers = new List<SignLayer>();
			foreach (XElement item in element.Elements("layer"))
			{
				layers.Add(SignLayer.LayerFromXml(item));
			}
		}
	}

	public class TextSignLayer : SignLayer
	{
		public string font;

		public string text;

		public float direction;

		public float spacing;

		public float softness;

		public float dilate;

		public override string defaultName
		{
			[PublicizedFrom(EAccessModifier.Protected)]
			get
			{
				return "Text";
			}
		}

		public override LayerType TypeId
		{
			[PublicizedFrom(EAccessModifier.Protected)]
			get
			{
				return LayerType.Text;
			}
		}

		public TextSignLayer()
		{
			name = "Text";
			font = "LiberationSans";
			text = Localization.Get("lblSignLayerTypeText");
			direction = 0f;
			spacing = 1f;
			softness = 0f;
			dilate = 0f;
		}

		public TextSignLayer(string name, Vector2 position, float rotation, Vector2 scale, SignRenderSettings renderSettings, List<SignWarp> warps, string font, string text, float direction, float spacing, float softness, float dilate)
			: base(name, position, rotation, scale, renderSettings, warps)
		{
			this.font = font;
			this.text = text;
			this.direction = direction;
			this.spacing = spacing;
			this.softness = softness;
			this.dilate = dilate;
		}

		public override SignLayer Clone()
		{
			return new TextSignLayer(name, transform.position, transform.rotation, transform.scale, renderSettings, CloneWarps(), font, text, direction, spacing, softness, dilate);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void InternalWrite(BinaryWriter bw)
		{
			bw.Write(font);
			bw.Write(text);
			bw.Write(direction);
			bw.Write(spacing);
			bw.Write(softness);
			bw.Write(dilate);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void InternalRead(BinaryReader br)
		{
			font = br.ReadString();
			text = br.ReadString();
			direction = br.ReadSingle();
			spacing = br.ReadSingle();
			softness = br.ReadSingle();
			dilate = br.ReadSingle();
		}

		public override void WriteXml(XmlElement element)
		{
			base.WriteXml(element);
			element.SetAttrib("font", font);
			element.SetAttrib("text", text);
			element.SetAttrib("direction", direction.ToCultureInvariantString());
			element.SetAttrib("spacing", spacing.ToCultureInvariantString());
			element.SetAttrib("softness", softness.ToCultureInvariantString());
			element.SetAttrib("dilate", dilate.ToCultureInvariantString());
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void ReadXml(XElement element)
		{
			base.ReadXml(element);
			font = element.GetAttribute("font");
			text = element.GetAttribute("text");
			direction = float.Parse(element.GetAttribute("direction"), Utils.StandardCulture);
			spacing = float.Parse(element.GetAttribute("spacing"), Utils.StandardCulture);
			softness = float.Parse(element.GetAttribute("softness"), Utils.StandardCulture);
			dilate = float.Parse(element.GetAttribute("dilate"), Utils.StandardCulture);
		}
	}

	public class PolygonSignLayer : SignLayer
	{
		public enum ShapeMode : byte
		{
			Normal,
			Invert,
			Line,
			Ripple
		}

		public int sides;

		public float smoothness;

		public float starify;

		public float softness;

		public float dilate;

		public float frequency;

		public ShapeMode shapeMode;

		public override string defaultName
		{
			[PublicizedFrom(EAccessModifier.Protected)]
			get
			{
				return "Poly";
			}
		}

		public override LayerType TypeId
		{
			[PublicizedFrom(EAccessModifier.Protected)]
			get
			{
				return LayerType.Polygon;
			}
		}

		public PolygonSignLayer()
		{
			name = "Polygon";
			sides = 4;
			smoothness = 0f;
			starify = 0f;
			softness = 0f;
			dilate = 0f;
			frequency = 5f;
			shapeMode = ShapeMode.Normal;
		}

		public PolygonSignLayer(string name, Vector2 position, float rotation, Vector2 scale, SignRenderSettings renderSettings, List<SignWarp> warps, int sides, float smoothness, float starify, float softness, float dilate, float frequency, ShapeMode shapeMode)
			: base(name, position, rotation, scale, renderSettings, warps)
		{
			this.sides = sides;
			this.smoothness = smoothness;
			this.starify = starify;
			this.softness = softness;
			this.dilate = dilate;
			this.frequency = frequency;
			this.shapeMode = shapeMode;
		}

		public override SignLayer Clone()
		{
			return new PolygonSignLayer(name, transform.position, transform.rotation, transform.scale, renderSettings, CloneWarps(), sides, smoothness, starify, softness, dilate, frequency, shapeMode);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void InternalWrite(BinaryWriter bw)
		{
			bw.Write(sides);
			bw.Write(smoothness);
			bw.Write(starify);
			bw.Write(softness);
			bw.Write(dilate);
			bw.Write(frequency);
			bw.Write((byte)shapeMode);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void InternalRead(BinaryReader br)
		{
			sides = br.ReadInt32();
			smoothness = br.ReadSingle();
			starify = br.ReadSingle();
			softness = br.ReadSingle();
			dilate = br.ReadSingle();
			frequency = br.ReadSingle();
			shapeMode = (ShapeMode)br.ReadByte();
		}

		public override void WriteXml(XmlElement element)
		{
			base.WriteXml(element);
			element.SetAttrib("sides", sides.ToString());
			element.SetAttrib("smoothness", smoothness.ToCultureInvariantString());
			element.SetAttrib("starify", starify.ToCultureInvariantString());
			element.SetAttrib("softness", softness.ToCultureInvariantString());
			element.SetAttrib("dilate", dilate.ToCultureInvariantString());
			element.SetAttrib("frequency", frequency.ToCultureInvariantString());
			element.SetAttrib("shapeMode", shapeMode.ToString());
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void ReadXml(XElement element)
		{
			base.ReadXml(element);
			sides = int.Parse(element.GetAttribute("sides"));
			smoothness = float.Parse(element.GetAttribute("smoothness"), Utils.StandardCulture);
			starify = float.Parse(element.GetAttribute("starify"), Utils.StandardCulture);
			if (element.TryGetAttribute("softness", out var _result) && element.TryGetAttribute("dilate", out var _result2))
			{
				softness = float.Parse(_result, Utils.StandardCulture);
				dilate = float.Parse(_result2, Utils.StandardCulture);
			}
			else
			{
				softness = 0f;
				dilate = 0f;
			}
			if (element.TryGetAttribute("frequency", out var _result3))
			{
				frequency = float.Parse(_result3, Utils.StandardCulture);
			}
			else
			{
				frequency = 5f;
			}
			if (element.TryGetAttribute("shapeMode", out var _result4) && Enum.TryParse<ShapeMode>(_result4, out var result))
			{
				shapeMode = result;
			}
			else
			{
				shapeMode = ShapeMode.Normal;
			}
		}
	}

	public class NoiseSignLayer : SignLayer
	{
		public int seed;

		public int detail;

		public float softness;

		public float dilate;

		public float fade;

		public override string defaultName
		{
			[PublicizedFrom(EAccessModifier.Protected)]
			get
			{
				return "Noise";
			}
		}

		public override LayerType TypeId
		{
			[PublicizedFrom(EAccessModifier.Protected)]
			get
			{
				return LayerType.Noise;
			}
		}

		public NoiseSignLayer()
		{
			name = "Noise";
			seed = 0;
			detail = 4;
			softness = 0f;
			dilate = 0f;
			fade = 0f;
		}

		public NoiseSignLayer(string name, Vector2 position, float rotation, Vector2 scale, SignRenderSettings renderSettings, List<SignWarp> warps, int seed, int detail, float softness, float dilate, float fade)
			: base(name, position, rotation, scale, renderSettings, warps)
		{
			this.seed = seed;
			this.detail = detail;
			this.softness = softness;
			this.dilate = dilate;
			this.fade = fade;
		}

		public override SignLayer Clone()
		{
			return new NoiseSignLayer(name, transform.position, transform.rotation, transform.scale, renderSettings, CloneWarps(), seed, detail, softness, dilate, fade);
		}

		public override void WriteXml(XmlElement element)
		{
			base.WriteXml(element);
			element.SetAttrib("seed", seed.ToString());
			element.SetAttrib("detail", detail.ToString());
			element.SetAttrib("softness", softness.ToCultureInvariantString());
			element.SetAttrib("dilate", dilate.ToCultureInvariantString());
			element.SetAttrib("fade", fade.ToCultureInvariantString());
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void ReadXml(XElement element)
		{
			base.ReadXml(element);
			seed = int.Parse(element.GetAttribute("seed"));
			detail = int.Parse(element.GetAttribute("detail"));
			softness = float.Parse(element.GetAttribute("softness"), Utils.StandardCulture);
			dilate = float.Parse(element.GetAttribute("dilate"), Utils.StandardCulture);
			if (element.TryGetAttribute("fade", out var _result))
			{
				fade = float.Parse(_result, Utils.StandardCulture);
			}
			else
			{
				fade = 0f;
			}
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void InternalWrite(BinaryWriter bw)
		{
			bw.Write(seed);
			bw.Write(detail);
			bw.Write(softness);
			bw.Write(dilate);
			bw.Write(fade);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void InternalRead(BinaryReader br)
		{
			seed = br.ReadInt32();
			detail = br.ReadInt32();
			softness = br.ReadSingle();
			dilate = br.ReadSingle();
			fade = br.ReadSingle();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static class DefaultSignData
	{
		public static readonly Color color1 = new Color(0.318f, 0.396f, 0.686f);

		public static readonly Color color2 = new Color(0.286f, 0.518f, 1f);

		public static readonly Color color3 = new Color(0.514f, 0.671f, 1f);

		public static readonly Vector2 textSize = 0.65f * Vector2.one;

		public const float textRotation = 8f;

		public const string font1 = "LiberationSans";

		public const string font2 = "AmaticSC";

		public const string text = "Hello World!";

		public const int polygonSides = 5;

		public const float polygonSmoothness = 0.2f;

		public const float polygonStarify = 0.5f;

		public static List<SignLayer> PolygonGroupLayers => new List<SignLayer>
		{
			new PolygonSignLayer("Polygon Sign Layer", Vector2.zero + new Vector2(0.005f, -0.005f), 0f, 0.25f * Vector2.one, new SignRenderSettings(Color.black), new List<SignWarp>(), 5, 0.2f, 0.5f, 0f, 0f, 20f, PolygonSignLayer.ShapeMode.Normal),
			new PolygonSignLayer("Polygon Sign Layer", Vector2.zero, 0f, 0.25f * Vector2.one, new SignRenderSettings(Color.white), new List<SignWarp>(), 5, 0.2f, 0.5f, 0f, 0f, 20f, PolygonSignLayer.ShapeMode.Normal),
			new PolygonSignLayer("Polygon Sign Layer", Vector2.zero, 0f, 0.23f * Vector2.one, new SignRenderSettings(Color.HSVToRGB(0.05f, 1f, 1f)), new List<SignWarp>(), 5, 0.165f, 0.5f, 0f, 0f, 20f, PolygonSignLayer.ShapeMode.Normal),
			new PolygonSignLayer("Polygon Sign Layer", Vector2.zero, 0f, 0.18f * Vector2.one, new SignRenderSettings(Color.HSVToRGB(0.13f, 1f, 1f)), new List<SignWarp>(), 5, 0.15f, 0.5f, 0f, 0f, 20f, PolygonSignLayer.ShapeMode.Normal)
		};
	}

	public Guid guid;

	public string name;

	public List<SignLayer> layers;

	public DateTime lastModified;

	[PublicizedFrom(EAccessModifier.Private)]
	public int nextPolyId;

	[PublicizedFrom(EAccessModifier.Private)]
	public int nextTextId;

	[PublicizedFrom(EAccessModifier.Private)]
	public int nextNoiseId;

	[PublicizedFrom(EAccessModifier.Private)]
	public int nextGroupId;

	public static readonly GlobalSignId defaultSignDataID = new GlobalSignId("[D]", Guid.Empty);

	[PublicizedFrom(EAccessModifier.Private)]
	public SignData()
	{
	}

	public static SignData Duplicate(SignData original, string copyName = "")
	{
		SignData signData = new SignData();
		signData.guid = Guid.NewGuid();
		signData.name = (string.IsNullOrEmpty(copyName) ? original.name : copyName);
		signData.layers = null;
		signData.lastModified = DateTime.Now;
		signData.nextPolyId = original.nextPolyId;
		signData.nextGroupId = original.nextGroupId;
		signData.nextTextId = original.nextTextId;
		signData.nextNoiseId = original.nextNoiseId;
		signData.CloneLayersFrom(original);
		return signData;
	}

	public void CloneLayersFrom(SignData source)
	{
		if (source.layers == null)
		{
			Log.Error("Failed to clone layers from source with null layers list.");
			return;
		}
		if (layers != null)
		{
			layers.Clear();
		}
		else
		{
			layers = new List<SignLayer>();
		}
		foreach (SignLayer layer in source.layers)
		{
			layers.Add(layer.Clone());
		}
		nextPolyId = source.nextPolyId;
		nextTextId = source.nextTextId;
		nextNoiseId = source.nextNoiseId;
		nextGroupId = source.nextGroupId;
		lastModified = DateTime.Now;
	}

	public void Write(BinaryWriter bw)
	{
		StreamUtils.Write(bw, guid);
		bw.Write(name);
		bw.Write(lastModified.ToUniversalTime().Ticks);
		bw.Write(nextPolyId);
		bw.Write(nextTextId);
		bw.Write(nextNoiseId);
		bw.Write(nextGroupId);
		bw.Write(layers?.Count ?? 0);
		if (layers == null)
		{
			return;
		}
		foreach (SignLayer layer in layers)
		{
			layer.Write(bw);
		}
	}

	public static SignData Read(BinaryReader br)
	{
		SignData signData = new SignData
		{
			guid = StreamUtils.ReadGuid(br),
			name = br.ReadString(),
			lastModified = new DateTime(br.ReadInt64(), DateTimeKind.Utc)
		};
		signData.nextPolyId = br.ReadInt32();
		signData.nextTextId = br.ReadInt32();
		signData.nextNoiseId = br.ReadInt32();
		signData.nextGroupId = br.ReadInt32();
		int num = br.ReadInt32();
		signData.layers = new List<SignLayer>(num);
		for (int i = 0; i < num; i++)
		{
			signData.layers.Add(SignLayer.Read(br));
		}
		return signData;
	}

	public void WriteXml(XmlElement parentElement)
	{
		XmlElement xmlElement = parentElement.AddXmlElement("sign");
		xmlElement.SetAttrib("guid", guid.ToString());
		xmlElement.SetAttrib("name", name.ToString());
		xmlElement.SetAttrib("modified", lastModified.ToString("u", Utils.StandardCulture));
		xmlElement.SetAttribute("next_poly_id", nextPolyId.ToString());
		xmlElement.SetAttribute("next_text_id", nextTextId.ToString());
		xmlElement.SetAttribute("next_noise_id", nextNoiseId.ToString());
		xmlElement.SetAttribute("next_group_id", nextGroupId.ToString());
		foreach (SignLayer layer in layers)
		{
			layer.WriteXml(xmlElement.AddXmlElement("layer"));
		}
	}

	public static SignData ReadXML(XElement signElement)
	{
		SignData signData = new SignData
		{
			guid = Guid.Parse(signElement.GetAttribute("guid")),
			name = signElement.GetAttribute("name"),
			lastModified = DateTime.ParseExact(signElement.GetAttribute("modified"), "u", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal),
			layers = new List<SignLayer>()
		};
		signData.nextPolyId = ReadNextIdAttribute("next_poly_id");
		signData.nextTextId = ReadNextIdAttribute("next_text_id");
		signData.nextNoiseId = ReadNextIdAttribute("next_noise_id");
		signData.nextGroupId = ReadNextIdAttribute("next_group_id");
		foreach (XElement item in signElement.Elements("layer"))
		{
			signData.layers.Add(SignLayer.LayerFromXml(item));
		}
		return signData;
		[PublicizedFrom(EAccessModifier.Internal)]
		int ReadNextIdAttribute(string attribute)
		{
			if (signElement.TryGetAttribute(attribute, out var _result))
			{
				return StringParsers.ParseSInt32(_result);
			}
			return 0;
		}
	}

	public void SetLayerDefaultName(SignLayer layer)
	{
		if (!(layer is PolygonSignLayer polygonSignLayer))
		{
			if (!(layer is TextSignLayer textSignLayer))
			{
				if (!(layer is NoiseSignLayer noiseSignLayer))
				{
					if (layer is GroupSignLayer groupSignLayer)
					{
						nextGroupId++;
						groupSignLayer.SetDefaultName(nextGroupId);
					}
				}
				else
				{
					nextNoiseId++;
					noiseSignLayer.SetDefaultName(nextNoiseId);
				}
			}
			else
			{
				nextTextId++;
				textSignLayer.SetDefaultName(nextTextId);
			}
		}
		else
		{
			nextPolyId++;
			polygonSignLayer.SetDefaultName(nextPolyId);
		}
	}

	public void UnpackGroups(bool recursive)
	{
		for (int num = layers.Count - 1; num >= 0; num--)
		{
			TryUnpackTopLevelGroup(num, recursive, out var _);
		}
	}

	public bool TryUnpackTopLevelGroup(int groupIndex, bool recursive, out int unpackedLayerCount)
	{
		unpackedLayerCount = 0;
		if (layers[groupIndex] is GroupSignLayer groupSignLayer)
		{
			layers.RemoveAt(groupIndex);
			UnpackLayers(groupSignLayer.transform, groupSignLayer.layers, layers, groupIndex, recursive, ref unpackedLayerCount);
			return true;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UnpackLayers(SignTransform parentTransform, List<SignLayer> sourceLayers, List<SignLayer> destLayers, int destIndex, bool recursive, ref int unpackedLayerCount)
	{
		for (int num = sourceLayers.Count - 1; num >= 0; num--)
		{
			SignLayer signLayer = sourceLayers[num];
			sourceLayers.RemoveAt(num);
			if (recursive && signLayer is GroupSignLayer groupSignLayer)
			{
				UnpackLayers(parentTransform * groupSignLayer.transform, groupSignLayer.layers, destLayers, destIndex, recursive, ref unpackedLayerCount);
			}
			else
			{
				signLayer.transform = parentTransform * signLayer.transform;
				destLayers.Insert(destIndex, signLayer);
				unpackedLayerCount++;
			}
		}
	}

	public static SignData GetTestSignData(string text = "Hello World!", float hueOffset = 0f)
	{
		SignData signData = new SignData();
		signData.guid = Guid.NewGuid();
		signData.name = text;
		List<SignLayer> list = new List<SignLayer>();
		for (int i = 0; i < 20; i++)
		{
			float t = (float)i / 20f;
			float rotation = 8f + Mathf.Lerp(360f, 0f, t);
			float h = (Mathf.Lerp(1f, 0f, t) + hueOffset) % 1f;
			list.Add(new TextSignLayer("Text Sign Layer", Vector2.zero, rotation, DefaultSignData.textSize, new SignRenderSettings(Color.HSVToRGB(h, 1f, 1f)), new List<SignWarp>(), "LiberationSans", text, 0f, 0f, 0f, 0.5f));
		}
		list.Add(new GroupSignLayer("Group Sign Layer", new Vector2(-1f, 0.3f), 8f, Vector2.one, new SignRenderSettings(Color.white), new List<SignWarp>(), DefaultSignData.PolygonGroupLayers));
		list.Add(new GroupSignLayer("Group Sign Layer", new Vector2(-0.7f, 0.45f), 8f, 0.5f * Vector2.one, new SignRenderSettings(Color.white), new List<SignWarp>(), DefaultSignData.PolygonGroupLayers));
		list.Add(new GroupSignLayer("Group Sign Layer", new Vector2(0.5f, -0.5f), 8f, 1.25f * Vector2.one, new SignRenderSettings(Color.white), new List<SignWarp>(), DefaultSignData.PolygonGroupLayers));
		list.Add(new TextSignLayer("Text Sign Layer", new Vector2(0.025f, -0.025f), 8f, DefaultSignData.textSize, new SignRenderSettings(Color.black), new List<SignWarp>(), "LiberationSans", text, 0f, 0f, 1f, 0.25f));
		list.Add(new TextSignLayer("Text Sign Layer", Vector2.zero, 8f, DefaultSignData.textSize, new SignRenderSettings(Color.white), new List<SignWarp>(), "LiberationSans", text, 0f, 0f, 0f, 0.5f));
		list.Add(new TextSignLayer("Text Sign Layer", Vector2.zero, 8f, DefaultSignData.textSize, new SignRenderSettings(Color.HSVToRGB(0f + hueOffset, 1f, 1f)), new List<SignWarp>(), "LiberationSans", text, 0f, 0f, 0f, 0.15f));
		list.Add(new TextSignLayer("Text Sign Layer", new Vector2(-0.005f, 0.005f), 8f, DefaultSignData.textSize, new SignRenderSettings(Color.HSVToRGB(0.05f + hueOffset, 0.9f, 1f)), new List<SignWarp>(), "LiberationSans", text, 0f, 0f, 1f, -0.5f));
		signData.layers = new List<SignLayer>
		{
			new GroupSignLayer("Group Sign Layer", Vector2.zero, 0f, 0.55f * Vector2.one, new SignRenderSettings(Color.white), new List<SignWarp>(), list)
		};
		signData.lastModified = DateTime.Now;
		return signData;
	}

	public static SignData GetBasicTestSignData(string text = "Hello World!", float hueOffset = 0f)
	{
		return new SignData
		{
			guid = Guid.NewGuid(),
			name = text,
			layers = new List<SignLayer>
			{
				new PolygonSignLayer("Polygon Sign Layer", Vector2.zero, 0f, 0.85f * Vector2.one, new SignRenderSettings(Color.HSVToRGB(0.58f + hueOffset, 1f, 1f)), new List<SignWarp>(), 8, 0f, 0f, 0f, 0f, 20f, PolygonSignLayer.ShapeMode.Normal),
				new TextSignLayer("Text Sign Layer", Vector2.zero, 8f, 0.55f * DefaultSignData.textSize, new SignRenderSettings(Color.HSVToRGB(0.16f + hueOffset, 1f, 1f)), new List<SignWarp>(), "LiberationSans", text, 0f, 0f, 0f, 0.15f)
			},
			lastModified = DateTime.Now
		};
	}

	public static SignData GetErrorSignData()
	{
		return new SignData
		{
			guid = Guid.NewGuid(),
			name = "Error",
			layers = new List<SignLayer>
			{
				new PolygonSignLayer("Polygon Sign Layer", Vector2.zero, 0f, 3f * Vector2.one, new SignRenderSettings(new Color(1f, 0f, 1f)), new List<SignWarp>(), 4, 1f, 0f, 0f, 0f, 20f, PolygonSignLayer.ShapeMode.Normal)
			},
			lastModified = DateTime.Now
		};
	}

	public static SignData GetNewSignData(string signName)
	{
		return new SignData
		{
			guid = Guid.NewGuid(),
			name = signName,
			layers = new List<SignLayer>(),
			lastModified = DateTime.Now
		};
	}

	public static SignData GetDefaultSignData()
	{
		SignData testSignData = GetTestSignData();
		testSignData.guid = defaultSignDataID.signGuid;
		return testSignData;
	}
}
