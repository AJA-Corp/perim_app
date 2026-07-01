using System;
using System.Threading.Tasks;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Maui.Extensions;
using Microsoft.Maui.Controls;
using perimapp.PopUp;

namespace perimapp.Services
{
    public class DialogService : IDialogService
    {
        public async Task ShowAlertAsync(string title, string message, string buttonText = "OK")
        {
            var popUp = new InfosPopUp(title, message, buttonText);
            await Shell.Current.CurrentPage.ShowPopupAsync(popUp);
        }

        public async Task<bool> ShowConfirmAsync(string title, string message, string yesText = "Oui", string noText = "Non")
        {
            var confirmPopUp = new BoolPopUp(title, message, yesText, noText);
            await Shell.Current.CurrentPage.ShowPopupAsync(confirmPopUp);
            return confirmPopUp.Result;
        }

        public async Task<string?> ShowPromptAsync(string title, string message, string placeholder = "", string okText = "Valider", string cancelText = "Annuler")
        {
            var promptPopUp = new PromptPopUp(title, message, placeholder, okText, cancelText);
            await Shell.Current.CurrentPage.ShowPopupAsync(promptPopUp);
            return promptPopUp.Result;
        }

        public async Task<(string FirstName, string LastName)?> ShowEditProfileAsync(string currentFirstName, string currentLastName)
        {
            var popup = new EditProfilePopUp(currentFirstName, currentLastName);
            await Shell.Current.CurrentPage.ShowPopupAsync(popup);
            var result = popup.Result;
            if (result != null)
            {
                return (result.FirstName ?? "", result.LastName ?? "");
            }
            return null;
        }

        public async Task ShowNotificationSettingsAsync()
        {
            var popup = new NotificationPopUp();
            await Shell.Current.CurrentPage.ShowPopupAsync(popup);
        }

        public async Task<string> ShowActionSheetAsync(string title, string cancel, string destruction, params string[] buttons)
        {
            return await Shell.Current.CurrentPage.DisplayActionSheetAsync(title, cancel, destruction, buttons);
        }
    }
}
