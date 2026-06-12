using System.Security.Cryptography;

namespace SaborMercado.Web.Domain;

/// <summary>
/// Gera GUIDs no formato UUID v7 (prefixo temporal ordenável), conforme
/// docs/standards/data-standards.md. .NET 8 não possui Guid.CreateVersion7.
/// </summary>
public static class Ids
{
    public static Guid NewId() => NewId(DateTimeOffset.UtcNow);

    public static Guid NewId(DateTimeOffset timestamp)
    {
        Span<byte> bytes = stackalloc byte[16];
        RandomNumberGenerator.Fill(bytes);

        var unixMs = timestamp.ToUnixTimeMilliseconds();
        bytes[0] = (byte)(unixMs >> 40);
        bytes[1] = (byte)(unixMs >> 32);
        bytes[2] = (byte)(unixMs >> 24);
        bytes[3] = (byte)(unixMs >> 16);
        bytes[4] = (byte)(unixMs >> 8);
        bytes[5] = (byte)unixMs;

        bytes[6] = (byte)((bytes[6] & 0x0F) | 0x70);
        bytes[8] = (byte)((bytes[8] & 0x3F) | 0x80);

        return new Guid(bytes, bigEndian: true);
    }
}
