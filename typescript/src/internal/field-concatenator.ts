export function concatenateFields(fields: Array<[string, string | null]>): string {
  return fields
    .map(([name, value]) => {
      const trimmedValue = value ? value.trim() : '';
      return `${name}=${trimmedValue}`;
    })
    .join('&');
}
