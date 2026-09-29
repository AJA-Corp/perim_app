using CommunityToolkit.Maui.Views;
using System;

namespace perimapp.PopUp
{
    public partial class BoolPopUp : Popup
    {
        public bool Result { get; private set; }

        public BoolPopUp(string title, string message, string affirmativeText, string negativeText)
        {
            InitializeComponent();

            TitleLabel.Text = title;
            MessageLabel.Text = message;
            AffirmativeButton.Text = affirmativeText;
            NegativeButton.Text = negativeText;

            if (title.ToLower().Contains("supprimer"))
            {
                AffirmativeButton.BackgroundColor = Colors.Red;
            }
        }

        private async void OnNegativeClicked(object sender, EventArgs e)
        {
            Result = false;
            await CloseAsync();
        }

        private async void OnAffirmativeClicked(object sender, EventArgs e)
        {
            Result = true;
            await CloseAsync();
        }
    }
}