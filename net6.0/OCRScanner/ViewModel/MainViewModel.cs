

using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace OCRScanner.ViewModel;

public partial class MainViewModel : ObservableObject
{

    public MainViewModel()
    {
        Items = new ObservableCollection<ImageModel>();
    }
    [ObservableProperty]
    ObservableCollection<ImageModel> items;

}
