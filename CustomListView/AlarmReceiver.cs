using System;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Android.Media;
using Android.Views.Animations;
using Android.Graphics;

namespace Bedtime
{
    /// <summary>
    /// Alarm receiver that is displayed when an alarm is triggered
    /// </summary>
    /// <remarks>
    /// author: David Pyle 041110777
    /// version: 1.0
    /// date: 18/11/2016
    /// </remarks>
    
    [Activity(Label = "AlarmReceiver")]
    public class AlarmReceiver : Activity
    {
        /// <summary>
        /// Media player for playing the alarm sound
        /// </summary>
        private MediaPlayer mediaPlayer;
        /// <summary>
        /// Alarm id 
        /// </summary>
        private int alarmID;
        /// <summary>
        /// Username that the alarm belongs to
        /// </summary>
        private string username;
        /// <summary>
        /// Flag indicating whether the call is for the reminder or actual alarm
        /// </summary>
        private bool reminder;
        /// <summary>
        /// animates the alarm buttons
        /// </summary>
        private Animation fadeIn;
        /// <summary>
        /// Path to the ringtone
        /// </summary>
        private Android.Net.Uri ringTonePath;

        /// <summary>
        /// Displays the reminder or alarms screen and plays the alarm sound. Once the alarm is clicked the activity closes
        /// returns to the alarm app's main screen
        /// </summary>
        /// <param name="savedInstanceState"></param>
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            //get the alarm info passed from the pending intent
            alarmID = Intent.GetIntExtra("AlarmID", -1);
            username = Intent.GetStringExtra("Username");
            string alarmName = Intent.GetStringExtra("AlarmName");
            string alarmTime = Intent.GetStringExtra("AlarmTime");
            string alarmSound = Intent.GetStringExtra("AlarmSound");

            //if an alarm sound was set then get the path
            if (alarmSound != null && alarmSound != "")
            {
                ringTonePath = Android.Net.Uri.Parse(alarmSound);
            } else
            {
                ringTonePath = null;
            }

            //check if this is the reminder alarm
            reminder = Intent.GetBooleanExtra("Reminder", false);

            //make the screen full screen without any title bars
            this.RequestWindowFeature(WindowFeatures.NoTitle);
            this.Window.AddFlags(WindowManagerFlags.Fullscreen);

            //set the font
            Typeface font = Typeface.CreateFromAsset(Assets, "fonts/Montserrat-Bold.ttf");

            //if the call is not from a reminder alarm then display the red alarm button and play the alarm
            if (!reminder)
            {
                SetContentView(Resource.Layout.alarm);

                //set the name and time of the alarm
                TextView name = FindViewById<TextView>(Resource.Id.txtAlarmName);
                TextView time = FindViewById<TextView>(Resource.Id.txtAlarmTime);

                //set the font                
                name.Typeface = font;
                time.Typeface = font;
                //set the values
                name.Text = alarmName;
                time.Text = alarmTime;

                //pulse the button
                fadeIn = AnimationUtils.LoadAnimation(this, Resource.Drawable.button_anim);

                //show the alarm off button
                ImageButton stopAlarm = FindViewById<ImageButton>(Resource.Id.btnAlarmOff);
                stopAlarm.StartAnimation(fadeIn);

                stopAlarm.Click += StopAlarm_Click;
                //play the sound
                playSound(this, getAlarmUri());

            } else //set up the screen for the reminder alarm
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

        /// <summary>
        /// Stops the reminder alarm and closes the activity
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StopReminder_Click(object sender, EventArgs e)
        {
            mediaPlayer.Stop();
            Finish();
        }

        /// <summary>
        /// Stops the main alarm and returns the user to the main alarm screen of the app
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StopAlarm_Click(object sender, EventArgs e)
        {
            //stop the alarm
            mediaPlayer.Stop();

            //go back to the main alarm screen
            Intent main = new Intent(this, typeof(MainActivity));
            //main.SetFlags(ActivityFlags.ReorderToFront);
            main.PutExtra("Username", username);
            main.PutExtra("AlarmJustStopped", alarmID);
            StartActivity(main);
            Finish();
            
        }

        /// <summary>
        /// Plays the alarm ringtone from the path
        /// </summary>
        /// <param name="context"></param>
        /// <param name="alert"></param>
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

        /// <summary>
        /// Sets the path to the ringtone. If the user did not set a sound then the default device ringtone is used.
        /// </summary>
        /// <returns></returns>
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