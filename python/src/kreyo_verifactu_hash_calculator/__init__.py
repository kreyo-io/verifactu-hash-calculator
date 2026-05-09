from .records import RegistroAltaInput, RegistroAnulacionInput, RegistroEventoInput
from .hash_calculator import (
    compute_registro_alta,
    compute_registro_anulacion,
    compute_registro_evento,
    verify_registro_alta,
    verify_registro_anulacion,
    verify_registro_evento,
)

__all__ = [
    "RegistroAltaInput",
    "RegistroAnulacionInput",
    "RegistroEventoInput",
    "compute_registro_alta",
    "compute_registro_anulacion",
    "compute_registro_evento",
    "verify_registro_alta",
    "verify_registro_anulacion",
    "verify_registro_evento",
]
