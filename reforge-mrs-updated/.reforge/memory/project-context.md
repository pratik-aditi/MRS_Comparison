# Project Context

## Project Identity
- **Name**: TMDS-MRS (Train Management/Dispatch System — MRS railroad variant)
- **Description**: Legacy VB.NET traffic/train dispatch and signaling control system (`TMDS` namespace), with railroad-specific business rules for MRS (Mount Rainier Scenic / regional short-line, inferred from `_RAILROAD-SPECIFIC/_MRS`) and a `_CRC` variant. Covers track authority (warrants/single-form authority), signal interlocking, switches, track circuits, train ID/routing, and PTC (Positive Train Control) integration.
- **Root Path**: `../legacy` (relative to `reforge-mrs-updated/`)

## Technology Stack
- **Languages**: VB.NET (`Option Strict On`), targeting .NET (uses `System.Data.OleDb`, `System.Reflection`)
- **Frameworks**: None detected (no `.csproj`/`.vbproj`/`packages.config` present in the provided slice — this appears to be an extracted business-logic subset, not the full solution)
- **Build System**: Unknown — no project/solution file included in this codebase slice
- **Database**: OleDb-based data access referenced (`Imports System.Data.OleDb`); extent of actual SQL usage to be confirmed by `/reforge.extract database`
- **Other**: Namespace structure: `TMDS.GraphicComponent`, `TMDS.JoinAuthorization`, `TMDS.ManagementClasses`, `TMDS.SystemUtilities.Utilities`, `TMDS.TrackComponents`, `TMDS.TrainComponents`

## Domain Knowledge
Railroad traffic control / dispatch domain. **MRS = MRS Logística S.A.**, a major Brazilian freight railroad (iron-ore corridor) — confirmed by Portuguese-language UI strings in `modComboBoxListItems.vb` (e.g. "SINAL INOPERANTE", "TRILHO PARTIDO", "CHAVE INOPERANTE"). Core concepts observed in code: Track Authority (single-form authority, track warrants), Signals (interlocking, join authorization, PSS = per-reason-code dropdown for signal/switch/track faults), Switches, Track circuits/components, Train identification and routing (fast routes, grouped routes, mobile tracking, route scanning), PTC (Positive Train Control) integration using ATCS-style CRC-31/32 framing (suggests wayside-equipment protocol compatibility). A class named `RequestAuthorizationGoingToRumo`/`CancelAuthorizationGoingToRumo` suggests join-track interchange with Rumo, another major Brazilian rail operator. The `_RAILROAD-SPECIFIC` directory separates generic TMDS platform logic from per-railroad customization (`_MRS`, `_CRC`).

## Analysis Guidelines
- Focus directory: `1_business_logic/` (the only top-level directory present in this slice)
- Largest/highest-risk files: `clsComponentTrack.vb` (44K lines), `clsManagement.vb` (30.9K lines), `clsTrackAuthority.vb` (30.3K lines) — these carry the bulk of complexity
- `_RAILROAD-SPECIFIC` subfolders represent railroad-specific customization points — flag any coupling between generic TMDS classes and these customizations as a modernization risk (partial classes, e.g. `Partial Public Class Signal` extended in `Signal_JoinAuthorization.vb`)
- No UI/presentation layer files present in this slice beyond `frm*.vb` (WinForms code-behind) — treat these as UI+logic mixed files

## Custom Rules
- VB.NET `Partial Class` pattern is used to split class logic across files (e.g. `Signal` class split between core component file and `_MRS/BusinessRule/Signal_JoinAuthorization.vb`) — extraction must track partial class membership, not just per-file class boundaries
- XML doc comments (`'''`) are present in places — treat these as a confidence signal (high) for business rule/purpose extraction where available
- Treat this as a safety-relevant system (railroad signaling/authority) — flag business rules related to track authority, signal permissives, and PTC with elevated scrutiny in issues/audit outputs

*(Context inferred by the AI agent from static inspection, not interactively gathered — no stakeholder interview was conducted for this run.)*
