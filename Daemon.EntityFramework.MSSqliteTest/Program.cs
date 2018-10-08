using Daemon.EntityFramework.Core;
using Daemon.EntityFramework.MSSqliteTest.Entities;
using Daemon.EntityFramework.MSSqliteTest.Sqlite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Daemon.EntityFramework.MSSqliteTest
{
    class Program
    {
        static void Main(string[] args)
        {
            InsertTest();
        }

        static void InsertTest()
        {
            using (var db = new SqliteDbContext())
            {
                var list = new List<TEST_TABLE>();
                for (int i = 0; i < 26 * 26 * 26 * 26; i++)
                {
                    var dto = new TEST_TABLE() { ADDRESS = "1", AGE = 1, CREATE_DATE = DateTime.Now.ToLongDateString(), CREATE_OPER = "1", NAME = "1", SEX = "M", TEL = "1", VALID_STATE = "1" };
                    list.Add(dto);
                }
                ShowTime(() =>
                {
                    db.TEST_TABLE.AddRange(list);
                    db.TEST_TABLE.SaveChanges();
                });
            }
        }
        static void ShowTime(Action action)
        {
            var sw = new Stopwatch();
            sw.Start();
            action();
            sw.Stop();
            Console.WriteLine(sw.ElapsedMilliseconds);
        }
    }
}
