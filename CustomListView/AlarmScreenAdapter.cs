using System;
using System.Collections.Generic;

using Android.App;
using Android.Views;
using Android.Widget;

namespace Bedtime
{
    /// <summary>
    /// Adapater for the list view of alarms. Custom layout used for displaying the alarm data.
    /// </summary>
    /// <remarks>
    /// author: David Pyle 041110777
    /// version: 1.0
    /// date: 18/11/2016
    /// </remarks>
    
    class AlarmScreenAdapter : BaseAdapter<Alarm>
    {
        //list of alarms
        private List<Alarm> items;
        //context for the list view
        private Activity context;

        /// <summary>
        /// Constructor inheritd from base adapter
        /// </summary>
        /// <param name="context"></param>
        /// <param name="items"></param>
        public AlarmScreenAdapter(Activity context, List<Alarm> items)
            : base()
        {
            this.context = context;
            this.items = items;
        }

        /// <summary>
        /// Gets the position of the alarm item
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public override Alarm this[int position]
        {
            get
            {
                return items[position];
            }
        }

        /// <summary>
        /// Gets the number of alarm items
        /// </summary>
        public override int Count
        {
            get
            {
                return items.Count;
            }
        }

        /// <summary>
        /// Gets the item id
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public override long GetItemId(int position)
        {
            return position;
        }

        /// <summary>
        /// Uses a custom layout to display the alarm details. Returns this view for each alarm resulting in a list of alarms.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="convertView"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            //get the item
            var item = items[position];
            View view = convertView;
            
            //get the custom layout and inflate it
            if (view == null)
            {
                view = context.LayoutInflater.Inflate(Resource.Layout.custom, null);
            }

            //alarm name
            string alarmLabel = item.AlarmName.ToString();

            //if the alarm name is really long (>17 chars) then truncate it
            if (alarmLabel.Length > 17)
            {
                alarmLabel = alarmLabel.Substring(0, 16);
                alarmLabel += "...";
            }

            //set the alarm name and time in the view
            view.FindViewById<TextView>(Resource.Id.txtAlarmName).Text = alarmLabel;
            view.FindViewById<TextView>(Resource.Id.txtAlarmTime).Text = item.AlarmTime.ToString(@"hh\:mm");

            //format the alarm days from integers to days of the week
            var days = "";
            if (item.AlarmDays.Count > 0 && item.AlarmDays[0] != 0)
            { 
                days = getDays(item.AlarmDays);
            }

            //display the repeating days for the alarm
            view.FindViewById<TextView>(Resource.Id.txtAlarmDays).Text = days;
            //display the alarm on/off toggle
            Switch alarmSwitch = view.FindViewById<Switch>(Resource.Id.alarmActive);
            alarmSwitch.Checked = item.AlarmActive;

            //handle when the toggle is turned on or off
            alarmSwitch.Click += (object sender, EventArgs e) =>
            {
                Switch sw = (Switch)sender;
                //if the toggle is on then turn the alarm on
                if (sw.Checked)
                {
                    ((MainActivity)context).turnAlarmOn(item.AlarmID);
                }
                else //turn the alarm off
                {
                    ((MainActivity)context).turnAlarmOff(item.AlarmID);
                }
            };
                
                return view;
        }

        /// <summary>
        /// Returns an abbreviated day name from int value
        /// </summary>
        /// <param name="dayValues"></param>
        /// <returns></returns>
        private string getDays(List<int> dayValues)
        {
            string days = "";

            foreach (var item in dayValues)
            {
                switch (item)
                {

                    case 1:
                        days += "SUN";
                        break;
                    case 2:
                        days += "MON";
                        break;
                    case 3:
                        days += "TUE";
                        break;
                    case 4:
                        days += "WED";
                        break;
                    case 5:
                        days += "THU";
                        break;
                    case 6:
                        days += "FRI";
                        break;
                    case 7:
                        days += "SAT";
                        break;
                }
                days += " ";
            }
            return days;
        }


    }
}