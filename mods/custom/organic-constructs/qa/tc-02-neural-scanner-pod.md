# TC-02: Neural Scanner Pod

- **Procedure:**
  1. Build `NeuralScanner` and supply power.
  2. Insert a blank `NeuralBlueprintDisk`.
  3. Select a natural colonist and order them to enter the scanner.
  4. Attempt to order a construct to enter the scanner.
- **Expected:**
  - Colonist is scanned over 15,000 ticks. Upon completion, disc is encoded and ejected, and donor gains `Construct_NeuralFatigue`.
  - Constructs are explicitly blocked from entering the scanner with an informative rejection message.
