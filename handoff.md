# Handoff: OWCE iOS rebuild and TestFlight deployment

You're picking up work from a cloud Claude Code session. This file has everything you need. You don't need that session's transcript.

## Goal

The user wants to rebuild the Onewheel Community Edition (OWCE) iOS app and ship it through **their own** TestFlight. The original developer let their TestFlight lapse, and the app was Xamarin.Forms. Xamarin reached end of support in May 2024, and Apple has required App Store Connect uploads to be built with Xcode 26 / the iOS 26 SDK since April 2026. So the app had to be migrated before it could ship again.

## Current state (as of 2026-10-20)

- **Repo:** `sebastianwrightclaud/OWCE_App`. This is the user's fork of `OnewheelCommunityEdition/OWCE_App`.
- **`main` is at `09708c7`.** It has the complete migration. The working branch `claude/ios-testflight-deployment-64e0ip` was fast-forwarded into main and is identical to it. Work on main or on a new branch.
- **CI:** the workflow is `.github/workflows/ios.yml`. The first run on main (run `39360773351`) failed in the iOS job because of toolchain drift, not code: CI had floated to .NET SDK 10.0.402, whose iOS workload requires Xcode 27.1, and the runner only has Xcode up to 26.6. Commit `a2d5e61` fixes this by pinning the SDK, workload set and Xcode together (see gotcha 8). **Check that the latest run on main is green.**
- **Not done yet, and it needs the user, not you:** the one-time Apple Developer and App Store Connect setup, plus adding the GitHub secrets. Until then, the TestFlight deploy job fails fast with a "Missing repository secrets" error. That is expected.
- **Never run on a device.** Everything has been verified at the compile level only.

## What was done

The app was migrated from Xamarin.Forms to **.NET MAUI on .NET 10**. That's about 9,700 lines across 91 C# files, 25 XAML files, and the iOS and Android platform code.

| Before (Xamarin) | After (MAUI) |
|---|---|
| Separate projects: `OWCE` (netstandard) plus `OWCE.iOS`, `OWCE.Android`, `OWCE.MacOS`, `OWCE.WatchOS` | One single project: `OWCE/OWCE/OWCE.csproj` with a `Platforms/` folder |
| Xamarin.Forms / Xamarin.Essentials | Microsoft.Maui.Controls 10.0.90 / MAUI Essentials |
| Rg.Plugins.Popup | Mopups (`MopupService.Instance`) |
| Xamarin.Forms.PancakeView | MAUI `Border` (`StrokeShape="RoundRectangle N"`, `Shadow`) |
| Xamarin.CommunityToolkit (Expander, TouchEffect, IconTintColorEffect, InvertedBoolConverter) | CommunityToolkit.Maui 15 (Expander, `TouchBehavior`, `IconTintColorBehavior`, converters) |
| XCT `AsyncCommand` / `IAsyncCommand` | CommunityToolkit.Mvvm `AsyncRelayCommand` / `IAsyncRelayCommand` |
| `MessagingCenter` | `WeakReferenceMessenger` with the `UnitDisplayUpdatedMessage` type |
| SkiaSharp.Views.Forms | SkiaSharp.Views.Maui.Controls 4.x |
| `[assembly: Dependency]` attributes | `DependencyService.Register<,>()` calls in `MauiProgram.RegisterPlatformServices()` |
| Microsoft AppCenter (retired) | Removed |
| `ArcView` and its platform renderers | Removed (it was unused; `SpeedArcView` is the SkiaSharp one that's actually used) |
| Apple Watch app (`OWCE.WatchOS`) | Removed. .NET has no watchOS support, and upstream had already disabled it. The phone-side WatchConnectivity code is kept. See `Documentation/WatchApplication.md`. |

**Docs added or updated:** `Documentation/TestFlightDeployment.md` (the full Apple setup walkthrough), `Documentation/WatchApplication.md`, and the README build instructions.

## Project layout

```
OWCE/OWCE.sln
OWCE/OWCE/OWCE.csproj          # TFMs: net10.0; net10.0-android; + net10.0-ios on macOS
OWCE/OWCE/MauiProgram.cs       # builder, fonts, Mopups/Toolkit/Skia init, DependencyService registration
OWCE/OWCE/App.xaml(.cs)        # root page set via CreateWindow(); OWBLE resolved via DependencyService
OWCE/OWCE/Platforms/iOS/       # AppDelegate, Program, OWBLE (CoreBluetooth), Watch/WCSessionManager, Info.plist
OWCE/OWCE/Platforms/Android/   # MainActivity, MainApplication, OWBLE, BluetoothPermission, AndroidManifest.xml
OWCE/OWCE/Resources/           # Fonts/ (Saira), Images/ (MauiImage w/ BaseSize), AppIcon/, Splash/
```

The `net10.0` TFM (no platform) is a compile-check target. It builds all the shared pages, views and models without needing an Apple or Android SDK. Use it as the fast feedback loop.

## Building

You need the .NET 10 SDK.

```sh
dotnet workload install maui --version 10.0.401   # on macOS with Xcode 26.6; on Linux: maui-android maui-tizen
dotnet build OWCE/OWCE/OWCE.csproj -f net10.0 -c Release          # fast shared-code check, any OS
dotnet build OWCE/OWCE/OWCE.csproj -f net10.0-android -c Release  # needs Android SDK
dotnet build OWCE/OWCE/OWCE.csproj -f net10.0-ios -c Release -p:RuntimeIdentifier=ios-arm64 -p:EnableCodeSigning=false  # macOS + Xcode 26
```

To run on a connected iPhone from a Mac, you need a development signing identity. Then run:
`dotnet build OWCE/OWCE/OWCE.csproj -f net10.0-ios -t:Run -p:RuntimeIdentifier=ios-arm64`

**Always build `-c Release` to verify changes.** XAML compilation (XamlC) errors only appeared in Release, and Debug builds hid them during the migration.

## Gotchas learned the hard way (don't undo these)

1. **The iOS namespace is `OWCE.Platforms.iOS`, not `OWCE.iOS`.** A namespace ending in `.iOS` shadows MAUI's `Microsoft.Maui.Controls.PlatformConfiguration.iOS` type, which breaks the unqualified `On<iOS>()` calls in `CustomNavigationPage.cs` and `SideMenuPopup.xaml.cs`. Android uses `OWCE.Droid` for the same reason.
2. **Use `CBManagerState`, not `CBCentralManagerState`.** In .NET for iOS, `CBCentralManager.State` returns `CBManagerState`.
3. **`TouchBehavior` has no `NativeAnimation` property** in CommunityToolkit.Maui. Don't add it back.
4. **Microsoft.Maui.Controls must stay at ≥ 10.0.90.** CommunityToolkit.Maui 15.0.1 requires it, and a lower pin fails restore with error NU1605.
5. **Building `net10.0` on Linux needs the `maui-tizen` workload**, oddly enough. CI installs `maui-android maui-tizen` for the Linux jobs.
6. **NU1903 warning on SQLitePCLRaw 2.1.11.** There's an advisory (GHSA-2m69-gcr7-jv3q), but 2.1.11 is the newest stable release. It's overridden explicitly in the csproj, so bump it when a fixed version ships.
7. **`Info.plist` deliberately leaves out** the bundle ID, version, icon and launch-screen keys. MAUI injects those from the csproj (`ApplicationId`, `ApplicationDisplayVersion`, `ApplicationVersion`, `MauiIcon`, `MauiSplashScreen`). `ITSAppUsesNonExemptEncryption=false` is set so TestFlight doesn't ask the export-compliance question on every build.
8. **CI pins the .NET SDK (10.0.401), workload set (10.0.401) and Xcode (26.6) together**, at the top of `ios.yml`. Each .NET for iOS release requires one exact Xcode version, so letting the SDK float silently broke the iOS build once already. Workload set 10.0.401 is the newest .NET 10 set that still targets Xcode 26 (it ships .NET for iOS 26.5.10318, which requires Xcode 26.6); every later 10.0.4xx set requires Xcode 27.x. To upgrade, check the [.NET for iOS release notes](https://github.com/dotnet/macios/releases) for the required Xcode, confirm the runner image has it, and bump all three values together. Locally on a Mac, install the matching workload with `dotnet workload install maui --version 10.0.401` if you have Xcode 26.6, or pick the workload set that matches your own Xcode.

## CI / TestFlight workflow (`.github/workflows/ios.yml`)

- **`build-shared`** (Linux, `net10.0`), **`build-android`** (Linux) and **`build-ios`** (`macos-26`, unsigned device build) run on every push to `main`, `development` and `claude/**`, and on PRs.
- **`deploy-testflight`** runs on a `v*` tag push, or on a manual "Run workflow" with *deploy_to_testflight* checked. It imports the distribution certificate and provisioning profile into a temporary keychain, runs `dotnet publish` to produce a signed IPA (the build number is `github.run_number`), uploads the IPA as an artifact, then runs `fastlane pilot upload` using an App Store Connect API key.
- **Required secrets:** `IOS_DIST_CERT_BASE64`, `IOS_DIST_CERT_PASSWORD`, `IOS_PROVISIONING_PROFILE_BASE64`, `APPSTORE_API_KEY_ID`, `APPSTORE_ISSUER_ID`, `APPSTORE_API_PRIVATE_KEY`.
- **Required variables:** `IOS_BUNDLE_ID`, `IOS_CODESIGN_KEY`, `IOS_PROVISIONING_PROFILE_NAME`.
- **The user must use their own bundle ID** (for example `com.<name>.owce`). The original `com.owcommunity.owce` is registered to the original developer's Apple team. The csproj default is still the original ID; CI overrides it with `-p:ApplicationId` from the `IOS_BUNDLE_ID` variable.
- The deploy job has never actually run. Expect to debug its first real run, especially the signing parameters (`CodesignKey` / `CodesignProvision`) and the location of the IPA output.

## Known gaps and risks

These need a device, or the user's judgement:

- **No runtime testing.** Priority checks on a real phone with a real board:
  1. Bluetooth permission prompt, then scan, then the board appears.
  2. Connect and handshake (`OWBoard` handshake logic, firmware-specific paths around 4155 and 5059).
  3. Live telemetry updates: speed arc (SkiaSharp), battery, temperature, angle views.
  4. Switching the metric/imperial setting updates the displayed values (this now goes through `WeakReferenceMessenger`).
  5. Ride recording start/stop, then the Past Rides list, swipe actions and share/export.
  6. Popups: side menu, connecting/progress/jumpstart alerts (now Mopups).
  7. Expanders in the battery, ride-mode and speed/range views.
- **Visual fidelity.** The PancakeView-to-`Border` swap approximates corner radii and drop shadows. `CustomNavigationPage` lost its iOS renderer, which set the nav bar tint and hid the shadow line, so nav bar styling may differ.
- **`logo_a.png` is referenced in `Pages/BoardListPage.xaml` but missing from the assets.** This was already broken upstream, before the migration.
- **Android** compiles, but its runtime is completely untested. The old `SplashActivity` was replaced by the MAUI splash.
- **Obsolete-API warnings**, about 80 of them: `DisplayAlert` → `DisplayAlertAsync`, `Application.MainPage` → `Windows[0].Page`, `RotateTo`/`FadeTo` → `*Async`, `Device.StartTimer` → `Dispatcher.StartTimer`. They still work today but should be cleaned up eventually. There are also around 160 `XC0022` warnings asking for `x:DataType` to enable compiled bindings; those only affect performance.
- **Geolocation during ride recording** was already commented out upstream, and the `Xam.Plugin.Geolocator` dependency is gone. Re-enabling it would mean porting to MAUI `Geolocation`.
- **Upstream sync.** Upstream's `feature/pre-app-migration` branch (2025) only disables watch support and bumps versions. It doesn't include a MAUI port, so there's nothing there worth merging.

## Suggested next steps

1. Confirm the latest CI run on main is green. If it isn't, fix it, re-verify with a Release build, and push.
2. Help the user through `Documentation/TestFlightDeployment.md`. They need an Apple Developer account, their own bundle ID, a distribution certificate (.p12), an App Store provisioning profile, an App Store Connect API key, and the GitHub secrets and variables.
3. Trigger the deploy from Actions → *iOS build and TestFlight* → Run workflow, with *deploy_to_testflight* checked. Debug the first signed build and upload.
4. Install from TestFlight and work through the runtime checklist above with a real board. Fix whatever turns up.
5. Optional cleanup: obsolete APIs, compiled bindings, the missing logo asset, and updating `ApplicationId` in the csproj to the user's bundle ID so local builds match CI.

## Conventions

- Commit messages: an imperative subject line, plus a body explaining *why*.
- Don't push to `main` without the user asking. They explicitly approved the push of the migration.
- Verify locally with a Release build before pushing anything that touches CI.
