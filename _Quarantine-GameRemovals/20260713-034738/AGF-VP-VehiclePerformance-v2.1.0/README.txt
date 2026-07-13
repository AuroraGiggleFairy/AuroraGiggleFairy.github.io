========================================================================
                       AGF-VP-VEHICLEPERFORMANCE                        
========================================================================

Server-focused vehicle tuning boosts speed, accel, brakes, and handling.


NOTE: AGF Mod Guide and Changelog are further below.


------------------------------------------------------------------------
MOD SCOPE
------------------------------------------------------------------------

  - Mod Version: 2.1.0
  - 7d2d Version: 3
  - Website: https://auroragigglefairy.github.io/
  - Languages Supported: All 13 game-supported languages.
  - Mod Type: Server-Side (EAC-Friendly)
    - Server install works for all joining players.
    - EAC can be on or off.
    - Also works in singleplayer.
  - Safe to install on existing game: Yes (Safe)
  - Safe to remove from existing game: Yes (Safe)
  - Dependencies: None, works standalone.


------------------------------------------------------------------------
FEATURES
------------------------------------------------------------------------

  - All vehicles are tuned for stronger acceleration, braking, handling,
    and hill-climbing.
  - Gyrocopter reverse is smoother.
  - Speed increases are conservative to support server performance.
  - Top speeds (vanilla -> mod): bicycle 8.5 -> 10, minibike 9.2 -> 12,
    motorcycle 14 -> 16, 4x4 14 -> 16, gyrocopter air 15 -> 16.


------------------------------------------------------------------------
------------------------------------------------------------------------
OTHER DETAILS
------------------------------------------------------------------------

- NOTE: Speed values use this order: (a, b, c, d) a = forward max speed
  b = reverse max speed c = forward sprint max speed d = reverse sprint
  max speed

            - Bicycle
            - Speed (velocityMax_turbo): 6,4,8.5,4 -> 8,4,10,4
            - Braking (brakeTorque): 3000 -> 5000
            - Hill climb (upAngleMax): 70 -> 80
            - Handling: tiltDampening 0.22 -> 0.5, tiltThreshold 3 -> 5,
              tiltDampenThreshold 8 -> 80

            - Minibike
            - Speed (velocityMax_turbo): 7,4,9.2,4 -> 10,4,12,4
            - Acceleration (motorTorque_turbo): 400,200,560,200 ->
              900,300,1150,300
            - Braking (brakeTorque): 3000 -> 6000
            - Hill climb (upAngleMax): 70 -> 80
            - Handling: tiltDampening 0.22 -> 0.5, tiltThreshold 3 -> 5,
              tiltDampenThreshold 8 -> 80

            - Motorcycle
            - Speed (velocityMax_turbo): 9.8,6,14,8 -> 12,8,16,10
            - Acceleration (motorTorque_turbo): 1400,500,2100,650 ->
              5000,500,6000,650
            - Braking (brakeTorque): 3000 -> 10000
            - Hill climb (upAngleMax): 70 -> 90
            - Handling: tiltDampening 0.22 -> 0.5, tiltThreshold 3 -> 5,
              tiltDampenThreshold 8 -> 80

            - 4x4 Truck
            - Speed (velocityMax_turbo): 10,8,14,10 -> 12,8,16,10
            - Acceleration (motorTorque_turbo): 3500,1500,4500,2000 ->
              6000,1500,7500,2000
            - Braking (brakeTorque): 6000 -> 10000
            - Hill climb (upAngleMax): 70 -> 90

            - Gyrocopter
            - Speed (velocityMax_turbo): 9,9,15,9 -> 10,4,16,6
            - Acceleration/reverse torque (motorTorque_turbo): 1,1,2,2
              -> 10,100,10,100
            - Reverse behavior is tuned to feel smoother.
