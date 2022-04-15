using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using ReactiveUI;

namespace AvaloniaFixedWrapPanelExample.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public const int MaxItemsPerLine = 12;
        public const int MinItemsPerLine = 2;
        
        public MainWindowViewModel()
        {
            Items = new ObservableCollection<string>();
            Enumerable.Range(1, 24).ToList().ForEach(i => Items.Add($"Item {i}"));
            ItemsPerLine = 5;
            
            MoreCommand = ReactiveCommand.Create(() =>
            {
                int value = ItemsPerLine + 1;
                ItemsPerLine = value > MaxItemsPerLine ? MaxItemsPerLine : value;
            });
            LessCommand = ReactiveCommand.Create(() =>
            {
                int value = ItemsPerLine - 1;
                ItemsPerLine = value < MinItemsPerLine ? MinItemsPerLine : value;
            });
        }
        
        public ICommand MoreCommand { get; }

        public ICommand LessCommand { get; }
        
        public int ItemsPerLine
        {
            get => _itemsPerLine;
            set => this.RaiseAndSetIfChanged(ref _itemsPerLine, value);
        }

        public ObservableCollection<string> Items { get; }
        
        private int _itemsPerLine;
    }
}