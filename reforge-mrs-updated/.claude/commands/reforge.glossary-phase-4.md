---
description: Independent quality gate — re-verify entries modified in Phase 3 and all low-confidence entries by re-running verification from scratch. The agent MUST NOT trust Phase 3 results; every check is performed independently.
---

## User Input

```text
$ARGUMENTS
```

You **MUST** consider the user input before proceeding (if not empty).

## Execution

Load and execute the full command template at `.reforge/commands/glossary-phase-4.md`.
Follow all steps defined in that file exactly as written, passing through any
arguments provided above.
