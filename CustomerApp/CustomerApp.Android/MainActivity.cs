﻿using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

using Android.Content;
using Card.IO;

using Xamarin.Forms;

namespace CustomerApp.Droid
{
    [Activity(Label = "CustomerApp", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation, ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        
        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);

            //added for scanner capabilities
            GoogleVisionBarCodeScanner.Droid.RendererInitializer.Init();
            Plugin.CurrentActivity.CrossCurrentActivity.Current.Init(this, savedInstanceState);
            //finish google plugin

            //added for rating pop up
            Rg.Plugins.Popup.Popup.Init(this, savedInstanceState);
            //finish for rating pop up

            // Necessary for Swipe view on YourOrder page
            Forms.SetFlags("SwipeView_Experimental");

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            LoadApplication(new App());
        }

        // Added for Card.IO credit card scanner
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if (data != null)
            {
                InfoSharer.Instance.Card = data.GetParcelableExtra(CardIOActivity.ExtraScanResult).JavaCast<CreditCard>();
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            //added for google plugin
            Plugin.Permissions.PermissionsImplementation.Current.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            //finish add
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}