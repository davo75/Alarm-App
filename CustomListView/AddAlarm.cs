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
using Newtonsoft.Json;
using System.Net.NetworkInformation;
using CustomListView.au.edu.wa.central.mydesign.student;
using Android.Graphics;
using Android.Views.InputMethods;

namespace CustomListView
{
    [Activity(Label = "AddAlarm", WindowSoftInputMode = SoftInput.AdjustPan | SoftInput.StateHidden)]
    public class AddAlarm : Activity
    {
        EditText alarmName;
        EditText alarmTime;
       // EditText alarmReminder;
        TimeSpan timeOfAlarm;
        EditText alarmSound;
        Spinner alarmReminderSpinner;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            //this.RequestWindowFeature(WindowFeatures.NoTitle);
            //this.Window.AddFlags(WindowManagerFlags.Fullscreen);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.AddAlarm2);

            //add the toolbar
            var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetActionBar(toolbar);
            ActionBar.Title = "Add Alarm";

            alarmName = FindViewById<EditText>(Resource.Id.txtAlarmName);
            alarmTime = FindViewById<EditText>(Resource.Id.txtAlarmTime);
            //alarmReminder = FindViewById<EditText>(Resource.Id.txtAlarmReminder);
            alarmSound = FindViewById<EditText>(Resource.Id.txtAlarmSound);

            //set a custom font for edit text fields
            Typeface font = Typeface.CreateFromAsset(Assets, "fonts/Myriad-Pro-Bold.ttf");            
            alarmName.Typeface = font;           
            alarmTime.Typeface = font;            
           // alarmReminder.Typeface = font;           
            alarmSound.Typeface = font;

            alarmReminderSpinner = FindViewById<Spinner>(Resource.Id.reminderList);
            alarmReminderSpinner.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(alarmReminderSpinner_ItemSelected);

            var adapter = ArrayAdapter.CreateFromResource(
                this, Resource.Array.reminder_array, Resource.Layout.spinner_layout);

            //adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            alarmReminderSpinner.Adapter = adapter;

            alarmTime.Click += AlarmTime_Click;

            //alarmReminder.Click += AlarmReminder_Click;

            ImageButton saveBtn = FindViewById<ImageButton>(Resource.Id.btnSaveAlarm);
            saveBtn.Click += SaveBtn_Click;

            ImageButton cancelBtn = FindViewById<ImageButton>(Resource.Id.btnCancelAddAlarm);
            cancelBtn.Click += (object sender, EventArgs e) =>
            {
                Finish();
            };
        }

        //private void AlarmReminder_Click(object sender, EventArgs e)
        //{
        //    alarmReminderSpinner.PerformClick();
        //}

        private void alarmReminderSpinner_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            Spinner spinner = (Spinner)sender;

            string toast = string.Format("The reminder is {0}", spinner.GetItemAtPosition(e.Position));
            toast = spinner.SelectedItem.ToString();
            Toast.MakeText(this, toast, ToastLength.Long).Show();
        }

        private void AlarmTime_Click(object sender, EventArgs e)
        {
            hideSoftKeyboard();
            TimePickerDialog tpd = new TimePickerDialog(this, tdpCallback, DateTime.Now.Hour, DateTime.Now.Minute, false);
            tpd.Show();
        }

        private void tdpCallback(object sender, TimePickerDialog.TimeSetEventArgs e)
        {
            //Toast.MakeText(this, string.Format("{0}:{1}", e.HourOfDay, e.Minute.ToString().PadLeft(2, '0')), ToastLength.Short).Show();
            //alarmTime.Text = string.Format("{0}:{1}", e.HourOfDay, e.Minute.ToString().PadLeft(2, '0'));
            timeOfAlarm = new TimeSpan(e.HourOfDay, e.Minute, 0);
            alarmTime.Text = string.Format("{0}:{1}", timeOfAlarm.Hours, timeOfAlarm.Minutes.ToString().PadLeft(2, '0'));
            Toast.MakeText(this, timeOfAlarm.ToString(), ToastLength.Short).Show();
        }

        private void hideSoftKeyboard()
        {
            InputMethodManager imm = (InputMethodManager)GetSystemService(Context.InputMethodService);
            imm.HideSoftInputFromWindow(alarmName.WindowToken, 0);
        }

        public override void OnBackPressed()
        {
            Finish();
        }

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

            if (NetworkInterface.GetIsNetworkAvailable())
            {
                Service1 client = new Service1();

                int[] daysSelected = days.ToArray();

                client.AddNewAlarmAsync("dave", alarmName.Text, timeOfAlarm.ToString(), "y", int.Parse(alarmReminderSpinner.SelectedItem.ToString()), daysSelected);

                client.AddNewAlarmCompleted += (object sender1, AddNewAlarmCompletedEventArgs e1) =>
                {
                    //make a new alarm object           
                    Alarm alarm = new Alarm
                    {
                        AlarmID = e1.Result,
                        AlarmName = alarmName.Text,
                        AlarmTime = timeOfAlarm,
                        AlarmActive = true,
                        AlarmReminder = int.Parse(alarmReminderSpinner.SelectedItem.ToString()),
                        AlarmDays = days
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