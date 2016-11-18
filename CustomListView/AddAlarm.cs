using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;
using System.Net.NetworkInformation;
using Bedtime.au.edu.wa.central.mydesign.student;
using Android.Graphics;
using Android.Views.InputMethods;
using Android.Media;
using Android.Content.PM;

namespace Bedtime
{
    /// <summary>
    /// Adds a new alarm
    /// </summary>
    /// <remarks>
    /// author: David Pyle 041110777
    /// version: 1.0
    /// date: 18/11/2016
    /// </remarks
    
    [Activity(Label = "AddAlarm", ScreenOrientation = ScreenOrientation.Portrait, WindowSoftInputMode = SoftInput.AdjustPan | SoftInput.StateHidden)]
    public class AddAlarm : Activity
    {
        /// <summary>
        /// Alarm name
        /// </summary>
        private EditText alarmName;
        /// <summary>
        /// Alarm time
        /// </summary>
        private EditText alarmTime;
        /// <summary>
        /// Alarm time
        /// </summary>
        private TimeSpan timeOfAlarm;
        /// <summary>
        /// Alarm sound
        /// </summary>
        private EditText alarmSound;
        /// <summary>
        /// Drop down list for reminder times
        /// </summary>
        private Spinner alarmReminderSpinner;
        /// <summary>
        /// Path to ringtone
        /// </summary>
        private Android.Net.Uri uriToRingTone;
        /// <summary>
        /// Logged in username
        /// </summary>
        private string username;

        /// <summary>
        /// Displays the alarm field for the user to fill in to create a new alarm
        /// </summary>
        /// <param name="savedInstanceState"></param>
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.AddAlarm2);

            //add the toolbar
            var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetActionBar(toolbar);
            ActionBar.Title = "Add Alarm";

            //set the username
            username = Intent.GetStringExtra("Username");

            //get references to the text fields
            alarmName = FindViewById<EditText>(Resource.Id.txtAlarmName);
            alarmTime = FindViewById<EditText>(Resource.Id.txtAlarmTime);
            //alarmReminder = FindViewById<EditText>(Resource.Id.txtAlarmReminder);
            alarmSound = FindViewById<EditText>(Resource.Id.txtAlarmSound);

            //set a custom font for edit text fields
            Typeface font = Typeface.CreateFromAsset(Assets, "fonts/Myriad-Pro-Bold.ttf");            
            alarmName.Typeface = font;           
            alarmTime.Typeface = font;                    
            alarmSound.Typeface = font;

            //setup the reminder time drop down list
            alarmReminderSpinner = FindViewById<Spinner>(Resource.Id.reminderList);
            alarmReminderSpinner.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(alarmReminderSpinner_ItemSelected);

            var adapter = ArrayAdapter.CreateFromResource(
                this, Resource.Array.reminder_array, Resource.Drawable.spinner_style);

            adapter.SetDropDownViewResource(Resource.Drawable.spinner_item_style);
            alarmReminderSpinner.Adapter = adapter;

            alarmTime.Click += AlarmTime_Click;

            alarmSound.Click += AlarmSound_Click;

            //save button
            ImageButton saveBtn = FindViewById<ImageButton>(Resource.Id.btnSaveAlarm);
            saveBtn.Click += SaveBtn_Click;

            //cancel button
            ImageButton cancelBtn = FindViewById<ImageButton>(Resource.Id.btnCancelAddAlarm);
            cancelBtn.Click += (object sender, EventArgs e) =>
            {
                Finish();
            };
        }

        /// <summary>
        /// Displays the ringtone list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AlarmSound_Click(object sender, EventArgs e)
        {
            Intent intent = new Intent(RingtoneManager.ActionRingtonePicker);
            this.StartActivityForResult(intent, 3);
        }

        /// <summary>
        /// Sets the ringtone path from the user selection
        /// </summary>
        /// <param name="requestCode"></param>
        /// <param name="resultCode"></param>
        /// <param name="data"></param>
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if (requestCode == 3 && resultCode == Result.Ok)
            {
                uriToRingTone = (Android.Net.Uri)data.GetParcelableExtra(RingtoneManager.ExtraRingtonePickedUri);
                string alarmTitle = RingtoneManager.GetRingtone(this, uriToRingTone).GetTitle(this);
                if (uriToRingTone != null)
                {                    
                    alarmSound.Text = alarmTitle;
                    //Toast.MakeText(this, uriToRingTone.ToString(), ToastLength.Short).Show();
                }
            }
        }
        
        /// <summary>
        /// Displays the reminder time selected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void alarmReminderSpinner_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            Spinner spinner = (Spinner)sender;

            string toast = spinner.SelectedItem.ToString();
            //Toast.MakeText(this, toast, ToastLength.Long).Show();
        }

        /// <summary>
        /// Displays the time picker dialog for choosing the alarm time
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AlarmTime_Click(object sender, EventArgs e)
        {
            hideSoftKeyboard();
            TimePickerDialog tpd = new TimePickerDialog(this, tdpCallback, DateTime.Now.Hour, DateTime.Now.Minute, true);            
            tpd.Show();
        }

        /// <summary>
        /// Sets the alarm time chosen in the time picker dialog
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tdpCallback(object sender, TimePickerDialog.TimeSetEventArgs e)
        {            
            timeOfAlarm = new TimeSpan(e.HourOfDay, e.Minute, 0);
            alarmTime.Text = string.Format("{0}:{1}", timeOfAlarm.Hours, timeOfAlarm.Minutes.ToString().PadLeft(2, '0'));
            //Toast.MakeText(this, timeOfAlarm.ToString(), ToastLength.Short).Show();
        }

        /// <summary>
        /// Hides the onscreen keyboard
        /// </summary>
        private void hideSoftKeyboard()
        {
            InputMethodManager imm = (InputMethodManager)GetSystemService(Context.InputMethodService);
            imm.HideSoftInputFromWindow(alarmName.WindowToken, 0);
        }

        /// <summary>
        /// Displays an message dialog 
        /// </summary>
        /// <param name="msg">The message to display</param>
        private void showMsg(string msg)
        {
            //create the alert dialog
            AlertDialog.Builder builder = new AlertDialog.Builder(this);

            AlertDialog alert = builder.Create();
            builder.SetTitle("Error:");
            builder.SetMessage(msg);
            builder.SetNeutralButton("OK", (senderAlert, args) =>
            {
                //do nothing
                alert.Dismiss();
            });
            //show the dialog
            Dialog dialog = builder.Create();
            dialog.Show();
        }

        /// <summary>
        /// Closes the activity when the back button pressed without saving the changes
        /// </summary>
        public override void OnBackPressed()
        {
            Finish();
        }

        /// <summary>
        /// Saves the alarm details and adds the alarm to the database and alarm list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveBtn_Click(object sender, EventArgs e)
        {
            List<int> days = new List<int>();
            //get days for alarm
            if (FindViewById<ToggleButton>(Resource.Id.cBoxSun).Checked) days.Add(1);
            if (FindViewById<ToggleButton>(Resource.Id.cBoxMon).Checked) days.Add(2);
            if (FindViewById<ToggleButton>(Resource.Id.cBoxTue).Checked) days.Add(3);
            if (FindViewById<ToggleButton>(Resource.Id.cBoxWed).Checked) days.Add(4);
            if (FindViewById<ToggleButton>(Resource.Id.cBoxThu).Checked) days.Add(5);
            if (FindViewById<ToggleButton>(Resource.Id.cBoxFri).Checked) days.Add(6);
            if (FindViewById<ToggleButton>(Resource.Id.cBoxSat).Checked) days.Add(7);

            if (days.Count == 0)
            {
                days.Add(0);
            }

            //make sure the alarm time has been set
            if (alarmTime.Text == "")
            {
                showMsg("You must set an alarm time!");
            }
            else
            {
                //call the web service to add the new alarm
                if (NetworkInterface.GetIsNetworkAvailable())
                {
                    Service1 client = new Service1();

                    //get the days
                    int[] daysSelected = days.ToArray();
                    //get the reminder time
                    string reminderTime = alarmReminderSpinner.SelectedItem.ToString();

                    //get the alarm sound
                    string alarmSound = null;
                    if (uriToRingTone != null)
                    {
                        alarmSound = uriToRingTone.ToString();
                    }
                    //add the new alarm
                    client.AddNewAlarmAsync(username, alarmName.Text, timeOfAlarm.ToString(), "y", int.Parse(reminderTime.Substring(0, reminderTime.Length - 4)), alarmSound, daysSelected);

                    client.AddNewAlarmCompleted += (object sender1, AddNewAlarmCompletedEventArgs e1) =>
                    {
                    //make a new alarm object           
                    Alarm alarm = new Alarm
                        {
                            AlarmID = e1.Result,
                            AlarmName = alarmName.Text,
                            AlarmTime = timeOfAlarm,
                            AlarmActive = true,
                            AlarmReminder = int.Parse(alarmReminderSpinner.SelectedItem.ToString().Substring(0, reminderTime.Length - 4)),
                            AlarmDays = days,
                            AlarmSound = alarmSound
                        };

                        //pass the intent the alarm object via JSON
                        Intent intent = new Intent();
                        intent.PutExtra("NewAlarm", JsonConvert.SerializeObject(alarm));
                        SetResult(Result.Ok, intent);
                        Finish();
                    };

                }
            }

            
        }

       
    }
}