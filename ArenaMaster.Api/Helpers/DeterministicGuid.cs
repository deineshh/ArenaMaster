using System.Security.Cryptography;
using System.Text;

namespace ArenaMaster.Api.Helpers;

public static class DeterministicGuid
{
    public static Guid Create(string seed)
    {
        using var md5 = MD5.Create();
        var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(seed));
        return new Guid(hash);
    }
}
