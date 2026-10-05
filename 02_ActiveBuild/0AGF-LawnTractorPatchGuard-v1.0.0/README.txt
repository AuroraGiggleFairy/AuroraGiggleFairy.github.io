0AGF-LawnTractorPatchGuard

This folder loads before OCB Lawn Mowing.
It is packed inside the Lawn Tractor V3 Fix zip. It is not its own mod.

On 7d2d 3.3, OCB crashes because Vehicle.SetItemValueMods was removed.
This patch skips that one OCB patch so the game can start.
On 3.2 that method still exists, so this patch leaves OCB alone.
