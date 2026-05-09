import json
import os
from pathlib import Path

from kreyo_verifactu_hash_calculator.records import (
    RegistroAltaInput,
    RegistroAnulacionInput,
)
from kreyo_verifactu_hash_calculator.hash_calculator import (
    compute_registro_alta,
    compute_registro_anulacion,
    verify_registro_alta,
    verify_registro_anulacion,
    _concatenate_registro_alta,
    _concatenate_registro_anulacion,
)

VECTORS_PATH = Path(__file__).parent.parent.parent / "shared" / "test-vectors.json"

with open(VECTORS_PATH, "r", encoding="utf-8") as f:
    VECTORS_DATA = json.load(f)

OFFICIAL_VECTORS = VECTORS_DATA.get("official", [])


def test_official_vectors() -> None:
    for v in OFFICIAL_VECTORS:
        if v["type"] == "alta":
            input_data = RegistroAltaInput(
                id_emisor_factura=v["input"]["idEmisorFactura"],
                num_serie_factura=v["input"]["numSerieFactura"],
                fecha_expedicion_factura=v["input"]["fechaExpedicionFactura"],
                tipo_factura=v["input"]["tipoFactura"],
                cuota_total=v["input"]["cuotaTotal"],
                importe_total=v["input"]["importeTotal"],
                huella_anterior=v["input"]["huellaAnterior"],
                fecha_hora_huso_gen_registro=v["input"]["fechaHoraHusoGenRegistro"],
            )
            concatenation = _concatenate_registro_alta(input_data)
            assert concatenation == v["expectedConcatenation"]

            hash_result = compute_registro_alta(input_data)
            assert hash_result == v["expectedHash"]

            assert verify_registro_alta(input_data, v["expectedHash"]) is True
            assert verify_registro_alta(input_data, v["expectedHash"].lower()) is True
        elif v["type"] == "anulacion":
            input_data = RegistroAnulacionInput(
                id_emisor_factura_anulada=v["input"]["idEmisorFacturaAnulada"],
                num_serie_factura_anulada=v["input"]["numSerieFacturaAnulada"],
                fecha_expedicion_factura_anulada=v["input"]["fechaExpedicionFacturaAnulada"],
                huella_anterior=v["input"]["huellaAnterior"],
                fecha_hora_huso_gen_registro=v["input"]["fechaHoraHusoGenRegistro"],
            )
            concatenation = _concatenate_registro_anulacion(input_data)
            assert concatenation == v["expectedConcatenation"]

            hash_result = compute_registro_anulacion(input_data)
            assert hash_result == v["expectedHash"]

            assert verify_registro_anulacion(input_data, v["expectedHash"]) is True
