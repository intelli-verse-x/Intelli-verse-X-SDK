// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.
//
// Cocos2d-x bridge for the IntelliVerseX Multiplayer Kernel.
//
// Cocos2d-x is just C++ at runtime, so this file is a thin re-export of the
// canonical adapter `SDKs/cpp/include/intelliversex/ivx_multiplayer_kernel.h`.
// The bridge exists so Cocos2d-x project templates can include
//   #include "IntelliVerseX/IVXMultiplayerKernel.h"
// without leaking the SDKs/cpp dependency layout into game code.
//
// Build wiring lives in `SDKs/cocos2dx/CMakeLists.txt` (adds the C++ adapter
// as a submodule and propagates `-I` to the cocos2d project).

#pragma once

#include "intelliversex/ivx_multiplayer_kernel.h"

namespace ivx { namespace cocos2dx {

using ::ivx::multiplayer::MultiplayerKernel;
using ::ivx::multiplayer::MatchSession;
using ::ivx::multiplayer::Envelope;
using ::ivx::multiplayer::Header;
using ::ivx::multiplayer::CreateMatchRequest;
using ::ivx::multiplayer::CreateMatchResponse;
using ::ivx::multiplayer::TransportState;
using ::ivx::multiplayer::EndReason;
using ::ivx::multiplayer::EnvelopeHandler;
using ::ivx::multiplayer::StateHandler;
using ::ivx::multiplayer::Subscription;

}} // namespace ivx::cocos2dx
