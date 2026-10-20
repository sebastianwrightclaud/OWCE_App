# Deploying OWCE to TestFlight

The app now builds with .NET MAUI on .NET 10 (see the `iOS build and
TestFlight` GitHub Actions workflow). Apple requires App Store Connect
uploads to be built with the current Xcode SDK (Xcode 26 / iOS 26 SDK since
April 2026), which the workflow's macOS 26 runner provides — you do not need
a Mac of your own.

Everything below is one-time setup. After it, a TestFlight release is a
single click (or a `v*` tag push).

## 1. Prerequisites

- An [Apple Developer Program](https://developer.apple.com/programs/enroll/)
  membership (US$99/year) for the Apple ID that will own the app.
- Admin access to this GitHub repository (to add secrets and run workflows).

## 2. Pick your own bundle ID

The original app shipped as `com.owcommunity.owce`, which is registered to
the original developer's Apple team — **you cannot upload under that ID**.
Choose your own, e.g. `com.yourname.owce`.

1. Go to [Certificates, Identifiers & Profiles → Identifiers](https://developer.apple.com/account/resources/identifiers/list)
   and register a new **App ID** (type *App*) with your bundle ID.
   No special capabilities need to be enabled — Bluetooth background mode is
   declared in the Info.plist and needs no entitlement.
2. In [App Store Connect → Apps](https://appstoreconnect.apple.com/apps),
   create a **New App** (platform iOS) using that bundle ID.

The workflow reads the bundle ID from a repository **variable** (not secret)
named `IOS_BUNDLE_ID`, so nothing in the repo needs editing.

## 3. Distribution certificate

1. Create a certificate signing request: on any Mac, Keychain Access →
   Certificate Assistant → *Request a Certificate from a Certificate
   Authority* (save to disk). Without a Mac, you can generate one with
   OpenSSL:
   ```sh
   openssl genrsa -out dist.key 2048
   openssl req -new -key dist.key -out dist.csr -subj "/emailAddress=you@example.com/CN=Your Name/C=US"
   ```
2. In [Certificates](https://developer.apple.com/account/resources/certificates/list),
   create an **Apple Distribution** certificate from the CSR and download
   `distribution.cer`.
3. Convert to a password-protected `.p12`:
   - Mac: import the .cer into Keychain Access, then export it as .p12.
   - OpenSSL:
     ```sh
     openssl x509 -in distribution.cer -inform DER -out distribution.pem
     openssl pkcs12 -export -inkey dist.key -in distribution.pem -out distribution.p12
     ```
4. Note the certificate's common name, e.g.
   `Apple Distribution: Your Name (TEAMID1234)` — shown on the certificate
   page. This goes into the `IOS_CODESIGN_KEY` variable.

## 4. Provisioning profile

In [Profiles](https://developer.apple.com/account/resources/profiles/list),
create an **App Store Connect** distribution profile for your App ID, select
the distribution certificate from step 3, give it a name (e.g.
`OWCE App Store`), and download the `.mobileprovision` file. The name goes
into the `IOS_PROVISIONING_PROFILE_NAME` variable.

## 5. App Store Connect API key (for the upload)

In [App Store Connect → Users and Access → Integrations → App Store Connect API](https://appstoreconnect.apple.com/access/integrations/api),
generate a **Team Key** with the **App Manager** role. Note the **Key ID**
and **Issuer ID**, and download the `.p8` private key (downloadable only
once).

## 6. Add the GitHub secrets and variables

In the repository: Settings → Secrets and variables → Actions.

Secrets:

| Secret | Value |
|---|---|
| `IOS_DIST_CERT_BASE64` | `base64 -i distribution.p12` output |
| `IOS_DIST_CERT_PASSWORD` | The password chosen when exporting the .p12 |
| `IOS_PROVISIONING_PROFILE_BASE64` | `base64 -i OWCE_App_Store.mobileprovision` output |
| `APPSTORE_API_KEY_ID` | Key ID from step 5 |
| `APPSTORE_ISSUER_ID` | Issuer ID from step 5 |
| `APPSTORE_API_PRIVATE_KEY` | Full contents of the `.p8` file |

Variables:

| Variable | Value | Example |
|---|---|---|
| `IOS_BUNDLE_ID` | Your bundle ID | `com.yourname.owce` |
| `IOS_CODESIGN_KEY` | Distribution certificate common name | `Apple Distribution: Your Name (TEAMID1234)` |
| `IOS_PROVISIONING_PROFILE_NAME` | Profile name from step 4 | `OWCE App Store` |

The deploy job runs in a GitHub environment named `testflight`; optionally
create it (Settings → Environments) and add protection rules such as
required reviewers.

## 7. Ship it

Either:

- **Manually**: Actions → *iOS build and TestFlight* → *Run workflow* →
  check *Upload the signed build to TestFlight*, or
- **By tag**: push a tag like `v3.0.0`.

The build number is the workflow run number, so every upload is unique. The
marketing version comes from `ApplicationDisplayVersion` in
`OWCE/OWCE/OWCE.csproj` (overridable per-run via the `app_version` input).

When the build finishes processing in App Store Connect (a few minutes),
add it to a TestFlight group under your app → TestFlight. The Info.plist
already declares `ITSAppUsesNonExemptEncryption = false`, so no export
compliance questionnaire blocks each build.

## Troubleshooting

- **"No profiles found" / signing errors** – the profile's bundle ID,
  certificate, and the `IOS_*` variables must all match each other.
- **"This version of .NET for iOS requires Xcode X"** – every .NET for iOS
  release requires one exact Xcode version. The workflow pins the .NET SDK,
  the workload set and Xcode together (`DOTNET_VERSION`,
  `DOTNET_WORKLOAD_VERSION` and `XCODE_VERSION` at the top of
  `.github/workflows/ios.yml`). To upgrade, pick a workload set from the
  [.NET for iOS release notes](https://github.com/dotnet/macios/releases)
  whose required Xcode is installed on the runner image, and bump all three
  values together.
- **Upload rejected for SDK version** – Apple raises the minimum SDK every
  April. When that happens, move to a runner image with the newer Xcode and
  bump the three pinned versions above to match.
- **The unsigned `build-ios` job fails** – if the error mentions Xcode, see
  the first item above. Otherwise it's a code problem, not a signing
  problem: fix the build first. Deployment only needs the steps above once
  CI is green.
