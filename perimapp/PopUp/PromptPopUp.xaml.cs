using CommunityToolkit.Maui.Views;
using System;

namespace perimapp.PopUp
{
    public partial class PromptPopUp : Popup
    {
        public string? Result { get; private set; }

        public PromptPopUp(string title, string subtitle, string placeholder, string affirmativeText, string negativeText)
        {
            InitializeComponent();

            TitleLabel.Text = title;
            SubtitleLabel.Text = subtitle;
            InputEntry.Placeholder = placeholder;
            AffirmativeButton.Text = affirmativeText;
            NegativeButton.Text = negativeText;
        }

        private async void OnNegativeClicked(object sender, EventArgs e)
        {
            await CloseAsync();
        }

        private async void OnAffirmativeClicked(object sender, EventArgs e)
        {
            Result = InputEntry.Text?.Trim();
            await CloseAsync();
        }
    }
}