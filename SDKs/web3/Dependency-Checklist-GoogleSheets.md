# IntelliVerseX Web3 SDK — Dependency Checklist for Google Sheets

**Purpose:** Copy-paste ready checklist for developers. Paste into Google Sheets to track setup and release.

---

## How to Use

1. **Open** [ sheets.new ](https://sheets.new) (new Google Sheet)
2. **Open** `Dependency-Checklist-GoogleSheets.tsv` in a text editor
3. **Select all** (Ctrl+A) and **Copy**
4. **Paste** into cell A1 of the Google Sheet  
   → Tabs become columns, rows align
5. **Add checkboxes:** Select column A (Done), `Format → Number → Checkbox` or use `=CHECKBOX()` if needed
6. **Filter/sort** by Category, Done, etc.

---

## Column Guide

| Column | Meaning |
|--------|---------|
| **Done** | Check off when dependency is set up / verified |
| **Dependency** | Name of package, RPC, or service |
| **Category** | Runtime, Backend, Dev, Config, Platform |
| **Why I Need This** | Plain explanation for developers |
| **Tool / Platform** | What runs it (npm, Nakama, MetaMask, etc.) |
| **AI Use** | When to use Cursor / Copilot / ChatGPT for this item |
| **Version** | Min or recommended version |
| **Install / Notes** | Copy-paste command or setup note |

---

## Quick Reference

- **Runtime:** Must be installed by SDK consumers (`npm install` with SDK)
- **Backend:** Server-side; implement on Nakama
- **Dev:** Build/test only; not shipped
- **Config:** Keys, env vars; use `config/keys.json`
