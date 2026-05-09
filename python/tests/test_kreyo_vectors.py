import json
from pathlib import Path

from kreyo_verifactu_hash_calculator.records import RegistroAltaInput
from kreyo_verifactu_hash_calculator.hash_calculator import (
    compute_registro_alta,
    _concatenate_registro_alta,
)

VECTORS_PATH = Path(__file__).parent.parent.parent / "shared" / "test-vectors.json"

with open(VECTORS_PATH, "r", encoding="utf-8") as f:
    VECTORS_DATA = json.load(f)

KREYO_VECTORS = VECTORS_DATA.get("kreyo_internal", [])


def test_kreyo_internal_vectors() -> None:
    if not KREYO_VECTORS:
        return

    for v in KREYO_VECTORS:
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
