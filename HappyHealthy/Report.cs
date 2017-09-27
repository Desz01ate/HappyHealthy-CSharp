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
using Java.Util;
using Java.Text;
using OxyPlot.Xamarin.Android;
using MySql.Data.MySqlClient;
using System.Data;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.Annotations;

namespace HappyHealthyCSharp
{

    [Activity(Theme = "@style/MyMaterialTheme.Base")]

    class Report : Activity
    {
        public MyClass myClass;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            RequestWindowFeature(WindowFeatures.NoTitle);
            SetContentView(Resource.Layout.activity_report);
            PlotView view = FindViewById<PlotView>(Resource.Id.plot_view);
            var fbs = FindViewById<RadioButton>(Resource.Id.report_fbs);
            var ckd = FindViewById<RadioButton>(Resource.Id.report_ckd);
            var bp = FindViewById<RadioButton>(Resource.Id.report_bp);
            
            fbs.Click += delegate {
                view.Model = CreatePlotModel("กราฟ FBS", new DiabetesTABLE().getDiabetesList($"SELECT * FROM FBS WHERE UD_ID = {GlobalFunction.getPreference("ud_id", "", this)} ORDER BY FBS_TIME"), "fbs_time", "fbs_fbs");
            };
            ckd.Click += delegate
            {
                view.Model = CreatePlotModel("กราฟ CKD", new KidneyTABLE().getKidneyList($@"SELECT * FROM CKD WHERE UD_ID = {GlobalFunction.getPreference("ud_id", "", this)} ORDER BY CKD_TIME"), "ckd_time", "ckd_gfr");
            };
            bp.Click += delegate {
                view.Model = CreatePlotModel("กราฟ BP", new PressureTABLE().getPressureList($@"SELECT * FROM BP WHERE UD_ID = {GlobalFunction.getPreference("ud_id", "", this)} ORDER BY BP_TIME"), "bp_time", "bp_up");
            };
            fbs.Checked = true;
            view.Model = CreatePlotModel("กราฟ FBS", new DiabetesTABLE().getDiabetesList($"SELECT * FROM FBS WHERE UD_ID = {GlobalFunction.getPreference("ud_id", "", this)} ORDER BY FBS_TIME"), "fbs_time", "fbs_fbs");
        }
        private PlotModel CreatePlotModel(string title,JavaList<IDictionary<string,object>> dataset,string key_time,string key_value)
        {
            var datalength = dataset.Count();
            var plotModel = new PlotModel { Title = title};
            var LastDateOnDataset = new object();
            if (datalength > 0)
            {
                dataset.Last().TryGetValue(key_time, out LastDateOnDataset);
            }
            else
            {
                LastDateOnDataset = DateTime.Now;
            }
            var startDate = DateTime.Parse(LastDateOnDataset.ToString()).AddDays(-7);
            var endDate = DateTime.Parse(LastDateOnDataset.ToString()).AddDays(0.1);
            var minValue = DateTimeAxis.ToDouble(startDate);
            var maxValue = DateTimeAxis.ToDouble(endDate);
            Console.WriteLine($@"{minValue}/{maxValue}");
            var x = new DateTimeAxis { Position = AxisPosition.Bottom, Minimum = minValue, Maximum = maxValue, MajorStep = 10, StringFormat = "d-MMMM" };
            var y = new LinearAxis { Position = AxisPosition.Left, Maximum = 100, Minimum = 0 };
            y.IsPanEnabled = false;
            y.IsZoomEnabled = false;
            plotModel.Axes.Add(x);
            plotModel.Axes.Add(y);
            var series1 = new LineSeries
            {
                MarkerType = MarkerType.Circle,
                MarkerSize = 4,
                MarkerStroke = OxyColors.White,
                Color = OxyColors.Red,
            };
            for (var i = 0; i < datalength; i++)
            {
                dataset[i].TryGetValue(key_time, out object Time);
                dataset[i].TryGetValue(key_value, out object Value);
                DateTime.TryParse(Time.ToString(), out DateTime dateResult);
                double value = Convert.ToDouble(Value.ToString());
                //series1.Points.Add(new DataPoint(DateTimeAxis.ToDouble(dateResult.Year > 2100 ? dateResult.AddYears(-543) : dateResult), value));
                series1.Points.Add(new DataPoint(DateTimeAxis.ToDouble(dateResult), value));
                var textAnnotations = new TextAnnotation() { TextPosition = new DataPoint(series1.Points.Last().X, series1.Points.Last().Y), Text = value.ToString(), Stroke = OxyColors.White };
                plotModel.Annotations.Add(textAnnotations);
            }
            plotModel.Series.Add(series1);
            return plotModel;
        }
    }
}