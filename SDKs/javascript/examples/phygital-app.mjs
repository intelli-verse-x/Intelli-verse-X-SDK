import { IVXPhygitalApp } from '../dist/index.mjs';

// Local demonstration only. These adapters do not send messages, place calls,
// display paid ads, verify kiosk hardware, or grant rewards.
const demoAdapter = (channel, capabilities) => ({
  channel,
  capabilities,
  async handle(request) {
    console.log(`[demo] ${request.appId} / ${request.channel} / ${request.intent.kind}`);
    return { operationId: request.operationId, status: 'pending', referenceId: 'demo-only' };
  },
});

const app = new IVXPhygitalApp({
  appId: 'trivia-demo',
  name: 'Trivia wherever you are',
  channels: {
    kiosk: ['quest', 'ad'],
    whatsapp: ['conversation'],
    chat: ['conversation'],
    voice: ['conversation'],
  },
}, [
  demoAdapter('kiosk', ['quest', 'ad']),
  demoAdapter('whatsapp', ['conversation']),
  demoAdapter('chat', ['conversation']),
  demoAdapter('voice', ['conversation']),
]);

await app.dispatch('kiosk', 'demo-quest-1', { kind: 'quest', action: 'start', questId: 'visit-trivia-kiosk' });
await app.dispatch('kiosk', 'demo-ad-1', { kind: 'ad', campaignId: 'discover-trivia', placementId: 'welcome-screen' });
for (const channel of ['whatsapp', 'chat', 'voice']) {
  await app.dispatch(channel, `demo-${channel}-1`, { kind: 'conversation', text: 'What can I do here?', conversationId: 'demo-conversation' });
}
console.log('Demo complete. Connect real adapters to deliver these experiences. No rewards were granted.');
