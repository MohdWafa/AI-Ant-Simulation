using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Point = System.Drawing.Point;
using Color = System.Drawing.Color;
using Bitmap = System.Drawing.Bitmap;
using Direction = WpfAntSimulator.Globals.Direction;
using WpfAntSimulator.SimObjects.Pheremone;

using System.Windows;

namespace WpfAntSimulator.SimObjects
{
    public class Ant : ISimObject
    {
        public Point PrevPosition { get; set; } // Maybe this won't be neccessary

        private Queue<Point> prevPoints;
        private Queue<bool> prevObstacleViews;

        private const int width = 1127;
        private const int height = 814; // 814
        private Random rnd;
        private int multiplyer = 1;
        private int lifeSpan;
        private int lik1 = 2;
        private int lik2 = 3;


        private bool hasFood = false;
        
        private List<Tuple<Point, Direction>> vision;



        public Point Position { get; set; }
        public Color MyColor { get; set; }        

        private Direction prevDir;
        private Direction dir;
        private Direction tmpDir;

        public Ant(Direction d, Point start, Random r)
        {
            vision = new List<Tuple<Point, Direction>>(5);
            prevPoints = new Queue<Point>(10);
            prevObstacleViews = new Queue<bool>(6);
            dir = d;
            Position = start;
            MyColor = Globals.antColor;
            rnd = r;
            lifeSpan = r.Next(1, 201)*40;
            prevDir = dir;
        }

        public bool ShouldBeRendered()
        {
            if (lifeSpan <= 0)
            {
                return false;
            }
            return true;
        }

        

        public void Update(Bitmap bm)
        {
            lifeSpan--;
            if (lifeSpan <= 0) return;

            UpdateTrail(); // 1.) update trail at current position

            GetVisionPoints(); // 2.) Get up to 3 points in view of the ant.

            AddPrevViewObstacle(bm);


            if (IsFoodPixel(Position, bm) && hasFood == false)
            {
                var food = Globals.GetFoodAt(Position);
                ((Food) food).FoodAmount--;
                hasFood = true;
                dir = Globals.FlipDirection(dir);
                GetVisionPoints();
            }
            
            if (!hasFood)
            {
                // if there is RedTrail in my view, then I go there
                tmpDir = IsRedTrailInFront();
                if (tmpDir != Direction.center)
                {
                    dir = tmpDir;
                }

                // if there is food in my view & I don't have food, then I go there
                tmpDir = IsFoodInFront(bm);
                if (tmpDir != Direction.center)
                {
                    dir = tmpDir;
                }
            }
            else
            {
                

                // if there is BlueTrail in my view, then I go there
                if (Globals.BlueTrailsFlag)
                {
                    tmpDir = IsBlueTrailInFront();
                    if (tmpDir != Direction.center)
                    {
                        dir = tmpDir;
                    }
                }


                // Head towards nest
                tmpDir = Globals.GetColonyDirection(Position);
                if (tmpDir != Direction.center)
                {
                    dir = tmpDir;
                    if(!Globals.CleanRoadsFlag) dir = WillIChange(dir);
                }

                // if there is RedTrail in my view, then I go there
                tmpDir = IsRedTrailInFront();
                if (tmpDir != Direction.center)
                {
                    dir = tmpDir;
                }

                // if there is BlueTrail in my view, then I go there
                if (Globals.BlueTrailsFlag && (IsObstacleInView(bm) || IsObstacleInPrev(bm)))
                {
                    tmpDir = IsBlueTrailInFront();
                    if (tmpDir != Direction.center)
                    {
                        dir = tmpDir;
                    }
                }

                //tmpDir = IsRedTrailInFront();
                //if (tmpDir != Direction.center && CloserToHome())
                //{
                //    dir = tmpDir;
                //}

                // Drops off food and creates an ant
                if (Globals.IsInColony(Position))
                {
                    hasFood = false;
                    dir = Globals.FlipDirection(dir);
                    Globals.toBeAdded.Add(new Ant(dir, Position, new Random(Guid.NewGuid().GetHashCode())));
                    //Globals.toBeAdded.Add(new Ant(Globals.NumToDir(rnd.Next(Globals.directions.Count)), Globals.OriginalColony.Position, new Random(Guid.NewGuid().GetHashCode())));
                }
            }

            Movement(bm);

            // Random movement - Exploration mode
            
            prevDir = dir;
            dir = WillIChange(dir);
            
        }

        private void GetVisionPoints()
        {
            vision.Clear();
            switch (dir)
            {
                case Direction.west:
                    if (IsBounds(Position.X, Position.Y - 1)) vision.Add(new Tuple<Point, Direction>(new Point(Position.X, Position.Y - 1), Direction.south));
                    if (IsBounds(Position.X - 1, Position.Y - 1)) vision.Add(new Tuple<Point, Direction>(new Point(Position.X - 1, Position.Y - 1), Direction.southwest));
                    if (IsBounds(Position.X - 1, Position.Y)) vision.Add(new Tuple<Point, Direction>(new Point(Position.X - 1, Position.Y), Direction.west));
                    if (IsBounds(Position.X - 1, Position.Y + 1)) vision.Add(new Tuple<Point, Direction>(new Point(Position.X - 1, Position.Y + 1), Direction.northwest));
                    if (IsBounds(Position.X, Position.Y + 1)) vision.Add(new Tuple<Point, Direction>(new Point(Position.X, Position.Y + 1), Direction.north));
                    break;
                case Direction.northwest:
                    if (IsBounds(Position.X - 1, Position.Y - 1)) vision.Add(new Tuple<Point, Direction>(new Point(Position.X - 1, Position.Y - 1), Direction.southwest));
                    if (IsBounds(Position.X - 1, Position.Y)) vision.Add(new Tuple<Point, Direction>(new Point(Position.X - 1, Position.Y), Direction.west));
                    if (IsBounds(Position.X - 1, Position.Y + 1)) vision.Add(new Tuple<Point, Direction>(new Point(Position.X - 1, Position.Y + 1), Direction.northwest));
                    if (IsBounds(Position.X, Position.Y + 1)) vision.Add(new Tuple<Point, Direction>(new Point(Position.X, Position.Y + 1), Direction.north));
                    if (IsBounds(Position.X + 1, Position.Y + 1)) vision.Add(new Tuple<Point, Direction>(new Point(Position.X + 1, Position.Y + 1), Direction.northeast));
                    break;
                case Direction.north:
                    if (IsBounds(Position.X - 1, Position.Y)) vision.Add(new Tuple<Point, Direction>(new Point(Position.X - 1, Position.Y), Direction.west));
                    if (IsBounds(Position.X - 1, Position.Y + 1)) vision.Add(new Tuple<Point, Direction>(new Point(Position.X - 1, Position.Y + 1), Direction.northwest));
                    if (IsBounds(Position.X, Position.Y + 1)) vision.Add(new Tuple<Point, Direction>(new Point(Position.X, Position.Y + 1), Direction.north));
                    if (IsBounds(Position.X + 1, Position.Y + 1)) vision.Add(new Tuple<Point, Direction>(new Point(Position.X + 1, Position.Y + 1), Direction.northeast));
                    if (IsBounds(Position.X + 1, Position.Y)) vision.Add(new Tuple<Point, Direction>(new Point(Position.X + 1, Position.Y), Direction.east));
                    break;
                case Direction.northeast:
                    if (IsBounds(Position.X - 1, Position.Y + 1)) vision.Add(new Tuple<Point, Direction>(new Point(Position.X - 1, Position.Y + 1), Direction.northwest));
                    if (IsBounds(Position.X, Position.Y + 1)) vision.Add(new Tuple<Point, Direction>(new Point(Position.X, Position.Y + 1), Direction.north));
                    if (IsBounds(Position.X + 1, Position.Y + 1)) vision.Add(new Tuple<Point, Direction>(new Point(Position.X + 1, Position.Y + 1), Direction.northeast));
                    if (IsBounds(Position.X + 1, Position.Y)) vision.Add(new Tuple<Point, Direction>(new Point(Position.X + 1, Position.Y), Direction.east));
                    if (IsBounds(Position.X + 1, Position.Y - 1)) vision.Add(new Tuple<Point, Direction>(new Point(Position.X + 1, Position.Y - 1), Direction.southeast));
                    break;
                case Direction.east:
                    if (IsBounds(Position.X, Position.Y + 1)) vision.Add(new Tuple<Point, Direction>(new Point(Position.X, Position.Y + 1), Direction.north));
                    if (IsBounds(Position.X + 1, Position.Y + 1)) vision.Add(new Tuple<Point, Direction>(new Point(Position.X + 1, Position.Y + 1), Direction.northeast));
                    if (IsBounds(Position.X + 1, Position.Y)) vision.Add(new Tuple<Point, Direction>(new Point(Position.X + 1, Position.Y), Direction.east));
                    if (IsBounds(Position.X + 1, Position.Y - 1)) vision.Add(new Tuple<Point, Direction>(new Point(Position.X + 1, Position.Y - 1), Direction.southeast));
                    if (IsBounds(Position.X, Position.Y - 1)) vision.Add(new Tuple<Point, Direction>(new Point(Position.X, Position.Y - 1), Direction.south));
                    break;
                case Direction.southeast:
                    if (IsBounds(Position.X + 1, Position.Y + 1)) vision.Add(new Tuple<Point, Direction>(new Point(Position.X + 1, Position.Y + 1), Direction.northeast));
                    if (IsBounds(Position.X + 1, Position.Y)) vision.Add(new Tuple<Point, Direction>(new Point(Position.X + 1, Position.Y), Direction.east));
                    if (IsBounds(Position.X + 1, Position.Y - 1)) vision.Add(new Tuple<Point, Direction>(new Point(Position.X + 1, Position.Y - 1), Direction.southeast));
                    if (IsBounds(Position.X, Position.Y - 1)) vision.Add(new Tuple<Point, Direction>(new Point(Position.X, Position.Y - 1), Direction.south));
                    if (IsBounds(Position.X - 1, Position.Y - 1)) vision.Add(new Tuple<Point, Direction>(new Point(Position.X - 1, Position.Y - 1), Direction.southwest));
                    break;
                case Direction.south:
                    if (IsBounds(Position.X + 1, Position.Y)) vision.Add(new Tuple<Point, Direction>(new Point(Position.X + 1, Position.Y), Direction.east));
                    if (IsBounds(Position.X + 1, Position.Y - 1)) vision.Add(new Tuple<Point, Direction>(new Point(Position.X + 1, Position.Y - 1), Direction.southeast));
                    if (IsBounds(Position.X, Position.Y - 1)) vision.Add(new Tuple<Point, Direction>(new Point(Position.X, Position.Y - 1), Direction.south));
                    if (IsBounds(Position.X - 1, Position.Y - 1)) vision.Add(new Tuple<Point, Direction>(new Point(Position.X - 1, Position.Y - 1), Direction.southwest));
                    if (IsBounds(Position.X - 1, Position.Y)) vision.Add(new Tuple<Point, Direction>(new Point(Position.X - 1, Position.Y), Direction.west));
                    break;
                case Direction.southwest:
                    if (IsBounds(Position.X + 1, Position.Y - 1)) vision.Add(new Tuple<Point, Direction>(new Point(Position.X + 1, Position.Y - 1), Direction.southeast));
                    if (IsBounds(Position.X, Position.Y - 1)) vision.Add(new Tuple<Point, Direction>(new Point(Position.X, Position.Y - 1), Direction.south));
                    if (IsBounds(Position.X - 1, Position.Y - 1)) vision.Add(new Tuple<Point, Direction>(new Point(Position.X - 1, Position.Y - 1), Direction.southwest));
                    if (IsBounds(Position.X - 1, Position.Y)) vision.Add(new Tuple<Point, Direction>(new Point(Position.X - 1, Position.Y), Direction.west));
                    if (IsBounds(Position.X - 1, Position.Y + 1)) vision.Add(new Tuple<Point, Direction>(new Point(Position.X - 1, Position.Y + 1), Direction.northwest));
                    break;
                case Direction.center:
                    switch (prevDir)
                    {
                        case Direction.west:
                            if (IsBounds(Position.X, Position.Y - 1)) vision.Add(new Tuple<Point, Direction>(new Point(Position.X, Position.Y - 1), Direction.south));
                            if (IsBounds(Position.X - 1, Position.Y - 1)) vision.Add(new Tuple<Point, Direction>(new Point(Position.X - 1, Position.Y - 1), Direction.southwest));
                            if (IsBounds(Position.X - 1, Position.Y)) vision.Add(new Tuple<Point, Direction>(new Point(Position.X - 1, Position.Y), Direction.west));
                            if (IsBounds(Position.X - 1, Position.Y + 1)) vision.Add(new Tuple<Point, Direction>(new Point(Position.X - 1, Position.Y + 1), Direction.northwest));
                            if (IsBounds(Position.X, Position.Y + 1)) vision.Add(new Tuple<Point, Direction>(new Point(Position.X, Position.Y + 1), Direction.north));
                            break;
                        case Direction.northwest:
                            if (IsBounds(Position.X - 1, Position.Y - 1)) vision.Add(new Tuple<Point, Direction>(new Point(Position.X - 1, Position.Y - 1), Direction.southwest));
                            if (IsBounds(Position.X - 1, Position.Y)) vision.Add(new Tuple<Point, Direction>(new Point(Position.X - 1, Position.Y), Direction.west));
                            if (IsBounds(Position.X - 1, Position.Y + 1)) vision.Add(new Tuple<Point, Direction>(new Point(Position.X - 1, Position.Y + 1), Direction.northwest));
                            if (IsBounds(Position.X, Position.Y + 1)) vision.Add(new Tuple<Point, Direction>(new Point(Position.X, Position.Y + 1), Direction.north));
                            if (IsBounds(Position.X + 1, Position.Y + 1)) vision.Add(new Tuple<Point, Direction>(new Point(Position.X + 1, Position.Y + 1), Direction.northeast));
                            break;
                        case Direction.north:
                            if (IsBounds(Position.X - 1, Position.Y)) vision.Add(new Tuple<Point, Direction>(new Point(Position.X - 1, Position.Y), Direction.west));
                            if (IsBounds(Position.X - 1, Position.Y + 1)) vision.Add(new Tuple<Point, Direction>(new Point(Position.X - 1, Position.Y + 1), Direction.northwest));
                            if (IsBounds(Position.X, Position.Y + 1)) vision.Add(new Tuple<Point, Direction>(new Point(Position.X, Position.Y + 1), Direction.north));
                            if (IsBounds(Position.X + 1, Position.Y + 1)) vision.Add(new Tuple<Point, Direction>(new Point(Position.X + 1, Position.Y + 1), Direction.northeast));
                            if (IsBounds(Position.X + 1, Position.Y)) vision.Add(new Tuple<Point, Direction>(new Point(Position.X + 1, Position.Y), Direction.east));
                            break;
                        case Direction.northeast:
                            if (IsBounds(Position.X - 1, Position.Y + 1)) vision.Add(new Tuple<Point, Direction>(new Point(Position.X - 1, Position.Y + 1), Direction.northwest));
                            if (IsBounds(Position.X, Position.Y + 1)) vision.Add(new Tuple<Point, Direction>(new Point(Position.X, Position.Y + 1), Direction.north));
                            if (IsBounds(Position.X + 1, Position.Y + 1)) vision.Add(new Tuple<Point, Direction>(new Point(Position.X + 1, Position.Y + 1), Direction.northeast));
                            if (IsBounds(Position.X + 1, Position.Y)) vision.Add(new Tuple<Point, Direction>(new Point(Position.X + 1, Position.Y), Direction.east));
                            if (IsBounds(Position.X + 1, Position.Y - 1)) vision.Add(new Tuple<Point, Direction>(new Point(Position.X + 1, Position.Y - 1), Direction.southeast));
                            break;
                        case Direction.east:
                            if (IsBounds(Position.X, Position.Y + 1)) vision.Add(new Tuple<Point, Direction>(new Point(Position.X, Position.Y + 1), Direction.north));
                            if (IsBounds(Position.X + 1, Position.Y + 1)) vision.Add(new Tuple<Point, Direction>(new Point(Position.X + 1, Position.Y + 1), Direction.northeast));
                            if (IsBounds(Position.X + 1, Position.Y)) vision.Add(new Tuple<Point, Direction>(new Point(Position.X + 1, Position.Y), Direction.east));
                            if (IsBounds(Position.X + 1, Position.Y - 1)) vision.Add(new Tuple<Point, Direction>(new Point(Position.X + 1, Position.Y - 1), Direction.southeast));
                            if (IsBounds(Position.X, Position.Y - 1)) vision.Add(new Tuple<Point, Direction>(new Point(Position.X, Position.Y - 1), Direction.south));
                            break;
                        case Direction.southeast:
                            if (IsBounds(Position.X + 1, Position.Y + 1)) vision.Add(new Tuple<Point, Direction>(new Point(Position.X + 1, Position.Y + 1), Direction.northeast));
                            if (IsBounds(Position.X + 1, Position.Y)) vision.Add(new Tuple<Point, Direction>(new Point(Position.X + 1, Position.Y), Direction.east));
                            if (IsBounds(Position.X + 1, Position.Y - 1)) vision.Add(new Tuple<Point, Direction>(new Point(Position.X + 1, Position.Y - 1), Direction.southeast));
                            if (IsBounds(Position.X, Position.Y - 1)) vision.Add(new Tuple<Point, Direction>(new Point(Position.X, Position.Y - 1), Direction.south));
                            if (IsBounds(Position.X - 1, Position.Y - 1)) vision.Add(new Tuple<Point, Direction>(new Point(Position.X - 1, Position.Y - 1), Direction.southwest));
                            break;
                        case Direction.south:
                            if (IsBounds(Position.X + 1, Position.Y)) vision.Add(new Tuple<Point, Direction>(new Point(Position.X + 1, Position.Y), Direction.east));
                            if (IsBounds(Position.X + 1, Position.Y - 1)) vision.Add(new Tuple<Point, Direction>(new Point(Position.X + 1, Position.Y - 1), Direction.southeast));
                            if (IsBounds(Position.X, Position.Y - 1)) vision.Add(new Tuple<Point, Direction>(new Point(Position.X, Position.Y - 1), Direction.south));
                            if (IsBounds(Position.X - 1, Position.Y - 1)) vision.Add(new Tuple<Point, Direction>(new Point(Position.X - 1, Position.Y - 1), Direction.southwest));
                            if (IsBounds(Position.X - 1, Position.Y)) vision.Add(new Tuple<Point, Direction>(new Point(Position.X - 1, Position.Y), Direction.west));
                            break;
                        case Direction.southwest:
                            if (IsBounds(Position.X + 1, Position.Y - 1)) vision.Add(new Tuple<Point, Direction>(new Point(Position.X + 1, Position.Y - 1), Direction.southeast));
                            if (IsBounds(Position.X, Position.Y - 1)) vision.Add(new Tuple<Point, Direction>(new Point(Position.X, Position.Y - 1), Direction.south));
                            if (IsBounds(Position.X - 1, Position.Y - 1)) vision.Add(new Tuple<Point, Direction>(new Point(Position.X - 1, Position.Y - 1), Direction.southwest));
                            if (IsBounds(Position.X - 1, Position.Y)) vision.Add(new Tuple<Point, Direction>(new Point(Position.X - 1, Position.Y), Direction.west));
                            if (IsBounds(Position.X - 1, Position.Y + 1)) vision.Add(new Tuple<Point, Direction>(new Point(Position.X - 1, Position.Y + 1), Direction.northwest));
                            break;
                        case Direction.center: // Default to east if the center direction occurs twice.
                            if (IsBounds(Position.X, Position.Y + 1)) vision.Add(new Tuple<Point, Direction>(new Point(Position.X, Position.Y + 1), Direction.north));
                            if (IsBounds(Position.X + 1, Position.Y + 1)) vision.Add(new Tuple<Point, Direction>(new Point(Position.X + 1, Position.Y + 1), Direction.northeast));
                            if (IsBounds(Position.X + 1, Position.Y)) vision.Add(new Tuple<Point, Direction>(new Point(Position.X + 1, Position.Y), Direction.east));
                            if (IsBounds(Position.X + 1, Position.Y - 1)) vision.Add(new Tuple<Point, Direction>(new Point(Position.X + 1, Position.Y - 1), Direction.southeast));
                            if (IsBounds(Position.X, Position.Y - 1)) vision.Add(new Tuple<Point, Direction>(new Point(Position.X, Position.Y - 1), Direction.south));
                            break;
                    }
                    break;
            }
        }

        private Direction IsFoodInFront(Bitmap bm)
        {
            foreach(var p in vision)
            {
                if (IsFoodPixel(p.Item1, bm)) return p.Item2;
            }
            return Direction.center;
        }

        private Direction IsRedTrailInFront()
        {
            int max = 0;

            Direction tmp = Direction.center;

            foreach (var p in vision)
            {
                var t = Globals.GetRedTrailAt(p.Item1);
                if (t != null && ((RedTrail)t).ScentValue >= max) tmp = p.Item2;                
            }
            return tmp;
        }

        private Direction IsBlueTrailInFront()
        {
            int max = 0;

            Direction tmp = Direction.center;

            foreach (var p in vision)
            {
                var t = Globals.GetBlueTrailAt(p.Item1);
                if (t != null && ((BlueTrail)t).ScentValue >= max) tmp = p.Item2;
            }
            return tmp;
        }

        private bool IsObstacleInView(Bitmap bm)
        {
            foreach(var p in vision)
            {
                if(IsObstacle(p.Item1, bm))
                {
                    return true;
                }
            }
            return false;
        }

        private void AddPrevViewObstacle(Bitmap bm)
        {
            if (prevObstacleViews.Count > 5) prevObstacleViews.Dequeue();
            prevObstacleViews.Enqueue(IsObstacleInView(bm));
        }

        private bool IsObstacleInPrev(Bitmap bm)
        {
            foreach (var b in prevObstacleViews)
            {
                if (b)
                {
                    return true;
                }
            }
            return false;
        }

        private bool CloserToHome()
        {
            if (CalcEucliDist(Position, Globals.OriginalColony.Position) < CalcEucliDist(PrevPosition, Globals.OriginalColony.Position))
                return true;
            return false;
        }


        private Direction WillIChange(Direction d)
        {
            int r;
            if(hasFood) r = lik1;
            else r = lik2;

            if (rnd.Next(7) < r) // 0 - 6
            {
                switch (d)
                {
                    case Direction.west:
                        return Globals.directions[rnd.Next(6,9)]; // 6 - 8
                    case Direction.northwest:
                        return Globals.directions[rnd.Next(7, 10)]; // 7-9
                    case Direction.north:
                        return Globals.directions[rnd.Next(0, 3)]; // 0-2
                    case Direction.northeast:
                        return Globals.directions[rnd.Next(1, 4)]; // 1-3
                    case Direction.east:
                        return Globals.directions[rnd.Next(2, 5)]; // 2-4
                    case Direction.southeast:
                        return Globals.directions[rnd.Next(3, 6)]; // 3-5
                    case Direction.south:
                        return Globals.directions[rnd.Next(4, 7)]; // 4-6
                    case Direction.southwest:
                        return Globals.directions[rnd.Next(5, 8)]; // 5-7
                    case Direction.center:
                        return Globals.directions[rnd.Next(Globals.directions.Count)];
                }
            }
            return d;
        }
        private void UpdateTrail()
        {
            ISimObject trail = null;
            if (!hasFood)
            {
                if(Globals.BlueTrailsFlag) trail = Globals.GetBlueTrailAt(Position);
            }
            else
            {
                trail = Globals.GetRedTrailAt(Position);
            }

            if (trail == null)
            {
                if (!hasFood)
                {
                    if (Globals.BlueTrailsFlag) Globals.BlueTrailObjects.Add(new BlueTrail(Position));
                }
                else
                {
                    // RedTrail stuff
                    Globals.RedTrailObjects.Add(new RedTrail(Position));
                }
            }
            else
            {
                if (!hasFood)
                {
                    if (Globals.BlueTrailsFlag) ((BlueTrail)trail).AddScent();
                }
                else
                {
                    // RedTrail stuff
                    ((RedTrail)trail).AddScent();
                }
            }
        }
        private void Movement(Bitmap bm)
        {   
            switch (dir)
            {
                case Direction.west:
                    UpdatePos(-1, 0, bm);
                    break;
                case Direction.northwest:
                    UpdatePos(-1, 1, bm);
                    break;
                case Direction.north:
                    UpdatePos(0, 1, bm);
                    break;
                case Direction.northeast:
                    UpdatePos(1, 1, bm);
                    break;
                case Direction.east:
                    UpdatePos(1, 0, bm);
                    break;
                case Direction.southeast:
                    UpdatePos(1, -1, bm);
                    break;
                case Direction.south:
                    UpdatePos(0, -1, bm);
                    break;
                case Direction.southwest:
                    UpdatePos(-1, -1, bm);
                    break;
                case Direction.center:
                    break;
            }
        }
        private void UpdatePos(int x, int y, Bitmap bm)
        {
            PrevPosition = Position;
            if (IsBounds(Position.X + x * multiplyer, Position.Y) && !IsObstacle(Position.X + x * multiplyer, Position.Y, bm))
            {
                Position = new Point(Position.X + x * multiplyer, Position.Y);
            }
            if (IsBounds(Position.X , Position.Y + y * multiplyer) && !IsObstacle(Position.X, Position.Y + y * multiplyer, bm))
            {
                Position = new Point(Position.X, Position.Y + y * multiplyer);
            }
            
            if (prevPoints.Count > 9) prevPoints.Dequeue();
            prevPoints.Enqueue(Position);
        }

        private bool InPrevPoints(Point p)
        {
            foreach(var po in prevPoints)
            {
                if (po.X == p.X && po.Y == p.Y) return true;
            }
            return false;
        }



        public void Render(Bitmap bm)
        {
            if (lifeSpan <= 0) return;
            bm.SetPixel(Position.X, Position.Y, MyColor);
            Enlarge(bm);
        }
        public void Enlarge(Bitmap bm)
        {
            Globals.ColorPixel(Position.X - 1, Position.Y, MyColor, bm);
            Globals.ColorPixel(Position.X + 1, Position.Y, MyColor, bm);
            Globals.ColorPixel(Position.X, Position.Y + 1, MyColor, bm);
            Globals.ColorPixel(Position.X, Position.Y - 1, MyColor, bm);
        }

        private bool IsObstacle(int i, int j, Bitmap bm)
        {
            var c = bm.GetPixel(i, j);
            if (c.ToArgb() == Globals.obstacleColor.ToArgb()) return true;
            return false;
        }
        private bool IsObstacle(Point p, Bitmap bm)
        {
            var c = bm.GetPixel(p.X, p.Y);
            if (c.ToArgb() == Globals.obstacleColor.ToArgb()) return true;
            return false;
        }
        private bool IsFoodPixel(int i, int j, Bitmap bm)
        {
            var c = bm.GetPixel(i, j);
            if (c.ToArgb() == Globals.foodColor.ToArgb()) return true;
            return false;
        }
        private bool IsFoodPixel(Point p, Bitmap bm)
        {
            //var c = bm.GetPixel(p.X, p.Y);
            //if (c.ToArgb() == Globals.foodColor.ToArgb()) return true;
            //return false;

            return Globals.IsFoodAt(p);
        }

        private bool IsBounds(int i, int j)
        {
            if (!(i >= 0 && i < width)) return false;
            if (!(j >= 0 && j < height)) return false;
            return true;
        }

        private double CalcEucliDist(Point p1, Point p2)
        {
            return Math.Sqrt(Math.Pow(p2.Y - p1.Y, 2) + Math.Pow(p2.X - p1.X, 2));
        }

    }
}
