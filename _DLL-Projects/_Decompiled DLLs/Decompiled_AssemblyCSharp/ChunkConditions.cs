public static class ChunkConditions
{
	public delegate bool Delegate(Chunk chunk);

	public static readonly Delegate Decorated = [PublicizedFrom(EAccessModifier.Internal)] (Chunk chunk) => !chunk.NeedsDecoration && !chunk.NeedsLightCalculation;

	public static readonly Delegate MeshesCopied = [PublicizedFrom(EAccessModifier.Internal)] (Chunk chunk) => !chunk.InProgressDecorating && !chunk.InProgressLighting && !chunk.InProgressRegeneration && !chunk.InProgressCopying && !chunk.NeedsDecoration && !chunk.NeedsLightCalculation && !chunk.NeedsRegeneration && !chunk.NeedsCopying;

	public static readonly Delegate Displayed = [PublicizedFrom(EAccessModifier.Internal)] (Chunk chunk) => MeshesCopied(chunk) && chunk.displayState == Chunk.DisplayState.Done;
}
