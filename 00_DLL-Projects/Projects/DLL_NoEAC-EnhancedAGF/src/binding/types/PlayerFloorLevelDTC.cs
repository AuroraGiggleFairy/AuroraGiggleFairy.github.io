/*Copyright 2021 Christopher Beda

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

   http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.*/

using StatControllers;

public class PlayerFloorLevelDTC : Binding
{
    private const float FirstLower = 62f;
    private const float FirstUpper = 67f;
    private const float Step = 6f;
    private const int MaxLevel = 14;

    public PlayerFloorLevelDTC(int value, string name) : base(value, name)
    {
    }

    public override string GetCurrentValue(EntityPlayer player)
    {
        int level = GetLevel(player.position.y);
        return Localization.Get(string.Concat("player_floor_level_", level));
    }

    private static int GetLevel(float elevation)
    {
        if (elevation <= FirstLower)
        {
            return 0;
        }

        if (elevation <= FirstUpper)
        {
            return 1;
        }

        for (int level = 1; level <= MaxLevel; level++)
        {
            float lower;
            float upper;
            if (level == 1)
            {
                lower = FirstLower;
                upper = FirstUpper;
            }
            else
            {
                lower = FirstUpper + (level - 2) * Step;
                upper = FirstUpper + (level - 1) * Step;
            }

            if (elevation > lower && elevation <= upper)
            {
                return level;
            }
        }

        return MaxLevel;
    }
}
