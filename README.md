Onewheel Community Edition (OWCE) App
===========

A cross-platform app for use with the [Onewheel](https://onewheel.com/) V1, Plus, XR, Pint, Pint X and GT.

NOTE: GT support requires patching with [Rewheel](https://github.com/rewheel-app/rewheel).
The newer board firmware no longer sends through voltage, as Future Motion removed it. Those versions are:
- XR with firmware 4155 and higher
- Pint with firmware 5059 and higher
_ Pint X, all firmware
- GT, all firmware

NOTE: Onewheel Community Edition app is not endorsed by or affiliated with Future Motion in any way.

Written in C# with [.NET MAUI](https://dotnet.microsoft.com/apps/maui)

Open Source Project by [@beeradmoore](http://www.twitter.com/beeradmoore) 

## Available (soon) for free on:
* Android: Available on Google Play
* iPhone: Available on App Store (the Apple Watch companion app is not currently supported, see [Documentation/WatchApplication.md](Documentation/WatchApplication.md))

## How to build and run this project. 

The app is a .NET MAUI single project targeting .NET 10. Install the [.NET 10 SDK](https://dotnet.microsoft.com/download) and the MAUI workload pinned to the version CI uses (`dotnet workload install maui --version 10.0.402`; iOS builds need Xcode 27.0 to match it), then:

    dotnet build OWCE/OWCE/OWCE.csproj -f net10.0-android    # Android
    dotnet build OWCE/OWCE/OWCE.csproj -f net10.0-ios        # iOS (requires macOS + Xcode)

`-f net10.0` builds the shared code on any OS without the mobile workloads. Each .NET for iOS release requires one exact Xcode version; CI pins the .NET SDK, workload set and Xcode together at the top of `.github/workflows/ios.yml`, so match those locally (`dotnet workload install maui --version <workload set>`) if your iOS build complains about the Xcode version. CI builds all targets on every push (see `.github/workflows/ios.yml`), and TestFlight deployment is documented in [Documentation/TestFlightDeployment.md](Documentation/TestFlightDeployment.md).

NOTE: Because the app depends on the Onewheels low-energy Bluetooth, it will not function correctly in a simulator/emulator. For best results, deploy to a physical device. 



## Frequently (or not so frequently) Asked Questions

### Why did you create this?

There are quite a number of members on the [Onewheel Owners Facebook group](https://www.facebook.com/groups/onewheelownersgroup/) that, for one reason or another, don't like the stock app by Future Motion. I figured, why not create an app with its development shaped by features that the community wants?

### Do other third-party apps already exist?

Yes, but Future Motion's firmware lockdowns prohibit them from being used. Such as pOnewheel[https://github.com/ponewheel/android-ponewheel] (deprecated) for Android and Float Deck (obsolete) for iOS. However, the problem is that one is for Android, and the other is for iOS. Wouldn't it be better if there was just 1 app with the exact same feature sets shared across both platforms?

### Does this change how my Onewheel performs?

No. This app uses the same Bluetooth low energy (BLE) interface that the official Onewheel app uses to read and display various stats.

### What Onewheels are supported?

Currently, v1, Plus, XR, and Pint. Pint X and GT have not yet been thoroughly tested.

### Will using this app void my warranty?

Although things such as riding your board without a helmet can void your warranty, we don't believe that using third-party apps will void your warranty.
