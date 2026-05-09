import { describe, it, expect } from 'vitest';
import * as fs from 'node:fs';
import * as path from 'node:path';
import {
  computeRegistroAlta,
  _concatenateRegistroAlta,
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

describe('Kreyo internal test vectors', () => {
  if (!vectors.kreyo_internal || vectors.kreyo_internal.length === 0) {
    it('no internal vectors', () => {
      expect(true).toBe(true);
    });
    return;
  }

  for (const v of vectors.kreyo_internal) {
    it(`Case ${v.case} - ${v.description}`, () => {
      if (v.type === 'alta') {
        const concatenation = _concatenateRegistroAlta(v.input);
        expect(concatenation).toBe(v.expectedConcatenation);
        
        const hash = computeRegistroAlta(v.input);
        expect(hash).toBe(v.expectedHash);
      }
    });
  }
});
