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
            CountTest();
        }

        static void InsertTest()
        {
            //using (var db = new SqliteDbContext())
            //{
            //    var list = new List<TEST_TABLE>();
            //    for (int i = 0; i < 26 * 26 * 26 * 26; i++)
            //    {
            //        var dto = new TEST_TABLE() { ADDRESS = "1", AGE = 1, CREATE_DATE = DateTime.Now.ToLongDateString(), CREATE_OPER = "1", NAME = "1", SEX = "M", TEL = "1", VALID_STATE = "1" };
            //        list.Add(dto);
            //    }
            //    ShowTime(() =>
            //    {
            //        db.TestTable.AddRange(list);
            //        db.TestTable.SaveChanges();
            //    });
            //}
        }
        static void CountTest()
        {
            using (var db = new SqliteDbContext())
            {
                var l1 = db.Student.Join(db.Class,
                    p => p.CLASS_ID,
                    p => p.CLASS_ID,
                    (pl, pr) => new { pl.CLASS_ID, pl.STUDENT_ID, pl.STUDENT_NAME, pr.CLASS_NAME });
                var l2 = db.Score.Join(db.Subject,
                    p => p.SUBJECT_ID,
                    p => p.SUBJECT_ID,
                    (pl, pr) => new { pl.SCORE_ID, pl.STUDENT_ID, pl.SUBJECT_ID, pl.POINTS, pr.SUBJECT_NAME });
                var l3 = l1.Join(l2,
                     p => p.STUDENT_ID,
                     p => p.STUDENT_ID,
                     (pl, pr) => new { pl.STUDENT_NAME, pl.CLASS_NAME, pr.SUBJECT_NAME, pr.POINTS });
                foreach (var item in l3)
                {
                    Console.WriteLine($"{item.STUDENT_NAME} {item.CLASS_NAME} {item.SUBJECT_NAME} {item.POINTS}");
                }
                //db.JoinTable.Where(p => p.TEST_ID < 10000).ToList();
            }
        }

        static void ViewTest()
        {
            using (var db = new SqliteDbContext())
            {
                var list = db.VStats.ToList();
                foreach (var item in list)
                {
                    Console.WriteLine($"{item.STUDENT_NAME}\t{item.CLASS_NAME}\t{item.SUBJECT_NAME}\t{item.POINTS}");
                }
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
