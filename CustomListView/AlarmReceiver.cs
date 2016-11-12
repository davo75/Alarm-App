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

namespace CustomListView
{
    [Activity(Label = "AlarmReceiver")]
    public class AlarmReceiver : Activity
    {
        private MediaPlayer mediaPlayer;
        int alarmID;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            alarmID = Intent.GetIntExtra("AlarmID", -1);

            Toast.MakeText(this, "alarm " + alarmID + " rx", ToastLength.Short).Show();

            this.RequestWindowFeature(WindowFeatures.NoTitle);
            this.Window.AddFlags(WindowManagerFlags.Fullscreen);

            SetContentView(Resource.Layout.alarm);

            Button stop = FindViewById<Button>(Resource.Id.stopAlarm);

            stop.Click += Stop_Click;

            playSound(this, getAlarmUri());
        }

        private void Stop_Click(object sender, EventArgs e)
        {
            mediaPlayer.Stop();

            Intent main = new Intent(this, typeof(MainActivity));
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