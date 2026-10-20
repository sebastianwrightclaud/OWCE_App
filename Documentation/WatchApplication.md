# Watch Support for OWCE

> **Status: the Apple Watch app is not currently built.** .NET MAUI (and .NET for iOS in general) does not support watchOS, so the Xamarin `OWCE.WatchOS` project was removed during the migration to .NET MAUI. Watch support had already been disabled upstream before that. The phone-side plumbing described below is still in the app, so a future watch app (for example, a native Swift watchOS target) can reuse it. The old Xamarin watch extension source is in git history at commit `029c486`, under `OWCE/OWCE.WatchOS/`.

## How the code works

The phone uses a Dependency Interface (`IWatch`) to communicate between the core (platform independent) OWCE code and the native iOS code, and WatchConnectivity to communicate between iOS and watchOS.

### When board is connected

In [BoardListPage.xaml.cs](../OWCE/OWCE/Pages/BoardListPage.xaml.cs), when the board is connected, `IWatch.ListenForWatchMessages` is called:
* iOS app registers as a WatchConnectivity listener to receive messages from the watch (eg when the watch wakes up) and sends the latest values to the watch
* In future, can also be used for the watch to send actions back to the phone, such as changing ride mode or start ride tracking

### When watch app wakes up

This was handled by `InterfaceController.cs` in the removed watchOS extension (see git history above):
* WatchOS Extension registers itself as a WatchConnectivity listener for receiving messages
* Upon receiving a message from the phone, the app will update the necessary parts of the UI
* When the watch is about to be activated (ie about to appear to the wearer), the watch app sends a message to the WatchConnectivity session for the phone to respond with the latest up-to-date values

### How the phone updates the watch with new values

In [OWBoard.cs](../OWCE/OWCE/OWBoard.cs) under `OWBoard()`, `WatchSyncEventHandler` is registered as a `PropertyChanged` listener to keep the watch app in sync with the latest values (eg speed, battery percent, distance).

[WatchSyncEventHandler](../OWCE/OWCE/PropertyChangeHandlers/WatchSyncEventHandler.cs) filters the list of property changes and sends the relevant updates to the [IWatch](../OWCE/OWCE/DependencyInterfaces/IWatch.cs) Dependency Interface, implemented by [Watch.cs](../OWCE/OWCE/Platforms/iOS/DependencyImplementations/Watch.cs) on iOS.

[Watch.cs](../OWCE/OWCE/Platforms/iOS/DependencyImplementations/Watch.cs) uses the WatchConnectivity session ([WCSessionManager.cs](../OWCE/OWCE/Platforms/iOS/SessionManager/WCSessionManager.cs), started from [AppDelegate.cs](../OWCE/OWCE/Platforms/iOS/AppDelegate.cs)) to send update messages to the watch.
