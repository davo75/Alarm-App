using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Media;
using Android.Views.Animations;
using Android.Graphics;

namespace CustomListView
{
    [Activity(Label = "AlarmReceiver")]
    public class AlarmReceiver : Activity
    {
        private MediaPlayer mediaPlayer;
        int alarmID;
        string username;
        bool reminder;
        Animation fadeIn;
        Android.Net.Uri ringTonePath;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            alarmID = Intent.GetIntExtra("AlarmID", -1);
            username = Intent.GetStringExtra("Username");
            string alarmName = Intent.GetStringExtra("AlarmName");
            string alarmTime = Intent.GetStringExtra("AlarmTime");
            string alarmSound = Intent.GetStringExtra("AlarmSound");

            if (alarmSound != null && alarmSound != "")
            {
                ringTonePath = Android.Net.Uri.Parse(alarmSound);
            } else
            {
                ringTonePath = null;
            }

            //check if this is the reminder alarm
            reminder = Intent.GetBooleanExtra("Reminder", false);

            Toast.MakeText(this, "alarm " + alarmID + " rx", ToastLength.Short).Show();

            this.RequestWindowFeature(WindowFeatures.NoTitle);
            this.Window.AddFlags(WindowManagerFlags.Fullscreen);
            Typeface font = Typeface.CreateFromAsset(Assets, "fonts/Montserrat-Bold.ttf");

            if (!reminder)
            {
                SetContentView(Resource.Layout.alarm);

                TextView name = FindViewById<TextView>(Resource.Id.txtAlarmName);
                TextView time = FindViewById<TextView>(Resource.Id.txtAlarmTime);

                
                name.Typeface = font;
                time.Typeface = font;

                name.Text = alarmName;
                time.Text = alarmTime;

                fadeIn = AnimationUtils.LoadAnimation(this, Resource.Drawable.button_anim);

                ImageButton stopAlarm = FindViewById<ImageButton>(Resource.Id.btnAlarmOff);
                stopAlarm.StartAnimation(fadeIn);

                stopAlarm.Click += StopAlarm_Click;

                playSound(this, getAlarmUri());

            } else
            {
                SetContentView(Resource.Layout.reminder);
                TextView name = FindViewById<TextView>(Resource.Id.txtAlarmName);
                name.Typeface = font;
                name.Text = alarmName;

                fadeIn = AnimationUtils.LoadAnimation(this, Resource.Drawable.button_anim);

                ImageButton stopReminder = FindViewById<ImageButton>(Resource.Id.btnAlarmOff);
                stopReminder.StartAnimation(fadeIn);

                stopReminder.Click += StopReminder_Click;

                playSound(this, getAlarmUri());
            }
        }

        private void StopReminder_Click(object sender, EventArgs e)
        {
            mediaPlayer.Stop();
            Finish();
        }

        private void StopAlarm_Click(object sender, EventArgs e)
        {
            mediaPlayer.Stop();

            Intent main = new Intent(this, typeof(MainActivity));
            //main.SetFlags(ActivityFlags.ReorderToFront);
            main.PutExtra("Username", username);
            main.PutExtra("AlarmJustStopped", alarmID);
            StartActivity(main);
            Finish();
            
        }

        private void playSound(Context context, Android.Net.Uri alert)
        {
            mediaPlayer = new MediaPlayer();

            try
            {
                mediaPlayer.SetDataSource(context, alert);
                AudioManager am = (AudioManager)context.GetSystemService(Context.AudioService);

                if (am.GetStreamVolume(Stream.Alarm) != 0)
                {
                    mediaPlayer.SetAudioStreamType(Stream.Alarm);
                    mediaPlayer.Prepare();
                    mediaPlayer.Start();
                }

            } catch (Exception e)
            {
                Console.WriteLine("Audio Error" + e.Message);
            }
        }

        private Android.Net.Uri getAlarmUri()
        {
            Android.Net.Uri alert = ringTonePath;

            if (alert == null)
            {
                alert = RingtoneManager.GetDefaultUri(RingtoneType.Alarm);
                if (alert == null)
                {
                    alert = RingtoneManager.GetDefaultUri(RingtoneType.Notification);
                    if (alert == null)
                    {
                        alert = RingtoneManager.GetDefaultUri(RingtoneType.Ringtone);
                    }
                }
            }
            return alert;
        }
    }
}