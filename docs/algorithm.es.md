# Especificación del algoritmo de Hash VeriFactu

Este documento explica los detalles técnicos para la generación de la huella (hash) de los registros de facturación, según la especificación de la AEAT **v0.1.2**.

## 1. Función Hash
**SHA-256** es el único algoritmo soportado.

## 2. Reglas de Concatenación
Antes de aplicar el hash, los campos específicos del registro XML deben concatenarse en una única cadena con el siguiente formato:
`nombreCampo1=valor1&nombreCampo2=valor2&...&nombreCampoN=valorN`

### Reglas de Normalización:
- **Trimming**: Se eliminan los espacios al principio y al final de cada valor.
- **Campos Ausentes o Vacíos**: Se incluyen como `nombreCampo=` (sin valor).
- **Codificación**: No se aplica URL-encoding ni se escapan los caracteres especiales.
- **Campos Numéricos**: Los ceros a la derecha en los decimales son irrelevantes para la AEAT pero deben enviarse exactamente igual que como aparecen en el XML.

## 3. Codificación de Bytes y Hash
1. La cadena concatenada se codifica en **UTF-8** (sin BOM).
2. Se calcula el hash SHA-256.
3. Los 32 bytes resultantes se codifican en **hexadecimal en mayúsculas**, produciendo una cadena de 64 caracteres.
