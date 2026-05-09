namespace Kreyo.VerifactuHashCalculator.Records;

public sealed record RegistroAltaInput(
    string IDEmisorFactura,
    string NumSerieFactura,
    string FechaExpedicionFactura,
    string TipoFactura,
    string CuotaTotal,
    string ImporteTotal,
    string? HuellaAnterior,
    string FechaHoraHusoGenRegistro
);
