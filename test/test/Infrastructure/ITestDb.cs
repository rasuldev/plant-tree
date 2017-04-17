using System;
using Microsoft.EntityFrameworkCore;

namespace test.Infrastructure
{
    public interface ITestDb<out T>: IDisposable where T : DbContext
    {
        T CreateContext();
    }
}