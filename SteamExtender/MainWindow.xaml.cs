using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using LiveCharts;
using LiveCharts.Wpf;
using LiveCharts.Defaults;
using System.Threading;
using LiveCharts.Configurations;
using System.ComponentModel;

namespace SteamExtender
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public SeriesCollection Series { get; set; }

        public class DateModel
        {
            public System.DateTime DateTime { get; set; }
            public double Value { get; set; }
        }

        public Func<double, string> XFormatter { get; set; }
        public Func<double, string> YFormatter { get; set; }

        private double resolutionInSeconds;
        private const double Div = 10; // коэффициент отдаления
        private const double VisibleRange = 10; // количество видимых шагов
        private const double StepInPixels = 40; // размер шага(Res.InSeconds) в пикселах
        private double ResolutionInSeconds // размер одного шага
        {
            get { return resolutionInSeconds; }
            set
            {
                if (value < 1)
                {
                    resolutionInSeconds = 1;
                    return;
                }
                resolutionInSeconds = value;
            }
        }
        private double ResolutionInTicks //размер шага в тиках
        {
            get
            {
                return ResolutionInSeconds * TimeSpan.FromSeconds(1).Ticks;
            }
        }
        private double StepsAvailable
        {
            get
            {
                return chart.ActualWidth / StepInPixels;
            }
        }

        public LiveCharts.Configurations.CartesianMapper<DateModel> dayConfig;

        private ChartValues<MainWindow.DateModel> observableValues;

        private Point currMousePosition { get; set; }
        private double SizeOfDiff {
            get
            {
                return StepInPixels / ResolutionInSeconds;
            }
        }
        private double MinimumPoint
        {
            get
            {
                if (chart.Series == null)
                    return 0;
                if (((StepLineSeries)(chart.Series[0])).Values.Count == 0)
                {
                    return 0;
                }
                return (double)(chart.AxisX[0].MinValue = DateToDoubleTicks(((DateModel)((StepLineSeries)(chart.Series[0])).Values[0]).DateTime));
            }
            set
            {
                if (value < 0)
                {
                    chart.AxisX[0].MinValue = 0;
                    return;
                }
                chart.AxisX[0].MinValue = value;
            }
        }
        private double MaximumPoint
        {
            get
            {
                if (chart.Series == null)
                    return MinimumPoint + ResolutionInTicks;
                if (((StepLineSeries)(chart.Series[0])).Values.Count == 0)
                {
                    return MinimumPoint + ResolutionInTicks;
                }
                return (double)(chart.AxisX[0].MaxValue = MinimumPoint + StepsAvailable);
            }
            set
            {
                if (value < 0)
                {
                    chart.AxisX[0].MaxValue = MinimumPoint + ResolutionInTicks;
                    return;
                }
                chart.AxisX[0].MaxValue = value;
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            MinimumPoint = 0;
            Init();
            InitArray();
            DataContext = this;
            currMousePosition = new Point(-1, -1);
            ResolutionInSeconds = 1;
            
        }

        private void InitMatrix()
        {
            double temp = MinimumPoint;
            MaximumPoint = -1;
        }

        private void Init()
        {
            dayConfig = Mappers.Xy<DateModel>()
               .X(dayModel => DateToDoubleTicks(dayModel.DateTime))
               .Y(dayModel => dayModel.Value);
            //and the formatter
            XFormatter = (value => TicksToStringFormatter(value));
        }

        private void InitArray()
        {
            
             Thread.Sleep(1000);
             observableValues = new ChartValues<DateModel>
             {
                 new DateModel{ DateTime=System.DateTime.Now, Value = 5 }
             };
             Thread.Sleep(1000);
             observableValues.Add(new DateModel { DateTime = System.DateTime.Now, Value = 6 });
             Thread.Sleep(3000);
             observableValues.Add(new DateModel { DateTime = System.DateTime.Now, Value = 1 });
             Series = new SeriesCollection(dayConfig) { new StepLineSeries { Values = observableValues } };
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            //observableValues.Add(new DateModel { DateTime = System.DateTime.Now, Value = 3 });
            
            
            InitMatrix();
        }

        private void chart_MouseMove(object sender, MouseEventArgs e)
        {
            double offset;
            if (e.LeftButton == MouseButtonState.Pressed || e.RightButton == MouseButtonState.Pressed)
            {
                if (currMousePosition.X == -1 || currMousePosition.Y == -1)
                {
                    currMousePosition = new Point(e.GetPosition(null).X, e.GetPosition(null).Y);
                    return;
                }
                offset = e.GetPosition(null).X - currMousePosition.X;
                double min, max;
                if (!chart.AxisX[0].MinValue.HasValue)
                {
                    MinimumPoint = DateToDoubleTicks(((DateModel)((StepLineSeries)(chart.Series[0])).Values[0]).DateTime);
                }
                if (!chart.AxisX[0].MaxValue.HasValue)
                {
                    MaximumPoint = MinimumPoint + StepsAvailable * ResolutionInTicks;
                }
                min = MinimumPoint;
                max = MaximumPoint;
                button.Content = "offset: " + offset + " size: " + SizeOfDiff;
                // offset > 0 => move left
                    chart.AxisX[0].MinValue = min - (offset * SizeOfDiff) * ResolutionInTicks;
                    chart.AxisX[0].MaxValue = max - (offset * SizeOfDiff) * ResolutionInTicks;
                currMousePosition = new Point(e.GetPosition(null).X, e.GetPosition(null).Y);
            }
        }

        private double DateToDoubleTicks(DateTime dt)
        {
            return (double)dt.Ticks / TimeSpan.FromSeconds(1).Ticks;
        }

        private string TicksToStringFormatter(double value)
        {
            return new DateTime((long)(value * TimeSpan.FromSeconds(1).Ticks)).ToString("HH:mm:ss");
        }

        private void chart_MouseDown(object sender, MouseButtonEventArgs e)
        {
            currMousePosition = new Point(e.GetPosition(null).X, e.GetPosition(null).Y);
        }

        private void chart_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                double min = MinimumPoint;
                double max = MaximumPoint;
                double diff = max - min;
                double center = (max - min) / 2;
                diff /= Div;
                min = center - diff / 2;
                max = center + diff / 2;
            }

            // If the mouse wheel delta is negative, move the box down.
            if (e.Delta < 0)
            {
                double min = MinimumPoint;
                double max = MaximumPoint;
                double diff = max - min;
                double center = (max - min) / 2;
                diff *= Div;
                min = center - diff / 2;
                max = center + diff / 2;
            }
        }
    }
}
