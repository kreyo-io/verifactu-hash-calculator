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
