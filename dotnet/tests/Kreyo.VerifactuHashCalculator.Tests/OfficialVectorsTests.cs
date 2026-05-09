using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Kreyo.VerifactuHashCalculator.Records;
using Xunit;

namespace Kreyo.VerifactuHashCalculator.Tests;

public class OfficialVectorsTests
{
    private readonly TestVectorsFile _vectors;

    public OfficialVectorsTests()
    {
        var json = File.ReadAllText("test-vectors.json");
        _vectors = JsonSerializer.Deserialize<TestVectorsFile>(json, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })!;
    }

    [Fact]
    public void RunOfficialVectors()
    {
        foreach (var vector in _vectors.Official)
        {
            if (vector.Type == "alta")
            {
                var input = JsonSerializer.Deserialize<RegistroAltaInput>((JsonElement)vector.Input, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })!;
                
                var concatenation = HashCalculator.ConcatenateRegistroAlta(input);
                Assert.Equal(vector.ExpectedConcatenation, concatenation);
                
                var hash = HashCalculator.ComputeRegistroAlta(input);
                Assert.Equal(vector.ExpectedHash, hash);

                Assert.True(HashCalculator.VerifyRegistroAlta(input, vector.ExpectedHash));
                Assert.False(HashCalculator.VerifyRegistroAlta(input, "0000000000000000000000000000000000000000000000000000000000000000"));
            }
            else if (vector.Type == "anulacion")
            {
                var input = JsonSerializer.Deserialize<RegistroAnulacionInput>((JsonElement)vector.Input, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })!;
                
                var concatenation = HashCalculator.ConcatenateRegistroAnulacion(input);
                Assert.Equal(vector.ExpectedConcatenation, concatenation);
                
                var hash = HashCalculator.ComputeRegistroAnulacion(input);
                Assert.Equal(vector.ExpectedHash, hash);

                Assert.True(HashCalculator.VerifyRegistroAnulacion(input, vector.ExpectedHash));
                Assert.False(HashCalculator.VerifyRegistroAnulacion(input, "0000000000000000000000000000000000000000000000000000000000000000"));
            }
        }
    }
}

public class TestVectorsFile
{
    public string Spec_Version { get; set; } = "";
    public string Spec_Url { get; set; } = "";
    public List<TestVector> Official { get; set; } = new();
    public List<TestVector> Kreyo_Internal { get; set; } = new();
}

public class TestVector
{
    public string Case { get; set; } = "";
    public string Description { get; set; } = "";
    public string Type { get; set; } = "";
    public object Input { get; set; } = new();
    public string ExpectedConcatenation { get; set; } = "";
    public string ExpectedHash { get; set; } = "";
}
