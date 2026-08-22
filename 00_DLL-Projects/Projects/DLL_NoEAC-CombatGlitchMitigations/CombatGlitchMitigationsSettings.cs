namespace CombatGlitchMitigations
{
	/// <summary>
	/// v1 toggles. Hooks come after decompile; these only record what we intend to ship.
	/// </summary>
	public static class CombatGlitchMitigationsSettings
	{
		public static bool BlockPunchThroughClosedBarrier = true;
		public static bool BlockPunchWithNoSwing = true;
		public static bool RecheckReachWhenPunchLands = true;
		public static bool SnapOutOfFloorAndWalls = true;
		public static bool DebugDrawReach = false;

		/// <summary>
		/// v1 #5: face-hit pain anim (hands on face, or legs buckle) must not walk them closer.
		/// They do not punch during that anim. Not PainResistPerHit.
		/// </summary>
		public static bool BlockFaceHitStaggerTowardPlayer = true;

		/// <summary>
		/// v1 #6: player swing through loose grass/plant if the zombie is on this ray.
		/// </summary>
		public static bool PlayerSwingThroughLoosePlants = true;

		/// <summary>
		/// v1 #7: get-up empty hitbox — count melee and ranged through chest/pelvis. Do not re-enable colliders.
		/// </summary>
		public static bool CountHitsDuringGetUp = true;
	}
}
