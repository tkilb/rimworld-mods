# TC-05: Construct Stasis & Maintenance

- **Procedure:**
  1. Have construct operate past warning ticks or trigger "Enter Stasis" gizmo.
  2. Test clean wake after 48 hours.
  3. Manually interrupt stasis before 48 hours.
- **Expected:**
  - Clean wake: Operating ticks reset, rest maxed, positive message.
  - Interrupted stasis: Gains `Construct_InterruptedStasis` (consciousness/moving penalties).
