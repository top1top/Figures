using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Foundation.Numerics;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Microsoft.Graphics.Canvas.UI.Xaml;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Figures
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private Scene _scene;

        public MainPage()
        {
            this.InitializeComponent();

            var ch = 'A';
            foreach (var child in ControlsPanel.Children)
                ((FrameworkElement)child).DataContext = new SliderItem() { Header = (ch++).ToString() };
            _scene = new Cube3DPerspectiveStereopairScene();
        }

        private void Canvas_OnDraw(ICanvasAnimatedControl sender, CanvasAnimatedDrawEventArgs args)
        {
            _scene.Invalidate(sender.Size.Width, sender.Size.Height, args.DrawingSession);
        }

        private void Slider_OnValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            var slider = (Slider)sender;
            _scene.Parameters[(string)slider.Header] = e.NewValue / 180 * Math.PI;
        }

        public class SliderItem
        {
            public string Header { get; set; }
            public double Value { get; set; }
            public bool IsTimed { get; set; }
        }

        private void CheckBox_OnClick(object sender, RoutedEventArgs e)
        {
            var checkbox = (CheckBox)sender;
            var item = (SliderItem)checkbox.DataContext;
            _scene.IsTimed[item.Header] = item.IsTimed;
        }
    }
}
