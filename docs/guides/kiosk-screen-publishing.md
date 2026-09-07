# Build beyond the mobile screen

An app can run on a phone, a kiosk alone, or both. In a two-screen experience, the kiosk is the display and the phone can be the controller. Your app owns the shared game/session backend; KioskX maps the display to a machine and presents the controller link as a QR.

## Release status

This is a coordinated preview: the matching KioskX cloud API, Operator X controls and ZHZN player must be released together. The SDK alone cannot update a cabinet. Existing Reyeah firmware does not support this player. A ZHZN Android 9+ build must report `experiencePlayerV1`; the API refuses enabling on unsupported devices. A saved mapping is not proof of playback.

## Try two screens locally

From `SDKs/javascript`:

```sh
npm ci
npm run build
node examples/kiosk-screen/server.mjs
```

Open the two URLs printed by the server in separate browser windows. Tap the phone controller arrows; the dot moves on the display. This example has no wallet, rewards, purchases, or user identity. Its random room lasts for that server process; restarting changes the URLs. Do not use it as a durable multiplayer service.

For a real kiosk, host the example behind your own HTTPS origin and start it with `PUBLIC_ORIGIN=https://games.your-domain.example`. Copy the printed manifest into `manifest.json`. Use one room per mapping. Keep all scripts, assets and API requests on the same origin. The first player denies cross-origin resources, cookies, local storage, camera, microphone, popups and native device access. Bundle dependencies locally or proxy your own game APIs server-side. Unity WebGL compatibility depends on the cabinet WebView and memory; native mobile binaries are not uploaded or converted by this API.

## Publish from Operator X

Open a machine → **Publish an app to this screen**. Enter the app ID, display name, kiosk HTTPS URL, optional phone controller HTTPS URL, and session limit (30–900 seconds). Save disabled while preparing the device. When `playerReady` is true, publish. The hosted storefront refreshes the mapping every 30 seconds and offers an **Open** button. The app runs in a separate ZHZN WebView process with its own data directory and no kiosk bridge. A native **Back to kiosk** control remains visible and the session expires automatically.

Disable prevents new opens after the next mapping refresh; an already opened session finishes within its configured limit. This is not an emergency stop. Reload before editing after a conflict. A machine has one published mapping in v1; keep your app menu inside your experience if it contains several games.

## Publish from your backend

Use the preview source build until this API is included in a public package release. Keep the operator API key on your trusted backend or terminal. It needs the existing `machines:read` and `machines:write` scopes; its operator ownership restricts which machines it can map.

```js
import { IVXKioskPublisher } from '@intelliversex/sdk';
const publisher = new IVXKioskPublisher({ apiKey: () => process.env.KIOSKX_API_KEY });
const current = await publisher.get('YOUR_MACHINE_NUMBER');
await publisher.publish('YOUR_MACHINE_NUMBER', current.revision, {
  schemaVersion: 1,
  appId: 'my-game',
  name: 'My game',
  kioskUrl: 'https://games.your-domain.example/display?room=YOUR_ROOM',
  controllerUrl: 'https://games.your-domain.example/controller?room=YOUR_ROOM',
  sessionSeconds: 180,
}, true);
```

Or run `node examples/kiosk-screen/publish.mjs YOUR_MACHINE_NUMBER manifest.json stage`, then replace `stage` with `publish` once the device is ready. Provide `KIOSKX_API_KEY` through your environment/secret manager; never commit it.

The client uses `GET` and `PUT /api/v1/machines/{machineNo}/experience`. PUT requires `expectedRevision`, `enabled`, and `manifest` (nullable to clear). The cloud checks ownership again in the durable write transaction and rejects stale revisions with 409. After a timeout, read the current mapping before retrying. Publishing cannot dispense goods, issue rewards, claim an ad impression, or alter payments; integrate those separately through the existing verified backend contracts.
