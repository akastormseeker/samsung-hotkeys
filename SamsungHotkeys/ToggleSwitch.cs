using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace SamsungHotkeys
{
    class ToggleSwitch : ToggleButton
    {
        public static DependencyProperty ThumbWidthProperty = DependencyProperty.Register("ThumbWidth", typeof(double), typeof(ToggleSwitch), new FrameworkPropertyMetadata(32.0));

        public double ThumbWidth
        {
            get { return (double)GetValue(ThumbWidthProperty); }
            set { SetValue(ThumbWidthProperty, value); }
        }

        public ToggleSwitch()
        {

        }
    }
}
