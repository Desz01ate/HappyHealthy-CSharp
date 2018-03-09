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
using Java.IO;
using Android.Graphics;
using Android.Provider;
using Newtonsoft.Json;

namespace HappyHealthyCSharp
{
    [Activity(Label = "Pill", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, WindowSoftInputMode = SoftInput.StateHidden)]
    public class Medicine : Activity,ILocalActivity
    {
        public struct App
        {
            public static File _file;
            public static File _dir;
            public static Bitmap bitmap;
        }
        ImageView medImage;
        EditText medName;
        EditText medDesc;
        MedicineTABLE medObject;
        CheckBox breakfast, lunch, dinner, sleep;
        RadioButton before, after;
        TextView beforeText, afterText;
        string filePath;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            SetTheme(Resource.Style.Base_Theme_AppCompat_Light);
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_add_pill);
            var camerabtt = FindViewById<ImageView>(Resource.Id.imageView_button_camera);
            var backbtt = FindViewById<ImageView>(Resource.Id.imageView_button_back_pill);
            medName = FindViewById<EditText>(Resource.Id.ma_name);
            medDesc = FindViewById<EditText>(Resource.Id.ma_desc);
            medImage = FindViewById<ImageView>(Resource.Id.imageView_show_image);
            var saveButton = FindViewById<ImageView>(Resource.Id.imageView_button_save_pill);
            //var deleteButton = FindViewById<ImageView>(Resource.Id.imageView_button_delete_pill);
            //code goes below
            var flagObjectJson = Intent.GetStringExtra("targetObject") ?? string.Empty;
            medObject = string.IsNullOrEmpty(flagObjectJson) ? new MedicineTABLE() { ma_name = string.Empty } : JsonConvert.DeserializeObject<MedicineTABLE>(flagObjectJson);
            if (medObject.ma_name == string.Empty)
            {
                saveButton.Click += SaveValue;
            }
            else
            {
                InitialForUpdateEvent();
                saveButton.Click += UpdateValue;
                //deleteButton.Click += DeleteValue;
                App._file = new File(medObject.ma_pic);
                LoadImage();
            }
            //end
            backbtt.Click += delegate
            {
                this.Finish();
            };
            if (IsAppToTakePicturesAvailable())
            {
                CreateDirForPictures();
                camerabtt.Click += cameraClickEvent;
                //System.Console.WriteLine(IsAppToTakePicturesAvailable());
            }

            // Create your application here
        }

        public void DeleteValue(object sender, EventArgs e)
        {
            throw new NotImplementedException();
            /*
            Extension.CreateDialogue2(
                 this
                 , "ต้องการลบข้อมูลนี้หรือไม่?"
                 , Android.Graphics.Color.White, Android.Graphics.Color.LightGreen
                 , Android.Graphics.Color.White, Android.Graphics.Color.Red
                 , Extension.adFontSize
                 , delegate
                 {
                     var deleteUri = CalendarHelper.GetDeleteEventURI(medObject.ma_calendar_uri);
                     ContentResolver.Delete(deleteUri, null, null);
                     var time = MedicineTABLE.Morning;
                     CustomNotification.CancelAllAlarmManager(this, medObject.ma_id, medObject.ma_name, time);
                     medObject.Delete<MedicineTABLE>(medObject.ma_id);
                     Finish();
                 }
                 , delegate { }
                 , "\u2713"
                 , "X");
                 */
        }

        public void InitialForUpdateEvent()
        {
            medName.Text = medObject.ma_name;
            medDesc.Text = medObject.ma_desc;
            //Waiting for image initialize
        }

        public void UpdateValue(object sender, EventArgs e)
        {
            medObject.ma_name = medName.Text;
            medObject.ma_desc = medDesc.Text;
            medObject.ma_set_time = medObject.ma_set_time;
            medObject.ma_pic = App._file != null ? App._file.AbsolutePath : medObject.ma_pic;
            medObject.ud_id = Extension.getPreference("ud_id", 0, this);
            medObject.Update();
            Finish();
        }

        public void SaveValue(object sender, EventArgs e)
        {
            if (!Extension.TextFieldValidate(new List<object>() {
                medName
            }))
            {
                Toast.MakeText(this, "กรุณากรอกค่าให้ครบ ก่อนทำการบันทึก", ToastLength.Short).Show();
                return;
            }
            var picPath = string.Empty;
            if (App._file != null)
            {
                picPath = filePath;
                //picPath = App._file.AbsolutePath;
            }
            //pillTable.InsertPillToSQL(medName.Text, medDesc.Text, DateTime.Now,picPath , GlobalFunction.getPreference("ud_id", "", this));
            medObject.ma_name = medName.Text;
            medObject.ma_desc = medDesc.Text;
            medObject.ma_set_time = DateTime.Now;
            medObject.ma_pic = picPath;
            medObject.ud_id = Extension.getPreference("ud_id", 0, this);
            medObject.Insert();
            var idHeader = medObject.ma_id.ToString();
            var beforeAfterDecision = before.Checked ? Convert.ToInt32(afterText.Text) : Convert.ToInt32(beforeText.Text);
            var user = new UserTABLE().Select<UserTABLE>($@"SELECT * FROM UserTABLE WHERE UD_ID = '{medObject.ud_id}'")[0];
            DateTime time;
            if (breakfast.Checked)
            {
                time = user.ud_bf_time.AddMinutes(before.Checked ? -beforeAfterDecision : beforeAfterDecision);
                CustomNotification.SetAlarmManager(this, Convert.ToInt32(idHeader + "1"), medObject.ma_name, time);
            }
            if (lunch.Checked)
            {
                time = user.ud_bf_time.AddMinutes(before.Checked ? -beforeAfterDecision : beforeAfterDecision);
                CustomNotification.SetAlarmManager(this, Convert.ToInt32(idHeader + "2"), medObject.ma_name, time);
            }
            if (dinner.Checked)
            {
                time = user.ud_bf_time.AddMinutes(before.Checked ? -beforeAfterDecision : beforeAfterDecision);
                CustomNotification.SetAlarmManager(this, Convert.ToInt32(idHeader + "3"), medObject.ma_name, time);
            }
            if (sleep.Checked)
            {
                time = user.ud_bf_time.AddMinutes(before.Checked ? -beforeAfterDecision : beforeAfterDecision);
                CustomNotification.SetAlarmManager(this, Convert.ToInt32(idHeader + "4"), medObject.ma_name, time);
            }
            /*
            var time = MedicineTABLE.GetCustom;
            CustomNotification.SetAlarmManager(this,medObject.ma_id, medObject.ma_name,time );
            */
            //if(a) morning if(b) lunch and so on...
            //medObject.Update();
            
            //CustomNotification.SetAlarmManager(this, $"ได้เวลาทานยา {medObject.ma_name}",(int)DateTime.Now.DayOfWeek,medObject.ma_set_time, Resource.Raw.notialert);
            this.Finish();
        }

        private void cameraClickEvent(object sender, EventArgs e)
        {
            var intent = new Intent(MediaStore.ActionImageCapture);
            //App._file = new File(App._dir, string.Format($@"HappyHealthyCS_{Guid.NewGuid()}.jpg"));
            filePath = System.IO.Path.Combine(App._dir.AbsolutePath, $@"HappyHealthyCS_{Guid.NewGuid()}.jpg");
            App._file = new File(filePath);
            intent.PutExtra(MediaStore.ExtraOutput, Android.Net.Uri.FromFile(App._file));
            StartActivityForResult(intent, 0);
        }
        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {

            base.OnActivityResult(requestCode, resultCode, data);
            LoadImage();
        }

        private void LoadImage()
        {
            var mediaScanIntent = new Intent(Intent.ActionMediaScannerScanFile);
            var contentUri = Android.Net.Uri.FromFile(App._file);
            mediaScanIntent.SetData(contentUri);
            SendBroadcast(mediaScanIntent);
            var height = Resources.DisplayMetrics.HeightPixels;
            var width = medImage.Width;
            App.bitmap = App._file.Path.LoadBitmap(width, height);//LoadBitmap(App._file.Path, width, height);
            if (App.bitmap != null)
            {
                medImage.SetImageBitmap(App.bitmap);
                App.bitmap = null;
            }
            GC.Collect();
        }

        private void CreateDirForPictures()
        {
            App._dir = new File(Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryPictures), "HappyHealthyCSharp");
            if (!App._dir.Exists())
                App._dir.Mkdirs();
        }
        private bool IsAppToTakePicturesAvailable()
        {
            var intent = new Intent(MediaStore.ActionImageCapture);
            var availableActivities = PackageManager.QueryIntentActivities(intent, Android.Content.PM.PackageInfoFlags.MatchDefaultOnly);
            return availableActivities != null && availableActivities.Count > 0;
        }
    }
}