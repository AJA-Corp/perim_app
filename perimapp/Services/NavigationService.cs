using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace perimapp.Services
{
    public class NavigationService : INavigationService
    {
        public Task GoToAsync(string state)
        {
            return Shell.Current.GoToAsync(state);
        }

        public Task PopModalAsync()
        {
            return Shell.Current.Navigation.PopModalAsync();
        }

        public Task PushModalAsync(Page page)
        {
            return Shell.Current.Navigation.PushModalAsync(page);
        }
    }
}
