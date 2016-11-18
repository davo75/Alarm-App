using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Android.Graphics;
using Android.Content.PM;
using System.Net.NetworkInformation;
using Bedtime.au.edu.wa.central.mydesign.student;
using Android.Views.InputMethods;

namespace Bedtime
{
    /// <summary>
    /// Screen for creating a new account. Gets the username and password.
    /// </summary>
    /// <remarks>
    /// author: David Pyle 041110777
    /// version: 1.0
    /// date: 18/11/2016
    /// </remarks>
    /// 
    [Activity(Label = "Create User", WindowSoftInputMode = SoftInput.AdjustResize | SoftInput.StateHidden, ScreenOrientation = ScreenOrientation.Portrait)]
    public class CreateUser : Activity
    {
        //username
        EditText username;
        //password
        EditText password;
        //password confirmation
        EditText confirmPassword;

        /// <summary>
        /// Creates the UI for getting the new account details
        /// </summary>
        /// <param name="savedInstanceState"></param>
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.CreateUser);
            
            //set custom font
            Typeface font = Typeface.CreateFromAsset(Assets, "fonts/Montserrat-Bold.ttf");

            //reference the text fields
            username = FindViewById<EditText>(Resource.Id.txtUsername);
            password = FindViewById<EditText>(Resource.Id.txtPassword);
            confirmPassword = FindViewById<EditText>(Resource.Id.txtConfirmPassword);

            //reference the create account button
            Button create = FindViewById<Button>(Resource.Id.btnCreateUser);
            create.Typeface = font;
            create.Click += Create_Click;

            //main title
            TextView createTitle = FindViewById<TextView>(Resource.Id.txtCreateTitle);
            createTitle.Typeface = font;

            //link to this activity - redundant but maintains UI consistency
            TextView createLink = FindViewById<TextView>(Resource.Id.txtCreateLink);
            createLink.Typeface = font;
            createLink.Click += delegate {
                Intent intent = new Intent(this, typeof(CreateUser));
                StartActivity(intent);
            };

            //link to the online user guide
            TextView help = FindViewById<TextView>(Resource.Id.txtHelpLink);
            help.Typeface = font;
            help.Click += delegate {
                var uri = Android.Net.Uri.Parse("http://student.mydesign.central.wa.edu.au/041110777/bedtime/");
                var intent = new Intent(Intent.ActionView, uri);
                StartActivity(intent);
            };
        }

        /// <summary>
        /// Calls the web service to create the user. Do some basic error checking before making the call
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Create_Click(object sender, EventArgs e)
        {
            //hide the onscreen keyboard after the click
            hideSoftKeyboard();

            //check fields have been filled in
            if (username.Text == "" || password.Text == "" || confirmPassword.Text == "")
            {
                showMsg("All Fields must be filled in");
            }
            //check the passwords match
            else if (!password.Text.Equals(confirmPassword.Text))
            {
                //passwords do not match
                showMsg("Passwords do not match");
            }
            else
            {
                //connect to web service and create user
                if (NetworkInterface.GetIsNetworkAvailable())
                {
                    //call the web service
                    Service1 client = new Service1();
                    client.CreateUserAsync(username.Text, password.Text);
                    //when the call compeles handle the response
                    client.CreateUserCompleted += Client_CreateUserCompleted;
                }
                else
                {
                    // display toast error message if no connection
                    Toast.MakeText(this, "No internet connection", ToastLength.Long).Show();
                }
            }
        }

        /// <summary>
        /// Handles response from web service for creating user by redirecting user to the main screen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Client_CreateUserCompleted(object sender, CreateUserCompletedEventArgs e)
        {
            if (e.Result == 0)
            {
                //display toast welcome message
                Toast.MakeText(this, "Account created!", ToastLength.Short).Show();

                //if ok show the main screen and pass the username
                Intent main = new Intent(this, typeof(MainActivity));
                main.PutExtra("Username", username.Text);
                StartActivity(main);
                Finish();
            }
            //show error message if the username is already in use
            else if (e.Result == 1)
            {
                //display toast error message
                Toast.MakeText(this, "Username already taken. Please try again..", ToastLength.Long).Show();
            }
            //display error message in case user account was not created
            else
            {
                //display toast error message
                Toast.MakeText(this, "Couldn't create account. Please try again..", ToastLength.Long).Show();
            }
        }

        /// <summary>
        /// Display a message in an alert dialog
        /// </summary>
        /// <param name="msg">Message to display</param>
        private void showMsg(string msg)
        {
            AlertDialog.Builder builder = new AlertDialog.Builder(this);
            //set the dialog parameters
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
        /// Hides the onscreen keybaord
        /// </summary>
        private void hideSoftKeyboard()
        {
            InputMethodManager imm = (InputMethodManager)GetSystemService(Context.InputMethodService);
            imm.HideSoftInputFromWindow(confirmPassword.WindowToken, 0);
        }
    }
}