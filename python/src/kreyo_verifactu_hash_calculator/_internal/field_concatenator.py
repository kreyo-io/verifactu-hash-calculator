def concatenate_fields(fields: list[tuple[str, str | None]]) -> str:
    parts: list[str] = []
    for name, value in fields:
        trimmed_value = value.strip() if value is not None else ""
        parts.append(f"{name}={trimmed_value}")
    return "&".join(parts)
