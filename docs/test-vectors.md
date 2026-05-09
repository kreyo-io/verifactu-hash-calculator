# Test Vectors

This document lists the test vectors used to validate the `verifactu-hash-calculator` implementation.

## Official AEAT Vectors

These vectors are directly extracted from the AEAT specification **v0.1.2**.

### Case 1: First "alta" record
- **Description**: First billing record (no previous hash).
- **Input**:
  - `IDEmisorFactura`: `89890001K`
  - `NumSerieFactura`: `12345678/G33`
  - `FechaExpedicionFactura`: `01-01-2024`
  - `TipoFactura`: `F1`
  - `CuotaTotal`: `12.35`
  - `ImporteTotal`: `123.45`
  - `Huella`: *(empty)*
  - `FechaHoraHusoGenRegistro`: `2024-01-01T19:20:30+01:00`
- **Expected Concatenation**: `IDEmisorFactura=89890001K&NumSerieFactura=12345678/G33&FechaExpedicionFactura=01-01-2024&TipoFactura=F1&CuotaTotal=12.35&ImporteTotal=123.45&Huella=&FechaHoraHusoGenRegistro=2024-01-01T19:20:30+01:00`
- **Expected Hash**: `3C464DAF61ACB827C65FDA19F352A4E3BDC2C640E9E9FC4CC058073F38F12F60`

### Case 2: Second "alta" record
- **Description**: Second billing record chained to Case 1.
- **Expected Hash**: `F7B94CFD8924EDFF273501B01EE5153E4CE8F259766F88CF6ACB8935802A2B97`

### Case 3: "Anulación" record
- **Description**: Cancellation record chained to Case 2.
- **Expected Hash**: `177547C0D57AC74748561D054A9CEC14B4C4EA23D1BEFD6F2E69E3A388F90C68`

## Kreyo Internal Vectors

Additional vectors added by Kreyo to cover edge cases not exemplified in the official document. Currently empty, to be populated as needed.
