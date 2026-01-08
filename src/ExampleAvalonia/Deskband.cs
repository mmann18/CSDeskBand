using System;
using System.Runtime.InteropServices;
using Avalonia.Controls;
using CSDeskBand;

namespace ExampleAvalonia;

[ComVisible(true)]
[Guid("7ECA4E59-8F9D-4599-8C80-7F12AAFB4091")]
[CSDeskBandRegistration(Name = "Sample Avalonia", ShowDeskBand = false)]
public sealed class Deskband : CSDeskBandAvalonia
{
    public Deskband()
    {
        Options.MinHorizontalSize = new DeskBandSize(220, 32);
        Options.MinVerticalSize = new DeskBandSize(32, 220);
    }

    protected override Control Control => new DeskbandControl();
}
