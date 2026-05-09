# Contributing to verifactu-hash-calculator

First off, thanks for taking the time to contribute!

This is a multi-language monorepo containing implementations in .NET, TypeScript, and Python. They all share the same test vectors to ensure consistency.

## Test Vectors

The source of truth for all tests is `shared/test-vectors.json`.

If you find an edge case or a bug, please **do not** add a test in just one language. Instead:
1. Add the case to the `kreyo_internal` array in `shared/test-vectors.json`.
2. Run the tests in all three languages to ensure they all handle it correctly (or fail if it's a bug).
3. Fix the implementation in all languages if needed.

## Running Tests

### .NET
```bash
cd dotnet
dotnet test
```

### TypeScript
```bash
cd typescript
npm install
npm test
```

### Python
```bash
cd python
pip install -e ".[test]"
pytest
```
