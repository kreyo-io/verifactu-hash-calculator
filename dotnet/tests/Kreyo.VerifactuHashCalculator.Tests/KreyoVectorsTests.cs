using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Kreyo.VerifactuHashCalculator.Records;
using Xunit;

namespace Kreyo.VerifactuHashCalculator.Tests;

public class KreyoVectorsTests
{
    private readonly TestVectorsFile _vectors;

    public KreyoVectorsTests()
    {
        var json = File.ReadAllText("test-vectors.json");
        _vectors = JsonSerializer.Deserialize<TestVectorsFile>(json, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })!;
    }

    [Fact]
    public void RunKreyoVectors()
    {
        if (_vectors.Kreyo_Internal == null || _vectors.Kreyo_Internal.Count == 0) return;

        foreach (var vector in _vectors.Kreyo_Internal)
        {
            if (vector.Type == "alta")
            {
                var input = JsonSerializer.Deserialize<RegistroAltaInput>((JsonElement)vector.Input, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })!;
                
                var concatenation = HashCalculator.ConcatenateRegistroAlta(input);
                Assert.Equal(vector.ExpectedConcatenation, concatenation);
                
                var hash = HashCalculator.ComputeRegistroAlta(input);
                Assert.Equal(vector.ExpectedHash, hash);
            }
            // Add other cases when needed...
        }
    }
}
