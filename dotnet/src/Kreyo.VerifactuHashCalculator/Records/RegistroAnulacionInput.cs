namespace Kreyo.VerifactuHashCalculator.Records;

public sealed record RegistroAnulacionInput(
    string IDEmisorFacturaAnulada,
    string NumSerieFacturaAnulada,
    string FechaExpedicionFacturaAnulada,
    string? HuellaAnterior,
    string FechaHoraHusoGenRegistro
);
