using UnityEngine;

internal static class DayNightBindingUtil
{
    private static int snapshotFrame = -1;
    private static string isDay = "false";
    private static string isNight = "false";

    public static string GetIsDay()
    {
        Refresh();
        return isDay;
    }

    public static string GetIsNight()
    {
        Refresh();
        return isNight;
    }

    private static void Refresh()
    {
        int frame = Time.frameCount;
        if (snapshotFrame == frame)
        {
            return;
        }

        snapshotFrame = frame;
        World world = GameManager.Instance?.World;
        if (world == null)
        {
            isDay = "false";
            isNight = "false";
            return;
        }

        bool day = world.IsDaytime();
        isDay = day ? "true" : "false";
        isNight = day ? "false" : "true";
    }
}