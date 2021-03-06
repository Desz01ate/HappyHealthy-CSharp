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
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Threading;
using Android.Speech.Tts;
using Android.Media;

namespace HappyHealthyCSharp
{

    [Activity(Label = "Diabetes", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, WindowSoftInputMode = SoftInput.StateHidden)]
    public class Diabetes : Activity, TextToSpeech.IOnInitListener
    {
        TextToSpeech textToSpeech;
        private readonly int CheckCode = 101, NeedLang = 103;
        Java.Util.Locale lang;
        ImageView imageView;
        TTS t2sEngine;
        Intent voiceIntent;
        private bool isRecording, LetsVoiceRunning;
        private readonly int VOICE = 10;
        //Edit below
        EditText BloodValue, SumBloodValue;
        ImageView micButton, saveButton, deleteButton;
        private TextView header;
        DiabetesTABLE diaObject = null;
        Dictionary<string, string> dataNLPList;

        private EditText currentControl;
        private static AutoResetEvent autoEvent = new AutoResetEvent(false);
        private ImageView addhiding;
        private ImageView backBtt;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            SetTheme(Resource.Style.Base_Theme_AppCompat_Light);
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_diabetes);
            var flagObjectJson = Intent.GetStringExtra("targetObject") ?? string.Empty;
            diaObject = string.IsNullOrEmpty(flagObjectJson) ? new DiabetesTABLE() { fbs_fbs = Extension.flagValue } : JsonConvert.DeserializeObject<DiabetesTABLE>(flagObjectJson);
            InitializeControl();
            InitializeControlEvent();
            InitializeData();
            string rec = Android.Content.PM.PackageManager.FeatureMicrophone;
            if (rec != "android.hardware.microphone")
            {
                // no microphone, no recording. Disable the button and output an alert
                Extension.CreateDialogue(this, "ไม่พบไมโครโฟนบนระบบของคุณ").Show();
            }
            else
            {
                micButton.Click += delegate
                {
                    isRecording = !isRecording;
                    if (isRecording)
                    {
                        AutomationTalker();
                    }
                };
                if (this.GetPreference("autosound", false))
                {
                    AutomationTalker();
                }
            }
            t2sEngine = new TTS(this);
        }
        private void InitializeData()
        {
            header.Text = "บันทึกค่าเบาหวาน";
            addhiding.Visibility = ViewStates.Gone;
        }

        private void InitializeControlEvent()
        {
            backBtt.Click += delegate
            {
                Finish();
                LetsVoiceRunning = false;
            };  
            if (diaObject.fbs_fbs == Extension.flagValue)
            {
                saveButton.Click += SaveValue;
            }
            else
            {
                InitialValueForUpdateEvent();
                saveButton.Click += UpdateValue;
            }
        }

        private void InitializeControl()
        {
            BloodValue = FindViewById<EditText>(Resource.Id.sugar_value);
            SumBloodValue = FindViewById<EditText>(Resource.Id.sugar_sum_value);
            micButton = FindViewById<ImageView>(Resource.Id.ic_microphone_diabetes);
            saveButton = FindViewById<ImageView>(Resource.Id.imageView_button_save_diabetes);
            header = FindViewById<TextView>(Resource.Id.textView_header_name_diabetes);
            addhiding = FindViewById<ImageView>(Resource.Id.ClickAddDia);
            backBtt = FindViewById<ImageView>(Resource.Id.imageView38);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            LetsVoiceRunning = false;
        }
        public override void OnBackPressed()
        {
            base.OnBackPressed();
            LetsVoiceRunning = false;
        }
        private async Task<bool> StartMicrophoneAsync(string speakValue, int soundRawResource)
        {
            try
            {
                //await t2sEngine.SpeakAsync($@"กรุณาบอกระดับค่า{speakValue}");
                MediaPlayer mPlayer = MediaPlayer.Create(this, soundRawResource);
                mPlayer.Start();
                mPlayer.Completion += delegate
                {
                    voiceIntent = new Intent(RecognizerIntent.ActionRecognizeSpeech);
                    voiceIntent.PutExtra(RecognizerIntent.ExtraLanguageModel, RecognizerIntent.LanguageModelFreeForm);
                    voiceIntent.PutExtra(RecognizerIntent.ExtraPrompt, $@"กรุณาบอกระดับค่า{speakValue}");
                    voiceIntent.PutExtra(RecognizerIntent.ExtraLanguage, "th-TH");
                    voiceIntent.PutExtra(RecognizerIntent.ExtraSpeechInputCompleteSilenceLengthMillis, 1500);
                    voiceIntent.PutExtra(RecognizerIntent.ExtraSpeechInputPossiblyCompleteSilenceLengthMillis, 1500);
                    voiceIntent.PutExtra(RecognizerIntent.ExtraSpeechInputMinimumLengthMillis, 50000);//15000);
                    voiceIntent.PutExtra(RecognizerIntent.ExtraMaxResults, 1);
                    //voiceIntent.PutExtra(RecognizerIntent.ExtraLanguage, Java.Util.Locale.Default);
                    //Thread.Sleep(1000);
                    StartActivityForResult(voiceIntent, VOICE);
                };
                await Task.Run(() => autoEvent.WaitOne(new TimeSpan(0, 0, 15)));
            }
            catch
            {
                Extension.CreateDialogue(this, "อุปกรณ์ของคุณไม่รองรับการสั่งการด้วยเสียง").Show();
                return false;
            }
            return true;
        }

        private void DeleteValue(object sender, EventArgs e)
        {
            Extension.CreateDialogue2(
                 this
                 , "Do you want to delete this value?"
                 , Android.Graphics.Color.White, Android.Graphics.Color.LightGreen
                 , Android.Graphics.Color.White, Android.Graphics.Color.Red
                 , Extension.adFontSize
                 , delegate
                 {
                     diaObject.Delete();
                     //diaObject.TrySyncWithMySQL(this);
                     Finish();
                 }
                 , delegate { }
                 , "\u2713"
                 , "X");
        }

        private void InitialValueForUpdateEvent()
        {
            BloodValue.Text = diaObject.fbs_fbs.ToString();
            SumBloodValue.Text = diaObject.fbs_fbs_sum.ToString();
        }

        private void UpdateValue(object sender, EventArgs e)
        {
            diaObject.fbs_fbs = (decimal)double.Parse(BloodValue.Text);
            diaObject.fbs_fbs_sum = (decimal)double.Parse(SumBloodValue.Text);
            diaObject.ud_id = this.GetPreference("ud_id", 0);
            diaObject.Update();
            //diaObject.TrySyncWithMySQL(this);
            if (diaObject.IsInDangerousState())
                Extension.CreateDialogue(this, "ค่าที่คุณทำการบันทึก อยู่ในเกณฑ์เสี่ยง หรือ อันตราย กรุณาพบแพทย์เพื่อรับคำแนะนำเพิ่มเติม", delegate
                {
                    Finish();
                }).Show();
            else
                this.Finish();
        }
        private async Task AutomationTalker()
        {
            LetsVoiceRunning = true;
            currentControl = BloodValue;
            if (AllowToRun(currentControl))
                await StartMicrophoneAsync("น้ำตาล", Resource.Raw.bloodSugar);
            currentControl = SumBloodValue;
            if (AllowToRun(currentControl))
                await StartMicrophoneAsync("น้ำตาลสะสม", Resource.Raw.sumBloodSugar);
            LetsVoiceRunning = false;
        }

        private bool AllowToRun(EditText currentControl)
        {
            return currentControl.Text == string.Empty && LetsVoiceRunning;
        }

        protected override void OnActivityResult(int requestCode, Result resultVal, Intent data)
        {
            base.OnActivityResult(requestCode, resultVal, data);
            if (requestCode == VOICE)
            {
                if (resultVal == Result.Ok)
                {
                    var matches = data.GetStringArrayListExtra(RecognizerIntent.ExtraResults);
                    if (matches.Count != 0)
                    {
                        string textInput = matches[0];
                        if (int.TryParse(textInput, out int numericData))
                        {
                            currentControl.Text = numericData.ToString();
                        }
                        else
                        {
                            currentControl.Text = "0";
                        }

                    }
                    else
                        Toast.MakeText(this, "Unrecognized value", ToastLength.Short);
                }
            }
            autoEvent.Set();
        }



        public void SaveValue(object sender, EventArgs e)
        {
            if (!Extension.TextFieldValidate(new List<object>() {
                BloodValue,SumBloodValue
            }))
            {
                Toast.MakeText(this, "กรุณากรอกค่าให้ครบ ก่อนทำการบันทึก", ToastLength.Short).Show();
                return;
            }
            var diabetesObject = new DiabetesTABLE();
            try
            {

                diabetesObject.fbs_id = SQLiteInstance.GetConnection.ExecuteScalar<int>($"SELECT MAX(fbs_id)+1 FROM DiabetesTABLE");
                //diaTable.fbs_id = new SQLite.SQLiteConnection(Extension.sqliteDBPath).ExecuteScalar<int>($"SELECT MAX(fbs_id)+1 FROM DiabetesTABLE");
            }
            catch
            {
                diabetesObject.fbs_id = 1;
            }
            diabetesObject.fbs_fbs = (decimal)double.Parse(BloodValue.Text);
            diabetesObject.fbs_fbs_sum = (decimal)double.Parse(SumBloodValue.Text);
            diabetesObject.ud_id = this.GetPreference("ud_id", 0);
            diabetesObject.fbs_time = DateTime.Now;//.ToThaiLocale();
            //diabetesObject.fbs_time_string = diabetesObject.fbs_time.ToString("dd-MMMM-yyyy hh:mm:ss tt");
            diabetesObject.Insert();
            //diaTable.TrySyncWithMySQL(this);
            if (diabetesObject.IsInDangerousState())
                Extension.CreateDialogue(this, "ค่าที่คุณทำการบันทึก อยู่ในเกณฑ์เสี่ยง หรือ อันตราย กรุณาพบแพทย์เพื่อรับคำแนะนำเพิ่มเติม", delegate
                {
                    Finish();
                }).Show();
            else
                Finish();
        }

        #region Experiment TTS methods
        public void OnInit([GeneratedEnum] OperationResult status)
        {
            if (status == OperationResult.Error)
                textToSpeech.SetLanguage(Java.Util.Locale.Default);
            // if the listener is ok, set the lang
            if (status == OperationResult.Success)
                textToSpeech.SetLanguage(lang);
        }
        #endregion
    }
}