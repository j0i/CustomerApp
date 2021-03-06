﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using CustomerApp.Models;
using CustomerApp.Models.ServiceRequests;

namespace CustomerApp.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class paymentPage : ContentPage
    {
        static double total;

        public paymentPage()
        {
            NavigationPage.SetHasNavigationBar(this, false);
            InitializeComponent();

            confirmPayButton.IsVisible = false;

            total = RealmManager.All<User>().FirstOrDefault().contribution + RealmManager.All<User>().FirstOrDefault().tip;
        }

        /// <summary>
        /// Activates the Card.IO scanner, then swaps the visibility of the Scan Card button with that of the Confirm Payment button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        async void scanCard(object sender, EventArgs e)
        {
            DependencyService.Get<ICard>().StartRead();

            // Give the card reader time to come up
            await System.Threading.Tasks.Task.Delay(1000);

            scanCardButton.IsVisible = false;

            // Add button to confirm order if it was not already added
            confirmPayButton.Text = "Confirm Payment of " + total.ToString("C");
            confirmPayButton.IsVisible = true;

        }

        /// <summary>
        /// Becomes visible after the Card.IO scanner appears.
        /// If a valid card has been provided, allows the user to confirm their payment, then leave
        /// If no valid card has been read, swaps visibility with the Scan Card button
        /// If a valid card is read but the user denies confirmation, shows both buttons to allow a rescan or confirm the scanned card
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        async void confirmButton(object sender, EventArgs e)
        {
            if (DependencyService.Get<ICard>().ReadSuccesful())
            {
                if (await DisplayAlert("Card Details", DependencyService.Get<ICard>().GetCardName() + "\n"
                    + DependencyService.Get<ICard>().GetCardNum() + "\n"
                    + DependencyService.Get<ICard>().GetCardCvv() + "\n"
                    + "Valid expiry? " + (DependencyService.Get<ICard>().IsExpiryValid() ? "Yes" : "No"), "Confirm", "Cancel"))
                {
                    await DisplayAlert("Confimed", "Payment confirmed", "OK");

                    if(RealmManager.All<User>().FirstOrDefault().tip > 0)
                    {
                        // Send tip
                        await PostTipRequest.SendPostTipRequest(RealmManager.All<Order>().FirstOrDefault().employee_id, RealmManager.All<User>().FirstOrDefault().tip);
                    }

                    await LeavePage();
                    return;
                }
                else
                {
                    scanCardButton.IsVisible = true;
                    confirmPayButton.IsVisible = true;
                }
            }
            else
            {
                await DisplayAlert("Error", "Couldn't read card data, please try to scan your card again", "OK");
                scanCardButton.IsVisible = true;
                confirmPayButton.IsVisible = false;
            }
        }

        /// <summary>
        /// Sends notification to the waitstaff of the amount to be collected, then allows the user to leave the page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        async void payWithCashClicked(object sender, EventArgs e)
        {
            // Send notification to waitstaff
            string notificationType = "Cash transaction";
            await PostNotificationsRequest.SendNotificationRequest(notificationType, RealmManager.All<Table>().FirstOrDefault().employee_id, RealmManager.All<Table>().FirstOrDefault().tableNumberString);

            await DisplayAlert("Cash Payment", "Your server is on their way to collect your cash payment", "OK");

            await LeavePage();
        }

        /// <summary>
        /// Leave the current page. Unlocks the user's payment-in-progress attribute, canceling the persistence to this page.
        /// Prompts users if they wish to play a game of chance
        /// Will add points to their account eventually
        /// </summary>
        /// <returns></returns>
        async Task LeavePage()
        {
            // Remove previous page to prevent double payment
            if(Navigation.NavigationStack.Count() > 1)
                Navigation.RemovePage(Navigation.NavigationStack[Navigation.NavigationStack.Count() - 2]);


            if (RealmManager.All<User>().FirstOrDefault().contribution > 0) {
                double contribution = Math.Round(RealmManager.All<User>().FirstOrDefault().contribution, 2);
                // Add points
                await UserAuthenticationRequest.SendUserAuthenticationRequest(RealmManager.All<User>().FirstOrDefault().email, RealmManager.All<User>().FirstOrDefault().password); // Get most recent points

                await UpdatePointsRequest.SendUpdatePointsRequest(RealmManager.All<User>().FirstOrDefault()._id, RealmManager.All<User>().FirstOrDefault().points + (contribution * PointsPerDollar.rate)); // Add new points
                RealmManager.Write(() =>
                {
                    RealmManager.All<User>().FirstOrDefault().points += (int)(contribution * PointsPerDollar.rate);
                });


                await DisplayAlert("Points Gained!", "Hey!\n\nYou just gained " + (contribution * PointsPerDollar.rate) + " points!\n"
                    + "That makes " + RealmManager.All<User>().FirstOrDefault().points + " points available to you!", "Yay!");

                // Offer game
                if (await DisplayAlert("Game Opportunity", "Would you like to play a game for a chance at a free dessert?", "Yes, please!", "No thanks"))
                {
                    Navigation.InsertPageBefore(new gamePage(), this);
                }
                else
                {
                    Navigation.InsertPageBefore(new endPage(), this);
                }
            }
            else
                Navigation.InsertPageBefore(new endPage(), this);

            // Unlock user from payment page
            RealmManager.Write(() =>
            {
                RealmManager.All<User>().FirstOrDefault().paymentInProgress = false;
                RealmManager.All<User>().FirstOrDefault().contribution = 0;
                RealmManager.All<User>().FirstOrDefault().tip = 0;
            });

            await Navigation.PopAsync();
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

        // Prevent going back to previous pages, as the order has already been sent. Must continue and pay
        protected override bool OnBackButtonPressed()
        {
            return true;
        }
    }
}