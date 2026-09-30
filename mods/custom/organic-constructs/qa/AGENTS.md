# AGENTS.md — QA Procedures & Test Cases

## Role & Context

- **Human In-The-Loop:** The human user executes all QA test cases in-game. **Agents cannot run or playtest the mod in RimWorld.**
- **Agent Role:** Test Case Author and Compiler Verifier. The agent's responsibility is to draft precise, deterministic test procedures for the human tester to follow, verify code compilation (`dotnet build`), and maintain the test catalog.
- **Context:** Refer to [../spec.md](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/organic-constructs/spec.md) for mod architecture, Defs, and game mechanics. Root test tracking is at [qa.md](qa.md).
- **Directory Scope:** This directory (`qa/`) contains all modular QA test cases written for the human tester.

---

## File Naming Convention

- **Format:** `tc-<number>-<kebab-case-slug>.md`
- **Numbering:** Zero-padded two digits (`01`, `02`, etc.) matching the test case ID.
- **Example:** `tc-01-construct-synthesizer-disc-operation.md`

---

## Desired Test Case Format & Template

Every test case file must follow this Markdown structure:

```markdown
# TC-XX: <Descriptive Title>

- **Procedure:**
  1. <Specific action or setup, using exact defNames (e.g., `ConstructSynthesizer`, `GenomeBlueprintDisk`).>
  2. <Step-by-step action to execute.>
  3. <Further interaction, pawn order, or gizmo activation.>
- **Expected:**
  - <Clear, verifiable outcome (e.g., inspect string content, container status).>
  - <Hediff applied, gene changes, or error/rejection messages.>
  - <Behavioral checks, thought states, or UI validation.>
```

---

## Guidelines for Authors

1. **Concrete defNames:** Always cite exact `Def` names (e.g., `NeuralScanner`, `Construct_NeuralFatigue`, `CompGrowthVatImprinter`) so testers can easily use Dev Mode to spawn things.
2. **Deterministic steps:** Steps must be clearly sequence-ordered and minimal to reproduce and verify the target feature.
3. **Explicit negative cases:** When testing restrictions (e.g., constructs rejected from scanners, locked xenogerm surgery), explicitly state both the failed condition and the expected rejection feedback/message.
4. **Index Synchronization:** Whenever adding or updating a test case file in `qa/`, keep the test cases index in [qa.md](qa.md) synchronized.
