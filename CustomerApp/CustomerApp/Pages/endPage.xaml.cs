﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CustomerApp.Models;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using CustomerApp.Models.ServiceRequests;

namespace CustomerApp.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class endPage : ContentPage
    {
        static double numUnpaid = 0;
        public endPage()
        {
            NavigationPage.SetHasNavigationBar(this, false);
            InitializeComponent();

            // Necessary for the refresh view to work
            System.Windows.Input.ICommand cmd = new Command(onRefresh);
            refresher.Command = cmd;
            updateLabel();
        }

        /// <summary>
        /// Called when at least 1 item remains unpaid after the order is updated
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        async void unpaidBalanceButton(object sender, EventArgs e)
        {
            if (numUnpaid > 0)
            {
                if (await DisplayAlert("Unpaid balance remaining", "Not all items have been paid for. Would you like to make an additional contribution?", "Yes", "No"))
                {
                    await Navigation.PushAsync(new checkoutPage());
                }
            }
        }

        /// <summary>
        /// Called when 0 items remain unpaid
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        async void logOutButton(object sender, EventArgs e)
        {
            if (numUnpaid == 0)
                if (await DisplayAlert("Log out confirmation", "Once logged out, you will not be able to request refills until a new order is started. Are you sure you want to leave the table?", "Yes", "No"))
                {
                    RealmManager.RemoveAll<MenuFoodItem>();
                    RealmManager.RemoveAll<Order>();
                    RealmManager.RemoveAll<Table>();
                    RealmManager.RemoveAll<User>();
                    await Navigation.PushAsync(new Login());
                }
        }

        // Called when button is refreshed by pulling down
        void onRefresh()
        {
            updateLabel();
            refresher.IsRefreshing = false;
        }

        async void ReturnToOrder(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new YourOrderPage());
        }

        /// <summary>
        /// Updates the text and functions of the continue button to reflect the most recent order status
        /// Called by the constructor and refreshView
        /// </summary>
        async void updateLabel()
        {
            // Pull unpaid items from server
            await GetOrderRequest.SendGetOrderRequest(RealmManager.All<Order>().FirstOrDefault()._id);

            // Check if any items are unpaid
            numUnpaid = RealmManager.All<Order>().FirstOrDefault().menuItems.Where((OrderItem o) => !o.paid).ToList().Count();
            

            // Change button based on remaining number of items
            if (numUnpaid > 1)
            {
                continueButton.Text = "Make payment towards remaining " + numUnpaid + " items";
                continueButton.Clicked -= unpaidBalanceButton;
                continueButton.Clicked -= logOutButton;
                continueButton.Clicked += unpaidBalanceButton;
                return;
            }
            else if (numUnpaid == 1)
            {
                continueButton.Text = "Make payment towards remaining item";
                continueButton.Clicked -= unpaidBalanceButton;
                continueButton.Clicked -= logOutButton;
                continueButton.Clicked += unpaidBalanceButton;
                return;
            }
            else
            {
                // Finish order
                await GetTableRequest.SendGetTableRequest(RealmManager.All<Order>().FirstOrDefault().table_number);
                await FinishOrderRequest.SendFinishOrderRequest(RealmManager.All<Table>().FirstOrDefault()._id);

                // Change button to log out of order and remove the back to order button
                continueButton.Text = "Log out of table";
                continueButton.Clicked -= unpaidBalanceButton;
                continueButton.Clicked -= logOutButton;
                continueButton.Clicked += logOutButton;

                backToOrderButton.IsVisible = false;
                return;
            }
        }

        async void OnRefillButtonClicked(object sender, EventArgs e)
        {
            // Send refill request
            string notificationType = "Refill";
            await PostNotificationsRequest.SendNotificationRequest(notificationType, RealmManager.All<Table>().FirstOrDefault().employee_id, RealmManager.All<Table>().FirstOrDefault().tableNumberString);
            await DisplayAlert("Refill", "Server Notified of Refill Request", "OK");
        }

        async void OnServerButtonClicked(object sender, EventArgs e)
        {
            // Send Help Request
            string notificationType = "Help requested";
            await PostNotificationsRequest.SendNotificationRequest(notificationType, RealmManager.All<Table>().FirstOrDefault().employee_id, RealmManager.All<Table>().FirstOrDefault().tableNumberString);
            await DisplayAlert("Help Request", "Server Notified of Help Request", "OK");
        }

        // Prevent going back to previous pages, as the order has already been sent. Must start new order via button
        protected override bool OnBackButtonPressed()
        {
            return true;
        }
    }
}