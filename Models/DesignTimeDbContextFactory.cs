using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace KinmuReport.Models;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AttendanceContext>
{
    public AttendanceContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AttendanceContext>();
        optionsBuilder.UseSqlite("Data Source=Data/kinmu.db");

        // デザインタイムでは Interceptor なしで作成
        return new AttendanceContext(optionsBuilder.Options);
    }
}
