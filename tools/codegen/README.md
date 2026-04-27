# IVX Multiplayer Codegen

Generates TypeScript and C# from the canonical proto3 schemas in `schemas/multiplayer/` and `schemas/avatar/`, then mirrors the outputs into:

| Destination                                                                  | Purpose                          |
| ---------------------------------------------------------------------------- | -------------------------------- |
| `nakama/data/modules/src/multiplayer-kernel/proto/v1/`                       | Nakama TS runtime + game plugins |
| `Intelli-verse-X-SDK/SDKs/javascript/multiplayer/src/proto/v1/`              | `@intelliversex/multiplayer` npm |
| `Intelli-verse-X-SDK/Assets/_IntelliVerseXSDK/Multiplayer/Generated/V1/`     | Unity adapter                    |

> Future emitters (Go, Dart, Java, C++, Unreal): add a `gen-<lang>.mjs` script and a `buf.build/<plugin>` entry to `buf.gen.yaml`.

## Usage

```bash
cd Intelli-verse-X-SDK/tools/codegen
pnpm install     # one-time
pnpm gen         # lint + generate + mirror
pnpm ci          # lint + breaking-check + generate (used in CI)
```

## CI

`pnpm ci` is wired into `.github/workflows/proto-codegen.yml` (added by P12). Any PR that mutates a `.proto` runs `buf lint` and `buf breaking` against `main` before regenerating; downstream packages publish only when generation succeeds.

## Adding a new template

1. Add `schemas/multiplayer/templates/<name>.proto`.
2. Allocate a fresh opcode range in `schemas/multiplayer/opcodes.proto` (do **not** repurpose existing values).
3. Add the new file to `scripts/gen-js-pkg.mjs`'s barrel.
4. Run `pnpm gen` and commit the regenerated outputs.
