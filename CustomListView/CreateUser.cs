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
    [Activity(Label = "Create User", WindowSoftInputMode = SoftInput.AdjustResize | SoftInput.StateHidden, ScreenOrientation = ScreenOrientation.Portrait)]
    public class CreateUser : Activity
    {
        EditText username;
        EditText password;
        EditText confirmPassword;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.CreateUser);

            Typeface font = Typeface.CreateFromAsset(Assets, "fonts/Montserrat-Bold.ttf");

            username = FindViewById<EditText>(Resource.Id.txtUsername);
            password = FindViewById<EditText>(Resource.Id.txtPassword);
            confirmPassword = FindViewById<EditText>(Resource.Id.txtConfirmPassword);

            Button create = FindViewById<Button>(Resource.Id.btnCreateUser);
            create.Typeface = font;
            create.Click += Create_Click;

            TextView createTitle = FindViewById<TextView>(Resource.Id.txtCreateTitle);
            createTitle.Typeface = font;

            TextView createLink = FindViewById<TextView>(Resource.Id.txtCreateLink);
            createLink.Typeface = font;
            createLink.Click += delegate {
                Intent intent = new Intent(this, typeof(CreateUser));
                StartActivity(intent);
            };

            TextView help = FindViewById<TextView>(Resource.Id.txtHelpLink);
            help.Typeface = font;
            help.Click += delegate {
                var uri = Android.Net.Uri.Parse("http://student.mydesign.central.wa.edu.au/041110777/bedtime/");
                var intent = new Intent(Intent.ActionView, uri);
                StartActivity(intent);
            };
        }

        private void Create_Click(object sender, EventArgs e)
        {
            hideSoftKeyboard();

            //error checking
            if (username.Text == "" || password.Text == "" || confirmPassword.Text == "")
            {
                showMsg("All Fields must be filled in");
            }

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
                    Service1 client = new Service1();
                    client.CreateUserAsync(username.Text, password.Text);

                    client.CreateUserCompleted += Client_CreateUserCompleted;
                }
            }
        }

        private void Client_CreateUserCompleted(object sender, CreateUserCompletedEventArgs e)
        {
            if (e.Result == 1)
            {
                //display toast welcome message
                Toast.MakeText(this, "Account created!", ToastLength.Long).Show();

                //if ok show the next screen
                Intent main = new Intent(this, typeof(MainActivity));
                main.PutExtra("Username", username.Text);
                StartActivity(main);
                Finish();
            }
            else
            {
                //display toast error message
                Toast.MakeText(this, "Couldn't create account. Please try again..", ToastLength.Long).Show();
            }
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

        private void hideSoftKeyboard()
        {
            InputMethodManager imm = (InputMethodManager)GetSystemService(Context.InputMethodService);
            imm.HideSoftInputFromWindow(confirmPassword.WindowToken, 0);
        }
    }
}