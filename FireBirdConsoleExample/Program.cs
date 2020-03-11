using System;
using System.Linq;
using FirebirdSql.Data.FirebirdClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;
using System.Data.Odbc;

namespace FireBirdConsoleExample
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            //odbc2();
            
            //Odbc();
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

        static void odbc2()
        {
//            ODBC_EVENT_INFO eventInfo[] =
//            {
//                INIT_ODBC_EVENT("new_order"),
//                INIT_ODBC_EVENT("change_order")
//            };
            //https://www.easysoft.com/developer/languages/csharp/ado-net-odbc.html
            OdbcConnection DbConnection = new OdbcConnection("DSN=FireBirdDemoDB");
            DbConnection.InfoMessage +=  (sender, e) => Console.WriteLine($"Event: {e}");

            DbConnection.Open();
            OdbcCommand DbCommand = DbConnection.CreateCommand();
            DbCommand.CommandText = "SELECT * FROM DEMO";
            OdbcDataReader DbReader = DbCommand.ExecuteReader();

            int fCount = DbReader.FieldCount;
            Console.Write(":");
            for ( int i = 0; i < fCount; i ++ )
            {
                String fName = DbReader.GetName(i);
                Console.Write( fName + ":");
            }
            Console.WriteLine();

            while( DbReader.Read()) 
            {
                Console.Write( ":" );
                for (int i = 0; i < fCount; i++)
                {
                    String col = DbReader.GetString(i);
                        
                    Console.Write(col + ":");
                }
                Console.WriteLine();
            }
            Console.ReadLine();
            DbReader.Close();
            DbCommand.Dispose();
            DbConnection.Close();
        }

        static void Odbc()
        {
            //https://dev.mysql.com/doc/connector-odbc/en/connector-odbc-examples-programming-net-csharp.html
            try
        {
          //Connection string for Connector/ODBC 3.51
          string MyConString = "DRIVER={FireBird DemoDB};" +
            "SERVER=localhost;" +
            "DATABASE=demo;" +
            "UID=SYSDBA;" +
            "PASSWORD=masterkey;";

          //Connect to MySQL using Connector/ODBC
          OdbcConnection MyConnection = new OdbcConnection(MyConString);
          MyConnection.Open();

          Console.WriteLine("\n !!! success, connected successfully !!!\n");

          //Display connection information
          Console.WriteLine("Connection Information:");
          Console.WriteLine("\tConnection String:" +
                            MyConnection.ConnectionString);
          Console.WriteLine("\tConnection Timeout:" +
                            MyConnection.ConnectionTimeout);
          Console.WriteLine("\tDatabase:" +
                            MyConnection.Database);
          Console.WriteLine("\tDataSource:" +
                            MyConnection.DataSource);
          Console.WriteLine("\tDriver:" +
                            MyConnection.Driver);
          Console.WriteLine("\tServerVersion:" +
                            MyConnection.ServerVersion);

          //Create a sample table
          OdbcCommand MyCommand =
            new OdbcCommand("DROP TABLE IF EXISTS my_odbc_net",
                            MyConnection);
          MyCommand.ExecuteNonQuery();
          MyCommand.CommandText =
            "CREATE TABLE my_odbc_net(id int, name varchar(20), idb bigint)";
          MyCommand.ExecuteNonQuery();

          //Insert
          MyCommand.CommandText =
            "INSERT INTO my_odbc_net VALUES(10,'venu', 300)";
          Console.WriteLine("INSERT, Total rows affected:" +
                            MyCommand.ExecuteNonQuery());;

          //Insert
          MyCommand.CommandText =
            "INSERT INTO my_odbc_net VALUES(20,'mysql',400)";
          Console.WriteLine("INSERT, Total rows affected:" +
                            MyCommand.ExecuteNonQuery());

          //Insert
          MyCommand.CommandText =
            "INSERT INTO my_odbc_net VALUES(20,'mysql',500)";
          Console.WriteLine("INSERT, Total rows affected:" +
                            MyCommand.ExecuteNonQuery());

          //Update
          MyCommand.CommandText =
            "UPDATE my_odbc_net SET id=999 WHERE id=20";
          Console.WriteLine("Update, Total rows affected:" +
                            MyCommand.ExecuteNonQuery());

          //COUNT(*)
          MyCommand.CommandText =
            "SELECT COUNT(*) as TRows FROM my_odbc_net";
          Console.WriteLine("Total Rows:" +
                            MyCommand.ExecuteScalar());

          //Fetch
          MyCommand.CommandText = "SELECT * FROM my_odbc_net";
          OdbcDataReader MyDataReader;
          MyDataReader =  MyCommand.ExecuteReader();
          while (MyDataReader.Read())
            {
              if(string.Compare(MyConnection.Driver,"myodbc3.dll") == 0) {
                //Supported only by Connector/ODBC 3.51
                Console.WriteLine("Data:" + MyDataReader.GetInt32(0) + " " +
                                  MyDataReader.GetString(1) + " " +
                                  MyDataReader.GetInt64(2));
              }
              else {
                //BIGINTs not supported by Connector/ODBC
                Console.WriteLine("Data:" + MyDataReader.GetInt32(0) + " " +
                                  MyDataReader.GetString(1) + " " +
                                  MyDataReader.GetInt32(2));
              }
            }

          //Close all resources
          MyDataReader.Close();
          MyConnection.Close();
        }
      catch (OdbcException MyOdbcException) //Catch any ODBC exception ..
        {
          for (int i=0; i < MyOdbcException.Errors.Count; i++)
            {
              Console.Write("ERROR #" + i + "\n" +
                            "Message: " +
                            MyOdbcException.Errors[i].Message + "\n" +
                            "Native: " +
                            MyOdbcException.Errors[i].NativeError.ToString() + "\n" +
                            "Source: " +
                            MyOdbcException.Errors[i].Source + "\n" +
                            "SQL: " +
                            MyOdbcException.Errors[i].SQLState + "\n");
            }
        }
        }

        static void FirebirdEventListener(string connectionString)
        {
            using (var events = new FbRemoteEvent(connectionString))
            {
                events.RemoteEventCounts += (sender, e) => Console.WriteLine($"Event: {e.Name} | Counts: {e.Counts}");
                events.RemoteEventError += (sender, e) => Console.WriteLine($"ERROR: {e.Error}");
                events.QueueEvents("NEWFOOBARD_POSTEVENT", "OtherEvent");
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
