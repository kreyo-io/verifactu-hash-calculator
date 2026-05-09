# VeriFactu Hash Algorithm Specification

This document explains the technical details for generating the footprint (hash) of the billing records, according to the AEAT specification **v0.1.2**.

## 1. Hash Function
**SHA-256** is the only supported algorithm.

## 2. Concatenation Rules
Before applying the hash, specific fields from the XML record must be concatenated into a single string using the following format:
`nombreCampo1=valor1&nombreCampo2=valor2&...&nombreCampoN=valorN`

### Normalization Rules:
- **Trimming**: Spaces at the beginning and end of each value are removed.
- **Missing or Empty Fields**: Included as `nombreCampo=` (no value).
- **Encoding**: No URL encoding or escaping is applied to special characters.
- **Numeric Fields**: Trailing zeroes in decimals are irrelevant to the AEAT but must be sent exactly as they appear in the XML.

## 3. Byte Encoding and Hash
1. The concatenated string is encoded in **UTF-8** (without BOM).
2. The SHA-256 hash is computed.
3. The resulting 32 bytes are encoded in **uppercase hexadecimal**, producing a 64-character string.
