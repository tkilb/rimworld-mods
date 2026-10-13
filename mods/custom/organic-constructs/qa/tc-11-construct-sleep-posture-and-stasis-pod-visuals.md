# TC-11: Construct Sleep Posture & Stasis Pod Visuals

## Objective
Verify that constructs strictly sleep face-up on their back in all bed types, that the stasis pod no longer emits purple light, and diagnose stasis pod / pawn rotational alignment across different compass headings and gravship landings.

---

### Phase 1: Routine Bed Sleep Posture
- **Procedure:**
  1. Have 2 or more constructs assigned to normal beds or sleeping spots.
  2. Wait until night / sleep timetable, or draft and force rest.
  3. Observe the sleep facing orientation of each construct.
- **Expected:**
  - Constructs lie flat on their back facing South (face-up towards the player/camera).
  - No construct renders on their side facing East or West.

---

### Phase 2: Stasis Pod Visuals & Glow Removal
- **Procedure:**
  1. Build a `Construct Stasis Pod` and connect it to power.
  2. Order a construct into active stasis (`Enter Stasis`).
  3. Inspect the pod in normal room lighting and in the dark.
- **Expected:**
  - The construct's head is visible inside the pod opening.
  - **No purple glow/light effect** illuminates the surrounding tiles or room.
  - Active stasis texture / status indicator displays correctly.

---

### Phase 3: Stasis Pod 4-Way Compass Rotation
- **Procedure:**
  1. Place four stasis pods facing each cardinal direction: **South**, **East**, **North**, **West**.
  2. Order a construct to enter each pod (or move one construct between them).
  3. Observe the pod sprite and pawn head alignment for each rotation.
- **Observations to Record:**
  - [ ] **South:** Does the pod face front? Is the head centered and facing front?
  - [ ] **East:** Does the pod face right? Does the head align with the opening, or does it clip outside?
  - [ ] **North:** Does the pod show its rear/back? Is the head hidden or visible?
  - [ ] **West:** Does the pod face left? Does the head align with the opening, or does it clip outside?

---

### Phase 4: Gravship Rotation & Relocation (If Applicable)
- **Procedure:**
  1. Install/place stasis pods aboard the gravship.
  2. Launch and land the gravship at a new destination that alters ship heading/rotation.
  3. Order a construct into a pod on the newly landed ship.
- **Observations to Record:**
  - [ ] Did the pod sprite rotate consistently with the ship's orientation?
  - [ ] Does the pawn head remain properly oriented and aligned within the pod alcove?
  - [ ] If misaligned, describe the offset (e.g. "head displaced 1 tile north", "head facing 90° away from pod doorway").
