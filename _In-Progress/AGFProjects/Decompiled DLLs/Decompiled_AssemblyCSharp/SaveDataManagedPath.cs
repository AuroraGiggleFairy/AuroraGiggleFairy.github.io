using System;
using System.Collections.Generic;
using System.IO;

public class SaveDataManagedPath : IEquatable<SaveDataManagedPath>, IComparable<SaveDataManagedPath>, IComparable
{
	public static readonly SaveDataManagedPath RootPath = new SaveDataManagedPath(string.Empty);

	public readonly string PathRelativeToRoot;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public SaveDataType Type { get; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public Range SlotPathRange
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
	}

	public StringSpan SlotPath
	{
		get
		{
			string pathRelativeToRoot = PathRelativeToRoot;
			Range slotPathRange = SlotPathRange;
			return pathRelativeToRoot[slotPathRange.Start..slotPathRange.End];
		}
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public SaveDataSlot Slot { get; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public Range PathRelativeToSlotRange
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
	}

	public StringSpan PathRelativeToSlot
	{
		get
		{
			string pathRelativeToRoot = PathRelativeToRoot;
			Range pathRelativeToSlotRange = PathRelativeToSlotRange;
			return pathRelativeToRoot[pathRelativeToSlotRange.Start..pathRelativeToSlotRange.End];
		}
	}

	public SaveDataManagedPath(StringSpan pathRelativeToRoot)
		: this(TryFormatPath(pathRelativeToRoot.AsSpan(), out var formattedPath) ? formattedPath : pathRelativeToRoot.ToString(), alreadyFormatted: true)
	{
	}

	public SaveDataManagedPath(string pathRelativeToRoot)
		: this(pathRelativeToRoot, alreadyFormatted: false)
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public SaveDataManagedPath(string pathRelativeToRoot, bool alreadyFormatted)
	{
		if (pathRelativeToRoot == null)
		{
			throw new ArgumentNullException("pathRelativeToRoot");
		}
		if (alreadyFormatted)
		{
			PathRelativeToRoot = pathRelativeToRoot;
		}
		else
		{
			PathRelativeToRoot = (TryFormatPath(pathRelativeToRoot, out var formattedPath) ? formattedPath : pathRelativeToRoot);
		}
		bool flag;
		try
		{
			flag = Path.IsPathRooted(PathRelativeToRoot);
		}
		catch (ArgumentException innerException)
		{
			throw new ArgumentException("Failed to check if path was rooted. " + PathRelativeToRoot, "pathRelativeToRoot", innerException);
		}
		if (flag)
		{
			throw new ArgumentException("Path should not be rooted. " + PathRelativeToRoot, "pathRelativeToRoot");
		}
		Type = GetSaveDataType();
		SlotPathRange = GetSlotPathRange();
		PathRelativeToSlotRange = GetPathRelativeToSlotRange();
		Slot = new SaveDataSlot(this);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public unsafe static bool TryFormatPath(ReadOnlySpan<char> unformattedPath, out string formattedPath)
	{
		ReadOnlySpan<char> readOnlySpan = unformattedPath.Trim(" \\/");
		if (readOnlySpan.Length <= 0)
		{
			formattedPath = string.Empty;
			return true;
		}
		bool flag = false;
		int num = 0;
		bool flag2 = false;
		ReadOnlySpan<char> readOnlySpan2 = readOnlySpan;
		for (int i = 0; i < readOnlySpan2.Length; i++)
		{
			char c = readOnlySpan2[i];
			if (c != '/')
			{
				if (c != '\\')
				{
					flag2 = false;
					continue;
				}
				flag = true;
			}
			if (!flag2)
			{
				flag2 = true;
			}
			else
			{
				num++;
			}
		}
		if (readOnlySpan.Length == unformattedPath.Length && !flag && num == 0)
		{
			formattedPath = null;
			return false;
		}
		fixed (char* ptr = readOnlySpan)
		{
			void* ptr2 = ptr;
			formattedPath = string.Create(state: ((IntPtr)ptr2, readOnlySpan.Length), length: readOnlySpan.Length - num, action: [PublicizedFrom(EAccessModifier.Internal)] (Span<char> span, (IntPtr, int Length) data) =>
			{
				(IntPtr, int Length) tuple = data;
				IntPtr item = tuple.Item1;
				int item2 = tuple.Length;
				ReadOnlySpan<char> readOnlySpan3 = new ReadOnlySpan<char>(item.ToPointer(), item2);
				int num2 = 0;
				bool flag3 = false;
				ReadOnlySpan<char> readOnlySpan4 = readOnlySpan3;
				for (int j = 0; j < readOnlySpan4.Length; j++)
				{
					char c2 = readOnlySpan4[j];
					if (c2 == '\\' || c2 == '/')
					{
						if (!flag3)
						{
							flag3 = true;
							span[num2++] = '/';
						}
					}
					else
					{
						flag3 = false;
						span[num2++] = c2;
					}
				}
			});
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public SaveDataType GetSaveDataType()
	{
		foreach (SaveDataType item in EnumUtils.Values<SaveDataType>())
		{
			if (!item.IsRoot())
			{
				string pathRaw = item.GetPathRaw();
				if (PathRelativeToRoot.IndexOf(pathRaw, StringComparison.Ordinal) == 0 && PathRelativeToRoot.Length >= pathRaw.Length + 2 && PathRelativeToRoot[pathRaw.Length] == '/')
				{
					return item;
				}
			}
		}
		return SaveDataType.User;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Range GetSlotPathRange()
	{
		int num = Type.GetSlotPathDepth();
		string pathRaw = Type.GetPathRaw();
		if (num <= 0 || pathRaw.Length == 0 || PathRelativeToRoot.Length < pathRaw.Length + 2)
		{
			return pathRaw.Length..pathRaw.Length;
		}
		for (int i = pathRaw.Length + 1; i < PathRelativeToRoot.Length; i++)
		{
			if (PathRelativeToRoot[i] == '/')
			{
				num--;
				if (num <= 0)
				{
					int num2 = pathRaw.Length + 1;
					int num3 = i;
					return num2..num3;
				}
			}
		}
		return pathRaw.Length..pathRaw.Length;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Range GetPathRelativeToSlotRange()
	{
		int offset = SlotPathRange.End.GetOffset(PathRelativeToRoot.Length);
		if (offset >= PathRelativeToRoot.Length)
		{
			return offset..offset;
		}
		int num = ((PathRelativeToRoot[offset] == '/') ? (offset + 1) : offset);
		if (num >= PathRelativeToRoot.Length)
		{
			return offset..offset;
		}
		return num..PathRelativeToRoot.Length;
	}

	public string GetOriginalPath()
	{
		return GameIO.GetNormalizedPath(Path.Combine(SaveDataUtils.s_saveDataRootPathPrefix, PathRelativeToRoot));
	}

	public SaveDataManagedPath GetChildPath(StringSpan childPath)
	{
		return new SaveDataManagedPath(SpanUtils.Concat(PathRelativeToRoot, "/", childPath));
	}

	public bool TryGetParentPath(out SaveDataManagedPath parentPath)
	{
		if (PathRelativeToRoot.Length <= 0)
		{
			parentPath = null;
			return false;
		}
		int num = PathRelativeToRoot.LastIndexOf('/');
		if (num < 0)
		{
			parentPath = RootPath;
			return true;
		}
		parentPath = new SaveDataManagedPath(PathRelativeToRoot.Substring(0, num));
		return true;
	}

	public bool IsParentOf(SaveDataManagedPath childPath)
	{
		return IsParentOfInternal(PathRelativeToRoot, childPath.PathRelativeToRoot);
		[PublicizedFrom(EAccessModifier.Internal)]
		static bool IsParentOfInternal(string parent, string child)
		{
			if (parent.Length >= child.Length)
			{
				return false;
			}
			if (parent.Length == 0)
			{
				return true;
			}
			if (child[parent.Length] != '/')
			{
				return false;
			}
			for (int i = 0; i < parent.Length; i++)
			{
				if (parent[i] != child[i])
				{
					return false;
				}
			}
			return true;
		}
	}

	public override string ToString()
	{
		return PathRelativeToRoot;
	}

	public bool Equals(SaveDataManagedPath other)
	{
		if ((object)other == null)
		{
			return false;
		}
		if ((object)this == other)
		{
			return true;
		}
		return PathRelativeToRoot == other.PathRelativeToRoot;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (this == obj)
		{
			return true;
		}
		if (obj.GetType() != GetType())
		{
			return false;
		}
		return Equals((SaveDataManagedPath)obj);
	}

	public override int GetHashCode()
	{
		return PathRelativeToRoot.GetHashCode();
	}

	public static bool operator ==(SaveDataManagedPath left, SaveDataManagedPath right)
	{
		return object.Equals(left, right);
	}

	public static bool operator !=(SaveDataManagedPath left, SaveDataManagedPath right)
	{
		return !object.Equals(left, right);
	}

	public int CompareTo(SaveDataManagedPath other)
	{
		if ((object)other == null)
		{
			return 1;
		}
		if ((object)this == other)
		{
			return 0;
		}
		return string.Compare(PathRelativeToRoot, other.PathRelativeToRoot, StringComparison.Ordinal);
	}

	public int CompareTo(object obj)
	{
		if (obj == null)
		{
			return 1;
		}
		if (this == obj)
		{
			return 0;
		}
		if (!(obj is SaveDataManagedPath other))
		{
			throw new ArgumentException("Object must be of type SaveDataManagedPath");
		}
		return CompareTo(other);
	}

	public static bool operator <(SaveDataManagedPath left, SaveDataManagedPath right)
	{
		return Comparer<SaveDataManagedPath>.Default.Compare(left, right) < 0;
	}

	public static bool operator >(SaveDataManagedPath left, SaveDataManagedPath right)
	{
		return Comparer<SaveDataManagedPath>.Default.Compare(left, right) > 0;
	}

	public static bool operator <=(SaveDataManagedPath left, SaveDataManagedPath right)
	{
		return Comparer<SaveDataManagedPath>.Default.Compare(left, right) <= 0;
	}

	public static bool operator >=(SaveDataManagedPath left, SaveDataManagedPath right)
	{
		return Comparer<SaveDataManagedPath>.Default.Compare(left, right) >= 0;
	}
}
