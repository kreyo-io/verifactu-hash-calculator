namespace Kreyo.VerifactuHashCalculator.Records;

public sealed record RegistroEventoInput(
    string? SistemaInformaticoNif,
    string? SistemaInformaticoId,
    string IdSistemaInformatico,
    string Version,
    string NumeroInstalacion,
    string ObligadoEmisionNif,
    string TipoEvento,
    string? HuellaEventoAnterior,
    string FechaHoraHusoGenEvento
);
