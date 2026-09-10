# CityLife Unity

Read `README.md` and `docs/WORLD-FOUNDATION.md` before changing the world. The current milestone is an explorable island foundation; road, plot, house-tool, shared-world and household integrations are separate future work.

- Preserve the versioned world definition and explicit migration boundary. Never silently regenerate a saved world using a changed algorithm/configuration.
- Terrain base and edits remain separate. Validate an entire edit candidate before applying it. Source browser saves are not Unity edit files.
- Determinism must not depend on render frames, UnityEngine.Random, clock time or chunk travel order.
- Keep rendering/input independent of the world definition and base field. Use source references for concepts, not as evidence that proposed integrations work.
- Build with the pinned Unity editor and run `CityLife.World.Editor.IslandValidation.Run` (also part of `CityLifeBuild.BuildWindows`). Retain actual player screenshots and timings for player-visible changes.
- Update documentation and `docs/VISUAL-MILESTONES.md` with meaningful visual milestones. Images must be real source/Unity observations, clearly labelled; preserve earlier captures.
- This is a public repository. Never commit credentials, private runtime profiles, player saves, private archives, or raw local logs containing machine paths. Reviewed synthetic evidence and explicit source provenance are appropriate.
- Use branches and pull requests for implementation. `main` requires review. Do not deploy or alter the existing browser service as part of a Unity change.
