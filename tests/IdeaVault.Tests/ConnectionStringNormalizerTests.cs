using IdeaVault.Infra.Data;
using Npgsql;

namespace IdeaVault.Tests;

public sealed class ConnectionStringNormalizerTests
{
    [Fact]
    public void Converts_neon_style_urls()
    {
        var result = new NpgsqlConnectionStringBuilder(ConnectionStringNormalizer.Normalize(
            "postgresql://neondb_owner:p%40ss@ep-cool-1.sa-east-1.aws.neon.tech/neondb?sslmode=require&channel_binding=require"));

        Assert.Equal("ep-cool-1.sa-east-1.aws.neon.tech", result.Host);
        Assert.Equal(5432, result.Port);
        Assert.Equal("neondb", result.Database);
        Assert.Equal("neondb_owner", result.Username);
        Assert.Equal("p@ss", result.Password);
        Assert.Equal(SslMode.Require, result.SslMode);
        Assert.Equal(ChannelBinding.Require, result.ChannelBinding);
    }

    [Fact]
    public void Keeps_explicit_port()
    {
        var result = new NpgsqlConnectionStringBuilder(
            ConnectionStringNormalizer.Normalize("postgres://u:p@localhost:6543/db"));

        Assert.Equal(6543, result.Port);
    }

    [Fact]
    public void Leaves_key_value_strings_untouched()
    {
        const string value = "Host=localhost;Database=ideavault;Username=postgres;Password=postgres";

        Assert.Equal(value, ConnectionStringNormalizer.Normalize(value));
    }
}
