#!/usr/bin/env node
// Mirror the generated TS into the published JS adapter package.
// (gen-ts.mjs already does the mirror; this script is a hook for any
//  package-specific transforms — e.g. emitting a barrel index.)
import { writeFileSync, existsSync, mkdirSync } from "node:fs";
import { resolve, dirname } from "node:path";
import { fileURLToPath } from "node:url";

const __filename = fileURLToPath(import.meta.url);
const __dirname  = dirname(__filename);
const sdkRoot    = resolve(__dirname, "..", "..", "..");
const jsPkgProto = resolve(sdkRoot, "SDKs", "javascript", "multiplayer", "src", "proto", "v1");

if (!existsSync(jsPkgProto)) {
  mkdirSync(jsPkgProto, { recursive: true });
}

const barrel = `// Auto-generated barrel for IVX Multiplayer V1 protos.
export * from "./envelope_pb.js";
export * from "./opcodes_pb.js";
export * from "./kernel_pb.js";
export * from "./templates/sync_turn_pb.js";
export * from "./templates/async_turn_pb.js";
export * from "./templates/realtime_tick_pb.js";
export * from "./templates/lobby_handoff_pb.js";
export * from "./templates/tournament_pb.js";
export * from "./templates/live_event_pb.js";
export * from "./templates/persistent_party_pb.js";
export * from "./templates/avatar_replication_pb.js";
export * from "./templates/mixed_reality_anchor_pb.js";
export * from "./templates/conversational_party_pb.js";
export * from "./services/agent_pb.js";
export * from "./services/moderation_pb.js";
export * from "./games/quizverse_pb.js";
`;

writeFileSync(resolve(jsPkgProto, "index.ts"), barrel);
console.log(`[codegen-js-pkg] wrote barrel index at ${jsPkgProto}/index.ts`);
