import { RegistroAltaInput, RegistroAnulacionInput, RegistroEventoInput } from './records';
import { concatenateFields } from './internal/field-concatenator';
import { computeSha256Hex } from './internal/sha256-hex';

/**
 * Solo expuesto para tests internos.
 */
export function _concatenateRegistroAlta(input: RegistroAltaInput): string {
  return concatenateFields([
    ['IDEmisorFactura', input.idEmisorFactura],
    ['NumSerieFactura', input.numSerieFactura],
    ['FechaExpedicionFactura', input.fechaExpedicionFactura],
    ['TipoFactura', input.tipoFactura],
    ['CuotaTotal', input.cuotaTotal],
    ['ImporteTotal', input.importeTotal],
    ['Huella', input.huellaAnterior],
    ['FechaHoraHusoGenRegistro', input.fechaHoraHusoGenRegistro]
  ]);
}

/**
 * Solo expuesto para tests internos.
 */
export function _concatenateRegistroAnulacion(input: RegistroAnulacionInput): string {
  return concatenateFields([
    ['IDEmisorFacturaAnulada', input.idEmisorFacturaAnulada],
    ['NumSerieFacturaAnulada', input.numSerieFacturaAnulada],
    ['FechaExpedicionFacturaAnulada', input.fechaExpedicionFacturaAnulada],
    ['Huella', input.huellaAnterior],
    ['FechaHoraHusoGenRegistro', input.fechaHoraHusoGenRegistro]
  ]);
}

/**
 * Solo expuesto para tests internos.
 */
export function _concatenateRegistroEvento(input: RegistroEventoInput): string {
  return concatenateFields([
    ['NIF', input.sistemaInformaticoNif],
    ['ID', input.sistemaInformaticoId],
    ['IdSistemaInformatico', input.idSistemaInformatico],
    ['Version', input.version],
    ['NumeroInstalacion', input.numeroInstalacion],
    ['NIF', input.obligadoEmisionNif],
    ['TipoEvento', input.tipoEvento],
    ['HuellaEvento', input.huellaEventoAnterior],
    ['FechaHoraHusoGenEvento', input.fechaHoraHusoGenEvento]
  ]);
}

export function computeRegistroAlta(input: RegistroAltaInput): string {
  const concatenation = _concatenateRegistroAlta(input);
  return computeSha256Hex(concatenation);
}

export function computeRegistroAnulacion(input: RegistroAnulacionInput): string {
  const concatenation = _concatenateRegistroAnulacion(input);
  return computeSha256Hex(concatenation);
}

export function computeRegistroEvento(input: RegistroEventoInput): string {
  const concatenation = _concatenateRegistroEvento(input);
  return computeSha256Hex(concatenation);
}

export function verifyRegistroAlta(input: RegistroAltaInput, expectedHash: string): boolean {
  return computeRegistroAlta(input).toLowerCase() === expectedHash.toLowerCase();
}

export function verifyRegistroAnulacion(input: RegistroAnulacionInput, expectedHash: string): boolean {
  return computeRegistroAnulacion(input).toLowerCase() === expectedHash.toLowerCase();
}

export function verifyRegistroEvento(input: RegistroEventoInput, expectedHash: string): boolean {
  return computeRegistroEvento(input).toLowerCase() === expectedHash.toLowerCase();
}
