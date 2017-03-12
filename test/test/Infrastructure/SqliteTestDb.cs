using System;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using PlantTree.Data;

namespace test
{
    public class SqliteTestDb : IDisposable
    {
        public DbContextOptions<AppDbContext> Options { get; protected set; }
        private readonly SqliteConnection _connection;

        public SqliteTestDb()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();
            try
            {
                Options = new DbContextOptionsBuilder<AppDbContext>()
                    .UseSqlite(_connection)
                    .Options;

                // Create the schema in the database
                using (var context = new AppDbContext(Options))
                {
                    context.Database.EnsureCreated();
                }
            }
            catch
            {
                this.Dispose();
                throw;
            }
        }

        public void Dispose()
        {
            _connection.Close();
        }
    }
}