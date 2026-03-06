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

public class PlayerStats : XUiC_PlayerStats
   {
      public override void Init()
      {
         base.Init();
         Log.Warning("[AGF DEBUG] PlayerStats.Init called.");
      }

      public override void Update(float _dt)
      {
         base.Update(_dt);
         Log.Warning("[AGF DEBUG] PlayerStats.Update called.");
      }

      public PlayerStats()
      {
         Log.Warning("[AGF DEBUG] PlayerStats controller instantiated.");
      }
   }