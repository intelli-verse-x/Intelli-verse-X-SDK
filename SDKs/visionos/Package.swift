// swift-tools-version:5.9
//
// IVXMultiplayer — visionOS / iOS / macOS Swift package.
//
// Bridges the IVX Multiplayer Kernel (a thin layer over Nakama) plus the
// LiveKit voice provider plus a RealityKit ECS spatial-frame translator.
// Targets visionOS 1.0+, iOS 17+, macOS 14+ (Universal App parity).

import PackageDescription

let package = Package(
    name: "IVXMultiplayer",
    platforms: [
        .iOS(.v17),
        .macOS(.v14),
        .visionOS(.v1)
    ],
    products: [
        .library(
            name: "IVXMultiplayer",
            targets: ["IVXMultiplayer"]
        )
    ],
    dependencies: [
        // nakama-cpp is delivered as a binary xcframework (see SDKs/nakama-cpp).
        // LiveKit Swift SDK from upstream:
        .package(url: "https://github.com/livekit/client-sdk-swift.git", from: "2.0.0")
    ],
    targets: [
        .target(
            name: "IVXMultiplayer",
            dependencies: [
                .product(name: "LiveKit", package: "client-sdk-swift")
            ],
            path: "Sources/IVXMultiplayer"
        )
    ]
)
