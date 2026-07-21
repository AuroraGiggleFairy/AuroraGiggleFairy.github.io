# EnhancedAGF ScreamerAlert Capability Probe Workflow

Scope:
- Source project: 00_DLL-Projects/Projects/DLL_NoEAC-EnhancedAGF
- Feature area: ScreamerAlert capability hello probe-response behavior
- Active build target DLL: 02_ActiveBuild/AGF-NoEAC-EnhancedAGF-v4.3.0/EnhancedAGF.dll

Working methods:
- Implement probe-driven hello logic in:
  - src/Utility/ScreamerAlertEnhancedCapabilityHello.cs
- Use a probe nonce pathway for fresh hello responses:
  - TrySendFromProbe(int entityId, int nonce)
- Keep a small duplicate-probe cooldown to avoid repeated sends from identical probe bursts.
- Build with:
  - msbuild EnhancedAGF.csproj /t:Build /p:Configuration=Release
- Deploy to active build only (when requested) and verify SHA256 hash match.

Change history:
- 2026-07-08: Added probe-specific hello method so server capability probes can force a fresh hello even when prior ack state was true.
- 2026-07-08: Added probe nonce dedupe/cooldown to avoid burst duplicate sends.
- 2026-07-08: Rebuilt EnhancedAGF.dll for active build deployment.

Do-not-do notes:
- Do not depend on command-only hello flow for capability certainty; probe flow requires a fresh-response path.
- Do not keep _acknowledged=true during probe-triggered hello; that blocks fresh responses.
- Do not deploy to live game folders when the request is active-build-only.
- Do not use mcs -recurse:*.cs at project root for this project because obj-generated files can cause duplicate attributes.
