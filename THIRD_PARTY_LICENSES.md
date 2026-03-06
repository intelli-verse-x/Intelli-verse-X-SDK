# Third-Party Licenses

This document lists third-party software used by or referenced by the IntelliVerseX SDK and its distribution channels. The IntelliVerseX SDK itself is MIT-licensed; dependencies may use different licenses.

## Backend & Networking

- **Nakama** (Heroic Labs) – Backend client libraries (Unity, Unreal, Godot, Defold, Cocos2d-x, JavaScript, C++, Java, Flutter).  
  - [nakama-unity](https://github.com/heroiclabs/nakama-unity), [nakama-js](https://github.com/heroiclabs/nakama-js), [nakama-cpp](https://github.com/heroiclabs/nakama-cpp), [nakama-java](https://github.com/heroiclabs/nakama-java), [nakama](https://pub.dev/packages/nakama) (Dart).  
  - License: Apache-2.0. See each repository for full text.

- **Photon PUN 2** (Photon Engine) – Optional; used for real-time multiplayer when integrated by the game.  
  - Not bundled with this SDK; install separately from Unity Asset Store or Photon.  
  - License: Photon Engine terms. See [Photon Engine](https://www.photonengine.com/).

## Unity-Only Dependencies (when using Unity SDK)

- **DOTween** (Demiurge Studios) – Animation utility. Optional; used by some UI modules.  
  - Not bundled; install via Asset Store or OpenUPM.  
  - License: DOTween license (see Asset Store or package).

- **Newtonsoft.Json** – JSON serialization. Often included via Unity’s package manager.  
  - License: MIT. See [Newtonsoft.Json](https://www.newtonsoft.com/json).

- **TextMeshPro** – Unity’s text rendering.  
  - License: Unity Companion License. See Unity documentation.

## JavaScript / TypeScript SDKs

- **@heroiclabs/nakama-js** – Nakama JavaScript client.  
  - License: Apache-2.0.

- **ethers** (Web3 SDK only) – Ethereum library for wallet and contract interaction.  
  - License: MIT. See [ethers](https://github.com/ethersio/ethers.js).

## Flutter / Dart SDK

- **nakama** (pub.dev) – Nakama Dart client.  
  - License: Apache-2.0. See package on pub.dev.

---

This list is not exhaustive. For each dependency, refer to its official source and license file. If you distribute a binary or bundle that includes any of these, you are responsible for complying with their licenses.
