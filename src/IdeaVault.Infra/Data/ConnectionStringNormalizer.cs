using Npgsql;

namespace IdeaVault.Infra.Data;

/// <summary>
/// Accepts both Npgsql key/value connection strings and the <c>postgresql://user:pass@host/db?sslmode=require</c>
/// URLs that Neon (and most hosts) hand out, converting the latter.
/// </summary>
public static class ConnectionStringNormalizer
{
    public static string Normalize(string connectionString)
    {
        if (!connectionString.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase)
            && !connectionString.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
        {
            return connectionString;
        }

        var uri = new Uri(connectionString);
        var credentials = uri.UserInfo.Split(':', 2);

        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Port = uri.Port > 0 ? uri.Port : 5432,
            Database = Uri.UnescapeDataString(uri.AbsolutePath.TrimStart('/')),
            Username = Uri.UnescapeDataString(credentials[0]),
            Password = credentials.Length > 1 ? Uri.UnescapeDataString(credentials[1]) : null,
        };

        foreach (var pair in uri.Query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var (key, value) = (pair.Split('=', 2)[0], Uri.UnescapeDataString(pair.Split('=', 2).ElementAtOrDefault(1) ?? ""));
            switch (key.ToLowerInvariant())
            {
                case "sslmode" when Enum.TryParse<SslMode>(value, ignoreCase: true, out var sslMode):
                    builder.SslMode = sslMode;
                    break;
                case "channel_binding" when Enum.TryParse<ChannelBinding>(value, ignoreCase: true, out var binding):
                    builder.ChannelBinding = binding;
                    break;
            }
        }

        return builder.ConnectionString;
    }
}
