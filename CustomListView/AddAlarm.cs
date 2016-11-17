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
using Android.Media;
using Android.Content.PM;

namespace CustomListView
{
    [Activity(Label = "AddAlarm", ScreenOrientation = ScreenOrientation.Portrait, WindowSoftInputMode = SoftInput.AdjustPan | SoftInput.StateHidden)]
    public class AddAlarm : Activity
    {
        EditText alarmName;
        EditText alarmTime;
       // EditText alarmReminder;
        TimeSpan timeOfAlarm;
        EditText alarmSound;
        Spinner alarmReminderSpinner;
        Android.Net.Uri uriToRingTone;
        private string username;

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

            username = Intent.GetStringExtra("Username");

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
                this, Resource.Array.reminder_array, Resource.Drawable.spinner_style);

            adapter.SetDropDownViewResource(Resource.Drawable.spinner_item_style);
            alarmReminderSpinner.Adapter = adapter;

            alarmTime.Click += AlarmTime_Click;

            alarmSound.Click += AlarmSound_Click;

            //alarmReminder.Click += AlarmReminder_Click;

            ImageButton saveBtn = FindViewById<ImageButton>(Resource.Id.btnSaveAlarm);
            saveBtn.Click += SaveBtn_Click;

            ImageButton cancelBtn = FindViewById<ImageButton>(Resource.Id.btnCancelAddAlarm);
            cancelBtn.Click += (object sender, EventArgs e) =>
            {
                Finish();
            };
        }

        private void AlarmSound_Click(object sender, EventArgs e)
        {
            Intent intent = new Intent(RingtoneManager.ActionRingtonePicker);
            this.StartActivityForResult(intent, 3);
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if (requestCode == 3 && resultCode == Result.Ok)
            {
                uriToRingTone = (Android.Net.Uri)data.GetParcelableExtra(RingtoneManager.ExtraRingtonePickedUri);
                string alarmTitle = RingtoneManager.GetRingtone(this, uriToRingTone).GetTitle(this);
                if (uriToRingTone != null)
                {
                    // RingtoneManager.SetActualDefaultRingtoneUri(this, RingtoneManager.TYPE_ALARM, uri);
                    alarmSound.Text = alarmTitle;
                    Toast.MakeText(this, uriToRingTone.ToString(), ToastLength.Short).Show();
                }
            }
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
            TimePickerDialog tpd = new TimePickerDialog(this, tdpCallback, DateTime.Now.Hour, DateTime.Now.Minute, true);            
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

        private void showMsg(string msg)
        {
            AlertDialog.Builder builder = new AlertDialog.Builder(this);

            AlertDialog alert = builder.Create();
            builder.SetTitle("Error:");
            builder.SetMessage(msg);
            builder.SetNeutralButton("OK", (senderAlert, args) =>
            {
                //do nothing
                alert.Dismiss();
            });

            Dialog dialog = builder.Create();
            dialog.Show();
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

            if (alarmTime.Text == "")
            {
                showMsg("You must set an alarm time!");
            }
            else
            {

                if (NetworkInterface.GetIsNetworkAvailable())
                {
                    Service1 client = new Service1();

                    int[] daysSelected = days.ToArray();

                    string reminderTime = alarmReminderSpinner.SelectedItem.ToString();

                    string alarmSound = null;
                    if (uriToRingTone != null)
                    {
                        alarmSound = uriToRingTone.ToString();
                    }

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