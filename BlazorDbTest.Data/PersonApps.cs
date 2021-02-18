using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace BlazorDbTest.Data
{
    public class DbObj<T>
        where T : DbObj<T>, new()
    {
        public string ID { get; set; }
    }

    public static class DbFunc
    {
        public static void AddParameters(this SqliteCommand command, params (string Column, object Value)[] parameters)
        {
            foreach (var parameter in parameters)
            {
                command.Parameters.AddWithValue(parameter.Column, parameter.Value);
            }
        }
    }

    public class BaseApps<TDbObj>
        where TDbObj : DbObj<TDbObj>, new()
    {
        protected TDbObj CreateDbObj() => new TDbObj() { ID = Guid.NewGuid().ToString() };
        public long GetCount() => ExecuteScalar<long>($"SELECT COUNT(*) FROM [{DbObjTypeName}]");
        public void ExecuteNonQuery(string sql, params (string Column, object Value)[] parameters)
        {
            var cmd = PrepareCommand(sql, parameters);
            cmd.ExecuteNonQuery();
        }
        protected SqliteCommand PrepareCommand(string sql, params (string Column, object Value)[] parameters)
        {
            var db = new SqliteConnection(ConnectionString);
            db.Open();
            var cmd = db.CreateCommand();
            cmd.AddParameters(parameters);
            cmd.CommandText = sql;
            return cmd;
        }

        public T ExecuteScalar<T>(string sql, params (string Column, object Value)[] parameters)
        {
            var cmd = PrepareCommand(sql, parameters);
            return (T)cmd.ExecuteScalar();
        }

        public DataTable ExecuteQuery(string sql, params (string Column, object Value)[] parameters)
        {
            var cmd = PrepareCommand(sql, parameters);
            var r = cmd.ExecuteReader();
            DataTable dt = new();
            dt.Load(r);
            return dt;
        }

        protected string ConnectionString { get; set; }

        protected string DbObjTypeName { get; } = typeof(TDbObj).Name;

        protected void CreateTable(params (string Column, string DataType)[] columnDefs) => 
            ExecuteNonQuery($@"CREATE TABLE IF NOT EXISTS [{DbObjTypeName}](ID VARCHAR(40) PRIMARY KEY, 
{string.Join($", {Environment.NewLine}", columnDefs.Select(col => $"{col.Column} {col.DataType}"))})");

        protected void Insert(params (string Column, object Value)[] values)
        {
            var sql = @$"INSERT INTO [{DbObjTypeName}] ({string.Join($", {Environment.NewLine}", values.Select(v => $"[{v.Column}]"))})
VALUES ({string.Join(", ", values.Select(v => $"@{v.Column}"))})";
            ExecuteNonQuery(sql, values);
        }
    }

    public class PersonApps : BaseApps<Person>
    {
        public void Init()
        {
            ConnectionString = $@"Data Source=c:\temp\mydb.db;";

            CreateTable();
        }

        public IEnumerable<Person> GetDbObjs()
        {
            var cmd = PrepareCommand("SELECT ID, FirstName, LastName, Birthdate FROM Person");
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var bd = DateTime.Parse((string)reader["Birthdate"]);
                yield return new Person()
                {
                    ID = (string)reader["ID"],
                    FirstName = (string)reader["FirstName"],
                    LastName = (string)reader["LastName"],
                    Birthdate = (DateTime)bd,
                };
            }
        }

        private void CreateTable() => CreateTable(
            (nameof(Person.FirstName), "TEXT"),
            (nameof(Person.LastName), "TEXT"),
            (nameof(Person.Birthdate), "DATETIME"));


        public Person CreateDbObj(string firstName, string lastName, DateTime birthdate)
        {
            var person = CreateDbObj();
            person.FirstName = firstName;
            person.LastName = lastName;
            person.Birthdate = birthdate;

            Insert((nameof(Person.ID), person.ID),
                (nameof(Person.FirstName), person.FirstName),
                (nameof(Person.LastName), person.LastName),
                (nameof(Person.Birthdate), person.Birthdate)
                );

            return person;
        }
    }
}
