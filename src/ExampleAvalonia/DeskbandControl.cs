using System;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace ExampleAvalonia;

public sealed class DeskbandControl : UserControl
{
    private readonly TextBlock _statusText;
    private int _clickCount;

    public DeskbandControl()
    {
        _statusText = new TextBlock
        {
            Text = "点击按钮进行计数。",
            VerticalAlignment = VerticalAlignment.Center,
            FontSize = 14,
        };

        var button = new Button
        {
            Content = "增加计数",
            Padding = new Thickness(12, 6),
            HorizontalAlignment = HorizontalAlignment.Left,
        };
        button.Click += (_, _) => UpdateCount();

        Content = new StackPanel
        {
            Spacing = 8,
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            Children =
            {
                new TextBlock
                {
                    Text = "Avalonia Deskband 示例",
                    FontWeight = FontWeight.SemiBold,
                    VerticalAlignment = VerticalAlignment.Center,
                },
                new Border
                {
                    Width = 1,
                    Height = 24,
                    Background = Brushes.Gray,
                    VerticalAlignment = VerticalAlignment.Center,
                },
                _statusText,
                button,
            },
        };
    }

    private void UpdateCount()
    {
        _clickCount++;
        _statusText.Text = $"已点击 {_clickCount} 次（{DateTime.Now:HH:mm:ss}）";
    }
}
