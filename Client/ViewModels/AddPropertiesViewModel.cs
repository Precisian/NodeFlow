using Client.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;

namespace Client.ViewModels
{
    // WindowAddPropertiesViewModel.cs
    public class AddPropertiesViewModel
    {
        private DBManager _dbManager;

        public ICommand SaveCommand { get; }
        private ObservableCollection<PropertyItem> Properties;

        public AddPropertiesViewModel(DBManager dbManager)
        {
            this._dbManager = dbManager;
        }
    }
}