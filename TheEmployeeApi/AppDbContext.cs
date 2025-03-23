using Microsoft.EntityFrameworkCore;
using TheEmployeeApi.Employees;

namespace TheEmployeeApi
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }
        public DbSet<Employee> Employees { get; set; }
    }
}