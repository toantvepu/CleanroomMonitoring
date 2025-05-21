 
using CleanroomMonitoring.Web.Models;

using Microsoft.EntityFrameworkCore;

namespace CleanroomMonitoring.Web.Data
{
    public class dbDataContext : DbContext
    {
        public dbDataContext(DbContextOptions<dbDataContext> options) : base(options) { }

        public DbSet<CleanRoom> CleanRooms { get; set; }
        public DbSet<SensorInfo> SensorInfos { get; set; }
        public DbSet<SensorReading> SensorReadings { get; set; }
        public DbSet<SensorType> SensorTypes { get; set; }
        public DbSet<Factory> Factories { get; set; } 
        public DbSet<SensorLocation> SensorLocations { get; set; }
        public DbSet<SensorConfig> SensorConfigs { get; set; }

        public DbSet<MaintenanceEvent> MaintenanceEvents { get; set; }
        public DbSet<AlertHistory> AlertHistorys { get; set; }
        public DbSet<AlertThreshold> AlertThresholds { get; set; }
        public DbSet<AuditTrail> AuditTrails { get; set; }
        public DbSet<EmailNotificationHistory> EmailNotificationHistorys { get; set; }
        public DbSet<ErrorLog> ErrorLogs { get; set; }
        public DbSet<SensorConnectionStatus> SensorConnectionStatuss { get; set; }
        public DbSet<SensorFlags> SensorFlagss { get; set; }
        public DbSet<SensorHealthCheckHistory> SensorHealthCheckHistorys { get; set; }
        public DbSet<LogReadSensor> LogReadSensors { get; set; }


        // Add User-related DbSets
        public DbSet<User> Users { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<UserRoleMapping> UserRoleMappings { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure default values as per SQL script
            modelBuilder.Entity<CleanRoom>()
                .Property(r => r.CreatedDate)
                .HasDefaultValueSql("getdate()");

            modelBuilder.Entity<SensorReading>()
                .Property(r => r.ReadingValue)
              .HasDefaultValue((decimal?)0);

            modelBuilder.Entity<SensorReading>()
                .Property(r => r.IsValid)
                .HasDefaultValue(true);

            // Configure relationships
            modelBuilder.Entity<CleanRoom>()
                .HasOne(r => r.Factory)
                .WithMany(f => f.CleanRooms)
                .HasForeignKey(r => r.FactoryID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SensorInfo>()
                .HasOne(s => s.CleanRoom)
                .WithMany(r => r.SensorInfos)
                .HasForeignKey(s => s.RoomID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SensorReading>()
                .HasOne(r => r.SensorInfo)
                .WithMany(s => s.SensorReadings)
                .HasForeignKey(r => r.SensorInfoID)
                .OnDelete(DeleteBehavior.Restrict);


            // User-related configurations
            modelBuilder.Entity<User>()
                .Property(u => u.IsActive)
                .HasDefaultValue(true);

            modelBuilder.Entity<User>()
                .Property(u => u.FailedLoginAttempts)
                .HasDefaultValue(0);

            modelBuilder.Entity<UserRoleMapping>()
                .Property(m => m.AssignedDate)
                .HasDefaultValueSql("getdate()");

            modelBuilder.Entity<UserRoleMapping>()
                .HasOne(m => m.User)
                .WithMany(u => u.UserRoleMappings)
                .HasForeignKey(m => m.UserID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserRoleMapping>()
                .HasOne(m => m.UserRole)
                .WithMany(r => r.UserRoleMappings)
                .HasForeignKey(m => m.RoleID)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}