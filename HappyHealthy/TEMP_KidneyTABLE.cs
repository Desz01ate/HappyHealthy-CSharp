﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System.Xml.Serialization;
using System.Runtime.Serialization;

namespace HappyHealthyCSharp
{
    [Serializable]
    [DataContract]
    class TEMP_KidneyTABLE : DatabaseHelper
    {
        [DataMember]
        public int      ckd_id_pointer { get; set; }
        [DataMember]
        public DateTime ckd_time_new   { get; set; }
        [DataMember]
        public DateTime ckd_time_old   { get; set; }
        [DataMember]
        public string   ckd_time_string_new { get; set; }
        [DataMember]
        public double   ckd_gfr_new    { get; set; }
        [DataMember]
        public double   ckd_gfr_old    { get; set; }
        [DataMember]
        public int      ckd_gfr_level_new { get; set; }
        [DataMember]
        public int      ckd_gfr_level_old { get; set; }
        [DataMember]
        public double   ckd_creatinine_new { get; set; }
        [DataMember]
        public double   ckd_creatinine_old { get; set; }
        [DataMember]
        public double   ckd_bun_new { get; set; }
        [DataMember]
        public double   ckd_bun_old { get; set; }
        [DataMember]
        public double   ckd_sodium_new { get; set; }
        [DataMember]
        public double   ckd_sodium_old { get; set; }
        [DataMember]
        public double   ckd_potassium_new { get; set; }
        [DataMember]
        public double   ckd_potassium_old { get; set; }
        [DataMember]
        public double   ckd_albumin_blood_new { get; set; }
        [DataMember]
        public double   ckd_albumin_blood_old { get; set; }
        [DataMember]
        public double   ckd_albumin_urine_new { get; set; }
        [DataMember]
        public double   ckd_albumin_urine_old { get; set; }
        [DataMember]
        public double   ckd_phosphorus_blood_new { get; set; }
        [DataMember]
        public double   ckd_phosphorus_blood_old { get; set; }
        [DataMember]
        public string   mode { get; set; }
        [DataMember]
        public int ud_id { get; set; }
        public override List<string> Column => throw new NotImplementedException();

        public TEMP_KidneyTABLE() { }
    }
}