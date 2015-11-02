using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using RGBLedLib;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace YaTraffic
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        // RGB Led pins
        private const int RED_PIN = 5;
        private const int GREED_PIN = 6;
        private const int BLUE_PIN = 13;

        // Brushes for ellipsis
        private SolidColorBrush redBrush = new SolidColorBrush(Windows.UI.Colors.Red);
        private SolidColorBrush greenBrush = new SolidColorBrush(Windows.UI.Colors.Green);
        private SolidColorBrush blueBrush = new SolidColorBrush(Windows.UI.Colors.Blue);
        private SolidColorBrush grayBrush = new SolidColorBrush(Windows.UI.Colors.LightGray);

        private RGBLed m_rgbLed = null;
        private YaTrafficManager m_trafficManager = null;

        // GUI Timer
        private DispatcherTimer m_timer;

        public MainPage()
        {
            this.InitializeComponent();

            m_trafficManager = new YaTrafficManager();
            m_trafficManager.OnDataChanged += OnTrafficDataChanged;

            try
            {
                m_rgbLed = new RGBLed(RED_PIN, GREED_PIN, BLUE_PIN);
            }
            catch(RGBLedException rgbLedEx)
            {
                switch(rgbLedEx.ErrorType)
                {
                    case RGBLedError.E_GPIO_NOT_FOUND:
                        tbErrorMessage.Text = @"GPIO does not found on this device!";
                        break;
                    case RGBLedError.E_OPEN_PIN_ERROR:
                        tbErrorMessage.Text = @"Opening pin error!";
                        break;
                    default:
                        goto case RGBLedError.E_GPIO_NOT_FOUND;
                }
                tbErrorMessage.Visibility = Visibility.Visible;
                m_rgbLed = null;
            }

            m_timer = new DispatcherTimer();
            m_timer.Interval = TimeSpan.FromMinutes(5.0);
            m_timer.Tick += OnTimerTick;
            m_timer.Start();

            m_trafficManager.UpdateData(); // First run async
        }

        private async void OnTrafficDataChanged(object sender, TrafficEventArgs e)
        {
            await Dispatcher.RunAsync(
                Windows.UI.Core.CoreDispatcherPriority.Normal, 
                () => { UpdateTrafficData(e.Level, e.Title); }
                );
        }

        private void OnTimerTick(object sender, object e)
        {
            m_trafficManager.UpdateData();
        }

        private void UpdateTrafficData(int level, string title)
        {
            tbTrafficInfo.Text = String.Format("{0}: {1}", title, level);
            RGBLedColor ledColor = RGBLedColor.UNDEFINED;

            if ((level >= 0) && (level < 4)) ledColor = RGBLedColor.GREEN;
            else if ((level >= 4) && (level < 7)) ledColor = RGBLedColor.BLUE;
            else if (level >= 7) ledColor = RGBLedColor.RED;

            SwitchLedColor(ledColor);
        }

        private void SwitchLedColor(RGBLedColor ledColor)
        {
            switch(ledColor)
            {
                case RGBLedColor.BLUE:
                    elRGB.Fill = blueBrush;
                    break;
                case RGBLedColor.GREEN:
                    elRGB.Fill = greenBrush;
                    break;
                case RGBLedColor.RED:
                    elRGB.Fill = redBrush;
                    break;
                default:
                    elRGB.Fill = grayBrush;
                    break;
            }

            if ((m_rgbLed != null) && (m_rgbLed.LedColor != ledColor)) m_rgbLed.LedColor = ledColor;
        }

        private void OnPageUnloaded(object sender, RoutedEventArgs e)
        {
            if (m_rgbLed != null) m_rgbLed.SwitchOff();
            m_timer.Stop();
        }
    }
}
