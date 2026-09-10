# Build Phygital and omnipresent apps

**Come build apps with Intelliverse.** Create App Quests, plan App Ads for participating locations on the AI Kiosk Network, and make your app something people can open, chat with, or talk to.

**Phygital = physical + digital.** A mobile game is digital. A quest at a real kiosk that can lead to a reward in that game is Phygital.

**Omnipresent apps** is our vision of one app available in the places people already use: mobile, web, WhatsApp, chat, conversational voice calls, and physical kiosks. A shared app definition is the starting point; each channel still needs its own working integration.

## What is included today?

The JavaScript source includes `IVXPhygitalApp`, an additive adapter toolkit for App Quests, App Ads, and conversation intents. It validates inputs and routes them to the adapters you explicitly install. It does not provision a kiosk network or connect a messaging/telephony provider.

| Capability | Included in the JavaScript source | Your deployment supplies |
| --- | --- | --- |
| App definition | App ID, name, channels, capability declarations | App registration and authorization |
| App Quests | Start and submit intents with quest/evidence IDs | QuestX or your backend, trusted evidence verification, reward rules |
| App Ads | Campaign and placement references | ContentX or your creative workflow, approved inventory, rendering and measurement |
| Kiosk channel | Explicit kiosk adapter interface | KioskX/device integration and participating locations |
| WhatsApp and web chat | Conversation intent and conversation ID | Connected messaging provider and consented identity mapping |
| Conversational voice | Text intent for a voice bridge | Voice-call provider, speech recognition/synthesis, call lifecycle |
| Mobile and web | The same adapter interface | Your mobile/web host and application UI |

This first runtime implementation is **JavaScript/TypeScript**. Other engine SDKs retain their existing features; this does not claim Phygital API parity across engines. Source version: JavaScript 5.9.0. Package-registry availability is separate from GitHub source availability.

## Run the local example

From a terminal:

```bash
git clone https://github.com/intelli-verse-x/Intelli-verse-X-SDK.git
cd Intelli-verse-X-SDK/SDKs/javascript
npm ci
npm run build
npm run example:phygital
```

The example uses local demo adapters. It prints five dispatches—one kiosk quest, one kiosk ad, and conversations on WhatsApp, chat, and voice—and returns `pending` acknowledgements. It sends no messages, places no calls, displays no paid ads, and grants no rewards.

## Connect a host-owned adapter

```typescript
import {
  IVXPhygitalApp,
  type IVXPhygitalAdapter,
} from '@intelliversex/sdk';

// yourKioskAdapter implements IVXPhygitalAdapter using your authenticated
// backend/device integration. There is deliberately no built-in public URL.
export function createKioskApp(yourKioskAdapter: IVXPhygitalAdapter) {
  return new IVXPhygitalApp({
    appId: 'your-app-id',
    name: 'Your app',
    channels: { kiosk: ['quest', 'ad'] },
  }, [yourKioskAdapter]);
}
```

After your application supplies the adapter:

```typescript
const app = createKioskApp(yourKioskAdapter);

if (app.supports('kiosk', 'quest')) {
  await app.dispatch('kiosk', operationId, {
    kind: 'quest',
    action: 'submit',
    questId: 'visit-kiosk',
    evidenceId: verifiedEvidenceReference,
  });
}
```

`operationId` comes from your application and must be reused for retries. `verifiedEvidenceReference` identifies evidence your backend can validate; a client-provided string is not itself proof. The adapter resolves `{ operationId, status: 'accepted' | 'pending' | 'rejected', referenceId? }`. The operation ID must match the request.

An acknowledgement only says the adapter accepted, queued, or rejected the operation. It is **not** a reward grant, a billable impression, or evidence of a completed quest. The toolkit has no wallet mutation API and does not render creative content.

## Provider and backend responsibilities

- Bind the authenticated actor to the app and channel server-side. An app ID is not an identity credential. Link identities across channels only through an authorized, consented flow.
- Authorize campaign, placement, quest, and evidence IDs against the app and physical location. IDs from a browser or kiosk are untrusted.
- Deduplicate operation IDs durably, scoped to the authenticated actor, app, and operation. Verify quest evidence and reward eligibility before awarding anything.
- Keep provider keys on your backend. Avoid storing phone numbers, transcripts, raw audio, or access tokens in manifests, operation IDs, logs, or evidence references.
- Enforce messaging consent, ad disclosures, placement eligibility, campaign budgets, and provider requirements in the connected system.
- Fail unavailable channels explicitly. `supports()` returns false unless both the manifest and an installed adapter allow the capability. There is no silent fallback to another identity/channel.
- Handle provider failures and retries in your application. The SDK propagates errors and does not automatically replay operations with side effects.

## Product story

**ContentX creates the content. QuestX gives people quests and rewards. KioskX connects physical places.** The Intelliverse SDK gives builders a common app contract to connect these pieces. Connected kiosk quests and campaigns remain a preview and depend on participating locations. WhatsApp and voice experiences require separately connected providers.

## Contribute

Try the example, open an issue describing your channel and use case, or contribute an adapter with tests and setup instructions. Label mocks and production integrations clearly. Before calling an adapter production-ready, demonstrate authorization, retries, durable deduplication, failure handling, and the provider/device integration it actually uses.

Start with the [JavaScript implementation](https://github.com/intelli-verse-x/Intelli-verse-X-SDK/blob/main/SDKs/javascript/src/IVXPhygitalApp.ts), [tests](https://github.com/intelli-verse-x/Intelli-verse-X-SDK/blob/main/SDKs/javascript/src/__tests__/IVXPhygitalApp.test.ts), and [example](https://github.com/intelli-verse-x/Intelli-verse-X-SDK/blob/main/SDKs/javascript/examples/phygital-app.mjs) in the GitHub repository.
