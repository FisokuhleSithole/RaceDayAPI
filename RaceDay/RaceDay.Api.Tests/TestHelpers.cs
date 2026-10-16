using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;

namespace RaceDay.Api.Tests
{
    /// <summary>
    /// Helper class for creating isolated in-memory databases for tests.
    /// Each call returns a fresh database so tests don't interfere.
    /// </summary>
    public static class TestHelpers
    {
        public static RaceDayDbContext CreateInMemoryDb()
        {
            var options = new DbContextOptionsBuilder<RaceDayDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new RaceDayDbContext(options);
        }
    }
}