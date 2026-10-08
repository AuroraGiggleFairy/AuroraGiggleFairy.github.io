namespace VisualEntityTrackerAddon
{
    public static class VisualEntityTrackerPresence
    {
        private const string VetModName = "AGF-2VisualEntityTracker";
        private static bool resolved;
        private static bool present;

        public static bool IsPresent()
        {
            if (resolved)
            {
                return present;
            }

            try
            {
                present = ModManager.GetMod(VetModName) != null;
            }
            catch
            {
                present = false;
            }

            resolved = true;
            return present;
        }
    }
}
