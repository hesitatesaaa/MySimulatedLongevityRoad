---
name: token-efficient-workflow
description: Always-on workflow for reducing unnecessary context in Codex tasks without lowering correctness, safety, or requested detail.
---

# Token-Efficient Workflow

Apply automatically to every task. Optimize context use, not merely file length: preserve the user's scope, required verification, safety, and output quality.

## Default workflow

1. **Intake:** identify the exact artifact or behavior, likely high-signal files, and the smallest check that can confirm the next fact.
2. **Search:** use `rg`/`rg --files` with a focused path and term. Start with metadata, entrypoints, routes/config, then the implicated code or logs. Exclude generated, vendor, dependency, image, and unrelated history directories unless evidence requires them.
3. **Read incrementally:** inspect headers, schemas, ranges, or samples before loading large files; expand only when the current evidence is insufficient.
4. **Control output:** cap command results, extract facts/paths/lines, and summarize them for reuse. If output is truncated, narrow the next command instead of repeating the scan.
5. **Close the loop:** make the requested change only, verify it in proportion to risk, and report the result, evidence, and any remaining limitation concisely.

## Long tasks

Maintain a compact working state: goal, confirmed facts, files changed/inspected, decisions, unknowns, and next action. Reuse it after follow-ups; do not rescan unchanged areas.

## Specialized routing

Read [references/specialized.md](references/specialized.md) only when the task involves browser/UI debugging, PHP/ThinkPHP or deployment, large documents/data, or a large repository requiring staged context.

## Non-negotiable boundary

Token savings never justify skipping needed context, tests, security checks, user-requested detail, or a safe stopping condition.

