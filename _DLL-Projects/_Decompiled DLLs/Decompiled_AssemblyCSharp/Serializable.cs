public interface Serializable
{
	bool IsDirty { get; set; }

	byte[] Serialize();
}
