# verifactu-hash-calculator

> 🇺🇸 [English version](https://github.com/kreyo-io/verifactu-hash-calculator/blob/main/README.md)

Implementación de referencia open-source del algoritmo de hash encadenado SHA-256 requerido por la Agencia Tributaria (AEAT) para los registros de facturación VeriFactu.

![License](https://img.shields.io/github/license/kreyo-io/verifactu-hash-calculator)
![.NET CI](https://github.com/kreyo-io/verifactu-hash-calculator/actions/workflows/dotnet.yml/badge.svg)
![TypeScript CI](https://github.com/kreyo-io/verifactu-hash-calculator/actions/workflows/typescript.yml/badge.svg)
![Python CI](https://github.com/kreyo-io/verifactu-hash-calculator/actions/workflows/python.yml/badge.svg)

[![NuGet](https://img.shields.io/nuget/v/Kreyo.VerifactuHashCalculator.svg)](https://www.nuget.org/packages/Kreyo.VerifactuHashCalculator/)
[![npm](https://img.shields.io/npm/v/@kreyo/verifactu-hash-calculator.svg)](https://www.npmjs.com/package/@kreyo/verifactu-hash-calculator)
[![PyPI](https://img.shields.io/pypi/v/kreyo-verifactu-hash-calculator.svg)](https://pypi.org/project/kreyo-verifactu-hash-calculator/)

## Qué hace

`verifactu-hash-calculator` proporciona funciones simples para calcular y verificar la huella criptográfica de un registro de facturación VeriFactu (alta, anulación o evento). Acepta los campos específicos requeridos por la AEAT como entrada y devuelve el hash SHA-256 estándar de 64 caracteres hexadecimales en mayúsculas.

## Por qué existe

La AEAT exige una lógica muy específica de concatenación de campos, reglas de limpieza de espacios (trim) y tratamiento de omisiones antes de aplicar el hash. Este repositorio sirve como una implementación de referencia open-source que pasa perfectamente todos los vectores de prueba oficiales, evitando a los desarrolladores la molestia de reimplementar estas reglas desde cero basándose en el PDF oficial.

## Conformidad

Cumple con la especificación de la AEAT **v0.1.2 (27/08/2024)**: *"Detalle de las especificaciones técnicas para la generación de la huella o hash de los registros de facturación"*.
[Documento PDF oficial](https://www.agenciatributaria.es/static_files/AEAT_Desarrolladores/EEDD/IVA/VERI-FACTU/Veri-Factu_especificaciones_huella_hash_registros.pdf)

## Instalación

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

## Ejemplo rápido

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

## Referencia de API

Para detalles sobre el algoritmo paso a paso, consulta [`docs/algorithm.es.md`](https://github.com/kreyo-io/verifactu-hash-calculator/blob/main/docs/algorithm.es.md).

- **Calcular (Compute)**:
  - `computeRegistroAlta(input)`
  - `computeRegistroAnulacion(input)`
  - `computeRegistroEvento(input)`
- **Verificar (Verify)**:
  - `verifyRegistroAlta(input, expectedHash)`
  - `verifyRegistroAnulacion(input, expectedHash)`
  - `verifyRegistroEvento(input, expectedHash)`

## Vectores de prueba

Vectores de prueba oficiales de la especificación de la AEAT:

### Caso 1: Primer registro de "alta"
Entrada:
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
Hash Esperado: `3C464DAF61ACB827C65FDA19F352A4E3BDC2C640E9E9FC4CC058073F38F12F60`

### Caso 2: Segundo registro de "alta" (encadenado)
Entrada:
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
Hash Esperado: `F7B94CFD8924EDFF273501B01EE5153E4CE8F259766F88CF6ACB8935802A2B97`

### Caso 3: Registro de "anulación" (encadenado)
Entrada:
```json
{
  "idEmisorFacturaAnulada": "89890001K",
  "numSerieFacturaAnulada": "12345679/G34",
  "fechaExpedicionFacturaAnulada": "01-01-2024",
  "huellaAnterior": "F7B94CFD8924EDFF273501B01EE5153E4CE8F259766F88CF6ACB8935802A2B97",
  "fechaHoraHusoGenRegistro": "2024-01-01T19:20:40+01:00"
}
```
Hash Esperado: `177547C0D57AC74748561D054A9CEC14B4C4EA23D1BEFD6F2E69E3A388F90C68`

## Limitaciones

Este repositorio **solo** calcula hashes. **No**:
- Genera ni formatea los XMLs de VeriFactu.
- Firma los registros XML (XMLDSig).
- Maneja el envío a la AEAT, conexión mTLS, ni reintentos.
- Genera códigos QR.
- Valida campos fiscales (e.g. sintaxis de NIF).

## Sobre Kreyo

Esta librería es mantenida por [Kreyo](https://kreyo.io), una plataforma de API de cumplimiento fiscal para España. Si necesitas la presentación completa de VeriFactu (firma, mTLS, integración con AEAT, QR, reintentos) en lugar de solo el cálculo del hash, échale un vistazo a Kreyo VeriFactu.

## Licencia

Licencia MIT. Ver [LICENSE](https://github.com/kreyo-io/verifactu-hash-calculator/blob/main/LICENSE).

## Contribuir

Por favor, revisa [CONTRIBUTING.md](https://github.com/kreyo-io/verifactu-hash-calculator/blob/main/CONTRIBUTING.md) para más detalles.
