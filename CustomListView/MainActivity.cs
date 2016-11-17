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
    [Activity(Label = "Bedtime Alarm", ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : Activity
    {


    List<Alarm> alarms;
        ListView listView;
        PeopleScreenAdapter psa;

        AlarmManager mgr;

        private string username;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // this.RequestWindowFeature(WindowFeatures.NoTitle);
            // this.Window.AddFlags(WindowManagerFlags.Fullscreen);
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

            listView = FindViewById<ListView>(Resource.Id.List);

            //load alarm data
            loadAlarms();
            

            Toast.MakeText(this, "OnCreate", ToastLength.Short).Show();
        }

        private void NextAlarm_Click(object sender, EventArgs e)
        {
            Alarm nextDueAlarm = getNextAlarm();
            

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

        private Alarm getNextAlarm()
        {
            Alarm nextAlarm = null;

            //set the lowest time to 7 days in milliseconds - gotta start somewhere
            long lowestTime = 7 * 86400000L;

            //get the time now
            Calendar now = Calendar.GetInstance(Java.Util.TimeZone.Default);
            //get a calendar instance for today and set the required alarm hour and minute
            Calendar theAlarmTime = Calendar.GetInstance(Java.Util.TimeZone.Default);

            foreach (Alarm alarm in alarms)
            {
                //if the alarm is on i.e. active
                if (alarm.AlarmActive)
                {
                    //convert its alarm time into a calendar instance
                    theAlarmTime.Set(CalendarField.HourOfDay, alarm.AlarmTime.Hours);
                    theAlarmTime.Set(CalendarField.Minute, alarm.AlarmTime.Minutes);
                    theAlarmTime.Set(CalendarField.Second, 0);

                    int daysFromNow = getNumDaysToAlarm(alarm);

                    long alarmMillis = theAlarmTime.TimeInMillis + (86400000L * daysFromNow);
                    //if the alarm time is before now then add a day
                    if (theAlarmTime.Before(now) && daysFromNow == 0 && alarm.AlarmDays[0] == 0)
                    {
                        alarmMillis += 86400000L;
                    }
                    //if alarm days have been set and the alarm time is before now and the days are 0 then it must be a week from now
                    //else if (theAlarmTime.Before(now) && daysFromNow == 0 && alarm.AlarmDays.Count > 0)
                    //{
                    //    alarmMillis += 7 * 86400000L;
                    //}


                    long timeToCheck = alarmMillis - now.TimeInMillis;

                    if (timeToCheck  > 0 && timeToCheck  < lowestTime)
                    {
                        lowestTime = timeToCheck;
                        nextAlarm = alarm;
                    }
                }
            
            }         
            

            return nextAlarm;
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.top_menu, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
           
            switch(item.TitleFormatted.ToString())
            {
                case "Add":            
                //show the AddALarm activity
                Intent i = new Intent(this, typeof(AddAlarm));
                i.PutExtra("Username", username);
                StartActivityForResult(i, 1);
                    break;

                case "Help":
                    var uri = Android.Net.Uri.Parse("http://student.mydesign.central.wa.edu.au/041110777/bedtime/");
                    var intent = new Intent(Intent.ActionView, uri);
                    StartActivity(intent);
                    break;
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

        private void ListView_ItemLongClick(object sender, AdapterView.ItemLongClickEventArgs e)
        {
            var listView = sender as ListView;
            var alarm = listView.GetItemAtPosition(e.Position).Cast<Alarm>();

            AlertDialog.Builder builder = new AlertDialog.Builder(this);

            AlertDialog alert = builder.Create();
            builder.SetTitle("Confirm delete");
            builder.SetMessage("Are you sure you want to delete the alarm?");
            builder.SetPositiveButton("Delete", (senderAlert, args) => {
                deleteAlarm(alarm.AlarmID);
                
            });

            builder.SetNegativeButton("Cancel", (senderAlert, args) => {
                alert.Dismiss();
            });

            Dialog dialog = builder.Create();
            dialog.Show(); ;
        }

        private void deleteAlarm(int alarmID)
        {
            if (NetworkInterface.GetIsNetworkAvailable())
            {
                au.edu.wa.central.mydesign.student.Service1 client = new au.edu.wa.central.mydesign.student.Service1();
                client.DeleteAlarmAsync(alarmID);

                client.DeleteAlarmCompleted += (object sender, DeleteAlarmCompletedEventArgs e) =>
                {
                    if (e.Result == 1)
                    {
                        Toast.MakeText(this, "Alarm Deleted!", ToastLength.Short).Show();
                        //delete from alarm manager
                        deleteFromAlarmMgr(alarmID);
                        //delete from list of alarms and update list view
                        alarms.RemoveAt(findAlarm(alarmID));
                        Console.WriteLine("NUM ALARMS: " + alarms.Count);
                        listView.RemoveAllViewsInLayout();
                        psa.NotifyDataSetChanged();
                    } else
                    {
                        Toast.MakeText(this, "Delete Failed on Database", ToastLength.Short).Show();
                    }

                };

                
            }
        }


        private int findAlarm(int alarmID)
        {
            int result = -1;

            for (int pos = 0; pos < alarms.Count; pos++)
            {
                if (alarms[pos].AlarmID == alarmID)
                {
                    result = pos;
                    //Toast.MakeText(this, "ALARM FOUND IS " + result.AlarmName, ToastLength.Short).Show();
                    break;
                }
            }

            return result;
        }

        



        //deal with the add alarm result (code 1) and edit alarm result (code 2)
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent alarmData)
        {
            base.OnActivityResult(requestCode, resultCode, alarmData);
            if (requestCode == 2 && resultCode == Result.Ok)
            {
                Alarm alarmEdited = JsonConvert.DeserializeObject<Alarm>(alarmData.GetStringExtra("EditedAlarm"));

                int index = -1;

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

            else if (requestCode == 1 && resultCode == Result.Ok)
            {
               

                Alarm newAlarm = JsonConvert.DeserializeObject<Alarm>(alarmData.GetStringExtra("NewAlarm"));
                //newAlarm.AlarmID = alarmIDIndex;
                //add alarm object
                alarms.Add(newAlarm);
                //update the list view
                listView.RemoveAllViewsInLayout();
                psa.NotifyDataSetChanged();
                //create the new alarm in alarm manager using alarmIdindex as request code in alarm manager

                int daysFromNow = getNumDaysToAlarm(newAlarm);

                addAlarm(newAlarm.AlarmID, newAlarm.AlarmTime, daysFromNow);
                //alarmIDIndex++;
            }
        }

        private int getNumDaysToAlarm(Alarm alarm)
        {
            //0 indicates the alarm is set for today or not repeating
            int daysToGo = 8;

            if (alarm.AlarmDays[0] != 0)
            {
                //get today's day
                Calendar now = Calendar.GetInstance(Java.Util.TimeZone.Default);
                int today = now.Get(CalendarField.DayOfWeek);

                Calendar alarmT = Calendar.GetInstance(Java.Util.TimeZone.Default);
                alarmT.Set(CalendarField.HourOfDay, alarm.AlarmTime.Hours);
                alarmT.Set(CalendarField.Minute, alarm.AlarmTime.Minutes);
                alarmT.Set(CalendarField.Second, 0);

                if (alarm.AlarmDays.Contains(today))
                {
                    if (alarmT.Before(now))
                    {
                        daysToGo = 7;
                    } else
                    {
                        daysToGo = 0;
                    }
                }

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

        public void turnAlarmOn(int alarmID)
        {
            Toast.MakeText(Application.Context, "Alarm " + alarmID + " ON", ToastLength.Short).Show();

            if (NetworkInterface.GetIsNetworkAvailable())
            {
                au.edu.wa.central.mydesign.student.Service1 client = new au.edu.wa.central.mydesign.student.Service1();
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

                            //Toast.MakeText(this, "ALARM FOUND IS " + result.AlarmName, ToastLength.Short).Show();
                            break;
                        }
                    }
                };
            }
            
        }


        public void turnAlarmOff(int alarmID)
        {
            Toast.MakeText(Application.Context, "Alarm " + alarmID + " OFF", ToastLength.Short).Show();

            if (NetworkInterface.GetIsNetworkAvailable())
            {
                Service1 client = new au.edu.wa.central.mydesign.student.Service1();
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

                            //Toast.MakeText(this, "ALARM FOUND IS " + result.AlarmName, ToastLength.Short).Show();
                            break;
                        }
                    }
                    deleteFromAlarmMgr(alarmID);
                };
            }
            
        }

        private void deleteFromAlarmMgr(int code)
        {
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

        private void addAlarm(int code, TimeSpan alarmTime, int daysFromNow)
        {
            Alarm alarmToSet= alarms[findAlarm(code)];

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
            //if alarm days have been set and the alarm time is before now and the days are 0 then it must be a week from now
            //else if (alarm.Before(now) && daysFromNow == 0 && alarmToSet.AlarmDays.Count > 0)
            //{
            //    alarmMillis += 7 * 86400000L;
            //}

            // mgr.Set(AlarmType.ElapsedRealtime, SystemClock.ElapsedRealtime() + 5 * 1000, pendingIntent);
            //mgr.Set(AlarmType.RtcWakeup, Calendar.GetInstance(Java.Util.TimeZone.Default).TimeInMillis + (code + 1) * 10 * 1000, pendingIntent);
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
            
            TimeSpan currentOffset = System.TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now);
            var time = TimeSpan.FromMilliseconds(alarmMillis);
            Toast.MakeText(this, (new DateTime(1970, 1, 1) + time + currentOffset).ToString(), ToastLength.Short).Show();

        }
        private void createAlarms()
        {
            foreach (Alarm alarm in alarms)
            {
                //only add alarms that are active
                if (alarm.AlarmActive)
                {
                    addAlarm(alarm.AlarmID, alarm.AlarmTime, getNumDaysToAlarm(alarm));
                }
            }

            //View v;

            //for (int i = 0; i < listView.Count; i++)
            //{
            //    v = listView.Adapter.GetView(i, null, null);
            //    TextView tv = v.FindViewById<TextView>(Resource.Id.Text1);
            //    Toast.MakeText(this, tv.Text + " created", ToastLength.Short).Show();

            //    //create the alarm

            //    addAlarm(i);
            //}

            
        }
                

        private void OnListItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            var listView = sender as ListView;
            var alarm = listView.GetItemAtPosition(e.Position).Cast<Alarm>();           

            //Alarm alarm = alarms[e.Position];
            Toast.MakeText(this, "AlarmID: " + (alarm.AlarmID).ToString(), ToastLength.Short).Show();

            //get the alarm object and serialize it so we can pass it to the edit alarm activity
            Intent i = new Intent(this, typeof(EditAlarm));           
            i.PutExtra("Alarm", JsonConvert.SerializeObject(alarm));
            StartActivityForResult(i, 2);
        }

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

        private void loadAlarms()
        {
            alarms = new List<Alarm>();

            

            if (NetworkInterface.GetIsNetworkAvailable())
            {
                Service1 client = new Service1();
                client.ListAlarmsAsync(username);

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

                        alarms.Add(alarm);
                    }

                    

                    //check if AlarmReceiver called main activity
                    int alarmIDTurnedOff = Intent.GetIntExtra("AlarmJustStopped", -1);

                    if (alarmIDTurnedOff != -1)
                    {
                        //must be the AlarmReceiver calling
                        Toast.MakeText(this, "Alarm Rx Just turned off Alarm: " + alarmIDTurnedOff, ToastLength.Short).Show();
                        //TODO set the next alarm for repeating alarms
                        Alarm alarmToUpdate = alarms[findAlarm(alarmIDTurnedOff)];
                        //TODO turn off any one-off alarms
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
                    psa = new PeopleScreenAdapter(this, alarms);
                    listView.Adapter = psa;
                    //set a click handler for when an list item is clicked (edit alarm)
                    listView.ItemClick += OnListItemClick;

                    //long click will give user option to delete alarm
                    listView.ItemLongClick += ListView_ItemLongClick;

                                     
                    
                };
            }
        }


        public override void OnBackPressed()
        {
            Toast.MakeText(this, "Back Button Pressed..", ToastLength.Short).Show();
            MoveTaskToBack(true);
        }


        protected override void OnResume()
        {
            base.OnResume();
            Toast.MakeText(this, "Resuming..", ToastLength.Short).Show();
            //get the name of the activity that called this one - i.e. check if called from AlarmReceiver
            
            // Recreate();
        }

    }

    public static class ObjectTypeHelper
    {
        public static T Cast<T>(this Java.Lang.Object obj) where T : class
        {
            var propertyInfo = obj.GetType().GetProperty("Instance");
            return propertyInfo == null ? null : propertyInfo.GetValue(obj, null) as T;
        }
    }

}

