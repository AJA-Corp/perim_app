using CommunityToolkit.Maui.Views;
using System;

namespace perimapp.PopUp
{
    public partial class InfosPopUp : Popup
    {
        public InfosPopUp(string title, string message, string buttonText = "OK")
        {
            InitializeComponent();

            TitleLabel.Text = title;
            MessageLabel.Text = message;
            OkButton.Text = buttonText;
        }

        private async void OnOkClicked(object sender, EventArgs e)
        {
            await CloseAsync();
        }
    }
}