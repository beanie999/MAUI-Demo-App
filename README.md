# MAUI Demo App

A .NET MAUI sample app demonstrating New Relic mobile monitoring, paired with a small Node.js
backend used to demo distributed tracing between the mobile app and a microservice.

## Structure

- `MauiDemoApp/` - the .NET MAUI app (iOS / Android / Mac Catalyst)
- `MauiBackend/` - an Express login microservice, monitored by New Relic APM

## What the app demonstrates

The main screen has an entry field to set the New Relic user ID, an entry field to set a custom
session attribute, and buttons that each exercise a different New Relic mobile feature:

- **Custom Event** - opens a second screen where you can send a `MauiCustomMobileEvent` custom
  event with `animal` and `country` attributes
- **Handled Exception** - throws and records one of three random exceptions, with a real stack
  trace
- **Crash** - triggers a real native crash for crash reporting
- **Network Error** - fires a random subset of requests against a mix of URLs (some returning
  4xx/5xx, one an unresolvable domain) to demonstrate network error tracking
- **Distributed Trace** - calls the `MauiBackend` login microservice's `/login` endpoint,
  producing a trace that spans the mobile app and the backend

Every action also drops a New Relic breadcrumb, sends a log at an appropriate severity, and wraps
its work in a named interaction.

## Prerequisites

- .NET SDK 9 with the `android`, `ios`, and `maui-android`/`maui-maccatalyst` workloads installed
  (`dotnet workload install ...`)
- Xcode and an iOS Simulator, if building for iOS
- Node.js 18+ and npm, for the backend
- New Relic mobile app tokens (Android/iOS) and a New Relic license key

## Secrets

New Relic keys are kept out of source control:

- **Mobile app**: copy `MauiDemoApp/Secrets.example.cs` to `MauiDemoApp/Secrets.cs` and fill in
  your New Relic mobile app tokens.
- **Backend**: copy `MauiBackend/.env.example` to `MauiBackend/.env` and fill in
  `NEW_RELIC_LICENSE_KEY`.

Both `Secrets.cs` and `.env` are git-ignored.

## Running the backend

```bash
cd MauiBackend
npm install
npm start
```

The service listens on port 3000 (`http://localhost:3000/login`). On iOS Simulator, `localhost`
resolves to the host Mac, so no extra networking config is needed. The mobile app's Distributed
Trace button expects this to be running.

## Running the mobile app

```bash
cd MauiDemoApp
dotnet build -t:Run -f net9.0-ios -p:_DeviceName=:v2:udid=<simulator-udid>
```

Find a booted simulator's UDID with:

```bash
xcrun simctl list devices | grep Booted
```

For Android or Mac Catalyst, swap the target framework (`net9.0-android` / `net9.0-maccatalyst`)
and drop the simulator-specific property.
