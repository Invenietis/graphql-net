using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphQL.Net;
using Microsoft.Data.Entity;
using Newtonsoft.Json;

namespace DnxConsoleParser
{
    // This project can output the Class library as a NuGet Package.
    // To enable this option, right-click on the project and select the Properties menu item. In the Build tab select "Produce outputs on build".
    public class Program
    {
        public static void Main()
        {
            var schema = GraphQL<TestContext>.CreateDefaultSchema(() => new TestContext());
            schema.AddType<User>()
                    .AddField( u => u.Id )
                    .AddField( u => u.Name )
                    .AddField( u => u.Account )
                    .AddField( "total", ( db, u ) => db.Users.Count() )
                    .AddField( "accountPaid", ( db, u ) => u.Account.Paid );
            schema.AddQuery( "users", db => db.Users );
            schema.AddQuery( "user", new { id = 0 }, ( db, args ) => db.Users.Where( u => u.Id == args.id ).FirstOrDefault() );

            //schema.AddType<Account>().AddAllFields(); // TODO:
            schema.AddType<Account>()
                .AddField( a => a.Id )
                .AddField( a => a.Name )
                .AddField( a => a.Paid );

            //Initialize();
            var queryStr0 = @"
query users {
    idAlias : id,
    nameAlias : name
}";

            var queryStr1 = @"
query user(id:1) {
    idAlias : id,
    nameAlias : name,
    account {
        id
    },
    total
}";

            var queryStr2 = @"
query user(id:0) {
    idAlias : id,
    nameAlias : name,
    account {
        id
    }
}";

            var queryStr3 = @"
query users {
    idAlias : id,
    nameAlias : name,
    account {
        id
    }
    total
    accountPaid
}";

            var queryStr4 = @"
query users {
   ...userFragment
    account {
        ...accountFragment
    }
    total
    accountPaid
}

fragment userFragment on User {
   idAlias : id,
   nameAlias : name
}

fragment accountFragment on Account {
    id
    name
    paid
}
";
            var dict = GraphQL<TestContext>.Execute(queryStr0);
            Console.WriteLine( JsonConvert.SerializeObject( dict, Formatting.Indented ) );

            dict = GraphQL<TestContext>.Execute( queryStr1 );
            Console.WriteLine( JsonConvert.SerializeObject( dict, Formatting.Indented ) );

            dict = GraphQL<TestContext>.Execute( queryStr2 );
            Console.WriteLine( JsonConvert.SerializeObject( dict, Formatting.Indented ) );

            dict = GraphQL<TestContext>.Execute( queryStr3 );
            Console.WriteLine( JsonConvert.SerializeObject( dict, Formatting.Indented ) );

            dict = GraphQL<TestContext>.Execute( queryStr4 );
            Console.WriteLine( JsonConvert.SerializeObject( dict, Formatting.Indented ) );

            Console.ReadLine();
        }

        private static void Initialize()
        {
            using( var db = new TestContext() )
            {
                var account = new Account
                {
                    Name = "My Test Account",
                    Paid = true
                };
                db.Accounts.Add( account );
                var user = new User
                {
                    Name = "Joe User",
                    Account = account
                };
                db.Users.Add( user );
                var account2 = new Account
                {
                    Name = "Another Test Account",
                    Paid = false
                };
                db.Accounts.Add( account2 );
                var user2 = new User
                {
                    Name = "Late Paying User",
                    Account = account2
                };
                db.Users.Add( user2 );
                db.SaveChanges();
            }
        }
    }
    public class TestContext : DbContext
    {
        public TestContext()
        {
        }

        protected override void OnConfiguring( DbContextOptionsBuilder optionsBuilder )
        {
            optionsBuilder.UseSqlServer( "Server=.;Database=GraphQL;Integrated Security=SSPI;" ).DisableQueryClientEvaluation();
            base.OnConfiguring( optionsBuilder );
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Account> Accounts { get; set; }
    }

    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public int AccountId { get; set; }
        public Account Account { get; set; }
    }

    public class Account
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool Paid { get; set; }
    }
}
