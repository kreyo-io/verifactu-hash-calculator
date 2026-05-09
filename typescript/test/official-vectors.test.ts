import { describe, it, expect } from 'vitest';
import * as fs from 'node:fs';
import * as path from 'node:path';
import {
  computeRegistroAlta,
  computeRegistroAnulacion,
  verifyRegistroAlta,
  verifyRegistroAnulacion,
  _concatenateRegistroAlta,
  _concatenateRegistroAnulacion
} from '../src/index';

interface TestVector {
  case: string;
  description: string;
  type: string;
  input: any;
  expectedConcatenation: string;
  expectedHash: string;
}

interface TestVectorsFile {
  spec_version: string;
  spec_url: string;
  official: TestVector[];
  kreyo_internal: TestVector[];
}

const vectorsPath = path.resolve(__dirname, '../../shared/test-vectors.json');
const vectorsRaw = fs.readFileSync(vectorsPath, 'utf-8');
const vectors: TestVectorsFile = JSON.parse(vectorsRaw);

describe('Official AEAT test vectors', () => {
  for (const v of vectors.official) {
    it(`Case ${v.case} - ${v.description}`, () => {
      if (v.type === 'alta') {
        const concatenation = _concatenateRegistroAlta(v.input);
        expect(concatenation).toBe(v.expectedConcatenation);
        
        const hash = computeRegistroAlta(v.input);
        expect(hash).toBe(v.expectedHash);

        expect(verifyRegistroAlta(v.input, v.expectedHash)).toBe(true);
        expect(verifyRegistroAlta(v.input, v.expectedHash.toLowerCase())).toBe(true);
      } else if (v.type === 'anulacion') {
        const concatenation = _concatenateRegistroAnulacion(v.input);
        expect(concatenation).toBe(v.expectedConcatenation);
        
        const hash = computeRegistroAnulacion(v.input);
        expect(hash).toBe(v.expectedHash);

        expect(verifyRegistroAnulacion(v.input, v.expectedHash)).toBe(true);
      }
    });
  }
});
