using System.Collections.ObjectModel;
using perimapp.Models; 

namespace perimapp.Data
{
    public static class AppData
    {
        // Cette collection observable stockera les produits qui sont chargés par la MainPage.
        // C'est elle qui sera utilisée par ProfilePage pour obtenir le compte des produits enregistrés.
        public static ObservableCollection<ProductInfos> CurrentProducts { get; set; } = new ObservableCollection<ProductInfos>();
    }
}