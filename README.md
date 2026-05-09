# verifactu-hash-calculator

> 🇪🇸 [Versión en español](./README.es.md)

Open-source reference implementation of the SHA-256 chained hash algorithm required by the Spanish Tax Agency (AEAT) for VeriFactu billing records.

![License](https://img.shields.io/github/license/kreyo-io/verifactu-hash-calculator)
![.NET CI](https://github.com/kreyo-io/verifactu-hash-calculator/actions/workflows/dotnet.yml/badge.svg)
![TypeScript CI](https://github.com/kreyo-io/verifactu-hash-calculator/actions/workflows/typescript.yml/badge.svg)
![Python CI](https://github.com/kreyo-io/verifactu-hash-calculator/actions/workflows/python.yml/badge.svg)

[![NuGet](https://img.shields.io/nuget/v/Kreyo.VerifactuHashCalculator.svg)](https://www.nuget.org/packages/Kreyo.VerifactuHashCalculator/)
[![npm](https://img.shields.io/npm/v/@kreyo/verifactu-hash-calculator.svg)](https://www.npmjs.com/package/@kreyo/verifactu-hash-calculator)
[![PyPI](https://img.shields.io/pypi/v/kreyo-verifactu-hash-calculator.svg)](https://pypi.org/project/kreyo-verifactu-hash-calculator/)

## What it does

`verifactu-hash-calculator` provides simple functions to calculate and verify the cryptographic footprint (hash) of a VeriFactu billing record (alta, anulación, or evento). It takes the specific fields required by the Spanish Tax Agency as input and produces the standard 64-character uppercase hexadecimal SHA-256 hash.

## Why it exists

The AEAT requires a very specific field concatenation logic, trimming rules, and omission handling before hashing. This repository serves as an open-source reference implementation that perfectly passes all the official AEAT test vectors, avoiding developers the hassle of implementing these rules from scratch based on the official PDF.

## Conformance

Conforms to AEAT spec **v0.1.2 (27/08/2024)**: *"Detalle de las especificaciones técnicas para la generación de la huella o hash de los registros de facturación"*.
[Official PDF Document](https://www.agenciatributaria.es/static_files/AEAT_Desarrolladores/EEDD/IVA/VERI-FACTU/Veri-Factu_especificaciones_huella_hash_registros.pdf)

## Install

### .NET
```bash
dotnet add package Kreyo.VerifactuHashCalculator
```

### Node.js / TypeScript
```bash
npm install @kreyo/verifactu-hash-calculator
```

### Python
```bash
pip install kreyo-verifactu-hash-calculator
```

## Quick example

### .NET
```csharp
using Kreyo.VerifactuHashCalculator;

var input = new RegistroAltaInput(
    IDEmisorFactura: "89890001K",
    NumSerieFactura: "12345678/G33",
    FechaExpedicionFactura: "01-01-2024",
    TipoFactura: "F1",
    CuotaTotal: "12.35",
    ImporteTotal: "123.45",
    HuellaAnterior: null,
    FechaHoraHusoGenRegistro: "2024-01-01T19:20:30+01:00"
);

string hash = HashCalculator.ComputeRegistroAlta(input);
// => "3C464DAF61ACB827C65FDA19F352A4E3BDC2C640E9E9FC4CC058073F38F12F60"
```

### TypeScript
```typescript
import { computeRegistroAlta } from '@kreyo/verifactu-hash-calculator';

const hash = computeRegistroAlta({
  idEmisorFactura: "89890001K",
  numSerieFactura: "12345678/G33",
  fechaExpedicionFactura: "01-01-2024",
  tipoFactura: "F1",
  cuotaTotal: "12.35",
  importeTotal: "123.45",
  huellaAnterior: null,
  fechaHoraHusoGenRegistro: "2024-01-01T19:20:30+01:00"
});
// => "3C464DAF61ACB827C65FDA19F352A4E3BDC2C640E9E9FC4CC058073F38F12F60"
```

### Python
```python
from kreyo_verifactu_hash_calculator import compute_registro_alta, RegistroAltaInput

input_data = RegistroAltaInput(
    id_emisor_factura="89890001K",
    num_serie_factura="12345678/G33",
    fecha_expedicion_factura="01-01-2024",
    tipo_factura="F1",
    cuota_total="12.35",
    importe_total="123.45",
    huella_anterior=None,
    fecha_hora_huso_gen_registro="2024-01-01T19:20:30+01:00"
)

hash_str = compute_registro_alta(input_data)
# => "3C464DAF61ACB827C65FDA19F352A4E3BDC2C640E9E9FC4CC058073F38F12F60"
```

## API reference

For details on the algorithm step-by-step, see [`docs/algorithm.md`](./docs/algorithm.md).

- **Compute**:
  - `computeRegistroAlta(input)`
  - `computeRegistroAnulacion(input)`
  - `computeRegistroEvento(input)`
- **Verify**:
  - `verifyRegistroAlta(input, expectedHash)`
  - `verifyRegistroAnulacion(input, expectedHash)`
  - `verifyRegistroEvento(input, expectedHash)`

## Test vectors

Official test vectors from the AEAT specification:

### Case 1: First "alta" record
Input:
```json
{
  "idEmisorFactura": "89890001K",
  "numSerieFactura": "12345678/G33",
  "fechaExpedicionFactura": "01-01-2024",
  "tipoFactura": "F1",
  "cuotaTotal": "12.35",
  "importeTotal": "123.45",
  "huellaAnterior": null,
  "fechaHoraHusoGenRegistro": "2024-01-01T19:20:30+01:00"
}
```
Expected Hash: `3C464DAF61ACB827C65FDA19F352A4E3BDC2C640E9E9FC4CC058073F38F12F60`

### Case 2: Second "alta" record (chained)
Input:
```json
{
  "idEmisorFactura": "89890001K",
  "numSerieFactura": "12345679/G34",
  "fechaExpedicionFactura": "01-01-2024",
  "tipoFactura": "F1",
  "cuotaTotal": "12.35",
  "importeTotal": "123.45",
  "huellaAnterior": "3C464DAF61ACB827C65FDA19F352A4E3BDC2C640E9E9FC4CC058073F38F12F60",
  "fechaHoraHusoGenRegistro": "2024-01-01T19:20:35+01:00"
}
```
Expected Hash: `F7B94CFD8924EDFF273501B01EE5153E4CE8F259766F88CF6ACB8935802A2B97`

### Case 3: "Anulación" record (chained)
Input:
```json
{
  "idEmisorFacturaAnulada": "89890001K",
  "numSerieFacturaAnulada": "12345679/G34",
  "fechaExpedicionFacturaAnulada": "01-01-2024",
  "huellaAnterior": "F7B94CFD8924EDFF273501B01EE5153E4CE8F259766F88CF6ACB8935802A2B97",
  "fechaHoraHusoGenRegistro": "2024-01-01T19:20:40+01:00"
}
```
Expected Hash: `177547C0D57AC74748561D054A9CEC14B4C4EA23D1BEFD6F2E69E3A388F90C68`

## Limitations / Non-goals

This repository **only** computes hashes. It does **not**:
- Generate or format VeriFactu XMLs.
- Sign the XML records (XMLDSig).
- Handle AEAT submission, mTLS connection, or retries.
- Generate QR codes.
- Validate fiscal fields (e.g. NIF syntax).

## About Kreyo

This library is maintained by [Kreyo](https://kreyo.io), a Spanish fiscal compliance API platform. If you need full VeriFactu submission (signing, mTLS, AEAT integration, QR, retries) instead of just hash calculation, check out Kreyo VeriFactu.

## License

MIT License. See [LICENSE](./LICENSE).

## Contributing

Please see [CONTRIBUTING.md](./CONTRIBUTING.md) for details.
