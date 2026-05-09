import { createHash } from 'node:crypto';

export function computeSha256Hex(input: string): string {
  // UTF-8 is the default encoding when passing a string to update
  return createHash('sha256').update(input, 'utf8').digest('hex').toUpperCase();
}
