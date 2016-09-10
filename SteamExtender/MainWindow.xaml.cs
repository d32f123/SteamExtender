﻿using System;
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
using LiveCharts.Definitions.Series;

namespace SteamExtender
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public SeriesCollection Series2 { get; set; }

        public enum SteamStatus //TODO: Перенести в отдельный класс
        {
            NaN = -1,
            Offline,
            Snooze,
            Away,
            Online,
            InGame
        }

        public string[] Labels { get; set; }

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
                chart.AxisX[0].Separator.Step = (MaximumPoint - MinimumPoint) / NumberOfSteps;
            }
        }
        private double StepWidthInPixels // ширина шага в пикселях
        {
            get
            {
                return chart.ActualWidth / NumberOfSteps;
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
                        FromDateToGraph(((chart.Series[0] as ColumnSeries).Values[0] as DateModel).DateTime);
                }
                return (double)chart.AxisX[0].MinValue;
            }
            set
            {
                if (value < 0)
                {
                    chart.AxisX[0].MinValue = DoubleConverter.
                        FromDateToGraph(((chart.Series[0] as ColumnSeries).Values[0] as DateModel).DateTime);
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
                    chart.AxisX[0].MaxValue = MinimumPoint + FullResolution;
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


        private struct StatusValues
        {
            internal SteamStatus previousAddition;

            public ChartValues<DateModel> OnlineValues { get; internal set; }
            public ChartValues<DateModel> InGameValues { get; internal set; }
            public ChartValues<DateModel> AwayValues { get; internal set; }
            public ChartValues<DateModel> SnoozeValues { get; internal set; }
            public ChartValues<DateModel> OfflineValues { get; internal set; }
            
            public void AddValue(DateTime Date, SteamStatus Status)
            {
                if (Status != previousAddition)
                {
                    switch (previousAddition)
                    {
                        case SteamStatus.InGame:
                            //InGameValues.Add(new DateModel { DateTime = Date, Value = 4 });
                            //InGameValues.Add(new DateModel { DateTime = Date, Value = Double.NaN });
                            break;
                        case SteamStatus.Online:
                            //OnlineValues.Add(new DateModel { DateTime = Date, Value = 3 });
                            //OnlineValues.Add(new DateModel { DateTime = Date, Value = Double.NaN });
                            break;
                        case SteamStatus.Away:
                            //AwayValues.Add(new DateModel { DateTime = Date, Value = 2 });
                            //AwayValues.Add(new DateModel { DateTime = Date, Value = Double.NaN });
                            break;
                        case SteamStatus.Snooze:
                            //SnoozeValues.Add(new DateModel { DateTime = Date, Value = 1 });
                            //SnoozeValues.Add(new DateModel { DateTime = Date, Value = Double.NaN });
                            break;
                        case SteamStatus.Offline:
                            //OfflineValues.Add(new DateModel { DateTime = Date, Value = 0 });
                            //OfflineValues.Add(new DateModel { DateTime = Date, Value = Double.NaN });
                            break;
                    }
                    previousAddition = Status;
                }
                InGameValues.Add(new DateModel { DateTime = Date, Value = (int)Status });
                /*switch (Status)
                {
                    case SteamStatus.InGame:
                        InGameValues.Add(new DateModel { DateTime = Date , Value = 4});
                        break;
                    case SteamStatus.Online:
                        OnlineValues.Add(new DateModel { DateTime = Date, Value = 3 });
                        break;
                    case SteamStatus.Away:
                        AwayValues.Add(new DateModel { DateTime = Date, Value = 2 });
                        break;
                    case SteamStatus.Snooze:
                        SnoozeValues.Add(new DateModel { DateTime = Date, Value = 1 });
                        break;
                    case SteamStatus.Offline:
                        OfflineValues.Add(new DateModel { DateTime = Date, Value = 0 });
                        break;
                }*/
            }
        }

        private StatusValues observableValues1 = new StatusValues
        {
            previousAddition = SteamStatus.NaN,
            OnlineValues = new ChartValues<DateModel>(),
            InGameValues = new ChartValues<DateModel>(),
            AwayValues = new ChartValues<DateModel>(),
            SnoozeValues = new ChartValues<DateModel>(),
            OfflineValues = new ChartValues<DateModel>()
        };

        private StatusValues observableValues2 = new StatusValues
        {
            previousAddition = SteamStatus.NaN,
            OnlineValues = new ChartValues<DateModel>(),
            InGameValues = new ChartValues<DateModel>(),
            AwayValues = new ChartValues<DateModel>(),
            SnoozeValues = new ChartValues<DateModel>(),
            OfflineValues = new ChartValues<DateModel>()
        };

        private Point currMousePosition { get; set; }
        

        public MainWindow()
        {
            InitializeComponent();
            MinimumPoint = 0;
            Init();
            InitArray();
            DataContext = this;
            currMousePosition = new Point(-1, -1);
            Labels = new string[]
            {
                "Offline",
                "Snooze",
                "Away",
                "Online",
                "In-Game",
            };
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
            Random rand = new Random();
            Series2 = new SeriesCollection(dayConfig) {
                 new ColumnSeries //In-game
                 {
                    Values = observableValues2.InGameValues,
                    Fill = new SolidColorBrush(Color.FromRgb(60, 83, 48)),
                    Stroke = new SolidColorBrush(Color.FromRgb(107, 186, 69)),
                    //PointGeometrySize = 15,
                    //PointForeround = new SolidColorBrush(Color.FromRgb(68, 95, 50)),
                    //AlternativeStroke = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0)),
                    StrokeThickness = 3,
                    
                 },
                 /*new ColumnSeries //Online
                 {
                     Values = observableValues2.OnlineValues,
                     Stroke = new SolidColorBrush(Color.FromRgb(83, 164, 196)),
                     Fill = new SolidColorBrush(Color.FromRgb(49, 85, 99)),
                     //PointGeometrySize = 15,
                     //PointForeround = new SolidColorBrush(Color.FromRgb(57, 95, 109)),
                     StrokeThickness = 3
                 },
                 new ColumnSeries //Away
                 {
                     Values = observableValues2.AwayValues,
                     Stroke = new SolidColorBrush(Color.FromRgb(61, 118, 141)),
                     Fill = new SolidColorBrush(Color.FromRgb(44, 83, 98)),
                     //PointGeometrySize = 15,
                     //PointForeround = new SolidColorBrush(Color.FromRgb(57, 95, 109)),
                     StrokeThickness = 3
                 },
                 new ColumnSeries //Snooze
                 {
                     Values = observableValues2.SnoozeValues,
                     Stroke = new SolidColorBrush(Color.FromRgb(61, 118, 141)),
                     Fill = new SolidColorBrush(Color.FromRgb(44, 83, 98)),
                     //PointGeometrySize = 15,
                     //PointForeround = new SolidColorBrush(Color.FromRgb(57, 95, 109)),
                     StrokeThickness = 3
                 },
                 new ColumnSeries //Offline
                 {
                     Values = observableValues2.OfflineValues,
                     Stroke = new SolidColorBrush(Color.FromRgb(106, 106, 106)),
                     Fill = new SolidColorBrush(Color.FromRgb(55, 55, 55)),
                     //PointGeometrySize = 15,
                     //PointForeround = new SolidColorBrush(Color.FromRgb(63, 63, 63)),
                     StrokeThickness = 3
                 }*/
             };
            observableValues2.AddValue(DateTime.Now, (SteamStatus)rand.Next(0, 4));
            observableValues2.AddValue(DateTime.Now, (SteamStatus)rand.Next(0, 4));
            observableValues2.AddValue(DateTime.Now, (SteamStatus)rand.Next(0, 4));
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            Random rand = new Random();
            observableValues2.AddValue(DateTime.Now, (SteamStatus)rand.Next(0, 5));
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            InitMatrix();
        }

        private void chart_MouseMove(object sender, MouseEventArgs e)
        {
            double offset;
            label.Content = "Pixels: " + StepWidthInPixels + " Seconds: " + StepWidthInSeconds;
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

        /// <summary>
        /// Логика при нажатии на одну из точек графика(открытие особенностей)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="chartPoint"></param>
        private void chart_DataClick(object sender, ChartPoint chartPoint)
        {
            var asPixels = chart.ConvertToPixels(chartPoint.AsPoint());
            MessageBox.Show("You clicked (" + chartPoint.X + ", " + chartPoint.Y + ") in pixels (" +
                                        asPixels.X + ", " + asPixels.Y + ")");
        }

        /// <summary>
        /// Возвращает индекс ближайшего по времени элемента(меньшего чем заданный в graphpt)
        /// </summary>
        /// <param name="series">Где искать</param>
        /// <param name="graphpt">Время в Graph</param>
        /// <returns></returns>
        private int findClosestFitInSeries(ISeriesView series, long ticks) 
        {
            IEnumerable<DateModel> arr = series.Values.Cast<DateModel>();
            int end = arr.Count() - 1;
            int beg = 0;
            int currMid = (end + beg) / 2;
            while (true)
            {
                long currTick = arr.ElementAt(currMid).DateTime.Ticks;
                if (currTick == ticks)
                {
                    return currMid;
                }
                else if (end - beg == 1) // максимальное приближение сейчас
                {
                    if (currTick > ticks)
                        return currMid - 1;
                    return currMid;
                }
                else if (currTick > ticks)
                {
                    end = currMid;
                    currMid = (end + beg) / 2;
                }
                else
                {
                    beg = currMid;
                    currMid = (end + beg) / 2;
                }
            }
        }
    }

    internal static class DoubleConverter
    {
        private static long k = TimeSpan.FromSeconds(5).Ticks;

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
