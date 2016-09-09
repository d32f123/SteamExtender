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

        private const double Div = 1.1; // коэффициент отдаления

        private const double NumberOfSteps = 10; //количество шагов

        private double fullRes = 30;
        private double FullResolution // количество секунд от начала до конца графика
        {
            get
            {
                return fullRes;
            }
            set
            {
                if (value < NumberOfSteps)
                {
                    fullRes = NumberOfSteps;
                    
                    return;
                }
                //fullRes = value;
                //MaximumPoint = -1;
                double diffset = value - fullRes;
                fullRes = value;
                MinimumPoint = MinimumPoint - diffset / 2;
                MaximumPoint = MaximumPoint + diffset / 2;

            }
        }
        private double StepWidthInPixels // ширина шага в пикселях
        {
            get
            {
                return Width / NumberOfSteps;
            }
        }
        private double StepWidthInSeconds
        {
            get
            {
                return FullResolution / NumberOfSteps;
            }
        }
        private double StepWidthInGraphs // ширина шага в graphs
        {
            get
            {
                return StepWidthInSeconds;
            }
        }

        private double MinimumPoint
        {
            get
            {
                if (!chart.AxisX[0].MinValue.HasValue)
                {
                    chart.AxisX[0].MinValue = DoubleConverter.
                        FromDateToGraph(((chart.Series[0] as StepLineSeries).Values[0] as DateModel).DateTime);
                }
                return (double)chart.AxisX[0].MinValue;
            }
            set
            {
                if (value < 0)
                {
                    chart.AxisX[0].MinValue = DoubleConverter.
                        FromDateToGraph(((chart.Series[0] as StepLineSeries).Values[0] as DateModel).DateTime);
                    return;
                }
                chart.AxisX[0].MinValue = value;
            }
        }

        private double MaximumPoint
        {
            get
            {
                if (!chart.AxisX[0].MaxValue.HasValue)
                {
                    chart.AxisX[0].MaxValue = chart.AxisX[0].MaxValue = MinimumPoint + FullResolution;
                }
                return (double)chart.AxisX[0].MaxValue;
            }
            set
            {
                if (value < 0 || value < MinimumPoint)
                {
                    chart.AxisX[0].MaxValue = MinimumPoint + FullResolution;
                    return;
                }
                chart.AxisX[0].MaxValue = value;
            }
        }

        public LiveCharts.Configurations.CartesianMapper<DateModel> dayConfig;

        private ChartValues<MainWindow.DateModel> observableValues;

        private Point currMousePosition { get; set; }
        

        public MainWindow()
        {
            InitializeComponent();
            MinimumPoint = 0;
            Init();
            InitArray();
            DataContext = this;
            currMousePosition = new Point(-1, -1);
            
        }

        private void InitMatrix()
        {
            MinimumPoint = -1;
            MaximumPoint = -1;
        }

        private void Init()
        {
            dayConfig = Mappers.Xy<DateModel>()
               .X(dayModel => DoubleConverter.FromDateToGraph(dayModel.DateTime)) //преобразователь из DateTime в Graph
               .Y(dayModel => dayModel.Value);
            //and the formatter
            XFormatter = (value => DoubleConverter.FromGraphToString(value)); //преобразователь из Graph в строку
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
                offset = e.GetPosition(null).X - currMousePosition.X;
                //если > 0, мин валуе уменьшается
                offset = (offset / StepWidthInPixels) * StepWidthInSeconds;
                MinimumPoint = MinimumPoint - offset;
                MaximumPoint = MaximumPoint - offset;
                currMousePosition = new Point(e.GetPosition(null).X, e.GetPosition(null).Y);
            }
        }


        private void chart_MouseDown(object sender, MouseButtonEventArgs e)
        {
            currMousePosition = new Point(e.GetPosition(null).X, e.GetPosition(null).Y);
        }

        private void chart_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                FullResolution = FullResolution / Div;
            }
            else
            {
                FullResolution = FullResolution * Div;
            }
        }
    }

    internal static class DoubleConverter
    {
        private static long k = TimeSpan.FromSeconds(1).Ticks;

        static public long FromDateToTicks(DateTime date)
        {
            return date.Ticks;
        }

        static public double FromDateToGraph(DateTime date)
        {
            return date.Ticks / k;
        }

        static public double FromTicksToGraph(long ticks)
        {
            return ticks / k;
        }

        static public long FromGraphToTicks(double graph)
        {
            return (long)(graph * k);
        }

        static public DateTime FromGraphToDate(double graph)
        {
            return new DateTime((long)(graph * k));
        }

        static public string FromGraphToString(double graph)
        {
            return new DateTime((long)(graph * k)).ToString("HH:mm:ss");
        }

        static public DateTime FromTicksToDate(long ticks)
        {
            return new DateTime(ticks);
        }
    }
}
