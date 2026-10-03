# TC-06: Bioware Augment Grafting

- **Procedure:**
  1. Attempt to graft a bioware augment to a baseline colonist.
  2. Graft a bioware augment (e.g. Basic Combat Package) to a construct.
  3. Attempt to queue the same bioware augment on the construct.
  4. Graft a different bioware augment (e.g. Advanced Combat Package or Laborer Package) to the construct with an existing bioware installed.
- **Expected:**
  - Baseline human rejected ("Incompatible biological neural bus (Requires Construct)").
  - Construct accepts surgery; enters assimilation coma (`Construct_Assimilation`).
  - Same bioware rejected ("Already has this bioware package installed").
  - Single bioware limit & clean swap: Installing a new bioware cleanly extracts and ejects the previous bioware item onto the floor next to the patient without destroying it, then grafts the new package. Construct never possesses more than 1 active bioware.
