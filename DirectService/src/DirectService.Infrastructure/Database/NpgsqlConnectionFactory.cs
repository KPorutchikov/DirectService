using System.Data;
using DirectService.Application.Database;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace DirectService.Infrastructure.Database;

public class NpgsqlConnectionFactory: IDbConnectionFactory
{
    private readonly IConfiguration _configuration;

    public NpgsqlConnectionFactory(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    public async Task<IDbConnection> CreateConnectionAsync(CancellationToken cancellationToken = default)
    {
        return new NpgsqlConnection(_configuration.GetConnectionString("Database"));
    }
}
