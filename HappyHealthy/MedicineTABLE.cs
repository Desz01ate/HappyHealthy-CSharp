﻿using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using SQLite;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace HappyHealthyCSharp
{
    class MedicineTABLE : DatabaseHelper
    {
        public override List<string> Column => new List<string> {
            "ma_id", 
            "ma_name",
            "ma_desc", 
            "ma_set_time",
            "ma_calendar_uri",
            "ma_pic"
        };
        [SQLite.PrimaryKey, SQLite.AutoIncrement]
        public int ma_id { get; set; }
        [SQLite.MaxLength(100)]
        public string ma_name { get; set; }
        [SQLite.MaxLength(255)]
        public string ma_desc { get; set; }
        public DateTime ma_set_time { get; set; }
        [SQLite.NotNull]
        public bool ma_repeat_monday { get; set; }
        [SQLite.NotNull]
        public bool ma_repeat_tuesday { get; set; }
        [SQLite.NotNull]
        public bool ma_repeat_wednesday { get; set; }
        [SQLite.NotNull]
        public bool ma_repeat_thursday { get; set; }
        [SQLite.NotNull]
        public bool ma_repeat_friday { get; set; }
        [SQLite.NotNull]
        public bool ma_repeat_saturday { get; set; }
        [SQLite.NotNull]
        public bool ma_repeat_sunday { get; set; }
        public string ma_calendar_uri { get; set; }
        [SQLite.MaxLength(255)]
        public string ma_pic { get; set; }
        public int ud_id { get; set; }
        [Ignore]
        public UserTABLE UserTABLE { get; set; }
        //reconstruct of sqlite keys + attributes
        public MedicineTABLE()
        {

            //constructor - no need for args since naming convention for instances variable mapping can be use : CB
        }
    }

}