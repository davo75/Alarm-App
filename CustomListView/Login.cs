using System;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using System.Net.NetworkInformation;
using Bedtime.au.edu.wa.central.mydesign.student;
using Android.Content.PM;
using Android.Views.InputMethods;
using Android.Graphics;

namespace Bedtime
{
    /// <summary>
    /// Login screen for the app.
    /// </summary>
    /// <remarks>
    /// author: David Pyle 041110777
    /// version: 1.0
    /// date: 18/11/2016
    /// </remarks>
    
    [Activity(Label = "Bedtime Alarm", MainLauncher = true, Icon = "@drawable/ic_launcher", WindowSoftInputMode = SoftInput.AdjustResize | SoftInput.StateHidden, ScreenOrientation = ScreenOrientation.Portrait)]
    public class Login : Activity
    {
        /// <summary>
        /// Username
        /// </summary>
        private EditText theUsername;
        /// <summary>
        /// Password
        /// </summary>
        private EditText thePassword;

        /// <summary>
        /// Gets the username and password and authenticates them against the database
        /// </summary>
        /// <param name="bundle"></param>
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            
            // Create your application here
            SetContentView(Resource.Layout.Login);

            //set font
            Typeface font = Typeface.CreateFromAsset(Assets, "fonts/Montserrat-Bold.ttf");

            //get login button
            Button login = FindViewById<Button>(Resource.Id.btnLogin);
            login.Typeface = font;
            login.Click += login_Click;

            //link to create an account
            TextView create = FindViewById<TextView>(Resource.Id.txtCreateLink);
            create.Typeface = font;
            create.Click += delegate {
                Intent intent = new Intent(this, typeof(CreateUser));
                StartActivity(intent);
            };

            //link to online help
            TextView help = FindViewById<TextView>(Resource.Id.txtHelpLink);
            help.Typeface = font;
            help.Click += delegate {
                var uri = Android.Net.Uri.Parse("http://student.mydesign.central.wa.edu.au/041110777/bedtime/");
                var intent = new Intent(Intent.ActionView, uri);
                StartActivity(intent);
            };
        }

        
        /// <summary>
        /// Check the database for correct login details
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void login_Click(object sender, EventArgs e)
        {
            
            // get the username and password
            theUsername = FindViewById<EditText>(Resource.Id.editUsername);
            thePassword = FindViewById<EditText>(Resource.Id.editPassword);

            //hide the keyboard
            hideSoftKeyboard();

            //call the webservice
            if (NetworkInterface.GetIsNetworkAvailable())
            {
                Service1 client = new Service1();
                client.CheckPasswordAsync(theUsername.Text, thePassword.Text);

                client.CheckPasswordCompleted += Client_CheckPasswordCompleted;
            } else
            {
                // display toast error message
                Toast.MakeText(this, "No internet connection", ToastLength.Long).Show();
            }

        }

        /// <summary>
        /// Checks the result from the database. If ok the main alarm screen is displayed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Client_CheckPasswordCompleted(object sender, CheckPasswordCompletedEventArgs e)
        {
            //if user login ok
            if (e.Result == 1)
            {
                //display toast welcome message
                Toast.MakeText(this, "Welcome " + theUsername.Text, ToastLength.Long).Show();

                //if ok show the next screen
                var viewIntent = new Intent(this, typeof(MainActivity));
                viewIntent.PutExtra("Username", theUsername.Text);
                StartActivity(viewIntent);
                Finish();
            }
            else
            {
                //display toast error message
                Toast.MakeText(this, "Please try again..", ToastLength.Long).Show();
            }
        }

        /// <summary>
        /// Hides the onscreen keyboard
        /// </summary>
        private void hideSoftKeyboard()
        {
            InputMethodManager imm = (InputMethodManager)GetSystemService(Context.InputMethodService);
            imm.HideSoftInputFromWindow(thePassword.WindowToken, 0);
        }
    }
}

