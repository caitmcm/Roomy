using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Roomy.API.Data;

namespace Roomy.API.RepositoryTests;

public sealed class TestDbContextFactory : IDisposable
{
    private readonly SqliteConnection connection;

    public TestDbContextFactory()
    {
        connection = new SqliteConnection("Filename=:memory:");
        connection.Open();

        using var context = CreateContext();
        context.Database.EnsureCreated();
    }

    public RoomyDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<RoomyDbContext>()
            .UseSqlite(connection)
            .Options;

        return new RoomyDbContext(options);
    }

    public void Dispose() => connection.Dispose();
}
