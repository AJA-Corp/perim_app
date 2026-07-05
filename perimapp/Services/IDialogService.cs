using System.Threading.Tasks;

namespace perimapp.Services
{
    public interface IDialogService
    {
        Task ShowAlertAsync(string title, string message, string buttonText = "OK");
        Task<bool> ShowConfirmAsync(string title, string message, string yesText = "Oui", string noText = "Non");
        Task<string?> ShowPromptAsync(string title, string message, string placeholder = "", string okText = "Valider", string cancelText = "Annuler");
        Task<(string FirstName, string LastName)?> ShowEditProfileAsync(string currentFirstName, string currentLastName);
        Task ShowNotificationSettingsAsync();
        Task<string> ShowActionSheetAsync(string title, string cancel, string destruction, params string[] buttons);
    }
}
