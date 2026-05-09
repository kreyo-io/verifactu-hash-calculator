import hashlib

def compute_sha256_hex(input_str: str) -> str:
    encoded = input_str.encode("utf-8")
    return hashlib.sha256(encoded).hexdigest().upper()
