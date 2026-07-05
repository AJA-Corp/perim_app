using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace perimapp.Services
{
    public interface INavigationService
    {
        Task GoToAsync(string state);
        Task PopModalAsync();
        Task PushModalAsync(Page page);
    }
}
