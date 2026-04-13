---
name: ivx-legal-compliance
description: Generates boilerplate legal documents (Privacy Policy, ToS) and auto-detects content ratings
version: 1.0.0
trigger_phrases:
  - "generate privacy policy"
  - "create terms of service"
  - "legal compliance"
  - "coppa compliance"
  - "auto-detect content rating"
---

# IntelliVerseX Legal Compliance Skill

## Overview
This skill generates necessary legal documents for game distribution and store submission. It runs the `legal_gen.py` pipeline.

## Capabilities

1. **Privacy Policy**: Generates a standard privacy policy, automatically including COPPA compliance clauses if the target audience is kids.
2. **Terms of Service**: Generates standard ToS with clauses tailored to the game's monetization strategy (e.g., virtual currency disclaimers).
3. **Content Rating**: Auto-detects ESRB, PEGI, and IARC ratings based on GDD content descriptors and monetization methods.
4. **Council Review**: Passes generated documents through the Model Council's compliance check for brand safety and legal claims verification.
5. **S3 Deployment**: Mocks or handles S3 deployment, returning public URLs required for App Store and Google Play submissions.

## Usage

To generate legal documents, run the pipeline:

```bash
python -m pipelines.runner run \
  --config configs/pipelines/legal_gen.yaml \
  --args '{"brand_id":"<brand_id>","game_id":"<game_id>"}'
```

## Integration
This pipeline is integrated into the `ivx_full_game` orchestrator, running in Phase 2 (Store & Legal) to ensure all legal URLs are available before store submission stages.
