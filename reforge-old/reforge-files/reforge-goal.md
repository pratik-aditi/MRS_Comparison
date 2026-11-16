Run Reforge exhaustively. Documentation only — NEVER /reforge.estimate, /reforge.migrate, /reforge.recommend, or Jira-style user-story backlogs. Output is "how this app works today."

EXTRACTION-BRIEF + GAP-BRIEF mandates from day 1:
- Every <snippet> wrapped in <![CDATA[...]]>. No exceptions. Raw <> & break XML parse.
- When a type doesn't apply, emit <no-extraction type="X" file="..." reason="..."/> NOT "no-artifacts" status. Preserves examined-vs-skipped distinction.
- Read line 1 → EOF contiguously via Read with iterative offset. Cite final line per file. Forbidden: "structurally surveyed", "spot-checked via grep", "sampled section".
- Write fragment XMLs every ~2000 lines, not batched at end (transport-failure resilience).
- If Write tool denied, fall back to Bash heredoc: cat > path << 'XMLEOF' ... XMLEOF.

merge_clusters.py auto-discovers cluster dirs from start:
  UNITS = sorted([d for d in os.listdir(CL) if os.path.isdir(os.path.join(CL,d)) and not d.startswith(('_','.'))])
Hard-coded UNITS silently drop GAP batches.

Phase 1 — file-oriented batches (NOT type-oriented). Bucket ~25 files/batch, ≤30K LOC each, sort by LOC desc. Each batch handles ALL 8 types per file. Type-oriented batches force re-reading large files multiple times.

Spawn 5–6 concurrent Sonnet agents. Each runs quality gate before reporting done:
  python3 -c "import xml.etree.ElementTree as ET, glob; [ET.parse(f) for f in glob.glob('*.xml')]; print('ok')"

Reject-and-retry: final-line-read < LOC; raw < or & outside CDATA; >30% no-extraction; <40 real artifacts in 25-file batch.

Phase 2 — validate continuously: every 3-4 batches run merge → coverage_matrix.py → validate. coverage_matrix.py must: count <no-extraction> as covered; normalize source paths (strip legacy-root prefix); use fair per-type denominators (DB = SQL-bearing only); auto-generate next batches from uncovered files.

Phase 3 — iterate until ≥95% per type. When 2 consecutive rounds add <50 artifacts, push final wave focused on emitting <no-extraction> for legitimately-empty files (DTOs, marker interfaces, Designer.vb, AssemblyInfo). Takes ISS/NFR from ~70% to ≥95% without fabrication.

Phase 4 — derived artifacts in parallel (3 sub-agents): trace, dead-code, hotspot. Frame hotspot as "where code is fragile," not modernization investment.

Phase 5 — documentation: _gen/generate.py covers only a subset. Launch doc agent to build business-rules.html, FR.html, workflows.html, database.html, state-machines.html, hotspot.html, dead-code.html, trace.html (summary-only), refreshed dashboard.html and index.html, milestone report, 6 CSV exports.

Phase 6 — diagrams from day one: ≥10 architecture/sequence diagrams + 1 per state machine + 1 per UI subdomain. PlantUML CLI (/opt/homebrew/bin/plantuml, Java). Sources in diagrams/sources/, SVGs in diagrams/rendered/. Embed in HTML pages via <img>.

Phase 7 — user-stories.xml (50-80 stories, ~10 epics) cross-linked to real BR/FR/WF IDs. Render user-stories.html with embedded diagrams.

Phase 8 — generated/reports/documentation-audit.html rating every HTML page, enumerating diagram coverage, naming gaps honestly.

Operating rules:
- Sonnet for batches; Opus only for orchestration/synthesis
- Pre-create cluster dirs (mkdir -p) before sub-agent launch
- Track with TaskCreate/TaskUpdate; filesystem state (ls _clusters/) beats agent notifications across session restarts
- Max 5-6 concurrent agents; more = transport failures
- Never claim 95% without running coverage_matrix.py against merged XML — audit headers can lie about denominators

Done means: ≥95% per-type coverage, 0 validation errors/duplicate IDs/unresolved refs, 50+ HTML pages with diagrams, 50+ rendered SVGs (every state machine + UI subdomain), user-stories.html, milestone + audit reports, 6 stakeholder CSVs, honest "what we didn't do" callouts.

Start by reading existing reforge_output state, then proceed.
