using System;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace test.Infrastructure
{
    public class SqliteTestDb<T> : ITestDb<T> where T : DbContext
    {
        public DbContextOptions<T> Options { get; protected set; }
        private readonly SqliteConnection _connection;

        public SqliteTestDb()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();
            try
            {
                Options = new DbContextOptionsBuilder<T>()
                    .UseSqlite(_connection)
                    .Options;

                // Create the schema in the database
                using (var context = CreateContext())
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

        public T CreateContext()
        {
            return (T) Activator.CreateInstance(typeof(T), Options);
        }

        public void Dispose()
        {
            _connection.Close();
        }
    }
}