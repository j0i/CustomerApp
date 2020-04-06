﻿using CustomerApp.Models;
using CustomerApp.Pages;
using System;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace CustomerApp
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            //if an order exists go to YourOrderPage
            //else no order but user logged on, OrderHerePage
            //else LoginPage
            if(!RealmManager.All<Order>().Count().Equals(0))
            {
                // If an order exists and has already been sent, default to the checkout page
                if(RealmManager.All<Order>().FirstOrDefault().send_to_kitchen)
                {
                    MainPage = new NavigationPage(new checkoutPage());

                    //gets access to navigation stack
                    INavigation navigation = Xamarin.Forms.Application.Current.MainPage.Navigation;
                    //gets current page (i.e. MainPage)
                    Page currentpage = navigation.NavigationStack.ElementAt(navigation.NavigationStack.Count - 1);
                }
                else
                {
                    MainPage = new NavigationPage(new YourOrderPage());

                    //gets access to navigation stack
                    INavigation navigation = Xamarin.Forms.Application.Current.MainPage.Navigation;
                    //gets current page (i.e. MainPage)
                    Page currentpage = navigation.NavigationStack.ElementAt(navigation.NavigationStack.Count - 1);
                    //gets new orderHerePage
                    Page orderHerePage = new orderHerePage();
                    //now have the ability to back from YourOrder to orderHerePage
                    navigation.InsertPageBefore(orderHerePage, currentpage);
                }
                
            }
            else if(!RealmManager.All<User>().Count().Equals(0))
            {
                MainPage = new NavigationPage(new orderHerePage());
            }
            else
                MainPage = new NavigationPage(new Login());
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
        }
    }
}
