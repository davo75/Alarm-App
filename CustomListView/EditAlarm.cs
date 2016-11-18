using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using System.Net.NetworkInformation;
using Bedtime.au.edu.wa.central.mydesign.student;
using Android.Views.InputMethods;
using Android.Media;
using Android.Content.PM;

namespace Bedtime
{
    /// <summary>
    /// Screen for editing an alarm
    /// </summary>
    /// <remarks>
    /// author: David Pyle 041110777
    /// version: 1.0
    /// date: 18/11/2016
    /// </remarks>
    
    [Activity(Label = "EditAlarm", ScreenOrientation = ScreenOrientation.Portrait, WindowSoftInputMode = SoftInput.AdjustPan | SoftInput.StateHidden)]
    public class EditAlarm : Activity
    {
        /// <summary>
        /// Alarm to edit
        /// </summary>
        private Alarm alarmToEdit;
        /// <summary>
        /// Alarm name text field
        /// </summary>
        private EditText alarmName;
        /// <summary>
        /// Alarm time text field
        /// </summary>
        private EditText alarmTime;
        /// <summary>
        /// Alarm sound text field
        /// </summary>
        private EditText alarmSound;
        /// <summary>
        /// Alarm time
        /// </summary>
        private TimeSpan timeOfAlarm;
        /// <summary>
        /// Alarm sound drop down lisr
        /// </summary>
        private Spinner alarmReminderSpinner;
        /// <summary>
        /// Path to ringtone
        /// </summary>
        private Android.Net.Uri uriToRingTone;
      
        /// <summary>
        /// Displays the alarm fields for editing
        /// </summary>
        /// <param name="savedInstanceState"></param>
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.EditAlarm2);

            //add the toolbar
            var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetActionBar(toolbar);
            ActionBar.Title = "Edit Alarm";                

            //get the alarm to edit
            alarmToEdit = JsonConvert.DeserializeObject<Alarm>(Intent.GetStringExtra("Alarm"));

            //set the text fields to the existing alarm values
            alarmName = FindViewById<EditText>(Resource.Id.txtAlarmName);
            alarmName.Text = alarmToEdit.AlarmName;

            //alarm time
            alarmTime = FindViewById<EditText>(Resource.Id.txtAlarmTime);
            alarmTime.Text = alarmToEdit.AlarmTime.ToString(@"hh\:mm");

            alarmTime.Click += AlarmTime_Click;

            //alarm sound
            alarmSound = FindViewById<EditText>(Resource.Id.txtAlarmSound);

            if (alarmToEdit.AlarmSound != null && alarmToEdit.AlarmSound != "")
            {
                alarmSound.Text = RingtoneManager.GetRingtone(this, Android.Net.Uri.Parse(alarmToEdit.AlarmSound)).GetTitle(this);
            } else
            {
                uriToRingTone = null;
            }

            alarmSound.Click += AlarmSound_Click;

            //drop down list for alarm sound
            alarmReminderSpinner = FindViewById<Spinner>(Resource.Id.reminderList);
            alarmReminderSpinner.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(alarmReminderSpinner_ItemSelected);

            //list for reminder values
            var adapter = ArrayAdapter.CreateFromResource(
                this, Resource.Array.reminder_array, Resource.Drawable.spinner_style);

            adapter.SetDropDownViewResource(Resource.Drawable.spinner_item_style);
            alarmReminderSpinner.Adapter = adapter;

            string[] str = Resources.GetStringArray(Resource.Array.reminder_array);
            int pos = Array.IndexOf(str, alarmToEdit.AlarmReminder.ToString() + " min");
            
            alarmReminderSpinner.SetSelection(pos); 

            //days for alarm days
            List<int> days = new List<int>();

            days = alarmToEdit.AlarmDays;

            //check the alarm days
            foreach (var day in days)
            {
                switch (day)
                {
                    case 1:
                        ToggleButton cbSun = (ToggleButton)FindViewById(Resource.Id.cBoxSun);
                        cbSun.Checked = true;
                        break;
                    case 2:
                        ToggleButton cbMon = (ToggleButton)FindViewById(Resource.Id.cBoxMon);
                        cbMon.Checked = true;
                        break;
                    case 3:
                        ToggleButton cbTue = (ToggleButton)FindViewById(Resource.Id.cBoxTue);
                        cbTue.Checked = true;
                        break;
                    case 4:
                        ToggleButton cbWed = (ToggleButton)FindViewById(Resource.Id.cBoxWed);
                        cbWed.Checked = true;
                        break;
                    case 5:
                        ToggleButton cbThu = (ToggleButton)FindViewById(Resource.Id.cBoxThu);
                        cbThu.Checked = true;
                        break;
                    case 6:
                        ToggleButton cbFri = (ToggleButton)FindViewById(Resource.Id.cBoxFri);
                        cbFri.Checked = true;
                        break;
                    case 7:
                        ToggleButton cbSat = (ToggleButton)FindViewById(Resource.Id.cBoxSat);
                        cbSat.Checked = true;
                        break;
                }
            }

            //save button
            ImageButton saveBtn = FindViewById<ImageButton>(Resource.Id.btnSaveAlarm);
            saveBtn.Click += SaveBtn_Click;

            //cancel button
            ImageButton cancelBtn = FindViewById<ImageButton>(Resource.Id.btnCancelEditAlarm);
            cancelBtn.Click += (object sender, EventArgs e) =>
            {
                Finish();
            };

        }

        /// <summary>
        /// Displays a list of ringtones
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AlarmSound_Click(object sender, EventArgs e)
        {
            Intent intent = new Intent(RingtoneManager.ActionRingtonePicker);
            this.StartActivityForResult(intent, 3);
        }

        /// <summary>
        /// Sets the path to the selected ringtone
        /// </summary>
        /// <param name="requestCode"></param>
        /// <param name="resultCode"></param>
        /// <param name="data"></param>
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            //if a ringtone was selected then set its path
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

        /// <summary>
        /// Gets the value selected in the list of reminder times
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
        /// Displays the time picker for setting the alarm time
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
        /// Hides the onscreen keyboard
        /// </summary>
        private void hideSoftKeyboard()
        {
            InputMethodManager imm = (InputMethodManager)GetSystemService(Context.InputMethodService);
            imm.HideSoftInputFromWindow(alarmName.WindowToken, 0);
        }

        /// <summary>
        /// Gets the time selected from the timepicker dialog and puts the value in the alarm time field
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
        /// Saves the edited alarm values
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveBtn_Click(object sender, EventArgs e)
        {
            //get the alarm name
            alarmToEdit.AlarmName = alarmName.Text;

            //get alarm time
            string[] aTime = alarmTime.Text.Split(':');
            TimeSpan aTimSpan = new TimeSpan(int.Parse(aTime[0]), int.Parse(aTime[1]), 0);
            alarmToEdit.AlarmTime = aTimSpan;

            //get the reminder time
            string reminderTime = alarmReminderSpinner.SelectedItem.ToString();
            alarmToEdit.AlarmReminder = int.Parse(reminderTime.Substring(0, reminderTime.Length - 4));
            
            //set alarm active
            alarmToEdit.AlarmActive = true;

            //get the alarm sound
            string alarmSound = "";
            if (uriToRingTone != null)
            {
                alarmToEdit.AlarmSound = uriToRingTone.ToString();
                alarmSound = uriToRingTone.ToString();
            }
            
            List<int> days = new List<int>();
            //get days for alarm
            if (FindViewById<ToggleButton>(Resource.Id.cBoxSun).Checked) days.Add(1);
            if (FindViewById<ToggleButton>(Resource.Id.cBoxMon).Checked) days.Add(2);
            if (FindViewById<ToggleButton>(Resource.Id.cBoxTue).Checked) days.Add(3);
            if (FindViewById<ToggleButton>(Resource.Id.cBoxWed).Checked) days.Add(4);
            if (FindViewById<ToggleButton>(Resource.Id.cBoxThu).Checked) days.Add(5);
            if (FindViewById<ToggleButton>(Resource.Id.cBoxFri).Checked) days.Add(6);
            if (FindViewById<ToggleButton>(Resource.Id.cBoxSat).Checked) days.Add(7);
            alarmToEdit.AlarmDays = days;

            //if no days selected at 0
            if (days.Count == 0)
            {
                days.Add(0);
            }

            //call web service to update the database with the edited alarm
            if (NetworkInterface.GetIsNetworkAvailable())
            {

                Service1 client = new Service1();
                int[] daysSelected = days.ToArray();
                
                //update the alarm
                client.UpdateAlarmAsync(alarmToEdit.AlarmID, alarmToEdit.AlarmName, alarmToEdit.AlarmTime.ToString(), "y", alarmToEdit.AlarmReminder, alarmSound, daysSelected);

                //get the database result. If ok then send the edited alarm back to the main alarm screen
                client.UpdateAlarmCompleted += (object sender1, UpdateAlarmCompletedEventArgs e1) =>
                {
                    if (e1.Result == 1)
                    {
                        Intent intent = new Intent();
                        intent.PutExtra("EditedAlarm", JsonConvert.SerializeObject(alarmToEdit));
                        SetResult(Result.Ok, intent);
                        Finish();
                    } else
                    {
                        Toast.MakeText(this, "Failed to Update DB", ToastLength.Short).Show();
                    }
                };

            } 
        }

        /// <summary>
        /// Close the activity if the back button pressed
        /// </summary>
        public override void OnBackPressed()
        {
            Finish();
        }
    }
}