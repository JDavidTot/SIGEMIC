using System.Security.Cryptography;
using System.Text;

namespace SIGEMIC.Security;

public static class CorreoHelper
{
    public static string Normalizar(
        string correo)
    {
        return correo
            .Trim()
            .ToLowerInvariant();
    }

    public static byte[] CalcularHash(
        string correo)
    {
        string normalizado =
            Normalizar(correo);

        return SHA256.HashData(
            Encoding.UTF8.GetBytes(
                normalizado
            )
        );
    }

    public static byte[] ConvertirABytes(
        string correo)
    {
        string normalizado =
            Normalizar(correo);

        return Encoding.UTF8.GetBytes(
            normalizado
        );
    }

    public static string? LeerCorreo(
        byte[]? correoCifrado)
    {
        if (
            correoCifrado == null ||
            correoCifrado.Length == 0
        )
        {
            return null;
        }

        return Encoding.UTF8.GetString(
            correoCifrado
        );
    }
}