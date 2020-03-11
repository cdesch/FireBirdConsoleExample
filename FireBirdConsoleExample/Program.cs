using System;
using System.Linq;
using FirebirdSql.Data.FirebirdClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;

namespace FireBirdConsoleExample
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            string connectionString = "User ID=sysdba;Password=masterkey;Database=localhost:C:\\data\\demo.fdb;DataSource=localhost;Charset=NONE;";
            FirebirdEventListener(connectionString);

            //https://github.com/FirebirdSQL/NETProvider/blob/d875d5b3c34ebb3d576ff08c0acb8087282998c7/Provider/docs/entity-framework-core.md
            //https://github.com/FirebirdSQL/NETProvider/blob/master/Provider/docs/entity-framework-core.md
            
            using (var db = new MyContext("database=localhost:C:\\data\\demo.fdb;user=sysdba;password=masterkey"))
            {
                //db.GetService<ILoggerFactory>().AddConsole();

                var demos = db.Demos.ToList();
                foreach (var demo in demos)
                {
                    Console.Write(demo.Id);
                }
                
            }
        }



        static void FirebirdEventListener(string connectionString)
        {
            using (var events = new FbRemoteEvent(connectionString))
            {
                events.RemoteEventCounts += (sender, e) => Console.WriteLine($"Event: {e.Name} | Counts: {e.Counts}");
                events.RemoteEventError += (sender, e) => Console.WriteLine($"ERROR: {e.Error}");
                //events.QueueEvents("EVENT1", "EVENT2", "EVENT3", "EVENT4");
                Console.WriteLine("Listening...");
                Console.ReadLine();
            }
        }
    }

    
    class MyContext : DbContext
    {
        static readonly ILoggerFactory MyLoggerFactory = LoggerFactory.Create(builder => { builder.AddConsole(); });

        readonly string _connectionString;

        public MyContext(string connectionString)
        {
            _connectionString = connectionString;
        }

        public DbSet<Demo> Demos { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            //optionsBuilder.UseFirebird(_connectionString);
            optionsBuilder
                .UseLoggerFactory(MyLoggerFactory)
                .UseFirebird(_connectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var demoConf = modelBuilder.Entity<Demo>();
            demoConf.Property(x => x.Id).HasColumnName("ID");
            demoConf.Property(x => x.FooBar).HasColumnName("FOOBAR");
            demoConf.ToTable("DEMO");
        }
    }

    class Demo
    {
        public int Id { get; set; }
        public string FooBar { get; set; }
    }
}
