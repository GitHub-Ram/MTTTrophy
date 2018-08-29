using Android.App;
using Android.Widget;
using Android.OS;
using Android;
using Android.Content.PM;
using Android.Support.V4.App;
using System;
using Android.Content;

namespace MTTrophy
{
    [Activity(Label = "MTTrophy", MainLauncher = true, Icon = "@mipmap/icon")]
    public class MainActivity : Activity
    {
        int count = 1;
        Handler handler = new Handler();

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            // Get our button from the layout resource,
            // and attach an event to it
            Button button = FindViewById<Button>(Resource.Id.myButton);

            button.Click += delegate { button.Text = $"{count++} clicks!"; };
            GetWritePermission(string.Empty);
        }


        #region Write permission
        readonly string[] permissions =
        {
            Manifest.Permission.ReadPhoneState,
            Manifest.Permission.WriteExternalStorage,
            Manifest.Permission.Camera
        };


        void GetWritePermission(string msg)
        {
            try
            {
                if (string.IsNullOrEmpty(msg))
                    msg = "App requires additional permissions to operate smoothly. Kindly click on ‘Allow’ in next screen to give permissions. App will not collect any personal data without your consent.";
                System.Collections.Generic.List<string> listPermissionsNeeded = new System.Collections.Generic.List<string>();
                Permission result;
                foreach (string p in permissions)
                {
                    result = ActivityCompat.CheckSelfPermission(this, p);
                    if (result != (int)Permission.Granted)
                    {
                        listPermissionsNeeded.Add(p);
                    }
                }
                if ((int)Build.VERSION.SdkInt < 23 || listPermissionsNeeded.Count <= 0)
                {
                    RunSplash();

                }
                else
                {
                    AlertDialog.Builder alert = new AlertDialog.Builder(this);
                    alert.SetCancelable(false);
                    alert.SetMessage(msg);
                    alert.SetPositiveButton("Continue", (senderAlert, args) =>
                    {
                        ActivityCompat.RequestPermissions(this, listPermissionsNeeded.ToArray(), 100);
                    });
                    alert.SetNegativeButton("Exit", (senderAlert, args) =>
                    {
                        ExitApp();
                    });

                    RunOnUiThread(() =>
                    {
                        alert.Show();
                    });
                    // ActivityCompat.RequestPermissions(this, permissionStorage, 100);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetWritePermission:"+ex.ToString());
            }

        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            bool Allowallpermission = false;
            switch (requestCode)
            {
                case 100:
                    foreach (Permission perm in grantResults)
                    {
                        if (perm == Permission.Granted)
                            Allowallpermission = true;
                        else
                        {
                            Allowallpermission = false;
                            GetWritePermission("App can not proceed until the requisite permissions are given. Kindly ‘Allow’ all permissions for uninterrupted game play.");
                            break;
                        }
                    }
                    if (Allowallpermission)
                    {
                        Console.WriteLine("OnRequestPermissionsResult All pemissions granted. Starting splash SPLASH");
                        RunSplash();
                    }
                    break;
                default:
                    RunSplash();
                    break;

            }
        }
        #endregion

        void RunSplash()
        {
            try
            {
                handler.PostDelayed(() =>
                {
                    this.RunOnUiThread(() =>
                    {
                        this.StartActivity(typeof(TrophyList));
                        Finish();
                    });
                }, 1000);

            }
            catch { }
        }

        private void ExitApp()
        {
            System.GC.Collect(GC.MaxGeneration);
            Java.Lang.Runtime.GetRuntime().Gc();
            //System.Environment.Exit(o);
            Android.OS.Process.KillProcess(Android.OS.Process.MyPid());
            Intent i = new Intent(Intent.ActionMain);
            i.AddCategory(Intent.CategoryHome);
            i.AddCategory(Intent.CategoryLauncher);
            i.AddFlags(ActivityFlags.NewTask);
            i.AddFlags(ActivityFlags.SingleTop);
            i.AddFlags(ActivityFlags.ClearTop);
            i.PutExtra("EXIT", true);
            this.StartActivity(i);
            this.Finish();

        }

    }
}

