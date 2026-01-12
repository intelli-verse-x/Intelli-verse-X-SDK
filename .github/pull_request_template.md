# Pull Request

## Description
<!-- Describe your changes in detail -->

## Type of Change
<!-- Mark the relevant option with an 'x' -->

- [ ] 🐛 Bug fix (non-breaking change that fixes an issue)
- [ ] ✨ New feature (non-breaking change that adds functionality)
- [ ] 💥 Breaking change (fix or feature that would cause existing functionality to change)
- [ ] 📝 Documentation update
- [ ] 🔧 Configuration change
- [ ] ♻️ Refactor (no functional changes)
- [ ] 🧪 Test update

## Related Issues
<!-- Link any related issues here -->
Fixes #

---

## 🛡️ Guardrails Checklist (REQUIRED)

### P0 - Must Pass (Blocking)

- [ ] **No read-only zone modifications**
  - [ ] Did not modify `Assets/Nakama/`
  - [ ] Did not modify `Assets/Photon/`
  - [ ] Did not modify `Assets/Appodeal/`
  - [ ] Did not modify `Assets/LevelPlay/`
  - [ ] Did not modify `Assets/AppleAuth/`
  - [ ] Did not modify `Assets/Plugins/Demigiant/`

- [ ] **No layer violations**
  - [ ] Public API does not expose third-party types
  - [ ] No circular dependencies introduced

- [ ] **Security**
  - [ ] No hardcoded secrets or credentials
  - [ ] No PII logged in plain text

- [ ] **Compilation**
  - [ ] Code compiles without errors
  - [ ] No new compiler warnings

### P1 - Should Pass

- [ ] **Naming conventions followed**
  - [ ] IVX prefix on all public types
  - [ ] Correct namespace (`IntelliVerseX.*`)
  - [ ] Private fields use `_camelCase`

- [ ] **Code quality**
  - [ ] No `Find()`/`GetComponent()` in Update loops
  - [ ] Singleton access null-checked
  - [ ] Async methods have try-catch
  - [ ] No `// TODO` in production code

- [ ] **Documentation**
  - [ ] XML docs on public APIs
  - [ ] CHANGELOG.md updated (if applicable)

### P2 - Nice to Have

- [ ] Unit tests added/updated
- [ ] AGENT.md updated (if new scripts added)
- [ ] Performance considered (no hot path allocations)

---

## 📋 Context Engineering Checklist

- [ ] Checked `.cursor/NON_GOALS.md` for scope boundaries
- [ ] Followed `.cursor/naming-and-style.md` conventions
- [ ] Verified against `.cursor/architecture.md` rules
- [ ] Checked `.cursor/ANTI_PATTERNS.md` before coding
- [ ] Updated `.cursor/assumptions.md` if assumptions changed

---

## 🧪 Testing

### How has this been tested?
<!-- Describe the tests you ran -->

- [ ] Unit tests
- [ ] Manual testing in Unity Editor
- [ ] Tested on target platform(s)

### Test Configuration
- Unity Version: 
- Platform(s): 

---

## 📸 Screenshots (if applicable)
<!-- Add screenshots to help explain your changes -->

---

## 📝 Additional Notes
<!-- Any additional information that reviewers should know -->

---

## 🔍 Reviewer Checklist
<!-- For reviewers to complete -->

- [ ] Code follows project conventions
- [ ] Changes are within scope
- [ ] No guardrail violations
- [ ] Tests are adequate
- [ ] Documentation is sufficient
