# `verifactu-hash-calculator` — Spec del repositorio OSS

> **Para quien implementa esto**: este documento es **autocontenido**. No requiere conocimiento previo del proyecto Kreyo, de su codebase, ni de sus convenciones internas más allá de lo que aquí se explica. Lee el documento entero antes de escribir código. Cualquier decisión que tomes fuera de lo descrito aquí, justifícala en el commit o pregunta antes.

> **Idioma**: el código, los comentarios, los identificadores, los mensajes de error, el README principal y el changelog van **en inglés**. Existe un README adicional en español como guía localizada para el mercado español.

---

## 0. Prerequisitos antes de empezar (LEER PRIMERO)

**Antes de escribir una sola línea de código, asegúrate de tener acceso a lo siguiente. Si falta algo, pídelo al owner del proyecto explícitamente y espera respuesta — no improvises sustitutos.**

### 0.1. Acceso al repo `nif-validator` (OBLIGATORIO)

Este spec asume que existe un repositorio hermano ya publicado en `github.com/kreyo-io/nif-validator` que **establece patrones operativos consolidados** que debes reutilizar literalmente. En particular, hay tres detalles no triviales resueltos allí que tienes que copiar tal cual:

1. **`.csproj` de .NET con el `<Target>` que copia `README.md` y `LICENSE` desde la raíz del monorepo al directorio del paquete antes del build.** Necesario para que el paquete publicado a NuGet incluya README. Sin esto, NuGet muestra "no description available" en la página del paquete.

2. **`package.json` de TypeScript con la configuración `"files"` y el script prebuild que hace copia equivalente al directorio del paquete.** Necesario para que npm incluya README en el paquete publicado.

3. **`pyproject.toml` de Python con el hatch build hook que copia README y LICENSE desde la raíz al directorio del paquete antes del build.** Mismo motivo para PyPI.

Estos tres mecanismos no son intuitivos, no son los que aparecen en la documentación oficial de cada gestor de paquetes, y reinventarlos suele llevar a fricciones de varias horas que ya están resueltas en `nif-validator`.

**Acción a tomar antes de empezar**: confirma con el owner que tienes acceso de lectura al repo `kreyo-io/nif-validator`. Si no lo tienes, **pídelo explícitamente antes de iniciar la fase 1 del plan de implementación**. Cuando lo tengas, los tres archivos a inspeccionar y replicar son:

- `dotnet/src/Kreyo.NifValidator/Kreyo.NifValidator.csproj` (o equivalente).
- `typescript/package.json` y cualquier `prebuild.js` o similar que referencie.
- `python/pyproject.toml` y cualquier `hatch_build.py` o hook custom.

### 0.2. PDF normativo de la AEAT (OBLIGATORIO consultar)

El PDF v0.1.2 *"Detalle de las especificaciones técnicas para la generación de la huella o hash de los registros de facturación"* es la fuente normativa. La sección 3 de este spec reproduce literalmente la información necesaria para implementar, así que **no necesitas el PDF para programar**. Pero si encuentras alguna ambigüedad en este spec o quieres verificar algo, la URL canónica es:

<https://www.agenciatributaria.es/static_files/AEAT_Desarrolladores/EEDD/IVA/VERI-FACTU/Veri-Factu_especificaciones_huella_hash_registros.pdf>

Nota técnica: el servidor de la AEAT tiene un certificado SSL con algoritmo de firma débil que rompe muchos clientes HTTP automáticos (curl con configuración estricta, libs Python recientes). Los navegadores aceptan el cert sin problema. Si tu entorno de desarrollo no puede descargarlo, pídeselo al owner.

### 0.3. Confirmaciones adicionales antes de iniciar

Antes de la primera línea de código, plantea al owner cualquier duda sobre estos puntos abiertos:

- **TypeScript: solo Node en v1, o también browser**. Recomendación del spec: solo Node en v0.1.0, browser en v0.2.0. Si vas a desviarte, confirma.
- **Versión inicial del paquete**: el spec propone `0.1.0` para los tres lenguajes. Confirma si se mantiene o se prefiere otra.
- **Publicación a registries**: confirma si tienes credenciales para NuGet, npm y PyPI desde el primer release, o si la fase 6 (publicación) la hará el owner manualmente.

**Si cualquiera de estos puntos no está claro, pregunta antes de empezar. No tomes decisiones unilaterales sobre nada que esté en esta sección 0.**

---

## 1. Qué es este repositorio

`verifactu-hash-calculator` es una **implementación de referencia, open source, de la huella criptográfica encadenada SHA-256 que la Agencia Tributaria española (AEAT) exige para los registros de facturación VeriFactu**, distribuida como tres librerías equivalentes en .NET, TypeScript y Python.

Es un repo bajo licencia MIT publicado por la organización GitHub `kreyo-io`, que mantiene también la librería hermana [`nif-validator`](https://github.com/kreyo-io/nif-validator) (de la que este repo hereda convenciones de monorepo, packaging y CI).

### Propósito

1. Que cualquier desarrollador implementando VeriFactu pueda calcular la huella correctamente con una llamada de función, sin tener que parsear el PDF normativo de la AEAT.
2. Que los **vectores oficiales de prueba publicados por la AEAT** estén siempre verde en CI, demostrando conformidad de la implementación.
3. Servir de anclaje técnico para Kreyo (kreyo.io), una plataforma de APIs de cumplimiento fiscal español. El README enlaza a Kreyo; el repo en sí es genuinamente útil sin instalar nada de Kreyo.

### No-objetivos

Este repo **solo** calcula y verifica huellas. En particular **no**:

- Genera el XML del registro VeriFactu.
- Firma con XMLDSig el registro.
- Establece la conexión mTLS con la AEAT.
- Genera el código QR.
- Maneja envío en lote ni control de flujo de respuestas.
- Ofrece persistencia, almacenamiento de la cadena, ni recuperación.
- Valida formato de NIF (eso lo hace la librería hermana `nif-validator`).

Todo eso lo cubre Kreyo VeriFactu como producto comercial; este repo cubre **solo** la pieza criptográfica concreta del hash.

---

## 2. Base normativa

La huella se calcula según lo establecido en:

- **Real Decreto 1007/2023**, de 5 de diciembre, que aprueba el Reglamento de Sistemas Informáticos de Facturación (RRSIF). BOE 6/12/2023.
- **Orden HAC/1177/2024**, de 17 de octubre. BOE 28/10/2024. Desarrolla los aspectos técnicos del RD 1007/2023. Sus artículos 13 y 14 tratan respectivamente la huella/hash y la firma electrónica.

El detalle técnico exacto del cálculo vive en un PDF específico publicado por la AEAT en su portal de desarrolladores:

- **Documento canónico**: *"Detalle de las especificaciones técnicas para la generación de la huella o hash de los registros de facturación"*.
- **Versión vigente al cerrar este spec**: **v0.1.2** (Autor: AEAT, Fecha: 27/08/2024).
- **URL canónica**: <https://www.agenciatributaria.es/static_files/AEAT_Desarrolladores/EEDD/IVA/VERI-FACTU/Veri-Factu_especificaciones_huella_hash_registros.pdf>
- **Página oficial que lo enlaza**: <https://sede.agenciatributaria.gob.es/Sede/iva/sistemas-informaticos-facturacion-verifactu/informacion-tecnica/algoritmo-calculo-codificacion-huella-hash.html>

### Versionado normativo

El PDF de la AEAT puede ser sustituido por v0.1.3, v0.2.0, etc. La librería debe:

- Documentar en el README **a qué versión del PDF está conformada** (al cerrar este spec: v0.1.2 de 27/08/2024).
- Cuando la AEAT publique una nueva versión, abrir un issue en el repo, comparar diffs, actualizar la implementación si rompe algo, subir versión semver minor o major según corresponda, y actualizar la nota del README.

---

## 3. Especificación del algoritmo (literal del PDF v0.1.2)

Esta sección reproduce **literalmente** la información necesaria del PDF normativo. Los implementadores no necesitan abrir el PDF para programar las librerías — todo lo necesario está aquí. El PDF queda como referencia oficial en caso de duda.

### 3.1. Función hash

**SHA-256.** Único algoritmo permitido a la fecha.

### 3.2. Datos de entrada según tipo de registro

Existen tres tipos de registro: **alta**, **anulación** y **evento**. Cada tipo tiene su propio conjunto de campos a concatenar **en el orden que aquí se enumera** (importante: el orden coincide con el orden de aparición en el XML del registro, no con orden alfabético).

#### 3.2.1. Registro de facturación de alta

8 campos, en este orden:

| # | Nombre del campo | Ruta XML |
|---|------------------|----------|
| 1 | `IDEmisorFactura` | `RegistroAlta/IDFactura/IDEmisorFactura` |
| 2 | `NumSerieFactura` | `RegistroAlta/IDFactura/NumSerieFactura` |
| 3 | `FechaExpedicionFactura` | `RegistroAlta/IDFactura/FechaExpedicionFactura` |
| 4 | `TipoFactura` | `RegistroAlta/TipoFactura` |
| 5 | `CuotaTotal` | `RegistroAlta/CuotaTotal` |
| 6 | `ImporteTotal` | `RegistroAlta/ImporteTotal` |
| 7 | `Huella` | `RegistroAlta/Encadenamiento/RegistroAnterior/Huella` |
| 8 | `FechaHoraHusoGenRegistro` | `RegistroAlta/FechaHoraHusoGenRegistro` |

#### 3.2.2. Registro de facturación de anulación

5 campos, en este orden:

| # | Nombre del campo | Ruta XML |
|---|------------------|----------|
| 1 | `IDEmisorFacturaAnulada` | `RegistroAnulacion/IDFactura/IDEmisorFacturaAnulada` |
| 2 | `NumSerieFacturaAnulada` | `RegistroAnulacion/IDFactura/NumSerieFacturaAnulada` |
| 3 | `FechaExpedicionFacturaAnulada` | `RegistroAnulacion/IDFactura/FechaExpedicionFacturaAnulada` |
| 4 | `Huella` | `RegistroAnulacion/Encadenamiento/RegistroAnterior/Huella` |
| 5 | `FechaHoraHusoGenRegistro` | `RegistroAnulacion/FechaHoraHusoGenRegistro` |

#### 3.2.3. Registro de evento

9 campos, en este orden:

| # | Nombre del campo | Ruta XML |
|---|------------------|----------|
| 1 | `NIF` | `RegistroEvento/Evento/SistemaInformatico/NIF` |
| 2 | `ID` | `RegistroEvento/Evento/SistemaInformatico/IDOtro/ID` |
| 3 | `IdSistemaInformatico` | `RegistroEvento/Evento/SistemaInformatico/IdSistemaInformatico` |
| 4 | `Version` | `RegistroEvento/Evento/SistemaInformatico/Version` |
| 5 | `NumeroInstalacion` | `RegistroEvento/Evento/SistemaInformatico/NumeroInstalacion` |
| 6 | `NIF` | `RegistroEvento/Evento/ObligadoEmision/NIF` |
| 7 | `TipoEvento` | `RegistroEvento/Evento/TipoEvento` |
| 8 | `HuellaEvento` | `RegistroEvento/Evento/Encadenamiento/EventoAnterior/HuellaEvento` |
| 9 | `FechaHoraHusoGenEvento` | `RegistroEvento/Evento/FechaHoraHusoGenEvento` |

> ⚠️ **Atención**: en el registro de evento, los campos 1 y 6 tienen ambos el nombre literal `NIF`. Son entradas distintas en la cadena concatenada, separadas por otros campos en el medio. Esto no es un error tipográfico: la AEAT lo define así porque ambos NIFs aparecen en la cadena en su posición correspondiente del XML.

### 3.3. Formato de la cadena concatenada

Todos los campos se concatenan en una **única cadena de texto** con la siguiente estructura:

```
nombreCampo1=valor1&nombreCampo2=valor2&...&nombreCampoN=valorN
```

Reglas:

- **El nombre del campo es un literal constante**, exactamente como aparece en las tablas anteriores. Es case-sensitive.
- **Separador entre pares**: el carácter `&` (ampersand).
- **Separador entre nombre y valor**: el carácter `=` (igual).
- **Sin espacios** alrededor de los `=` ni de los `&`.
- **Sin URL-encoding ni escape** del valor. Caracteres especiales (`/`, espacios internos, `:`, `+`, `T`) van literales. Esto incluye específicamente las fechas con formato ISO 8601 que contienen `T`, `:` y `+`.

### 3.4. Normalización de valores

#### 3.4.1. Trim de espacios

Eliminar espacios al inicio y al final de cada valor antes de concatenar.

> Ejemplo del PDF: si el XML contiene `<NumSerieFactura> 12345678 / G33 </NumSerieFactura>`, el valor en la cadena es `12345678 / G33` (con espacios internos preservados, sin espacios al inicio o al final).

#### 3.4.2. Numéricos: ceros a la derecha irrelevantes

Para campos numéricos (`CuotaTotal`, `ImporteTotal`), los ceros a la derecha en la parte decimal son **irrelevantes para la AEAT**: `123.1` y `123.10` producen la misma huella desde el punto de vista de validación.

> ⚠️ **Decisión de implementación crítica**: la librería debe producir una representación canónica del numérico que sea **reproducible** y **conforme**. La especificación recomendada de Kreyo:
>
> 1. La librería NO acepta números nativos (`decimal`, `number`, `Decimal`) como entrada. Acepta **strings** que representan el número exactamente como aparece en el XML del registro.
> 2. Antes de concatenar, la librería NO altera la cadena numérica del input: si el caller envía `"123.45"`, la librería usa `"123.45"`; si envía `"123.450"`, usa `"123.450"`.
> 3. Esto significa que **la huella generada por la librería coincide con la que validará la AEAT siempre que el caller envíe el mismo string que vaya en el XML del registro**. Es responsabilidad del caller que ambos coincidan.
>
> Esta decisión se documenta explícitamente en el README porque es contraintuitiva y un implementador podría sentirse tentado de "ayudar" parseando a decimal y reformateando — eso es exactamente lo que **no** hay que hacer, porque introduciría discrepancias entre el XML enviado y la huella calculada.
>
> Para la API de "calcular la huella desde objetos de dominio del usuario": fuera de scope. Cada caller mapea sus datos al formato textual que va al XML.

#### 3.4.3. Campos ausentes o con valor vacío

Si un campo no aparece en el registro, o aparece con valor vacío, en la cadena concatenada **siempre se incluye el nombre del campo seguido de `=` y nada más**.

Ejemplos del PDF:

```
...ImporteTotal=123.45&Huella=&FechaHoraHusoGenRegistro=2024-01-01T19:20:30+01:00
```

(primer registro de la cadena, sin huella anterior)

```
NIF=89890001K&ID=&IdSistemaInformatico=...
```

(en un registro de evento, NIF informado pero ID excluyente vacío)

### 3.5. Encoding y aplicación del hash

1. La cadena resultante se **codifica en UTF-8** (sin BOM) para obtener el array de bytes de entrada.
2. Se aplica SHA-256 al array de bytes.
3. Los 32 bytes de salida se codifican en **hexadecimal en mayúsculas**, produciendo una cadena de **64 caracteres alfanuméricos**.

### 3.6. Tratamiento del primer registro de la cadena

Cuando el registro es el primero del SIF, no hay registro anterior:

- En registros de alta o anulación: el campo `Huella` se concatena como `Huella=` (vacío). El campo es obligatorio en la cadena, solo el valor está vacío.
- En registros de evento: análogo con `HuellaEvento=`.
- El XML del registro tendrá adicionalmente `PrimerRegistro=S` o `PrimerEvento=S`, pero **ese campo no entra en el cálculo del hash**.

### 3.7. Validación AEAT

Cuando un sistema VERI*FACTU envía un registro, la AEAT recalcula la huella en su lado. Si la huella enviada por el SIF no coincide con la calculada por la AEAT, el registro se marca como **"Aceptado con errores"**.

---

## 4. Vectores de prueba oficiales (del PDF v0.1.2)

Estos tres casos son **vectores oficiales publicados por la AEAT en el PDF normativo**. La librería **debe pasar estos tres tests en CI** en los tres lenguajes. Si alguno falla, hay un bug, no negociable.

### Caso 1 — Primer registro de alta (sin huella anterior)

**Datos de entrada:**

| Campo | Valor |
|-------|-------|
| `IDEmisorFactura` | `89890001K` |
| `NumSerieFactura` | `12345678/G33` |
| `FechaExpedicionFactura` | `01-01-2024` |
| `TipoFactura` | `F1` |
| `CuotaTotal` | `12.35` |
| `ImporteTotal` | `123.45` |
| `Huella` | *(vacío — primer registro)* |
| `FechaHoraHusoGenRegistro` | `2024-01-01T19:20:30+01:00` |

**Cadena concatenada:**

```
IDEmisorFactura=89890001K&NumSerieFactura=12345678/G33&FechaExpedicionFactura=01-01-2024&TipoFactura=F1&CuotaTotal=12.35&ImporteTotal=123.45&Huella=&FechaHoraHusoGenRegistro=2024-01-01T19:20:30+01:00
```

**Huella esperada (SHA-256, hex mayúsculas, 64 chars):**

```
3C464DAF61ACB827C65FDA19F352A4E3BDC2C640E9E9FC4CC058073F38F12F60
```

### Caso 2 — Segundo registro de alta (encadenando con Caso 1)

**Datos de entrada:**

| Campo | Valor |
|-------|-------|
| `IDEmisorFactura` | `89890001K` |
| `NumSerieFactura` | `12345679/G34` |
| `FechaExpedicionFactura` | `01-01-2024` |
| `TipoFactura` | `F1` |
| `CuotaTotal` | `12.35` |
| `ImporteTotal` | `123.45` |
| `Huella` | `3C464DAF61ACB827C65FDA19F352A4E3BDC2C640E9E9FC4CC058073F38F12F60` |
| `FechaHoraHusoGenRegistro` | `2024-01-01T19:20:35+01:00` |

**Cadena concatenada:**

```
IDEmisorFactura=89890001K&NumSerieFactura=12345679/G34&FechaExpedicionFactura=01-01-2024&TipoFactura=F1&CuotaTotal=12.35&ImporteTotal=123.45&Huella=3C464DAF61ACB827C65FDA19F352A4E3BDC2C640E9E9FC4CC058073F38F12F60&FechaHoraHusoGenRegistro=2024-01-01T19:20:35+01:00
```

**Huella esperada:**

```
F7B94CFD8924EDFF273501B01EE5153E4CE8F259766F88CF6ACB8935802A2B97
```

### Caso 3 — Registro de anulación (encadenando con Caso 2)

**Datos de entrada:**

| Campo | Valor |
|-------|-------|
| `IDEmisorFacturaAnulada` | `89890001K` |
| `NumSerieFacturaAnulada` | `12345679/G34` |
| `FechaExpedicionFacturaAnulada` | `01-01-2024` |
| `Huella` | `F7B94CFD8924EDFF273501B01EE5153E4CE8F259766F88CF6ACB8935802A2B97` |
| `FechaHoraHusoGenRegistro` | `2024-01-01T19:20:40+01:00` |

**Cadena concatenada:**

```
IDEmisorFacturaAnulada=89890001K&NumSerieFacturaAnulada=12345679/G34&FechaExpedicionFacturaAnulada=01-01-2024&Huella=F7B94CFD8924EDFF273501B01EE5153E4CE8F259766F88CF6ACB8935802A2B97&FechaHoraHusoGenRegistro=2024-01-01T19:20:40+01:00
```

**Huella esperada:**

```
177547C0D57AC74748561D054A9CEC14B4C4EA23D1BEFD6F2E69E3A388F90C68
```

### Vector negativo recomendado (no oficial)

Como complemento, los tests deben incluir un caso que **debería fallar** la verificación: tomar el Caso 1 y cambiar un dígito de la huella esperada. Esto valida que la función `verify()` retorna `false` en lugar de un falso positivo.

### Vectores adicionales por escribir (Kreyo)

Más allá de los tres oficiales, la suite de tests debe incluir vectores adicionales generados por la librería para cubrir casos que el PDF no ejemplifica:

1. **Numérico con un decimal**: `CuotaTotal=12.3` (recordatorio: la librería preserva el string del input).
2. **Numérico entero sin decimales**: `CuotaTotal=12`.
3. **NumSerieFactura con espacios internos preservados** y trim de extremos: input `" A1 / B2 "` → valor concatenado `"A1 / B2"`.
4. **Registro de evento con NIF informado e ID vacío** (caso del PDF §3): `NIF=89890001K&ID=&IdSistemaInformatico=...`.
5. **Registro de evento con ID informado y NIF vacío** (caso inverso).
6. **Caracteres no-ASCII en algún campo de evento** (por ejemplo, `Version` con tilde) para verificar UTF-8 correcto.

Estos vectores son **internos de Kreyo**, no oficiales de la AEAT. Marcarlos así en los archivos de test (separar `official/` de `kreyo-internal/` para que sea evidente cuáles son normativos y cuáles auxiliares).

---

## 5. Estructura del monorepo

Idéntica al patrón ya consolidado en [`nif-validator`](https://github.com/kreyo-io/nif-validator). Reproducir literalmente esa estructura.

```
verifactu-hash-calculator/
├── README.md                        # Inglés, principal
├── README.es.md                     # Español, link desde README.md
├── LICENSE                          # MIT
├── CHANGELOG.md
├── .gitignore
├── .editorconfig
├── .github/
│   └── workflows/
│       ├── dotnet.yml               # CI .NET
│       ├── typescript.yml           # CI TypeScript
│       └── python.yml               # CI Python
├── docs/
│   ├── algorithm.md                 # Explicación del algoritmo (en inglés)
│   ├── algorithm.es.md              # Versión española
│   └── test-vectors.md              # Documentación de los vectores oficiales y los Kreyo internos
├── dotnet/                          # .NET — publica a NuGet como Kreyo.VerifactuHashCalculator
│   ├── Kreyo.VerifactuHashCalculator.sln
│   ├── src/
│   │   └── Kreyo.VerifactuHashCalculator/
│   │       ├── Kreyo.VerifactuHashCalculator.csproj
│   │       ├── HashCalculator.cs
│   │       ├── Records/
│   │       │   ├── RegistroAltaInput.cs
│   │       │   ├── RegistroAnulacionInput.cs
│   │       │   └── RegistroEventoInput.cs
│   │       └── Internal/
│   │           ├── FieldConcatenator.cs
│   │           └── Sha256Hex.cs
│   └── tests/
│       └── Kreyo.VerifactuHashCalculator.Tests/
│           ├── Kreyo.VerifactuHashCalculator.Tests.csproj
│           ├── OfficialVectorsTests.cs
│           ├── KreyoVectorsTests.cs
│           └── fixtures/
├── typescript/                      # TypeScript — publica a npm como @kreyo/verifactu-hash-calculator
│   ├── package.json
│   ├── tsconfig.json
│   ├── tsup.config.ts               # build dual ESM + CJS
│   ├── src/
│   │   ├── index.ts
│   │   ├── hash-calculator.ts
│   │   ├── records.ts
│   │   └── internal/
│   │       ├── field-concatenator.ts
│   │       └── sha256-hex.ts
│   └── test/
│       ├── official-vectors.test.ts
│       └── kreyo-vectors.test.ts
├── python/                          # Python — publica a PyPI como kreyo-verifactu-hash-calculator
│   ├── pyproject.toml
│   ├── src/
│   │   └── kreyo_verifactu_hash_calculator/
│   │       ├── __init__.py
│   │       ├── hash_calculator.py
│   │       ├── records.py
│   │       └── _internal/
│   │           ├── __init__.py
│   │           ├── field_concatenator.py
│   │           └── sha256_hex.py
│   └── tests/
│       ├── test_official_vectors.py
│       └── test_kreyo_vectors.py
└── shared/                          # Recursos compartidos entre las tres implementaciones
    └── test-vectors.json            # Single source of truth de los vectores
```

### Razones de monorepo

- Un único repo recibe stars; no se diluyen.
- README único marca presencia.
- Cambios coordinados entre lenguajes: si descubres un edge case, lo arreglas en los 3 a la vez.
- Pattern establecido por `nif-validator`.

---

## 6. Convenciones de packaging y publicación

| Lenguaje | Registry | Nombre del paquete | Versión inicial |
|----------|----------|--------------------|-----------------|
| .NET | NuGet | `Kreyo.VerifactuHashCalculator` | `0.1.0` |
| TypeScript | npm | `@kreyo/verifactu-hash-calculator` | `0.1.0` |
| Python | PyPI | `kreyo-verifactu-hash-calculator` | `0.1.0` |

### .NET (carpeta `dotnet/`)

- **Multi-target**: `net8.0;netstandard2.0`. Razón: muchos ERPs y software contable españoles siguen en .NET Framework 4.7.2 o están migrando; netstandard2.0 cubre ese caso.
- `<Nullable>enable</Nullable>` y `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>` en el csproj.
- Sin dependencias externas. Solo BCL (`System.Security.Cryptography.SHA256`, `System.Text.Encoding.UTF8`).
- README y LICENSE deben copiarse al directorio del csproj antes del build (`<Target Name="CopyReadmeAndLicense" BeforeTargets="GenerateNuspec">` o equivalente). Patrón ya resuelto en `nif-validator`.

### TypeScript (carpeta `typescript/`)

- **Build dual ESM + CJS** con `tsup` (no Rollup, no webpack — `tsup` es lo que usa `nif-validator`).
- `package.json`:
  - `"type": "module"`
  - `"exports"` con condiciones `import`/`require`/`types`.
  - `"files": ["dist", "README.md", "LICENSE"]`.
- TypeScript estricto (`strict: true`, `noUncheckedIndexedAccess: true`).
- Target `ES2020` mínimo.
- Sin dependencias runtime. Solo `crypto` (Node) — para browser, usar `crypto.subtle` con detección.
- Si el browser support resulta complicado por la API asíncrona de `crypto.subtle`, **v1 puede ser solo Node** (documentarlo claro en el README), y dejar browser para v0.2.

### Python (carpeta `python/`)

- **Versión mínima**: Python 3.10+.
- `pyproject.toml` con backend `hatchling`.
- Type hints en toda la API pública.
- `mypy --strict` debe pasar.
- Sin dependencias runtime más allá de stdlib (`hashlib`, `unicodedata` si hace falta).
- Hatch hook para copiar README y LICENSE desde la raíz del monorepo al directorio del paquete antes del build (patrón resuelto en `nif-validator`).

### Publicación

- **Cada subdirectorio se publica desde su propio workflow de CI** o manualmente desde su carpeta. No hay un comando único que publique a los tres registries.
- Tagging convention: `v0.1.0` (sin prefijo de lenguaje). Si en el futuro las versiones divergen entre lenguajes (poco probable en una librería estable), se reevalúa.

---

## 7. API pública por lenguaje

La API es deliberadamente minimalista: cuatro funciones por lenguaje, mismas semánticas, naming idiomático en cada uno.

### Operaciones

1. **Calcular la huella de un registro de alta** dado un objeto con sus campos.
2. **Calcular la huella de un registro de anulación** dado un objeto con sus campos.
3. **Calcular la huella de un registro de evento** dado un objeto con sus campos.
4. **Verificar una huella** dado un registro y la huella declarada → `true`/`false`.

> **No incluir** en v1: verificación de cadenas completas (sequence verification). Se puede componer trivialmente con las primitivas anteriores y mete superficie de API que no es estrictamente core. Si hay demanda explícita, se añade en v0.2.

### Forma de los inputs

Cada tipo de registro (alta, anulación, evento) tiene su propio tipo de input con los campos exactos del PDF. Todos los campos son **strings**. Los campos opcionales se modelan como `string | null` (TS), `string?` (.NET nullable), o `Optional[str]` / `str | None` (Python). `null`/`None` significa "campo ausente o vacío" → se serializa como `nombreCampo=` en la cadena concatenada.

### .NET

```csharp
namespace Kreyo.VerifactuHashCalculator;

public static class HashCalculator
{
    public static string ComputeRegistroAlta(RegistroAltaInput input);
    public static string ComputeRegistroAnulacion(RegistroAnulacionInput input);
    public static string ComputeRegistroEvento(RegistroEventoInput input);

    public static bool VerifyRegistroAlta(RegistroAltaInput input, string expectedHash);
    public static bool VerifyRegistroAnulacion(RegistroAnulacionInput input, string expectedHash);
    public static bool VerifyRegistroEvento(RegistroEventoInput input, string expectedHash);
}

public sealed record RegistroAltaInput(
    string IDEmisorFactura,
    string NumSerieFactura,
    string FechaExpedicionFactura,
    string TipoFactura,
    string CuotaTotal,
    string ImporteTotal,
    string? HuellaAnterior,           // null si es el primer registro
    string FechaHoraHusoGenRegistro
);

public sealed record RegistroAnulacionInput(
    string IDEmisorFacturaAnulada,
    string NumSerieFacturaAnulada,
    string FechaExpedicionFacturaAnulada,
    string? HuellaAnterior,
    string FechaHoraHusoGenRegistro
);

public sealed record RegistroEventoInput(
    string? SistemaInformaticoNif,    // NIF #1 (puede ser null si se informa ID)
    string? SistemaInformaticoId,     // ID (puede ser null si se informa NIF)
    string IdSistemaInformatico,
    string Version,
    string NumeroInstalacion,
    string ObligadoEmisionNif,        // NIF #2
    string TipoEvento,
    string? HuellaEventoAnterior,
    string FechaHoraHusoGenEvento
);
```

Notas:
- Los nombres de propiedades en C# son PascalCase, idiomático. La concatenación interna usa los nombres literales del PDF (`IDEmisorFactura`, etc.).
- `HuellaAnterior` se mapea a la columna `Huella` del PDF (o `HuellaEvento` en el caso del evento). El nombre del parámetro es más legible; el nombre que se concatena es el del PDF.
- El `null` se serializa como string vacío (la cadena queda `Huella=&...`).

### TypeScript

```typescript
export interface RegistroAltaInput {
  idEmisorFactura: string;
  numSerieFactura: string;
  fechaExpedicionFactura: string;
  tipoFactura: string;
  cuotaTotal: string;
  importeTotal: string;
  huellaAnterior: string | null;
  fechaHoraHusoGenRegistro: string;
}

export interface RegistroAnulacionInput {
  idEmisorFacturaAnulada: string;
  numSerieFacturaAnulada: string;
  fechaExpedicionFacturaAnulada: string;
  huellaAnterior: string | null;
  fechaHoraHusoGenRegistro: string;
}

export interface RegistroEventoInput {
  sistemaInformaticoNif: string | null;
  sistemaInformaticoId: string | null;
  idSistemaInformatico: string;
  version: string;
  numeroInstalacion: string;
  obligadoEmisionNif: string;
  tipoEvento: string;
  huellaEventoAnterior: string | null;
  fechaHoraHusoGenEvento: string;
}

export function computeRegistroAlta(input: RegistroAltaInput): string;
export function computeRegistroAnulacion(input: RegistroAnulacionInput): string;
export function computeRegistroEvento(input: RegistroEventoInput): string;

export function verifyRegistroAlta(input: RegistroAltaInput, expectedHash: string): boolean;
export function verifyRegistroAnulacion(input: RegistroAnulacionInput, expectedHash: string): boolean;
export function verifyRegistroEvento(input: RegistroEventoInput, expectedHash: string): boolean;
```

Notas:
- Naming camelCase, idiomático TS.
- Si la implementación es asíncrona por usar `crypto.subtle` (browser), las funciones devuelven `Promise<string>` / `Promise<boolean>`. Si v1 es solo Node, son síncronas. Decidir esto al empezar a implementar y documentar coherente.

### Python

```python
from dataclasses import dataclass

@dataclass(frozen=True)
class RegistroAltaInput:
    id_emisor_factura: str
    num_serie_factura: str
    fecha_expedicion_factura: str
    tipo_factura: str
    cuota_total: str
    importe_total: str
    huella_anterior: str | None
    fecha_hora_huso_gen_registro: str

@dataclass(frozen=True)
class RegistroAnulacionInput:
    id_emisor_factura_anulada: str
    num_serie_factura_anulada: str
    fecha_expedicion_factura_anulada: str
    huella_anterior: str | None
    fecha_hora_huso_gen_registro: str

@dataclass(frozen=True)
class RegistroEventoInput:
    sistema_informatico_nif: str | None
    sistema_informatico_id: str | None
    id_sistema_informatico: str
    version: str
    numero_instalacion: str
    obligado_emision_nif: str
    tipo_evento: str
    huella_evento_anterior: str | None
    fecha_hora_huso_gen_evento: str

def compute_registro_alta(input: RegistroAltaInput) -> str: ...
def compute_registro_anulacion(input: RegistroAnulacionInput) -> str: ...
def compute_registro_evento(input: RegistroEventoInput) -> str: ...

def verify_registro_alta(input: RegistroAltaInput, expected_hash: str) -> bool: ...
def verify_registro_anulacion(input: RegistroAnulacionInput, expected_hash: str) -> bool: ...
def verify_registro_evento(input: RegistroEventoInput, expected_hash: str) -> bool: ...
```

Notas:
- Naming snake_case, idiomático Python.
- `dataclass(frozen=True)` para inmutabilidad.

---

## 8. Implementación interna recomendada

Tres pasos comunes a las tres librerías:

### 8.1. Construcción de la cadena concatenada

Función interna (no expuesta) que recibe una lista ordenada de `(name, value)` y produce la cadena `name1=value1&name2=value2&...`.

Reglas:

1. Para cada par, aplicar **trim** al `value` (eliminar whitespace inicial y final). Si el value original es `null`/`None`, tratarlo como string vacío.
2. Concatenar `name + "=" + valueTrimmedOrEmpty`.
3. Unir todos los pares con `"&"`.

No URL-encoding. No escape. Los caracteres especiales del valor van literales.

### 8.2. Codificación a bytes

`UTF-8` sin BOM. En .NET: `Encoding.UTF8.GetBytes(...)` (la `Encoding.UTF8` por defecto no añade BOM al codificar, solo al decodificar; verificar). En TypeScript: `new TextEncoder().encode(...)`. En Python: `string.encode("utf-8")`.

### 8.3. SHA-256 y formato de salida

Aplicar SHA-256 al array de bytes. Convertir los 32 bytes resultantes a hexadecimal **mayúsculas** sin separadores. Resultado: cadena de 64 caracteres `[0-9A-F]+`.

- .NET: `SHA256.HashData(bytes).Aggregate("", (acc, b) => acc + b.ToString("X2"))` o equivalente con `Convert.ToHexString(...)` (disponible en .NET 5+, devuelve mayúsculas por defecto).
- TypeScript: `crypto.createHash("sha256").update(bytes).digest("hex").toUpperCase()` (Node).
- Python: `hashlib.sha256(bytes).hexdigest().upper()`.

### 8.4. Verificación

`verify(input, expectedHash)`:

1. Calcular el hash con la misma función `compute(input)`.
2. Comparar con `expectedHash`. **Comparación case-insensitive** (la AEAT define mayúsculas pero un caller paranoico puede normalizar a minúsculas; aceptar ambos en `verify`).
3. Devolver `true`/`false`.

---

## 9. README

Dos archivos:

- `README.md` — **inglés**, principal. Es el que ven los visitantes de GitHub por defecto.
- `README.es.md` — **español**, link destacado al inicio del README inglés (`> 🇪🇸 [Versión en español](./README.es.md)`).

### Estructura del README inglés

1. **Title + tagline** una línea.
2. **Badges**: CI status (3 lenguajes), npm version, NuGet version, PyPI version, license MIT.
3. **What it does** — un párrafo, sin jergon innecesario.
4. **Why it exists** — un párrafo: la AEAT exige cálculo de huella encadenada SHA-256 para VeriFactu, y este repo es una implementación de referencia open source que pasa los vectores oficiales.
5. **Conformance** — una sección breve declarando: "Conforms to AEAT spec v0.1.2 (27/08/2024)". Link a la URL del PDF.
6. **Install** — un snippet por lenguaje (npm, NuGet, pip).
7. **Quick example** — un snippet por lenguaje, calculando el Caso 1 del PDF y mostrando que devuelve `3C464DAF...`.
8. **API reference** — listado de las 6 funciones (3 compute + 3 verify) con su firma en cada lenguaje. Link a `docs/algorithm.md` para detalles.
9. **Test vectors** — sección breve listando los 3 vectores oficiales con sus inputs y outputs. Esto es **deliberado**: hace que el repo aparezca en búsquedas de Google y de LLMs cuando alguien busca "verifactu hash test vector".
10. **Limitations / non-goals** — listado breve (no firma XMLDSig, no envío AEAT, no QR, etc.).
11. **About Kreyo** — sección al final, breve. Algo como: "This library is maintained by [Kreyo](https://kreyo.io), a Spanish fiscal compliance API platform. If you need full VeriFactu submission (signing, mTLS, AEAT integration, QR, retries) instead of just hash calculation, check out Kreyo VeriFactu." No vendedor, factual.
12. **License** — MIT, link al `LICENSE`.
13. **Contributing** — link a `CONTRIBUTING.md` (puede ser básico en v1).

### Estructura del README español

Mismo orden y secciones que el inglés, pero **localizado**, no traducido literal. El lector español ya sabe qué es la AEAT y VeriFactu — no hace falta el contexto de "Spanish tax authority". Tono más directo.

---

## 10. CI con GitHub Actions

Tres workflows independientes, uno por lenguaje. Triggers: `push` a `main`, `pull_request` a `main`, y `release` (opcionalmente, para publicación automática).

### `.github/workflows/dotnet.yml`

- Matrix sobre `dotnet-version: [8.0.x]` (suficiente para v1).
- Pasos: `actions/checkout`, `actions/setup-dotnet`, `dotnet restore`, `dotnet build --configuration Release --no-restore`, `dotnet test --no-build --verbosity normal`.
- Working directory: `dotnet/`.

### `.github/workflows/typescript.yml`

- Matrix sobre `node-version: [20, 22]`.
- Pasos: `actions/checkout`, `actions/setup-node` con cache npm, `npm ci`, `npm run build`, `npm test`.
- Working directory: `typescript/`.

### `.github/workflows/python.yml`

- Matrix sobre `python-version: ["3.10", "3.11", "3.12"]`.
- Pasos: `actions/checkout`, `actions/setup-python`, `pip install -e ".[test]"`, `pytest`, `mypy --strict src/`.
- Working directory: `python/`.

### Paths-filter

Para no correr los tres workflows en cada push, añadir filtros de path:

```yaml
on:
  push:
    paths:
      - 'dotnet/**'
      - '.github/workflows/dotnet.yml'
  pull_request:
    paths:
      - 'dotnet/**'
      - '.github/workflows/dotnet.yml'
```

Análogo para los otros dos. Esto evita correr el CI de Python cuando solo cambia código .NET.

---

## 11. Vectores compartidos (`shared/test-vectors.json`)

Para que los tres lenguajes usen los mismos vectores sin duplicar datos, mantener un único `shared/test-vectors.json` con esta estructura:

```json
{
  "spec_version": "0.1.2",
  "spec_url": "https://www.agenciatributaria.es/static_files/AEAT_Desarrolladores/EEDD/IVA/VERI-FACTU/Veri-Factu_especificaciones_huella_hash_registros.pdf",
  "official": [
    {
      "case": "1",
      "description": "Primer registro de alta (sin huella anterior)",
      "type": "alta",
      "input": {
        "idEmisorFactura": "89890001K",
        "numSerieFactura": "12345678/G33",
        "fechaExpedicionFactura": "01-01-2024",
        "tipoFactura": "F1",
        "cuotaTotal": "12.35",
        "importeTotal": "123.45",
        "huellaAnterior": null,
        "fechaHoraHusoGenRegistro": "2024-01-01T19:20:30+01:00"
      },
      "expectedConcatenation": "IDEmisorFactura=89890001K&NumSerieFactura=12345678/G33&FechaExpedicionFactura=01-01-2024&TipoFactura=F1&CuotaTotal=12.35&ImporteTotal=123.45&Huella=&FechaHoraHusoGenRegistro=2024-01-01T19:20:30+01:00",
      "expectedHash": "3C464DAF61ACB827C65FDA19F352A4E3BDC2C640E9E9FC4CC058073F38F12F60"
    },
    {
      "case": "2",
      "description": "Segundo registro de alta (encadenando con caso 1)",
      "type": "alta",
      "input": {
        "idEmisorFactura": "89890001K",
        "numSerieFactura": "12345679/G34",
        "fechaExpedicionFactura": "01-01-2024",
        "tipoFactura": "F1",
        "cuotaTotal": "12.35",
        "importeTotal": "123.45",
        "huellaAnterior": "3C464DAF61ACB827C65FDA19F352A4E3BDC2C640E9E9FC4CC058073F38F12F60",
        "fechaHoraHusoGenRegistro": "2024-01-01T19:20:35+01:00"
      },
      "expectedConcatenation": "IDEmisorFactura=89890001K&NumSerieFactura=12345679/G34&FechaExpedicionFactura=01-01-2024&TipoFactura=F1&CuotaTotal=12.35&ImporteTotal=123.45&Huella=3C464DAF61ACB827C65FDA19F352A4E3BDC2C640E9E9FC4CC058073F38F12F60&FechaHoraHusoGenRegistro=2024-01-01T19:20:35+01:00",
      "expectedHash": "F7B94CFD8924EDFF273501B01EE5153E4CE8F259766F88CF6ACB8935802A2B97"
    },
    {
      "case": "3",
      "description": "Registro de anulación (encadenando con caso 2)",
      "type": "anulacion",
      "input": {
        "idEmisorFacturaAnulada": "89890001K",
        "numSerieFacturaAnulada": "12345679/G34",
        "fechaExpedicionFacturaAnulada": "01-01-2024",
        "huellaAnterior": "F7B94CFD8924EDFF273501B01EE5153E4CE8F259766F88CF6ACB8935802A2B97",
        "fechaHoraHusoGenRegistro": "2024-01-01T19:20:40+01:00"
      },
      "expectedConcatenation": "IDEmisorFacturaAnulada=89890001K&NumSerieFacturaAnulada=12345679/G34&FechaExpedicionFacturaAnulada=01-01-2024&Huella=F7B94CFD8924EDFF273501B01EE5153E4CE8F259766F88CF6ACB8935802A2B97&FechaHoraHusoGenRegistro=2024-01-01T19:20:40+01:00",
      "expectedHash": "177547C0D57AC74748561D054A9CEC14B4C4EA23D1BEFD6F2E69E3A388F90C68"
    }
  ],
  "kreyo_internal": []
}
```

Cada test runner (xUnit, vitest, pytest) lee este JSON y itera sobre los casos. Esto garantiza que los tres lenguajes prueban contra **exactamente** los mismos datos, y que añadir un vector nuevo se hace en un solo sitio.

El test runner debe verificar **dos cosas** por cada caso:

1. La cadena concatenada que produce la implementación coincide con `expectedConcatenation`.
2. La huella resultante coincide con `expectedHash`.

Verificar la cadena concatenada por separado es crítico porque si solo se verifica la huella final, un bug en la concatenación que casualmente produce el mismo hash sería invisible (improbable pero conceptualmente posible). Más importante: si falla solo el segundo paso, el desarrollador ya sabe que el bug está en SHA-256 / encoding y no en la concatenación.

---

## 12. Plan de implementación

Orden recomendado:

### Fase 1 — Scaffolding y vectores compartidos (2-4h)

> ⚠️ **Antes de iniciar esta fase, completa los prerequisitos de la sección 0**, especialmente el acceso al repo `nif-validator`. Si no tienes esos archivos a mano, no avances — la estructura de los `.csproj`, `package.json` y `pyproject.toml` que vas a crear aquí depende directamente de patrones que están allí resueltos.

1. Crear el repo en `github.com/kreyo-io/verifactu-hash-calculator`.
2. Crear estructura de carpetas vacía como en §5.
3. Añadir `LICENSE` (MIT), `.gitignore`, `.editorconfig`.
4. Añadir `shared/test-vectors.json` con los 3 casos oficiales.
5. Esqueleto del `README.md` y `README.es.md`.

### Fase 2 — Implementación .NET (4-6h)

1. Solución y proyectos creados (`Kreyo.VerifactuHashCalculator` + `Kreyo.VerifactuHashCalculator.Tests`).
2. Implementar los tres tipos de input + la API estática `HashCalculator`.
3. Implementar `FieldConcatenator` y `Sha256Hex` internos.
4. Tests xUnit que leen `shared/test-vectors.json` y validan los 3 casos oficiales.
5. Workflow CI `.github/workflows/dotnet.yml`.
6. Verificar que CI pasa verde.

### Fase 3 — Implementación TypeScript (4-6h)

Espejo de la fase 2 con tooling TS (vitest, tsup).

Punto de decisión: ¿v1 solo Node, o también browser? Recomendación: **solo Node en v1**, anunciar browser en v0.2.

### Fase 4 — Implementación Python (4-6h)

Espejo de las anteriores con tooling Python (pytest, hatchling, mypy).

### Fase 5 — Documentación final (2-3h)

1. README inglés y español completos.
2. `docs/algorithm.md` y `docs/algorithm.md.es.md` — versión expandida de la sección 3 de este spec, accesible para developers que vienen de Google.
3. `docs/test-vectors.md` — documentación de los 3 casos oficiales y los kreyo-internal.
4. `CHANGELOG.md` con `0.1.0 - Initial release`.

### Fase 6 — Publicación (2h)

1. Tag `v0.1.0`.
2. Publicar a NuGet desde `dotnet/`.
3. Publicar a npm desde `typescript/`.
4. Publicar a PyPI desde `python/`.
5. Crear GitHub Release con changelog.

**Total estimado**: 18-27h de trabajo. Razonable distribuirlo en 3-4 días.

---

## 13. Criterio de "hecho" (definition of done)

El repo está listo para anunciarse cuando:

- [ ] Los tres CI están en verde (.NET, TypeScript, Python).
- [ ] Los tres vectores oficiales del PDF v0.1.2 pasan en los tres lenguajes.
- [ ] Los vectores `kreyo_internal` (cuando los haya) pasan en los tres lenguajes.
- [ ] El paquete está publicado en NuGet (`Kreyo.VerifactuHashCalculator`), npm (`@kreyo/verifactu-hash-calculator`) y PyPI (`kreyo-verifactu-hash-calculator`) en versión `0.1.0`.
- [ ] El README inglés es navegable y tiene los snippets `Quick example` ejecutables.
- [ ] El README español es navegable y localizado (no traducción literal).
- [ ] La sección "Test vectors" del README inglés contiene los 3 casos oficiales con sus inputs y outputs en formato copiable.
- [ ] El repo declara en el README la conformidad con la versión `v0.1.2 (27/08/2024)` del PDF AEAT.
- [ ] Hay un `CONTRIBUTING.md` mínimo explicando cómo correr tests y cómo añadir vectores nuevos.
- [ ] Hay un GitHub Release `v0.1.0`.

---

## 14. Lo que NO hay que hacer

- **No** parsear el XML del registro VeriFactu. La librería recibe los campos como input estructurado; cada caller es responsable de extraerlos de su XML.
- **No** validar formato de NIF, fechas, importes. La librería es pura serialización + hash. Validación fiscal es scope de otras librerías.
- **No** añadir helpers de "construir el XML del registro" o "firmar el registro". Esos son productos comerciales (Kreyo VeriFactu).
- **No** soportar la generación de QR. Hay un PDF separado de la AEAT para QR ("Detalle de las especificaciones técnicas del código QR de la factura"); cuando merezca la pena, irá en otro repo (`verifactu-qr-generator`), no aquí.
- **No** soportar verificación de cadenas completas en v1 (ver §7).
- **No** "ayudar" reformateando importes (ver §3.4.2 — decisión de implementación crítica).
- **No** añadir dependencias runtime más allá de la stdlib de cada lenguaje.

---

## 15. Después de v0.1.0 (post-release)

Roadmap orientativo (no obligatorio, decisiones tomadas según tracción):

- **v0.2.0**: soporte browser en TypeScript con `crypto.subtle` (API asíncrona, requiere ajustar tipos de retorno).
- **v0.3.0**: helper opcional de verificación de cadenas (sequence verification): `verifyChain(records: Record[]): VerificationResult`.
- **v0.4.0**: actualización a la siguiente versión del PDF AEAT cuando salga.
- Si la AEAT publica vectores adicionales en alguna versión futura del PDF, incorporarlos al `shared/test-vectors.json`.
