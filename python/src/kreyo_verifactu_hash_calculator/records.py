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
