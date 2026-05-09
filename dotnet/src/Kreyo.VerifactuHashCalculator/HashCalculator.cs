using System;
using Kreyo.VerifactuHashCalculator.Internal;
using Kreyo.VerifactuHashCalculator.Records;

namespace Kreyo.VerifactuHashCalculator;

public static class HashCalculator
{
    public static string ComputeRegistroAlta(RegistroAltaInput input)
    {
        var concatenation = ConcatenateRegistroAlta(input);
        return Sha256Hex.Compute(concatenation);
    }

    public static string ComputeRegistroAnulacion(RegistroAnulacionInput input)
    {
        var concatenation = ConcatenateRegistroAnulacion(input);
        return Sha256Hex.Compute(concatenation);
    }

    public static string ComputeRegistroEvento(RegistroEventoInput input)
    {
        var concatenation = ConcatenateRegistroEvento(input);
        return Sha256Hex.Compute(concatenation);
    }

    public static bool VerifyRegistroAlta(RegistroAltaInput input, string expectedHash)
    {
        return ComputeRegistroAlta(input).Equals(expectedHash, StringComparison.OrdinalIgnoreCase);
    }

    public static bool VerifyRegistroAnulacion(RegistroAnulacionInput input, string expectedHash)
    {
        return ComputeRegistroAnulacion(input).Equals(expectedHash, StringComparison.OrdinalIgnoreCase);
    }

    public static bool VerifyRegistroEvento(RegistroEventoInput input, string expectedHash)
    {
        return ComputeRegistroEvento(input).Equals(expectedHash, StringComparison.OrdinalIgnoreCase);
    }

    internal static string ConcatenateRegistroAlta(RegistroAltaInput input)
    {
        return FieldConcatenator.Concatenate(new[]
        {
            ("IDEmisorFactura", input.IDEmisorFactura),
            ("NumSerieFactura", input.NumSerieFactura),
            ("FechaExpedicionFactura", input.FechaExpedicionFactura),
            ("TipoFactura", input.TipoFactura),
            ("CuotaTotal", input.CuotaTotal),
            ("ImporteTotal", input.ImporteTotal),
            ("Huella", input.HuellaAnterior),
            ("FechaHoraHusoGenRegistro", input.FechaHoraHusoGenRegistro)
        });
    }

    internal static string ConcatenateRegistroAnulacion(RegistroAnulacionInput input)
    {
        return FieldConcatenator.Concatenate(new[]
        {
            ("IDEmisorFacturaAnulada", input.IDEmisorFacturaAnulada),
            ("NumSerieFacturaAnulada", input.NumSerieFacturaAnulada),
            ("FechaExpedicionFacturaAnulada", input.FechaExpedicionFacturaAnulada),
            ("Huella", input.HuellaAnterior),
            ("FechaHoraHusoGenRegistro", input.FechaHoraHusoGenRegistro)
        });
    }

    internal static string ConcatenateRegistroEvento(RegistroEventoInput input)
    {
        return FieldConcatenator.Concatenate(new[]
        {
            ("NIF", input.SistemaInformaticoNif),
            ("ID", input.SistemaInformaticoId),
            ("IdSistemaInformatico", input.IdSistemaInformatico),
            ("Version", input.Version),
            ("NumeroInstalacion", input.NumeroInstalacion),
            ("NIF", input.ObligadoEmisionNif),
            ("TipoEvento", input.TipoEvento),
            ("HuellaEvento", input.HuellaEventoAnterior),
            ("FechaHoraHusoGenEvento", input.FechaHoraHusoGenEvento)
        });
    }
}
