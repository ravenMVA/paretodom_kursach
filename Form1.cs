using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace ParetoDomination
{
    public partial class Form1 : Form
    {
        // Объявление экземпляра класса Random для генерации случайных чисел
        private Random random = new Random();
        // Список решений (маршрутов)
        private List<Solution> solutions = new List<Solution>();
        // Список городов, через которые проходят маршруты
        private List<City> cities = new List<City>
        {
            // Инициализация списка городов с заданными координатами
            new City { X = 10, Y = 20 },
            new City { X = 30, Y = 50 },
            new City { X = 50, Y = 30 },
            new City { X = 70, Y = 10 },
            new City { X = 90, Y = 70 }
        };

        // Конструктор формы
        public Form1()
        {
            InitializeComponent();
            InitializeChart();
        }

        // Инициализация элемента управления Chart
        private void InitializeChart()
        {
            // Очистка серий и областей чарта
            chart1.Series.Clear();
            chart1.ChartAreas.Clear();

            // Добавление новой области чарта
            ChartArea chartArea = new ChartArea();
            chart1.ChartAreas.Add(chartArea);
            // Установка минимальных и максимальных значений осей X и Y
            chart1.ChartAreas[0].AxisX.Minimum = 0;
            chart1.ChartAreas[0].AxisX.Maximum = 200;
            chart1.ChartAreas[0].AxisY.Minimum = 0;
            chart1.ChartAreas[0].AxisY.Maximum = 200;

            // Добавление серий для отображения решений и фронта Парето
            chart1.Series.Add("Solutions");
            chart1.Series["Solutions"].ChartType = SeriesChartType.Point;
            chart1.Series["Solutions"].MarkerStyle = MarkerStyle.Circle;
            chart1.Series["Solutions"].MarkerSize = 5;
            chart1.Series["Solutions"].MarkerColor = Color.Blue;

            chart1.Series.Add("ParetoFront");
            chart1.Series["ParetoFront"].ChartType = SeriesChartType.Point;
            chart1.Series["ParetoFront"].MarkerStyle = MarkerStyle.Diamond;
            chart1.Series["ParetoFront"].MarkerSize = 7;
            chart1.Series["ParetoFront"].MarkerColor = Color.Red;
        }

        // Обработчик события нажатия кнопки "Generate"
        private void btnGenerate_Click(object sender, EventArgs e)
        {
            // Генерация решений (маршрутов), определение фронта Парето и отображение результатов на чарте
            GenerateSolutions(100);
            IdentifyParetoFront();
            DisplayResults();
        }

        // Генерация решений (маршрутов)
        private void GenerateSolutions(int count)
        {
            // Очистка списка решений и генерация новых решений
            solutions.Clear();
            for (int i = 0; i < count; i++)
            {
                var route = GenerateRandomRoute();
                double totalTime = CalculateTotalTime(route);
                double totalCost = CalculateTotalCost(route);
                solutions.Add(new Solution(route, totalTime, totalCost));
            }
        }

        // Генерация случайного маршрута
        private Route GenerateRandomRoute()
        {
            // Создание нового маршрута и перемешивание индексов городов
            var route = new Route();
            var cityIndices = Enumerable.Range(0, cities.Count).ToList();
            cityIndices.Shuffle(random);

            // Добавление городов в маршрут в перемешанном порядке
            foreach (int index in cityIndices)
            {
                route.Cities.Add(cities[index]);
            }

            return route;
        }

        // Расчет общего времени маршрута
        private double CalculateTotalTime(Route route)
        {
            double totalTime = 0;
            // Расчет времени между каждыми двумя городами и суммирование
            for (int i = 0; i < route.Cities.Count - 1; i++)
            {
                int dx = Math.Abs(route.Cities[i].X - route.Cities[i + 1].X);
                int dy = Math.Abs(route.Cities[i].Y - route.Cities[i + 1].Y);
                double distance = Math.Sqrt(dx * dy);
                totalTime += distance; // Линейная зависимость времени от расстояния
            }
            return totalTime;
        }

        // Расчет общей стоимости маршрута
        private double CalculateTotalCost(Route route)
        {
            double totalCost = 0;
            // Расчет стоимости между каждыми двумя городами и суммирование
            for (int i = 0; i < route.Cities.Count - 1; i++)
            {
                int dx = Math.Abs(route.Cities[i].X - route.Cities[i + 1].X);
                int dy = Math.Abs(route.Cities[i].Y - route.Cities[i + 1].Y);
                double distance = Math.Sqrt(dx * dy);
                totalCost += distance; // Линейная зависимость стоимости от расстояния
            }
            return totalCost;
        }

        // Определение фронта Парето
        private void IdentifyParetoFront()
        {
            // Создание списка решений, находящихся на фронте Парето, и проверка каждого решения на доминирование
            List<Solution> paretoFront = new List<Solution>();
            foreach (Solution solution in solutions)
            {
                if (!IsDominated(solution))
                {
                    paretoFront.Add(solution);
                }
            }

            // Установка флага IsParetoOptimal для решений на фронте Парето
            foreach (Solution paretoSolution in paretoFront)
            {
                paretoSolution.IsParetoOptimal = true;
            }
        }

        // Проверка решения на доминирование другими решениями
        private bool IsDominated(Solution solution)
        {
            // Проверка каждого решения из списка solutions на доминирование текущего решения
            foreach (Solution otherSolution in solutions)
            {
                if (otherSolution != solution && otherSolution.Dominates(solution))
                {
                    return true;
                }
            }
            return false;
        }

        // Отображение результатов на чарте
        private void DisplayResults()
        {
            // Очистка серий и добавление точек для решений и фронта Парето
            chart1.Series["Solutions"].Points.Clear();
            chart1.Series["ParetoFront"].Points.Clear();

            foreach (Solution solution in solutions)
            {
                chart1.Series["Solutions"].Points.AddXY(solution.TotalTime, solution.TotalCost);

                if (solution.IsParetoOptimal)
                {
                    chart1.Series["ParetoFront"].Points.AddXY(solution.TotalTime, solution.TotalCost);
                }
            }
        }
    }

    // Класс, представляющий город
    public class City
    {
        // Координаты города
        public int X { get; set; }
        public int Y { get; set; }
    }

    // Класс, представляющий маршрут
    public class Route
    {
        // Список городов в маршруте
        public List<City> Cities { get; set; } = new List<City>();
        // Общее время и стоимость маршрута
        public double TotalTime { get; set; }
        public double TotalCost { get; set; }
    }

    // Класс, представляющее решение (маршрут)
    public class Solution
    {
        // Маршрут, время и стоимость решения, а также флаг, указывающий, находится ли решение на фронте Парето
        public Route Route { get; set; }
        public double TotalTime { get; set; }
        public double TotalCost { get; set; }
        public bool IsParetoOptimal { get; set; }

        // Конструктор класса Solution
        public Solution(Route route, double totalTime, double totalCost)
        {
            Route = route;
            TotalTime = totalTime;
            TotalCost = totalCost;
            IsParetoOptimal = false;
        }

        // Проверка доминирования текущим решением другого решения
        public bool Dominates(Solution otherSolution)
        {
            if (TotalTime < otherSolution.TotalTime && TotalCost < otherSolution.TotalCost)
            {
                return true;
            }
            return false;
        }
    }

    // Статический класс с расширениями для IList<T>
    static class Extensions
    {
        // Метод расширения для перемешивания элементов списка
        public static void Shuffle<T>(this IList<T> list, Random rng)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}
