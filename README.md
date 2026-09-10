<div align="center">

# ✦ Kooker: Starfall

**Where the sky becomes a world.**

A world in the making · Unity 6 · Windows preview

[Explore the direction](docs/STARFALL.md) · [Visual progress](docs/KOKERBOOM-VISUAL-REVIEW.html) · [Build & run](#-build--run) · [Credits](docs/ASSET-CREDITS.md)

</div>

![R06 Unity study corresponding to the retained visual reference](evidence/milestones/kokerboom/round-06/2026-09-10-01-preview-eye-level.png)

*Actual R06 Unity study capture, 10 September 2026 · WIP. The running R06 tree is the user-approved visual reference. This image is an offscreen study capture, not a new native screenshot or a finished world.*

Beneath a blue giant and a river of stars, warm desert gives way to luminous seas. Explore, shape, and one day inhabit a world still becoming.

**That is the destination. Today, Starfall is an approximately 60 m tree and blue-giant study** with a separate local Windows preview. The sculpted landscape, visible galaxy and living sea are planned. World shaping, houses, building tools, inhabitants, bots and shared-world connections are future work.

## 🌌 The world ahead

| Element | Direction | Current state |
|---|---|---|
| Trees | Gold and ochre kokerboom trunks, rounded crowns and cool blue-green rosettes | R06 approved reference; R19 experiment frozen |
| Land | Warm sculpted mesas, rocky shores and turquoise bays | Small ground study only |
| Sky | A large blue gas giant, moons and a distant galaxy | Prototype giant and stars; wider sky work planned |
| Sea | Clear shallows, submerged arches, kelp-like growth, coral forms, fish schools and rays | [Living sea design](docs/STARFALL-LIVING-SEA.md); unimplemented |
| Life and building | Explore, shape and eventually inhabit the world | Future scope |

The [visual direction](docs/KOKERBOOM-REFERENCE.md) records the concept references separately from actual Unity evidence. The new landscape will have its own versioned world definition, preserving the earlier island and its saved edits.

## 🌳 Tree baseline and preserved experiments

[![Actual Unity R16 neutral tree inspection, preserved historical experiment](evidence/milestones/kokerboom/round-16/2026-09-10-02-neutral-three-quarter.png)](docs/KOKERBOOM-VISUAL-REVIEW.html)

*Actual Unity R16 neutral inspection · WIP. Open the local [visual review catalogue](docs/KOKERBOOM-VISUAL-REVIEW.html) in a browser to compare preserved rounds and camera views. GitHub displays the HTML source; the [Markdown milestone index](docs/VISUAL-MILESTONES.md) is readable there directly.*

**The user approved R06 as the visual reference; the final numeric/full R19 review is complete and tree polishing is frozen.** R19 is a separately preserved experimental candidate, not an automatic player replacement. Remaining defects are deferred; the historical 9/10 critic target no longer blocks world work. See the [tree baseline and closeout](docs/TREE-BASELINE.md).

R16's historical full review remains **5.75/10, rejected under that rubric**; its numeric buried-root failure is preserved. R17/R18 are unscored six-view pilots. The revised R18 experimental source [passed 536 numeric assertions](evidence/verified/kokerboom-round-18-family-validation.json); the closing [R19 set contains all 21 actual offscreen views](docs/VISUAL-MILESTONES.md#round-19-frozen-family-review), with an independent **7.375/10 (7.4)** result, rejected under the historical rubric and frozen with defects deferred. These results do not change the retained R06 executable or establish native performance.

## 🎮 Build & run

Use **Unity 6000.6.0f1** with Windows build support. URP **17.6.0** and Input System **1.20.0** are pinned in [Packages/manifest.json](Packages/manifest.json).

The approved reference package remains R06. The command below creates a new, separately unreviewed source build; it does not reproduce or promote the approved reference automatically. From the repository root:

~~~powershell
.\tools\render-kokerboom.ps1 -Round round-100 -PlayablePreview
~~~

Choose an unused round number; the label only identifies evidence and does not affect world generation. The script renders two study views and bakes a Windows player into `Builds/KookerStarfall-<UTC>/`. It preserves earlier captures, refuses to run alongside another Unity editor and does not launch the player. Open `KookerStarfall.exe` from the resulting folder; keep the data folder and DLLs beside it.

**The Starfall branding is source-only until this new build is verified.** The retained [R06 build record](evidence/milestones/kokerboom/round-06/preview-build.json) refers to the earlier `CosmicWorldPreview.exe`, which has already been used as a local WIP preview. The later R16/R19 images are not screenshots of that executable. The current `-PlayablePreview` command selects a later PH01 hybrid, not the PH02 R19 inspection family, and cannot reproduce exact R06 bytes. The [checked R06 package](evidence/verified/starfall-r06-package-check.json), `Kooker-Starfall-WIP-R06-Windows.zip` (161.42 MiB), preserves all 183 runtime files unchanged; packaging did not rebuild the player.

| Control | Action |
|---|---|
| WASD | Move |
| Hold right mouse button | Look |
| Shift | Move faster |
| F | Switch walk / inspection flight |
| Q / E | Lower / raise flight height |
| Escape | Release the pointer |
| Alt + Enter | Toggle window / fullscreen |

Walking follows the ground at 1.85 m eye clearance and stays within the study. This is an inspection controller; rocks do not yet have complete collision. F12 capture requires an explicit absolute folder passed with `-previewEvidence`; the preview does not capture automatically.

The preview has no implemented multiplayer, bot connections or saved-world loading. Offline operation and engine telemetry have not been fully validated. Legacy island build commands and controls remain in the [earlier island guide](docs/LEGACY-ISLAND.md).

## Evidence at a glance

| Claim | Status | Evidence | Remaining gap |
|---|---|---|---|
| Separate Windows study | R06 build succeeded; local WIP used | [Build record](evidence/milestones/kokerboom/round-06/preview-build.json) | Full native controls, collision and measured performance acceptance |
| Visual reference and tree closeout | **R06 user approved**; frozen after the completed R19 review | [Baseline record](docs/TREE-BASELINE.md) | Experimental R19 is separate; remaining defects deferred, no further 9/10 polishing gate |
| Revised experimental PH02 source | **R18 numeric PASS, 536 assertions** | [Exact report](evidence/verified/kokerboom-round-18-family-validation.json) | All 21 R19 captures complete; independent result 7.375/10, frozen; R16 failure preserved; no new native player |
| Starfall branding | Repository renamed; source updated | [Naming and continuity record](docs/STARFALL.md) | Branded build and runtime verification |
| Wider world and living sea | Planned | [World direction](docs/STARFALL.md), [sea plan](docs/STARFALL-LIVING-SEA.md) | Implementation and actual scene evidence |

## 🧭 Project guide

| Start here | Reference |
|---|---|
| World direction and naming | [Starfall](docs/STARFALL.md) · [Living sea](docs/STARFALL-LIVING-SEA.md) |
| Images and review | [Visual catalogue](docs/KOKERBOOM-VISUAL-REVIEW.html) · [Milestones](docs/VISUAL-MILESTONES.md) · [Tree critique](docs/KOKERBOOM-CRITIQUE.md) |
| Assets and provenance | [Asset catalogue](docs/ASSET-CATALOGUE.md) · [Credits](docs/ASSET-CREDITS.md) · [Portable notices](Assets/CityLife/Art/THIRD-PARTY-NOTICES.txt) |
| Determinism and saved edits | [World foundation](docs/WORLD-FOUNDATION.md) · [Legacy island](docs/LEGACY-ISLAND.md) |
| Verification and contribution | [Evidence record](docs/VERIFICATION.md) · [CI and safeguards](docs/CI.md) |

The repository is [duikindiesee/kooker-starfall](https://github.com/duikindiesee/kooker-starfall). Its history and draft review continue under the new name. Internal `CityLife.World` namespaces, `Assets/CityLife` paths, legacy product settings, world IDs and save contracts remain intact; branding does not migrate an existing world.

Contribute through branches and pull requests. Retain provenance, licences and earlier evidence. Keep credentials, private runtime profiles, player state, downloaded archives and raw machine logs out of the public repository.
