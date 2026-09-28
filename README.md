# Riverforge Dam Builder — Android Unity Vertical Slice

This is an original Android-first dam construction/management game foundation inspired by the *type* of mobile dam-builder reference you shared, without copying its proprietary art, UI, level data, or branding.

## What is already implemented

- Runtime-generated 3D valley, river, reservoir, dam, powerhouse, vegetation, clouds and construction props.
- Animated multi-stage dam construction.
- One-finger orbit camera + two-finger pinch zoom on Android; mouse controls in Editor.
- Reservoir hydrology with Clear/Rain/Storm/Drought weather states.
- Turbine-load and spillway-gate controls.
- Electricity production, revenue, operating costs and MWh objective tracking.
- Safety, ecology and public-trust systems with fail conditions.
- Wall, turbine and spillway upgrades (3 levels each).
- Construction and mission messages.
- Pause and 1x/2x/4x simulation speed.
- Autosave to PlayerPrefs JSON plus manual save.
- Win state after Day 5 + 1,000 MWh + >=70% safety.
- Android configuration helper for landscape, ARM64, IL2CPP and AAB.

## Open in Unity

1. Install **Unity 6** through Unity Hub and include **Android Build Support**, **Android SDK & NDK Tools**, and **OpenJDK**.
2. In Unity Hub choose **Add project from disk** and select this `RiverforgeAndroid` folder.
3. Open `Assets/Scenes/Main.unity`.
4. Press Play. The scene is generated entirely at runtime, so no imported art is required for this vertical slice.

## Build for Android

1. Connect an Android phone with USB debugging enabled.
2. In Unity use **Riverforge > Configure Android**.
3. For a device test, use **File > Build Profiles / Build Settings > Android > Build And Run**.
4. For Google Play, use **Riverforge > Build Android AAB** after setting your signing keystore.
5. Change the package identifier `com.riverforge.dambuilder` before publishing.

## Recommended production art pass

Replace runtime primitives with optimized low-poly meshes while keeping the scripts and object responsibilities intact. Recommended asset groups:

- 3 dam tiers: small gravity dam, medium concrete dam, large arch/gravity hybrid.
- Excavator, dump truck, concrete mixer, tower crane, bulldozer, worker crews.
- Construction stage prefabs: excavation, foundation, first lift, powerhouse, crest, gates.
- Modular cliffs, roads, villages, pylons and substations.
- Water shader with shoreline foam and spillway particles.
- Rain, storm mist, dust, concrete pour, sparks and turbine-water VFX.
- Original Android UI icon set and original title/branding.

## Production roadmap

The next production layer should add: ScriptableObject level definitions, addressable art, object pooling, proper localization, audio mixer, haptics, tutorial/onboarding, level-select map, achievements, analytics consent, optional rewarded ads/IAP, Play Games cloud saves, Android adaptive performance, quality tiers, crash reporting, automated playmode tests, and store assets.

