using System;
using Android.App;
using Android.Content;
using Android.Views;
using Android.Widget;
using Android.OS;
using System.Collections.Generic;
using Java.Util;
using Newtonsoft.Json;
using Android.Content.PM;
using System.Net.NetworkInformation;
using Bedtime.au.edu.wa.central.mydesign.student;
using System.Data;

namespace Bedtime
{
    /// <summary>
    /// Main UI for managing alarms. It displays a list of alarms that can be edited and turned on or off. New alarms can be added
    /// from here too.
    /// </summary>
    /// <remarks>
    /// author: David Pyle 041110777
    /// version: 1.0
    /// date: 18/11/2016
    /// </remarks>
    [Activity(Label = "Bedtime Alarm", ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : Activity
    {
        /// <summary>
        /// List of alarms for the user
        /// </summary>
        private List<Alarm> alarms;
        /// <summary>
        /// List view UI control for displaying alarms
        /// </summary>
        private ListView listView;
        /// <summary>
        /// Adapater for list view
        /// </summary>
        private AlarmScreenAdapter psa;
        /// <summary>
        /// Manages the alarms
        /// </summary>
        private AlarmManager mgr;
        /// <summary>
        /// Current logged in username
        /// </summary>
        private string username;

        /// <summary>
        /// Displays main UI and calls the method to load the alarms for the current user from the database
        /// </summary>
        /// <param name="savedInstanceState"></param>
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);
            //add the toolbar
            var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetActionBar(toolbar);
            ActionBar.Title = "My Alarms";

            //get the username
            username = Intent.GetStringExtra("Username");

            //button for navigating to next alarm screen
            ImageButton nextAlarm = FindViewById<ImageButton>(Resource.Id.btnNextAlarm);
            nextAlarm.Click += NextAlarm_Click;  

            //list of alarms
            listView = FindViewById<ListView>(Resource.Id.List);

            //load alarm data
            loadAlarms();
            
        }


        /// <summary>
        /// Navigates to the Next Alarm screen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NextAlarm_Click(object sender, EventArgs e)
        {
            //get the alarm that is due next
            Alarm nextDueAlarm = getNextAlarm();
            
            //go to the next alarm screen passing the alarm or if none set, pass no alarms set msg
            Intent intent = new Intent(this, typeof(NextAlarm));
            if (nextDueAlarm != null)
            {
                intent.PutExtra("Alarm", JsonConvert.SerializeObject(nextDueAlarm));
                intent.PutExtra("DaysFromNow", getNumDaysToAlarm(nextDueAlarm));
            }
            else
            {
                intent.PutExtra("Alarm", "No Alarms Set");
            }
            StartActivity(intent);
        }


        /// <summary>
        /// Gets the next alarm due from the list of alarms
        /// </summary>
        /// <returns>Next alarm due</returns>
        private Alarm getNextAlarm()
        {
            Alarm nextAlarm = null;

            //set the lowest time to 7 days in milliseconds - gotta start somewhere
            long lowestTime = 7 * 86400000L;

            //get the time now
            Calendar now = Calendar.GetInstance(Java.Util.TimeZone.Default);
            //get a calendar instance for today and set the required alarm hour and minute
            Calendar theAlarmTime = Calendar.GetInstance(Java.Util.TimeZone.Default);

            //search for the closest alarm to today
            foreach (Alarm alarm in alarms)
            {
                //if the alarm is on i.e. active
                if (alarm.AlarmActive)
                {
                    //convert its alarm time into a calendar instance
                    theAlarmTime.Set(CalendarField.HourOfDay, alarm.AlarmTime.Hours);
                    theAlarmTime.Set(CalendarField.Minute, alarm.AlarmTime.Minutes);
                    theAlarmTime.Set(CalendarField.Second, 0);

                    //get the number of days until the alarm
                    int daysFromNow = getNumDaysToAlarm(alarm);

                    //get alarm time in milliseconds
                    long alarmMillis = theAlarmTime.TimeInMillis + (86400000L * daysFromNow);
                    //if the alarm time is before now then add a day
                    if (theAlarmTime.Before(now) && daysFromNow == 0 && alarm.AlarmDays[0] == 0)
                    {
                        alarmMillis += 86400000L;
                    }
                    
                    //store the time until the next alarm
                    long timeToCheck = alarmMillis - now.TimeInMillis;

                    //check if the time just calculated is less than the stored one. If so, make that the lowest
                    if (timeToCheck  > 0 && timeToCheck  < lowestTime)
                    {
                        lowestTime = timeToCheck;
                        nextAlarm = alarm;
                    }
                }
            
            }         
            
            //return the alarm
            return nextAlarm;
        }

        /// <summary>
        /// Display the menu items for the toolbar
        /// </summary>
        /// <param name="menu"></param>
        /// <returns></returns>
        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.top_menu, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        /// <summary>
        /// Handles toolbar menu items click events
        /// </summary>
        /// <param name="item"></param>
        /// <returns>Selected item</returns>
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
           //get the menu item title
            switch(item.TitleFormatted.ToString())
            {
                //go to add alarm screen
                case "Add":            
                //show the AddALarm activity
                Intent i = new Intent(this, typeof(AddAlarm));
                i.PutExtra("Username", username);
                StartActivityForResult(i, 1);
                    break;
                
                //open browser and show online help site
                case "Help":
                    var uri = Android.Net.Uri.Parse("http://student.mydesign.central.wa.edu.au/041110777/bedtime/");
                    var intent = new Intent(Intent.ActionView, uri);
                    StartActivity(intent);
                    break;
                
                //log out and turn off the current alarms
                case "Logout":
                    foreach (Alarm alarm in alarms)
                    {
                        turnAlarmOff(alarm.AlarmID);
                    }
                    Intent logout = new Intent(this, typeof(Login));
                    StartActivity(logout);
                    Finish();
                    break;
            }
            return base.OnOptionsItemSelected(item);
        }

        /// <summary>
        /// Deletes the alarm if the user performs a long click on an alarm in the list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListView_ItemLongClick(object sender, AdapterView.ItemLongClickEventArgs e)
        {
            //get the alarm clicked
            var listView = sender as ListView;
            var alarm = listView.GetItemAtPosition(e.Position).Cast<Alarm>();

            //display delete confirmation
            AlertDialog.Builder builder = new AlertDialog.Builder(this);

            AlertDialog alert = builder.Create();
            builder.SetTitle("Confirm delete");
            builder.SetMessage("Are you sure you want to delete the alarm?");
            builder.SetPositiveButton("Delete", (senderAlert, args) => {
                //delete the alarm
                deleteAlarm(alarm.AlarmID);
                
            });

            builder.SetNegativeButton("Cancel", (senderAlert, args) => {
                alert.Dismiss();
            });

            Dialog dialog = builder.Create();
            dialog.Show(); ;
        }

        /// <summary>
        /// Deletes the alarm from the list of alarms and then from the database
        /// </summary>
        /// <param name="alarmID">Alarm id to delete</param>
        private void deleteAlarm(int alarmID)
        {
            if (NetworkInterface.GetIsNetworkAvailable())
            {
                //call the delete web service method
                Service1 client = new Service1();
                client.DeleteAlarmAsync(alarmID);

                //when the web service returns result deal with it
                client.DeleteAlarmCompleted += (object sender, DeleteAlarmCompletedEventArgs e) =>
                {
                    //if successful, show a message and delete from the alarm list
                    if (e.Result == 1)
                    {
                        Toast.MakeText(this, "Alarm Deleted!", ToastLength.Short).Show();
                        //delete from alarm manager
                        deleteFromAlarmMgr(alarmID);
                        //delete from list of alarms and update list view
                        alarms.RemoveAt(findAlarm(alarmID));
                        //update the list view of alarms
                        listView.RemoveAllViewsInLayout();
                        psa.NotifyDataSetChanged();
                    } else
                    {
                        //show error msg if db delete failed
                        Toast.MakeText(this, "Delete Failed on Database", ToastLength.Short).Show();
                    }
                };                
            }
        }

        /// <summary>
        /// Find an alarm from the list by searching for its alarm id
        /// </summary>
        /// <param name="alarmID">Alarm id to search</param>
        /// <returns>index position of alarm in the alarm list, -1 for not found</returns>
        private int findAlarm(int alarmID)
        {
            int result = -1;
            //search the list
            for (int pos = 0; pos < alarms.Count; pos++)
            {
                if (alarms[pos].AlarmID == alarmID)
                {
                    result = pos;
                    //Toast.MakeText(this, "ALARM FOUND IS " + result.AlarmName, ToastLength.Short).Show();
                    break;
                }
            }
            //return the alarm position
            return result;
        }

        
        //deal with the add alarm result (code 1) and edit alarm result (code 2)
        /// <summary>
        /// Called when the Add Alarm activity and Edit Alarm activity has finished
        /// </summary>
        /// <param name="requestCode">Add Alarm code is 1, Edit Alarm is 2</param>
        /// <param name="resultCode">result code</param>
        /// <param name="alarmData">data returned</param>
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent alarmData)
        {
            base.OnActivityResult(requestCode, resultCode, alarmData);

            //if the edit activity returned
            if (requestCode == 2 && resultCode == Result.Ok)
            {
                //get the edited alarm
                Alarm alarmEdited = JsonConvert.DeserializeObject<Alarm>(alarmData.GetStringExtra("EditedAlarm"));

                int index = -1;
                //find the original alarm in the alarm list
                for (int pos=0; pos<alarms.Count; pos++)
                {
                    if (alarms[pos].AlarmID == alarmEdited.AlarmID)
                    {
                        index = pos;
                        //Toast.MakeText(this, "ALARM FOUND IS " + result.AlarmName, ToastLength.Short).Show();
                        break;
                    }
                }

                //replace the old alarm object with the edited one (still has same id)
                alarms[index] = alarmEdited;
                //update the list view
                listView.RemoveAllViewsInLayout();
                psa.NotifyDataSetChanged();
                //delete exisiting alarm and create new one from edited one - cancelcurrent flag will actually do this for us
                addAlarm(alarmEdited.AlarmID, alarmEdited.AlarmTime, getNumDaysToAlarm(alarmEdited));

            }

            //if the add alarm activity has finished
            else if (requestCode == 1 && resultCode == Result.Ok)
            {               
                //get the new alarm
                Alarm newAlarm = JsonConvert.DeserializeObject<Alarm>(alarmData.GetStringExtra("NewAlarm"));
                //add alarm to alarm list
                alarms.Add(newAlarm);
                //update the list view
                listView.RemoveAllViewsInLayout();
                psa.NotifyDataSetChanged();
                //get number of days until the alarm
                int daysFromNow = getNumDaysToAlarm(newAlarm);
                //add the alarm to the alarm manager.Keep track of it by setting the request code to the alarm id
                addAlarm(newAlarm.AlarmID, newAlarm.AlarmTime, daysFromNow);
                
            }
        }

        /// <summary>
        /// Gets the number of days until an alarm is due
        /// </summary>
        /// <param name="alarm">Alarm to work out the days until</param>
        /// <returns>Days until alarm will trigger</returns>
        private int getNumDaysToAlarm(Alarm alarm)
        {
            //starting point
            int daysToGo = 8;

            //if the alarm is repeating and not just a one time alarm
            if (alarm.AlarmDays[0] != 0)
            {
                //get today's day
                Calendar now = Calendar.GetInstance(Java.Util.TimeZone.Default);
                int today = now.Get(CalendarField.DayOfWeek);
                //get the alarm time
                Calendar alarmT = Calendar.GetInstance(Java.Util.TimeZone.Default);
                alarmT.Set(CalendarField.HourOfDay, alarm.AlarmTime.Hours);
                alarmT.Set(CalendarField.Minute, alarm.AlarmTime.Minutes);
                alarmT.Set(CalendarField.Second, 0);

                //if the alarm has today as a repeat day check if its just today or next week
                if (alarm.AlarmDays.Contains(today))
                {
                    //if the alarm time is before the current time then the alarm is due in a week
                    if (alarmT.Before(now))
                    {
                        daysToGo = 7;
                    } else
                    {
                        daysToGo = 0;
                    }
                }

                //check the rest of the repeat days
                for (int i = 0; i < alarm.AlarmDays.Count; i++)
                {
                    if (alarm.AlarmDays[i] != today)
                    {
                        int temp = (7 + (alarm.AlarmDays[i] - today)) % 7;

                        if (temp < daysToGo)
                        {
                            daysToGo = temp;
                        }
                    }
                }                
            } else
            {
                daysToGo = 0;
            }
            return daysToGo;
        }

        /// <summary>
        /// When an alarm is turned on, set it in alarm manager and update the alarm state in the database and list
        /// </summary>
        /// <param name="alarmID">alarm to turn on</param>
        public void turnAlarmOn(int alarmID)
        {
            //call the web service
            if (NetworkInterface.GetIsNetworkAvailable())
            {
                Service1 client = new Service1();
                client.ToggleAlarmAsync(alarmID, "y");

                client.ToggleAlarmCompleted += (object sender, ToggleAlarmCompletedEventArgs e) =>
                {
                    //update the alarms active state
                    for (int pos = 0; pos < alarms.Count; pos++)
                    {
                        if (alarms[pos].AlarmID == alarmID)
                        {
                            alarms[pos].AlarmActive = true;

                            //turn the alarm back on
                            addAlarm(alarmID, alarms[pos].AlarmTime, getNumDaysToAlarm(alarms[pos]));
                            break;
                        }
                    }
                };
            }
            
        }

        /// <summary>
        /// Turns alarms off and updates the database and alarm list
        /// </summary>
        /// <param name="alarmID">Alarm to turn off</param>
        public void turnAlarmOff(int alarmID)
        {
            //call the web service to update the alarm state
            if (NetworkInterface.GetIsNetworkAvailable())
            {
                Service1 client = new Service1();
                client.ToggleAlarmAsync(alarmID, "n");

                client.ToggleAlarmCompleted += (object sender, ToggleAlarmCompletedEventArgs e) =>
                {
                    //update the alarms active state
                    for (int pos = 0; pos < alarms.Count; pos++)
                    {
                        if (alarms[pos].AlarmID == alarmID)
                        {
                            alarms[pos].AlarmActive = false;
                            listView.RemoveAllViewsInLayout();
                            psa.NotifyDataSetChanged();                           
                            break;
                        }
                    }
                    //remove alarm from the alarm manager
                    deleteFromAlarmMgr(alarmID);
                };
            }            
        }

        /// <summary>
        /// Deletes the alarm from alarm manager
        /// </summary>
        /// <param name="code">Code of alarm to delete</param>
        private void deleteFromAlarmMgr(int code)
        {
            //get the alarm manager
            mgr = (AlarmManager)GetSystemService(AlarmService);

            //cancel the alarm
            Intent intent = new Intent(this, typeof(AlarmReceiver));
            PendingIntent pendingIntent = PendingIntent.GetActivity(this, code, intent, PendingIntentFlags.UpdateCurrent); 
            mgr.Cancel(pendingIntent);

            //cancel any reminders too
            Intent intentReminder = new Intent(this, typeof(AlarmReceiver));
            PendingIntent pendingIntentReminder = PendingIntent.GetActivity(this, code+1000, intentReminder, PendingIntentFlags.UpdateCurrent);
            mgr.Cancel(pendingIntentReminder);

        }

        /// <summary>
        /// Adds an alarm to the alarm manager including its reminder
        /// </summary>
        /// <param name="code">Alarm ID of alarm used as code</param>
        /// <param name="alarmTime">Time of alarm</param>
        /// <param name="daysFromNow">Number of days until alarm</param>
        private void addAlarm(int code, TimeSpan alarmTime, int daysFromNow)
        {
            //get the alarm to set
            Alarm alarmToSet= alarms[findAlarm(code)];

            //call the AlarmReceiver activity when the alarm is triggered
            Intent intent = new Intent(this, typeof(AlarmReceiver));
            intent.PutExtra("Username", username);
            intent.PutExtra("AlarmID", code);
            intent.PutExtra("AlarmName", alarmToSet.AlarmName);
            intent.PutExtra("AlarmTime", (alarmToSet.AlarmTime).ToString(@"hh\:mm"));
            intent.PutExtra("AlarmSound", alarmToSet.AlarmSound);
            PendingIntent pendingIntent = PendingIntent.GetActivity(this, code, intent, PendingIntentFlags.CancelCurrent);

            mgr = (AlarmManager)GetSystemService(AlarmService);

            //work out the alarm time in milliseconds and account for times that are the next day
            //get a calendar instance for the current day/time
            Calendar now = Calendar.GetInstance(Java.Util.TimeZone.Default);
            //get a calendar instance for today and set the required alarm hour and minute
            Calendar alarm = Calendar.GetInstance(Java.Util.TimeZone.Default);
            alarm.Set(CalendarField.HourOfDay, alarmTime.Hours);
            alarm.Set(CalendarField.Minute, alarmTime.Minutes);
            alarm.Set(CalendarField.Second, 0);
            long alarmMillis = alarm.TimeInMillis + (86400000L * daysFromNow); 
            //if the alarm time is before now and no alarm days are set then add a day
            if (alarm.Before(now) && daysFromNow == 0 && alarmToSet.AlarmDays[0] == 0)
            {
                alarmMillis += 86400000L;
            }
            //set the alarm
            mgr.SetExact(AlarmType.RtcWakeup, alarmMillis, pendingIntent);

            //set the reminder for the alarm if set
            if (alarmToSet.AlarmReminder != 0)
            {
                Intent reminderIntent = new Intent(this, typeof(AlarmReceiver));
                reminderIntent.PutExtra("Reminder", true);
                reminderIntent.PutExtra("AlarmID", code);
                reminderIntent.PutExtra("AlarmName", alarmToSet.AlarmName);
                reminderIntent.PutExtra("AlarmTime", (alarmToSet.AlarmTime).ToString(@"hh\:mm"));
                reminderIntent.PutExtra("AlarmSound", alarmToSet.AlarmSound);
                PendingIntent pendingIntentReminder = PendingIntent.GetActivity(this, code+1000, reminderIntent, PendingIntentFlags.CancelCurrent);
                //set the reminder alarm in the alarm manager
                mgr.SetExact(AlarmType.RtcWakeup, alarmMillis - ((alarmToSet.AlarmReminder)*60*1000), pendingIntentReminder);
            }
            //Display message informing user that the alarm has been set
            TimeSpan currentOffset = System.TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now);
            var time = TimeSpan.FromMilliseconds(alarmMillis);
            Toast.MakeText(this, "Alarm set for: " + (new DateTime(1970, 1, 1) + time + currentOffset).ToString(), ToastLength.Short).Show();

        }

        /// <summary>
        /// Creates an alarm for each active alarm in the alarm list
        /// </summary>
        private void createAlarms()
        {
            foreach (Alarm alarm in alarms)
            {
                //only add alarms that are active
                if (alarm.AlarmActive)
                {
                    //add the alarm
                    addAlarm(alarm.AlarmID, alarm.AlarmTime, getNumDaysToAlarm(alarm));
                }
            }
        }
                
        /// <summary>
        /// Gets the alarm clicked on by user from the alarm list and calls the edit alarm activity
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnListItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            //get the alarm from the list
            var listView = sender as ListView;
            var alarm = listView.GetItemAtPosition(e.Position).Cast<Alarm>();           

            //get the alarm object and serialize it so we can pass it to the edit alarm activity
            Intent i = new Intent(this, typeof(EditAlarm));           
            i.PutExtra("Alarm", JsonConvert.SerializeObject(alarm));
            StartActivityForResult(i, 2);
        }

        /// <summary>
        /// Converts the string array of days from the database into a int list 
        /// </summary>
        /// <param name="days">string array to convert</param>
        /// <returns></returns>
        private List<int> convertDaysToInt(string[] days)
        {
            List<int> daysAsInt = new List<int>();

            foreach (var item in days)
            {
                switch (item)
                {
                    case "NONE":
                        daysAsInt.Add(0);
                        break;
                    case "SUN":
                        daysAsInt.Add(1);
                        break;
                    case "MON":
                        daysAsInt.Add(2);
                        break;
                    case "TUE":
                        daysAsInt.Add(3);
                        break;
                    case "WED":
                        daysAsInt.Add(4);
                        break;
                    case "THU":
                        daysAsInt.Add(5);
                        break;
                    case "FRI":
                        daysAsInt.Add(6);
                        break;
                    case "SAT":
                        daysAsInt.Add(7);
                        break;                    
                }
            }

            return daysAsInt;
        }

        /// <summary>
        /// Gets the alarms from the database and creates alarm objects and sets any active alarms in alarm manager.
        /// Called everytime the main activity is created to ensure latest alarm information is presented to user
        /// </summary>
        private void loadAlarms()
        {
            alarms = new List<Alarm>();            

            //call the web service to get the alarms
            if (NetworkInterface.GetIsNetworkAvailable())
            {
                Service1 client = new Service1();
                client.ListAlarmsAsync(username);

                //when the web service returns create the alarms
                client.ListAlarmsCompleted += (object sender, ListAlarmsCompletedEventArgs e) =>
                {
                    foreach (DataRow dr in e.Result.Rows)
                    {
                        //extract alarm days
                        string[] daysAlarmOn = (dr["Days"].ToString()).Split(',');
                        List<int> days = convertDaysToInt(daysAlarmOn);

                        //convert alarm time from string to TimeSpan
                        string[] timeAlarm = (dr["AlarmTime"].ToString()).Split(':');
                        TimeSpan alarmTimeSpan = new TimeSpan(int.Parse(timeAlarm[0]), int.Parse(timeAlarm[1]), 0);

                        //convert active y/n to boolean
                        string active = dr["AlarmActive"].ToString();
                        Boolean alarmActive = false;
                        if (active == "y")
                        {
                            alarmActive = true;
                        }

                        //create alarm object
                        Alarm alarm = new Alarm
                        {
                            AlarmID = int.Parse(dr["AlarmID"].ToString()),
                            AlarmName = dr["AlarmName"].ToString(),
                            AlarmTime = alarmTimeSpan,
                            AlarmActive = alarmActive,
                            AlarmReminder = int.Parse(dr["AlarmReminder"].ToString()),
                            AlarmDays = days,
                            AlarmSound = dr["AlarmSound"].ToString(),
                        };                      
                        //add the alarm to the list
                        alarms.Add(alarm);
                    }

                    //check if AlarmReceiver called main activity
                    int alarmIDTurnedOff = Intent.GetIntExtra("AlarmJustStopped", -1);

                    if (alarmIDTurnedOff != -1)
                    {
                        //must be the AlarmReceiver calling - update alarm manager                        
                        Alarm alarmToUpdate = alarms[findAlarm(alarmIDTurnedOff)];
                        //turn off any one-off alarms
                        if (alarmToUpdate.AlarmDays[0] == 0)
                        {
                            turnAlarmOff(alarmToUpdate.AlarmID);

                        }
                        else //update existing repeating alarm
                        {
                            turnAlarmOn(alarmToUpdate.AlarmID);

                        }
                    } else
                    {
                        //create the alarms for each one in the list
                        createAlarms();
                    }

                    //create a list view and set data source to the sample alarms list
                    psa = new AlarmScreenAdapter(this, alarms);
                    listView.Adapter = psa;
                    //set a click handler for when an list item is clicked (edit alarm)
                    listView.ItemClick += OnListItemClick;

                    //long click will give user option to delete alarm
                    listView.ItemLongClick += ListView_ItemLongClick;                                               
                    
                };
            }
        }

        /// <summary>
        /// Overrides the back button to prevent the alarm list view from regenerating
        /// </summary>
        public override void OnBackPressed()
        {
            //Toast.MakeText(this, "Back Button Pressed..", ToastLength.Short).Show();
            MoveTaskToBack(true);
        }

        

    }
    /// <summary>
    /// Helper class to cast an object correctly
    /// </summary>
    public static class ObjectTypeHelper
    {
        public static T Cast<T>(this Java.Lang.Object obj) where T : class
        {
            var propertyInfo = obj.GetType().GetProperty("Instance");
            return propertyInfo == null ? null : propertyInfo.GetValue(obj, null) as T;
        }
    }

}

