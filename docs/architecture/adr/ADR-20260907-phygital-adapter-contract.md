# Phygital app contract in the JavaScript SDK

Date: 2026-09-07. Scope: user-requested SDK support for Phygital apps and an omnipresent app vision.

Add `IVXPhygitalApp` inside the existing JavaScript package, with a manifest, validated intent types, and explicit host-owned channel adapters. This is an additive helper, not a new backend, authentication flow, Unity assembly, or advertising network. No third-party SDK source is modified.

A capability is available only when declared by the app and supported by an installed adapter. Requests are immutable copies that exclude unknown fields. Quest submission carries an opaque evidence reference; acknowledgements cannot grant rewards. Backend authorization, proof verification, consent, budget enforcement, and durable deduplication remain provider responsibilities.

The six channel labels describe mobile/web/kiosk/WhatsApp/chat/voice entry points. They do not imply bundled provider implementations or cross-engine parity. Runtime support starts in JavaScript 5.9.0, available from source; publishing a package is a separate release action.

Build validation uncovered existing Nakama RPC payload type errors and missing WebXR declarations. Correct the wrapper to pass the object expected by the installed Nakama client and retain support for object or legacy string responses. Include the WebXR type-only package required by the SDK's existing public WebXR declarations. No new network endpoints are added.
