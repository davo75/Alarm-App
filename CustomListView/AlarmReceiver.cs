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
        Animation fadeIn;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            alarmID = Intent.GetIntExtra("AlarmID", -1);
            username = Intent.GetStringExtra("Username");
            string alarmName = Intent.GetStringExtra("AlarmName");
            string alarmTime = Intent.GetStringExtra("AlarmTime");

            Toast.MakeText(this, "alarm " + alarmID + " rx", ToastLength.Short).Show();

            this.RequestWindowFeature(WindowFeatures.NoTitle);
            this.Window.AddFlags(WindowManagerFlags.Fullscreen);

            SetContentView(Resource.Layout.alarm);

            TextView name = FindViewById<TextView>(Resource.Id.txtAlarmName);
            TextView time = FindViewById<TextView>(Resource.Id.txtAlarmTime);

            Typeface font = Typeface.CreateFromAsset(Assets, "fonts/Montserrat-Bold.ttf");
            name.Typeface = font;
            time.Typeface = font;

            name.Text = alarmName;
            time.Text = alarmTime;

            fadeIn = AnimationUtils.LoadAnimation(this, Resource.Drawable.button_anim);

            ImageButton stop = FindViewById<ImageButton>(Resource.Id.btnAlarmOff);
            stop.StartAnimation(fadeIn);

            stop.Click += Stop_Click1;

            playSound(this, getAlarmUri());
        }

        private void Stop_Click1(object sender, EventArgs e)
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
            Android.Net.Uri alert = RingtoneManager.GetDefaultUri(RingtoneType.Alarm);
            if (alert == null)
            {
                alert = RingtoneManager.GetDefaultUri(RingtoneType.Notification);
                if (alert == null)
                {
                    alert = RingtoneManager.GetDefaultUri(RingtoneType.Ringtone);
                }
            }
            return alert;
        }
    }
}