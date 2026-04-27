#!/usr/bin/env node
// Generate TS sources from proto schemas, then mirror them into:
//   - nakama/data/modules/src/multiplayer-kernel/proto/v1/
//   - Intelli-verse-X-SDK/SDKs/javascript/multiplayer/src/proto/v1/
//
// Idempotent. Safe to run on every CI build.
import { execSync } from "node:child_process";
import { mkdirSync, cpSync, rmSync, existsSync } from "node:fs";
import { resolve, dirname } from "node:path";
import { fileURLToPath } from "node:url";

const __filename = fileURLToPath(import.meta.url);
const __dirname  = dirname(__filename);
const root       = resolve(__dirname, "..");
const repoRoot   = resolve(root, "..", "..");
const sdkRoot    = repoRoot;
const nakamaRoot = resolve(repoRoot, "..", "nakama");

const tsOut       = resolve(root, "gen", "ts");
const nakamaProto = resolve(nakamaRoot, "data", "modules", "src", "multiplayer-kernel", "proto", "v1");
const jsPkgProto  = resolve(sdkRoot, "SDKs", "javascript", "multiplayer", "src", "proto", "v1");

console.log("[codegen-ts] running buf generate");
execSync("buf generate buf.gen.yaml", { stdio: "inherit", cwd: root });

if (!existsSync(tsOut)) {
  console.error(`[codegen-ts] expected output at ${tsOut}`);
  process.exit(1);
}

for (const dst of [nakamaProto, jsPkgProto]) {
  if (existsSync(dst)) rmSync(dst, { recursive: true, force: true });
  mkdirSync(dst, { recursive: true });
  cpSync(tsOut, dst, { recursive: true });
  console.log(`[codegen-ts] mirrored to ${dst}`);
}
