using Avalonia.Controls;
using Avalonia.Layout;

namespace ExampleAvalonia;

public sealed class MainWindow : Window
{
    public MainWindow()
    {
        Title = "CSDeskBand Avalonia 示例";
        Width = 480;
        Height = 120;
        Content = new Border
        {
            Padding = new Thickness(12),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Child = new DeskbandControl(),
        };
    }
}
