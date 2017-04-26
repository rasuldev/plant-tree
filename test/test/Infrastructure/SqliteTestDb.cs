using System;
using System.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace test.Infrastructure
{
    public class SqliteTestDb<T> : ITestDb<T> where T : DbContext
    {
        private readonly Action<T> _seeder;
        public DbContextOptions<T> Options { get; protected set; }
        private SqliteConnection _connection;

        public SqliteTestDb(Action<T> seeder = null)
        {
            _seeder = seeder;
            Init();
        }

        void Init()
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
                    _seeder?.Invoke(context);
                }
            }
            catch
            {
                this.Dispose();
                throw;
            }
        }

        public void ResetDb()
        {
            if (_connection.State != ConnectionState.Closed)
                _connection.Close();
            Init();
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