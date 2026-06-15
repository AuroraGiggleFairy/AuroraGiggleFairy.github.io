# Boat Seat Axis And Rotation Direction Guide

This guide is for the current boat seat setup and uses seated forward as the reference.

## Reference Frame

- Forward means where the seated body is facing after seat rotation is applied.
- Left and right mean the character's own left and right (not camera left/right).
- For any "move opposite" result, use the same axis and decrease instead of increase.

## Direction Table (Increase Value)

| Row | Left Side (Hand/Foot) | Right Side (Hand/Foot) | Whole Body (Seat Position/Rotation) | What It Does |
|---|---|---|---|---|
| X axis | Moves inward toward center | Moves outward from center | Moves body to the right side | Side placement |
| Y axis | Moves up | Moves up | Moves body up | Height |
| Z axis | Moves forward/away | Moves forward/away | Moves body forward/away | Forward depth |
| X rotation | Pitches limb down (front edge dips) | Pitches limb down (front edge dips) | Leans body forward | Pitch |
| Y rotation | Turns limb inward toward torso | Turns limb outward away from torso | Turns body to its left | Yaw |
| Z rotation | Rolls wrist/ankle clockwise (from character view) | Rolls wrist/ankle counterclockwise (from character view) | Tilts body to its right shoulder | Roll |

## Fast Practical Rules

- If seated too far back, increase seat Z.
- If right-side limb is too far from body, decrease its X.
- If left-side limb is too far from body, increase its X.
- Use position first, then rotation.

## Your Current Seat1 Example

Current body position:

- 0.441, -0.407, -1.451

Move body forward:

- 0.441, -0.407, -1.401
