using CommunityToolkit.Maui.Views;
using System;

namespace perimapp.PopUp
{
    public partial class EditProfilePopUp : Popup
    {
        public class ProfileResult
        {
            public string? FirstName { get; set; }
            public string? LastName { get; set; }
        }

        public ProfileResult? Result { get; private set; }

        public EditProfilePopUp(string currentFirstName, string currentLastName)
        {
            InitializeComponent();

            FirstNameEntry.Text = currentFirstName;
            LastNameEntry.Text = currentLastName;
        }

        private async void OnCancelClicked(object sender, EventArgs e)
        {
            await CloseAsync();
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            Result = new ProfileResult
            {
                FirstName = FirstNameEntry.Text,
                LastName = LastNameEntry.Text
            };

            await CloseAsync();
        }
    }
}