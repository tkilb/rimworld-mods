# TC-05: Construct Stasis & Maintenance

- **Procedure:**
  1. Have construct operate past warning ticks, trigger "Enter Stasis" gizmo, or right-click a bed to order "Enter stasis".
  2. Verify food need does not deplete and hunger rate is 0% while in stasis.
  3. Verify construct remains in stasis after reaching 48 hours without automatically waking up prematurely.
  4. Test "Wake" gizmo before 48 hours: verify confirmation dialog warns of neural defragmentation penalties, and confirm waking triggers `Construct_InterruptedStasis`.
  5. Test "Wake" gizmo after 48 hours: verify construct wakes immediately with clean wake benefits.
  6. Test "Auto-wake" toggle: verify toggling ON automatically wakes pawn once 48-hour cycle finishes (or immediately if already complete).
- **Expected:**
  - Bed interaction: Right-clicking any valid bed offers "Enter stasis"; pawn reserves bed, walks over, and assumes laying down posture under covers.
  - Food need: Completely frozen while in stasis (no hunger drain, no malnutrition buildup, and doctors do not feed them).
  - Indefinite Stasis: Pawn stays in stasis indefinitely until woken, displaying "Stasis Complete" and "Ready to wake".
  - Wake Gizmo: Provides "Wake" command. Prompts confirmation dialog if < 48 hours; executes clean wake if >= 48 hours.
  - Auto-wake Toggle: Matches Biotech vampire auto-wake behavior; when active, automatically awakens the construct upon stasis completion.
  - Clean wake: Operating ticks reset, rest maxed, food restored to safe margin, positive message.
  - Interrupted stasis: Gains `Construct_InterruptedStasis` (consciousness/moving penalties).
