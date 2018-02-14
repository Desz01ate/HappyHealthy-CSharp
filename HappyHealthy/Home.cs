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
using Java.Interop;
using Android.Speech;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Net.Http;
using System.Threading;

namespace HappyHealthyCSharp
{
    [Activity(Theme = "@style/MyMaterialTheme.Base")]
    class Home : Activity
    {
        TextView labelTest;
        TextView homeHeaderText;
        #region Experimental Section
        private bool isRecording;
        private readonly int VOICE = 10;
        private Button recButton;
        private EditText testResult;
        #endregion
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_home);
            ImageView DiabetesButton = FindViewById<ImageView>(Resource.Id.imageView_button_diabetes);
            ImageView KidneyButton = FindViewById<ImageView>(Resource.Id.imageView_button_ckd);
            ImageView PressureButton = FindViewById<ImageView>(Resource.Id.imageView_button_pressure);
            ImageView FoodButton = FindViewById<ImageView>(Resource.Id.imageView_button_food);
            ImageView MedicineButton = FindViewById<ImageView>(Resource.Id.imageView_button_pill);
            ImageView DoctorButton = FindViewById<ImageView>(Resource.Id.imageView_button_doctor);
            homeHeaderText = FindViewById<TextView>(Resource.Id.textView18);

            DiabetesButton.Click += ClickDiabetes;
            KidneyButton.Click += ClickKidney;
            PressureButton.Click += ClickPressure;
            FoodButton.Click += ClickFood;
            MedicineButton.Click += ClickPill;
            DoctorButton.Click += ClickDoctor;
            var imageView = FindViewById<ImageView>(Resource.Id.imageView4);
            imageView.Click += NotImplemented;

            //TestSTTImplementation(imageView);
        }

        private void TestSTTImplementation(ImageView imageView)
        {
            string rec = Android.Content.PM.PackageManager.FeatureMicrophone;
            if (rec != "android.hardware.microphone")
            {
                // no microphone, no recording. Disable the button and output an alert
                Extension.CreateDialogue(this, "ไม่พบไมโครโฟนบนระบบของคุณ").Show();
            }
            else
                imageView.Click += delegate
                {
                    isRecording = !isRecording;
                    if (isRecording)
                    {
                        var voiceIntent = new Intent(RecognizerIntent.ActionRecognizeSpeech);
                        voiceIntent.PutExtra(RecognizerIntent.ExtraLanguageModel, RecognizerIntent.LanguageModelFreeForm);
                        voiceIntent.PutExtra(RecognizerIntent.ExtraPrompt, "Speak Now!");
                        voiceIntent.PutExtra(RecognizerIntent.ExtraSpeechInputCompleteSilenceLengthMillis, 1500);
                        voiceIntent.PutExtra(RecognizerIntent.ExtraSpeechInputPossiblyCompleteSilenceLengthMillis, 1500);
                        voiceIntent.PutExtra(RecognizerIntent.ExtraSpeechInputMinimumLengthMillis, 15000);
                        voiceIntent.PutExtra(RecognizerIntent.ExtraMaxResults, 1);
                        voiceIntent.PutExtra(RecognizerIntent.ExtraLanguage, Java.Util.Locale.Default);
                        StartActivityForResult(voiceIntent, VOICE);
                    }
                };
        }

        protected override void OnActivityResult(int requestCode, Result resultVal, Intent data)
        {
            if (requestCode == VOICE)
            {
                if (resultVal == Result.Ok)
                {
                    var matches = data.GetStringArrayListExtra(RecognizerIntent.ExtraResults);
                    if (matches.Count != 0)
                    {
                        string textInput = matches[0];

                        // limit the output to 500 characters
                        if (textInput.Length > 500)
                            textInput = textInput.Substring(0, 500);
                        //GlobalFunction.createDialog(this, textInput).Show();
                        Toast.MakeText(this, textInput, ToastLength.Short);
                        labelTest = FindViewById<TextView>(Resource.Id.textView18);
                        labelTest.Text = textInput;
                    }
                    else
                        Toast.MakeText(this, "Unrecognized", ToastLength.Short);
                }
            }
            base.OnActivityResult(requestCode, resultVal, data);
        }

        public async void ClickFood(object sender, EventArgs e)
        {
            ProgressDialog progressDialog = null;
            try
            {
                progressDialog = ProgressDialog.Show(this, "ดาวน์โหลดข้อมูล", "กำลังดาวน์โหลดข้อมูล กรุณารอสักครู่", true);
                var service = new HHCSService.HHCSService();
                service.Timeout = 10;
                var result = await TestConnectionValidate(service);
                if (result.Result == true)
                {
                    StartActivity(new Intent(this, typeof(History_Food)));
                    progressDialog.Dismiss();
                }
                else
                {
                    Extension.CreateDialogue(this, "Failed to connect to database").Show();
                    progressDialog.Dismiss();
                }
            }
            catch
            {
                Extension.CreateDialogue(this, "Connection timeout").Show();
            }
            //NotImplemented(sender, e);
        }
        private static void TransferCompletion<T>(TaskCompletionSource<T> tcs, System.ComponentModel.AsyncCompletedEventArgs e, Func<T> getResult)
        {
            if (e.Error != null)
            {
                tcs.TrySetException(e.Error);
            }
            else if (e.Cancelled)
            {
                tcs.TrySetCanceled();
            }
            else
            {
                tcs.TrySetResult(getResult());
            }
        }

        public static Task<HHCSService.TestConnectionCompletedEventArgs> TestConnectionValidate(HHCSService.HHCSService serviceInstance)
        {
            var result = new TaskCompletionSource<HHCSService.TestConnectionCompletedEventArgs>();
            serviceInstance.TestConnectionCompleted += (s, e) => TransferCompletion(result, e, () => e);
            serviceInstance.TestConnectionAsync();
            return result.Task;
        }
        public void ClickDiabetes(object sender, EventArgs e)
        {
            StartActivity(new Intent(this, typeof(History_Diabetes)));
        }
        public void ClickKidney(object sender, EventArgs e)
        {
            StartActivity(new Intent(this, typeof(History_Kidney)));
        }
        public void ClickPressure(object sender, EventArgs e)
        {
            StartActivity(new Intent(this, typeof(History_Pressure)));
        }
        public void ClickDevelop(object sender, EventArgs e)
        {
            //StartActivity(new Intent(this, typeof(Develop)));
            //GlobalFunction.createDialog(this, "Not implemented").Show();

        }
        public void ClickPill(object sender, EventArgs e)
        {
            //GlobalFunction.createDialog(this, "Not implemented").Show();
            StartActivity(new Intent(this, typeof(History_Pill)));
        }
        public void ClickDoctor(object sender, EventArgs e)
        {
            StartActivity(typeof(History_Doctor));
        }
        public void NotImplemented(object sender, EventArgs e)
        {
            //Extension.CreateDialogue(this, "Not Implemented").Show();
            /*
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://192.168.1.7/RESTservice/");
                var response = client.GetAsync($@"api/Encryption/{Extension.getPreference("ud_id", 0, this)}").Result;
                if (response.IsSuccessStatusCode)
                {
                    Extension.CreateDialogue(this, response.Content.ReadAsStringAsync().Result).Show();
                }
            }
            */
            return;
            var notifTime = DateTime.Now;
            notifTime.AddHours(-6);
            notifTime.AddSeconds(20);
            CustomNotification.SetAlarmManager(this, "TEST", 1, notifTime);
            Extension.CreateDialogue(this, notifTime.ToString()).Show();
        }
    }
}