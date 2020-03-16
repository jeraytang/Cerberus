using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dapper;
using IdentityServer4.Storage.Adapter;
using MySql.Data.MySqlClient;

namespace IdentityServer4.Storage.MySql.Extensions
{
    public static class IdentityServerOptionsExtensions
    {
        public static void Migrate(this IdentityServerOptions options)
        {
            var assembly = typeof(IdentityServerOptionsExtensions).Assembly;
            var files = assembly.GetManifestResourceNames()
                .Where(x => x.StartsWith("IdentityServer4.Storage.MySql.DDL")).ToList();
            var dict = new Dictionary<int, string>();
            foreach (var file in files)
            {
                var number = int.Parse(file.Replace("IdentityServer4.Storage.MySql.DDL.", "").Replace(".sql", ""));
                using var stream = assembly.GetManifestResourceStream(file);
                if (stream == null)
                {
                    continue;
                }

                using var reader = new StreamReader(stream);
                dict.Add(number, reader.ReadToEnd());
            }

            var numbers = dict.Keys.ToList();
            var createMigrationsHistoryTableSql = dict[00000001];
            numbers.Remove(00000001);
            numbers.Sort();

            var mySqlConnectionStringBuilder = new MySqlConnectionStringBuilder(options.ConnectionString);
            var database = mySqlConnectionStringBuilder.Database;
            mySqlConnectionStringBuilder.Database = "mysql";
            using var conn = new MySqlConnection(mySqlConnectionStringBuilder.ToString());
            conn.Open();
            conn.Execute($"CREATE DATABASE IF NOT EXISTS {database};");
            conn.ChangeDatabase(database);
            conn.Execute(createMigrationsHistoryTableSql);
            var migrations = conn.Query<string>($"SELECT MigrationId FROM __EFMigrationsHistory").ToList();

            var transaction = conn.BeginTransaction();
            foreach (var number in numbers)
            {
                if (!migrations.Contains(number.ToString()))
                {
                    var sql = dict[number];
                    conn.Execute(sql, transaction);
                    conn.Execute(
                        $"INSERT INTO __EFMigrationsHistory (MigrationId,ProductVersion) VALUES (@MigrationId, '2020-01-21');",
                        new {MigrationId = number}, transaction);
                }
            }

            transaction.Commit();
            conn.Dispose();
        }
    }
}