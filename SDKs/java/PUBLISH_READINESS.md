# IntelliVerseX Java SDK — Publish Readiness

## Maven Central registration (current process)

**OSSRH / issues.sonatype.org is retired.** Use the **Central Publisher Portal** instead:

1. **Sign up:** [https://central.sonatype.com](https://central.sonatype.com) → Sign In (top right) → create account (Google, GitHub, or email).
2. **Register a namespace:** [Register a Namespace](https://central.sonatype.com/register/namespace) — register `ai.intelli-verse-x` (or your groupId). You may need to prove ownership (e.g. DNS or code-hosting).
3. **Support:** [Central Support](https://central.sonatype.com/help) or email **central-support@sonatype.com** (e.g. to add publishers to an existing namespace). Do **not** send passwords or private keys.

Docs: [Register via Central Portal](https://central.sonatype.org/register/central-portal) · [Publish with Maven/Gradle](https://central.sonatype.org/publish/publish-portal-maven)

---

## Doing order (updated after Tasks 1–6)

| # | Task | Type | Status |
|---|------|------|--------|
| **1** | Add Gradle wrapper | AI | ✅ Done |
| **2** | Fix build errors | AI | ✅ Done |
| **3** | Verify JUnit tests pass | AI | ✅ Done |
| **4** | Integrate BasicExample into Gradle build | AI | ✅ Done |
| **5** | License headers in Java sources | AI | ✅ Done |
| **6** | Verify/fix Javadoc generation | AI | ✅ Done |
| **7** | Create Central Portal account + register namespace `ai.intelli-verse-x` | **Manual** | Pending |
| **8** | Generate GPG signing key | **Manual** | Pending |
| **9** | Configure Gradle maven-publish for Central Portal | AI | ✅ Done |
| **10** | README updates | AI | ✅ Done (Nakama v2.5+, runExample) |
| **11** | CHANGELOG updates | AI | Pending |
| **12** | Publish ai.intelli-verse-x:sdk to Maven Central | **Manual** | Pending |

## Next steps (7–9, 11–12)

- **7:** Central Portal sign-up + namespace registration (see section above). No Jira / issues.sonatype.org.
- **8:** Generate GPG key and publish public key (required for signing artifacts).
- **9:** ✅ Done — Signing + OSSRH Staging API (Central Portal). See “Publishing (Step 12)” below.
- **11:** Add Java SDK release notes to repo CHANGELOG.
- **12:** Run publish (see below); then send upload to Portal and publish from central.sonatype.com.

---

## Publishing (Step 12)

### 1. Get a Central Portal user token

- Log in at [central.sonatype.com](https://central.sonatype.com) → click your username → **View User Tokens**.
- Create a token and copy **username** and **password** (shown once).

### 2. Configure credentials and GPG

- Copy `gradle.properties.example` to `gradle.properties` (or add to `~/.gradle/gradle.properties`). Do **not** commit real values.
- Set `ossrhStagingApiUsername` and `ossrhStagingApiPassword` to your token username and password.
- Set either **Option A** (GPG command line) or **Option B** (key ring file) in `gradle.properties` as in the example.

### 3. Build, sign, and upload

```bash
./gradlew publish
```

This builds, signs, and uploads to the OSSRH Staging API.

### 4. Send the upload to the Central Portal

The staging API does **not** show the deployment on [central.sonatype.com](https://central.sonatype.com) until you trigger the transfer. From the **same machine/IP** that ran `publish`, run once (replace `TOKEN_USERNAME` and `TOKEN_PASSWORD` with your token). The API expects **Bearer** auth with base64(username:password):

**Git Bash / Linux / macOS:**

```bash
curl -H "Authorization: Bearer $(echo -n 'TOKEN_USERNAME:TOKEN_PASSWORD' | base64)" -X POST "https://ossrh-staging-api.central.sonatype.com/manual/upload/defaultRepository/ai.intelli-verse-x"
```

**PowerShell (Windows):**

```powershell
$pair = "TOKEN_USERNAME:TOKEN_PASSWORD"
$b64 = [Convert]::ToBase64String([System.Text.Encoding]::UTF8.GetBytes($pair))
Invoke-RestMethod -Uri "https://ossrh-staging-api.central.sonatype.com/manual/upload/defaultRepository/ai.intelli-verse-x" -Method Post -Headers @{ Authorization = "Bearer $b64" }
```

### 5. Publish on Central

- Open [central.sonatype.com/publishing/deployments](https://central.sonatype.com/publishing/deployments).
- Find the new deployment, check it, then click **Publish** to sync to Maven Central.

---

## Troubleshooting

### 401 Unauthorized

- Use a **Central Portal user token** (View User Tokens), not your account login. Old OSSRH tokens no longer work.

### 400 Bad Request on first PUT

If `./gradlew publish` fails with **400 Bad Request** when uploading the first artifact:

1. **Namespace** — Ensure `ai.intelli-verse-x` is **verified** in [central.sonatype.com → Publishing → Namespaces](https://central.sonatype.com/publishing/namespaces). You must register and verify the namespace before deploy.
2. **POM dependency** — The POM depends on `com.github.heroiclabs:nakama-java` (JitPack). Central expects dependencies to be resolvable from Central; if validation rejects non-Central dependencies, consider making that dependency `optional` or switching to an artifact that is published on Maven Central, then contact [Central Support](mailto:central-support@sonatype.com) if needed.
3. **Response body** — Gradle does not log the server’s error body. To get the exact reason, contact **central-support@sonatype.com** with your groupId, artifactId, version, and that you get 400 on PUT to the staging deploy URL; they can confirm the cause.

### Signing: “A problem occurred starting process 'command gpg.exe'”

- Install [Gpg4win](https://www.gpg4win.org/) and add `C:\Program Files\GnuPG\bin` to your PATH (or run `./gradlew publish` from a terminal where `gpg --version` works).
