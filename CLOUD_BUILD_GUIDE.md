# Riverforge Android — No Local Unity Required

This repository is a Unity 6 project. The project root must contain:

- `Assets/`
- `Packages/`
- `ProjectSettings/`

Detected Unity editor version: `6000.0.23f1`.

## Recommended route: GitHub + Unity Build Automation

### 1. Put this project on GitHub
1. Sign in at GitHub.
2. Create a new repository, for example `riverforge-dam-builder`.
3. Open the repository and choose **Add file > Upload files**.
4. Upload the CONTENTS of this folder. The repository root must directly show `Assets`, `Packages`, and `ProjectSettings`.
5. Commit the upload to the `main` branch.

### 2. Create a Unity Cloud project
1. Sign in to the Unity Dashboard with a Unity ID.
2. Create a project named `Riverforge` if you do not already have one.
3. Open **Development / DevOps > Build Automation**.
4. Start/enable the Build Automation service if requested.

### 3. Connect GitHub
1. In Build Automation, open **Source Control**.
2. Choose **GitHub**.
3. Authorize access to the `riverforge-dam-builder` repository.
4. Select the repository and save.

### 4. Create the Android build configuration
Use these values:

- Target platform: `Android`
- Branch: `main`
- Project subfolder: leave empty if `Assets`, `Packages`, and `ProjectSettings` are at repository root
- Unity version: enable **Auto detect Unity version** (ProjectVersion.txt specifies `6000.0.23f1`)
- Output for phone testing: **APK** (do NOT enable App Bundle)
- Credentials/signing: choose **Auto-generated debug keystore** for development/testing

Create/save the configuration and press **Build**.

### 5. Install it on your Android phone
1. When the cloud build succeeds, download the generated `.apk` from the build artifacts.
2. Open the APK on the Android phone.
3. If Android asks, allow the browser/file-manager app to **Install unknown apps**.
4. Install and launch Riverforge.

## For Google Play later
Google Play distribution uses an `.aab`, not a directly installable APK. When the game is ready for publishing:

1. Create and safely store a release Android keystore.
2. Configure that keystore in Unity Build Automation.
3. Enable **Build app bundles (.aab) instead of an APK**.
4. Upload the resulting `.aab` to Google Play Console.

Never lose the production keystore/passwords if you intend to update the same Play Store app later.

## About GitHub Actions / GameCI
It is possible to build Unity from GitHub Actions with GameCI, but Unity still requires a valid license activation. For a Unity Personal license, current GameCI instructions generally require creating/obtaining a Unity license file and storing Unity credentials as GitHub Actions secrets. If the goal is to avoid installing Unity locally, Unity Build Automation is the simpler first route.
