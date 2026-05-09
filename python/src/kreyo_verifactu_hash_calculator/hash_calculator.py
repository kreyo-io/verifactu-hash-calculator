from .records import RegistroAltaInput, RegistroAnulacionInput, RegistroEventoInput
from ._internal.field_concatenator import concatenate_fields
from ._internal.sha256_hex import compute_sha256_hex


def _concatenate_registro_alta(input_data: RegistroAltaInput) -> str:
    return concatenate_fields(
        [
            ("IDEmisorFactura", input_data.id_emisor_factura),
            ("NumSerieFactura", input_data.num_serie_factura),
            ("FechaExpedicionFactura", input_data.fecha_expedicion_factura),
            ("TipoFactura", input_data.tipo_factura),
            ("CuotaTotal", input_data.cuota_total),
            ("ImporteTotal", input_data.importe_total),
            ("Huella", input_data.huella_anterior),
            ("FechaHoraHusoGenRegistro", input_data.fecha_hora_huso_gen_registro),
        ]
    )


def _concatenate_registro_anulacion(input_data: RegistroAnulacionInput) -> str:
    return concatenate_fields(
        [
            ("IDEmisorFacturaAnulada", input_data.id_emisor_factura_anulada),
            ("NumSerieFacturaAnulada", input_data.num_serie_factura_anulada),
            ("FechaExpedicionFacturaAnulada", input_data.fecha_expedicion_factura_anulada),
            ("Huella", input_data.huella_anterior),
            ("FechaHoraHusoGenRegistro", input_data.fecha_hora_huso_gen_registro),
        ]
    )


def _concatenate_registro_evento(input_data: RegistroEventoInput) -> str:
    return concatenate_fields(
        [
            ("NIF", input_data.sistema_informatico_nif),
            ("ID", input_data.sistema_informatico_id),
            ("IdSistemaInformatico", input_data.id_sistema_informatico),
            ("Version", input_data.version),
            ("NumeroInstalacion", input_data.numero_instalacion),
            ("NIF", input_data.obligado_emision_nif),
            ("TipoEvento", input_data.tipo_evento),
            ("HuellaEvento", input_data.huella_evento_anterior),
            ("FechaHoraHusoGenEvento", input_data.fecha_hora_huso_gen_evento),
        ]
    )


def compute_registro_alta(input_data: RegistroAltaInput) -> str:
    concatenation = _concatenate_registro_alta(input_data)
    return compute_sha256_hex(concatenation)


def compute_registro_anulacion(input_data: RegistroAnulacionInput) -> str:
    concatenation = _concatenate_registro_anulacion(input_data)
    return compute_sha256_hex(concatenation)


def compute_registro_evento(input_data: RegistroEventoInput) -> str:
    concatenation = _concatenate_registro_evento(input_data)
    return compute_sha256_hex(concatenation)


def verify_registro_alta(input_data: RegistroAltaInput, expected_hash: str) -> bool:
    return compute_registro_alta(input_data).lower() == expected_hash.lower()


def verify_registro_anulacion(input_data: RegistroAnulacionInput, expected_hash: str) -> bool:
    return compute_registro_anulacion(input_data).lower() == expected_hash.lower()


def verify_registro_evento(input_data: RegistroEventoInput, expected_hash: str) -> bool:
    return compute_registro_evento(input_data).lower() == expected_hash.lower()
