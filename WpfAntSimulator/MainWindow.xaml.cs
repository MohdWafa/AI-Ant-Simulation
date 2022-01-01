
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using WpfAntSimulator.SimObjects;
using Bitmap = System.Drawing.Bitmap;
using Point = System.Drawing.Point;

namespace WpfAntSimulator
{
    public static class Globals
    {
        public static readonly int width = 1127;
        public static readonly int height = 814;

        public static readonly Color obstacleColor = Color.Brown;
        public static readonly Color foodColor = Color.Green;
        public static readonly Color antColor = Color.White;
        public static readonly Color blueTrailColor = Color.Blue;
        public static readonly Color redTrailColor = Color.Red;

        public static bool CleanRoadsFlag = false;
        public static bool BlueTrailsFlag = true;        

        public static List<ISimObject> simObjects;
        public static List<ISimObject> simStaticObjects;
        public static List<ISimObject> simFoodObjects;
        public static List<ISimObject> BlueTrailObjects;
        public static List<ISimObject> RedTrailObjects;

        public static List<ISimObject> toBeAdded;

        public static ISimObject OriginalColony; // The colony object

        public enum Direction
        {
            north,
            northeast,
            east,
            southeast,
            south,
            southwest,
            west,
            northwest,
            center
        }

        public static List<Direction> directions = new List<Direction>()
        { Direction.northwest, Direction.north, Direction.northeast, Direction.east,    // 0, 1, 2, 3
          Direction.southeast, Direction.south, Direction.southwest,                    // 4, 5, 6
          Direction.west, Direction.northwest, Direction.north,                         // 7, 8, 9
          Direction.center, Direction.center                                            // 10, 11
        };

        public static Direction NumToDir(int i)
        {
            return directions[i];
        }
        public static Direction FlipDirection(Direction dir)
        {
            switch (dir)
            {
                case Direction.west:
                    return Direction.east;
                case Direction.northwest:
                    return Direction.southeast;
                case Direction.north:
                    return Direction.south;
                case Direction.northeast:
                    return Direction.southwest;
                case Direction.east:
                    return Direction.west;
                case Direction.southeast:
                    return Direction.northwest;
                case Direction.south:
                    return Direction.north;
                case Direction.southwest:
                    return Direction.northeast;
            }
            return Direction.center;
        }

        public static ISimObject GetBlueTrailAt(Point p)
        {
            foreach(var t in BlueTrailObjects)
            {
                if (t.Position.X == p.X && t.Position.Y == p.Y) return t;
            }
            return null;
        }
        public static ISimObject GetRedTrailAt(Point p)
        {
            foreach (var t in RedTrailObjects)
            {
                if (t.Position.X == p.X && t.Position.Y == p.Y) return t;
            }
            return null;
        }
        public static ISimObject GetFoodAt(Point p)
        {
            foreach (var obj in simFoodObjects)
            {
                if (obj.Position.X == p.X && obj.Position.Y == p.Y) return obj;
            }
            return null;
        }

        public static Direction GetColonyDirection(Point p)
        {
            if (IsInColony(p)) return Direction.center;
            if (p.Y < OriginalColony.Position.Y && p.X == OriginalColony.Position.X) return Direction.north;
            if (p.Y < OriginalColony.Position.Y && p.X < OriginalColony.Position.X) return Direction.northeast;
            if (p.Y == OriginalColony.Position.Y && p.X < OriginalColony.Position.X) return Direction.east;
            if (p.Y > OriginalColony.Position.Y && p.X < OriginalColony.Position.X) return Direction.southeast;
            if (p.Y > OriginalColony.Position.Y && p.X == OriginalColony.Position.X) return Direction.south;
            if (p.Y > OriginalColony.Position.Y && p.X > OriginalColony.Position.X) return Direction.southwest;
            if (p.Y == OriginalColony.Position.Y && p.X > OriginalColony.Position.X) return Direction.west;
            //if (p.Y < OriginalColony.Position.Y && p.X > OriginalColony.Position.X) return Direction.northwest;
            return Direction.northwest;
        }

        public static bool IsFoodAt(Point p)
        {
            foreach (var obj in simFoodObjects)
            {
                if (obj.Position.X == p.X && obj.Position.Y == p.Y) return true;
            }
            return false;
        }

        public static void ColorPixel(int i, int j, Color c, Bitmap bm)
        {
            if(i>=0 && i<width && j >= 0 && j < height)
            {
                bm.SetPixel(i, j, c);
            }
        }

        public static bool IsInColony(Point p)
        {
            return ((Colony)OriginalColony).IsInCircle(p);
        }
    }

    public partial class MainWindow : Window
    {

        private const int width = 1127;
        private const int height = 814;

        private int numOfAnts;

        private Bitmap bm;
        private Bitmap bmStatic;
        private Bitmap mergedBMs;

        private string mylightRed = "#FF5555";
        private string mylightGreen = "#42f548";

        public delegate void nextSimulationTick();
        private bool continueCalculating;
        private bool simInit = false;
        Stopwatch stopwatch = new Stopwatch();

        private Random rnd;

        //private Colony OriginalColony;
        private List<ISimObject> toBeRemoved = new List<ISimObject>();
        private Point center = new Point(1126 / 2, 814 / 2);

        private static readonly Regex _regex = new Regex("[^0-9]+"); //regex that matches disallowed text

        private ISimObject selectedObject;

        private int tick = 0;

        private System.Windows.Media.BrushConverter bc = new System.Windows.Media.BrushConverter();


        public MainWindow()
        {
            InitializeComponent();
            Globals.simObjects = new List<ISimObject>();
            Globals.simStaticObjects = new List<ISimObject>();
            Globals.BlueTrailObjects = new List<ISimObject>();
            Globals.RedTrailObjects = new List<ISimObject>();
            Globals.simFoodObjects = new List<ISimObject>();
            Globals.toBeAdded = new List<ISimObject>();

            rnd = new Random(Guid.NewGuid().GetHashCode());

            numOfAnts = int.Parse(AntAmount.Text);
            bmStatic = new Bitmap(width, height);

            RenderAll();
            Globals.simStaticObjects.Add(Globals.OriginalColony = new Colony(center, 5));
            RenderStatics();
            RenderAll();

            CleanButton.Background = (System.Windows.Media.Brush)bc.ConvertFrom(mylightRed);
            BTrailsButton.Background = (System.Windows.Media.Brush)bc.ConvertFrom(mylightGreen);
        }

        private void UpdateAll()
        {
            // Update trails and add those that need to be removed to a "trash" list
            foreach (var trail in Globals.BlueTrailObjects)
            {
                if (!trail.ShouldBeRendered())
                {
                    toBeRemoved.Add(trail);
                    continue;
                }
                trail.Update(mergedBMs);
            }
            foreach (var o in toBeRemoved)
            {
                Globals.BlueTrailObjects.Remove(o);
            }
            toBeRemoved.Clear();

            // Update trails and add those that need to be removed to a "trash" list
            foreach (var trail in Globals.RedTrailObjects)
            {
                if (!trail.ShouldBeRendered())
                {
                    toBeRemoved.Add(trail);
                    continue;
                }
                trail.Update(mergedBMs);
            }
            foreach (var o in toBeRemoved)
            {
                Globals.RedTrailObjects.Remove(o);
            }
            toBeRemoved.Clear();

            // Update food and add those that need to be removed to a "trash" list
            foreach (var f in Globals.simFoodObjects)
            {
                if (!f.ShouldBeRendered())
                {
                    toBeRemoved.Add(f);
                    continue;
                }
                f.Update(mergedBMs);
            }
            foreach (var o in toBeRemoved)
            {
                Globals.simFoodObjects.Remove(o);
            }
            toBeRemoved.Clear();


            // Update food/obstacles and add those that need to be removed to a "trash" list
            foreach (var fORobs in Globals.simStaticObjects)
            {
                if (!fORobs.ShouldBeRendered())
                {
                    toBeRemoved.Add(fORobs);
                    continue;
                }
                fORobs.Update(mergedBMs);
            }
            foreach (var o in toBeRemoved)
            {
                Globals.simStaticObjects.Remove(o);
            }
            toBeRemoved.Clear();


            // Update simObjs and add those that need to be removed to a "trash" list
            foreach (var simObj in Globals.simObjects)
            {
                if (!simObj.ShouldBeRendered())
                {
                    toBeRemoved.Add(simObj);
                    continue;
                }
                simObj.Update(mergedBMs);
            }
            foreach (var o in toBeRemoved)
            {
                Globals.simObjects.Remove(o);
                AntsDied.Text = (int.Parse(AntsDied.Text) + 1).ToString();
            }
            toBeRemoved.Clear();

            foreach (var o in Globals.toBeAdded)
            {
                Globals.simObjects.Add(o);
                AntsAdded.Text = (int.Parse(AntsAdded.Text)+1).ToString();
            }
            Globals.toBeAdded.Clear();
            AntCount.Text = $"{Globals.simObjects.Count}";
        }
        private void RenderStatics()
        {
            bmStatic = new Bitmap(width, height);

            foreach (var simObj in Globals.simStaticObjects)
            {
                simObj.Render(bmStatic);
            }
        }

        private void RenderAll()
        {
            bm = new Bitmap(width, height);
            //bm = bmStatic;
            foreach (var trail in Globals.BlueTrailObjects)
            {
                trail.Render(bm);
            }
            foreach (var trail in Globals.RedTrailObjects)
            {
                trail.Render(bm);
            }
            foreach (var food in Globals.simFoodObjects)
            {
                food.Render(bm);
            }
            foreach (var simObj in Globals.simObjects)
            {
                simObj.Render(bm);
            }

            
            mergedBMs = MergedBitmaps(bmStatic, bm);

            DisplayImage(mergedBMs);
        }
        private void InitSim()
        {
            for (int i = 0; i < numOfAnts; ++i)
            {
                // Random rand = new Random(Guid.NewGuid().GetHashCode()); // Very useful for generating random objects with random seeds!
                Globals.simObjects.Add(new Ant(Globals.NumToDir(rnd.Next(Globals.directions.Count)), Globals.OriginalColony.Position, new Random(Guid.NewGuid().GetHashCode())));
            }
        }
        private void StartOrStopSimButton(object sender, RoutedEventArgs e)
        {
            if (continueCalculating)
            {
                continueCalculating = false;
                StartOrStopText.Text = "Resume";
            }
            else
            {
                selectedObject = null;
                continueCalculating = true;
                StartOrStopText.Text = "Stop";
                StartOrStopButton.Dispatcher.BeginInvoke(
                    DispatcherPriority.Normal,
                    new nextSimulationTick(RunNextTick));
            }
        }
        private void AntAmountChange(object sender, TextChangedEventArgs e)
        {
            int i;
            if (Int32.TryParse(AntAmount.Text, out i))
            {
                numOfAnts = i;

                if (AntCount != null)
                {
                    AntCount.Text = AntAmount.Text;
                }
            }
        }



        // This function is the engine.
        public void RunNextTick()
        {
            stopwatch.Start();
            if (!simInit)
            {
                InitSim();
                simInit = true;                
            }           


            RenderAll();

            UpdateAll();


            tick++;
            if (tick % 15 == 0)
            {
                stopwatch.Stop();
                TPS.Text = $"{(1 / stopwatch.Elapsed.TotalSeconds):0.##}";
                stopwatch.Reset();
            }
            ticker.Text = tick.ToString();
            //System.Threading.Thread.Sleep(10);
            if (continueCalculating)
            {
                StartOrStopButton.Dispatcher.BeginInvoke(
                    DispatcherPriority.SystemIdle,
                    new nextSimulationTick(RunNextTick));
            }
        }




        // Displays image from bitmap.
        private void DisplayImage(Bitmap b)
        {
            myImage.Source = Bitmap2BitmapImage(b); ;
        }
        // Converts Bitmaps to BitmapImages.
        private BitmapImage Bitmap2BitmapImage(Bitmap bitmap)
        {
            using (var memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();

                return bitmapImage;
            }
        }
        private static bool IsTextAllowed(string text)
        {
            return !_regex.IsMatch(text);
        }
        private void PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(e.Text);
        }
        private void SelectObstacle_Click(object sender, RoutedEventArgs e)
        {
            selectedObject = new Obstacle();
        }
        private void SelectNest_Click(object sender, RoutedEventArgs e)
        {
            selectedObject = new Colony();
        }
        private void myImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (selectedObject == null) return;

            bool changeWasMade = false;
            var prevSelectedObject = selectedObject;
            Point prevPos = selectedObject.Position;

            selectedObject.Position = new Point((int)e.GetPosition(myImage).X, (int)e.GetPosition(myImage).Y);
            switch (selectedObject)
            {
                case Obstacle obstacle:
                    int prevW = (selectedObject as Obstacle).Width;
                    int prevH = (selectedObject as Obstacle).Height;

                    int newW = Int32.Parse(ObstacleWidth.Text);
                    int newH = Int32.Parse(ObstacleHeight.Text);

                    (selectedObject as Obstacle).Width = newW;
                    (selectedObject as Obstacle).Height = newH;

                    if (prevW != newW || prevH != newH || AreDifferent(prevPos, selectedObject.Position))
                    {
                        changeWasMade = true;
                    }

                    break;
                case Colony colony:
                    int prevR = (selectedObject as Colony).Radius;
                    int newR = Int32.Parse(NestRadius.Text);
                    (selectedObject as Colony).Radius = newR;

                    Globals.simStaticObjects.Remove(Globals.OriginalColony);
                    Globals.simStaticObjects.Add(Globals.OriginalColony = new Colony(selectedObject.Position, newR));

                    if (prevR != newR || AreDifferent(prevPos, selectedObject.Position))
                    {
                        changeWasMade = true;
                    }

                    break;
                case Food food:
                    CreateFood(selectedObject.Position, Int32.Parse(FoodWidth.Text), Int32.Parse(FoodHeight.Text));
                    RenderStatics();
                    RenderAll();
                    return;
                default:
                    return;
            }
            if (!Globals.simStaticObjects.Contains(selectedObject) || changeWasMade)
            {
                if (changeWasMade)
                {
                    Globals.simStaticObjects.Remove(prevSelectedObject);
                    changeWasMade = false;
                }
                Globals.simStaticObjects.Add(selectedObject);
                RenderStatics();
            }
            RenderAll();
        }
        private bool AreDifferent(Point a, Point b)
        {
            if (a.X != b.X) return true;
            if (a.Y != b.Y) return true;
            return false;
        }
        private Bitmap MergedBitmaps(Bitmap bmp1, Bitmap bmp2)
        {
            Bitmap result = new Bitmap(Math.Max(bmp1.Width, bmp2.Width),
                                       Math.Max(bmp1.Height, bmp2.Height));
            using (Graphics g = Graphics.FromImage(result))
            {
                g.DrawImage(bmp2, Point.Empty);
                g.DrawImage(bmp1, Point.Empty);
            }
            return result;
        }
        private void ResetSimButton(object sender, RoutedEventArgs e)
        {
            Globals.simObjects.Clear();
            Globals.RedTrailObjects.Clear();
            Globals.BlueTrailObjects.Clear();
            Globals.simStaticObjects.Clear();
            Globals.simFoodObjects.Clear();

            Globals.simStaticObjects.Add(Globals.OriginalColony = new Colony(new Point(563, 407), Int32.Parse(NestRadius.Text)));

            if (StartOrStopText.Text == "Stop")
                StartOrStopSimButton(null, null);
            simInit = false;
            StartOrStopText.Text = "Start";
            AntCount.Text = AntAmount.Text;
            AntsDied.Text = "0";
            AntsAdded.Text = "0";
            tick = 0;
            ticker.Text = tick.ToString();
            RenderStatics();
            RenderAll();
        }

        private void CleanRoadsButton(object sender, RoutedEventArgs e)
        {
            if (Globals.CleanRoadsFlag)
            {
                Globals.CleanRoadsFlag = false;
                CleanButton.Background = (System.Windows.Media.Brush)bc.ConvertFrom(mylightRed);
            }
            else
            {
                Globals.CleanRoadsFlag = true;
                CleanButton.Background = (System.Windows.Media.Brush)bc.ConvertFrom(mylightGreen);
            }
        }
        private void BTButton(object sender, RoutedEventArgs e)
        {
            if (Globals.BlueTrailsFlag)
            {
                Globals.BlueTrailsFlag = false;
                BTrailsButton.Background = (System.Windows.Media.Brush)bc.ConvertFrom(mylightRed);
                Globals.BlueTrailObjects.Clear();
            }
            else
            {
                Globals.BlueTrailsFlag = true;                
                BTrailsButton.Background = (System.Windows.Media.Brush)bc.ConvertFrom(mylightGreen);
            }
        }
        public bool IsInBound(int x, int y, Bitmap bm)
        {
            if (x > 0 && x < bm.Width && y > 0 && y < bm.Height)
            {
                return true;
            }
            return false;
        }
        private void SelectFood_Click(object sender, RoutedEventArgs e)
        {
            selectedObject = new Food();
        }
        private void CreateFood(Point point, int width, int height)
        {
            for (int x = point.X - (width / 2); x < point.X + (width / 2); x++)
                for (int y = point.Y - (height / 2); y < point.Y + (height / 2); y++)
                    if (IsInBound(x, y, bm))
                        Globals.simFoodObjects.Add(new Food(new Point(x, y)));
        }
    }
}