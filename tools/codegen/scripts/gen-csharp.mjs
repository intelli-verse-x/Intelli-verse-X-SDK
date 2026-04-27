#!/usr/bin/env node
// Generate C# sources and mirror them into the Unity adapter.
// Output: Intelli-verse-X-SDK/Assets/_IntelliVerseXSDK/Multiplayer/Generated/V1/
import { execSync } from "node:child_process";
import { mkdirSync, cpSync, rmSync, existsSync } from "node:fs";
import { resolve, dirname } from "node:path";
import { fileURLToPath } from "node:url";

const __filename = fileURLToPath(import.meta.url);
const __dirname  = dirname(__filename);
const root       = resolve(__dirname, "..");
const sdkRoot    = resolve(root, "..", "..");

const csOut    = resolve(root, "gen", "csharp");
const unityOut = resolve(sdkRoot, "Assets", "_IntelliVerseXSDK", "Multiplayer", "Generated", "V1");

console.log("[codegen-csharp] running buf generate (csharp)");
execSync("buf generate --include-imports buf.gen.yaml", { stdio: "inherit", cwd: root });

if (!existsSync(csOut)) {
  console.error(`[codegen-csharp] expected output at ${csOut}`);
  process.exit(1);
}

if (existsSync(unityOut)) rmSync(unityOut, { recursive: true, force: true });
mkdirSync(unityOut, { recursive: true });
cpSync(csOut, unityOut, { recursive: true });
console.log(`[codegen-csharp] mirrored to ${unityOut}`);
