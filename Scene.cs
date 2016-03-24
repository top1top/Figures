using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Microsoft.Graphics.Canvas;

namespace Figures
{
    struct ScreenPoint
    {
        public float X, Y, Z, W;
    }

    abstract class Scene
    {
        protected const double PointSize = 0.06;

        private readonly Stopwatch _timer = Stopwatch.StartNew();

        protected readonly List<Matrix> Points = new List<Matrix>();
        protected readonly List<int[]> Edges = new List<int[]>();

        public Dictionary<string, double> Parameters = new Dictionary<string, double>()
        {
            ["A"] = 0,
            ["B"] = 0,
            ["C"] = 0,
            ["D"] = 0,
            ["E"] = 0,
            ["F"] = 0
        };
        public Dictionary<string, bool> IsTimed = new Dictionary<string, bool>()
        {
            ["A"] = false,
            ["B"] = false,
            ["C"] = false,
            ["D"] = false,
            ["E"] = false,
            ["F"] = false,
        };
        protected Dictionary<string, double> Values = new Dictionary<string, double>()
        {
            ["A"] = 0,
            ["B"] = 0,
            ["C"] = 0,
            ["D"] = 0,
            ["E"] = 0,
            ["F"] = 0
        };
        protected static ScreenPoint[] Project(double width, double height, IEnumerable<Matrix> points, Matrix transformation)
        {
            return points
                .Select(i => transformation * i)
                .Select(i => new ScreenPoint
                {
                    X = (float)((i.X / i.W + 1) * 0.5 * width),
                    Y = (float)((1 - i.Y / i.W) * 0.5 * height),
                    Z = (float)i.Z,
                    W = (float)i.W
                })
                .ToArray();
        }

        protected abstract void Update(double width, double height, double dt);

        protected abstract void Render(double width, double height, CanvasDrawingSession g);

        public void Invalidate(double width, double height, CanvasDrawingSession g)
        {
            var t = DateTime.Now.TimeOfDay.TotalSeconds * 0.2;
            var elapsed = _timer.ElapsedMilliseconds / 1000.0;
            foreach (var key in Values.Keys.ToList())
                Values[key] = IsTimed[key] ? t * Parameters[key] : Parameters[key];
            Update(width, height, elapsed);
            Render(width, height, g);
        }
    }

    abstract class RegularScene : Scene
    {
        protected Matrix TransformationMatrix { get; set; }

        protected override void Render(double width, double height, CanvasDrawingSession g)
        {
            var projected = Project(width, height, Points, TransformationMatrix);

            g.Clear(Colors.Black);
            
            foreach (var edge in Edges.Select(i => new { From = projected[i[0]], To = projected[i[1]] }))
                g.DrawLine(edge.From.X, edge.From.Y, edge.To.X, edge.To.Y, Colors.Lime);

            foreach (var point in projected.Where(i => i.Z >= 0))
                g.FillCircle(point.X, point.Y, (float)(PointSize * height / point.W), Colors.Red);
        }
    }
    abstract class StereopairScene : Scene
    {
        protected Matrix LeftEyeTransformationMatrix { get; set; }
        protected Matrix RightEyeTransformationMatrix { get; set; }

        protected override void Render(double width, double height, CanvasDrawingSession g)
        {
            var half = (float)(width / 2);

            g.Clear(Colors.Black);

            var pointsForLeftEye = Project(half, height, Points, LeftEyeTransformationMatrix);
            var pointsForRightEye = Project(half, height, Points, RightEyeTransformationMatrix);

            foreach (var edge in Edges.Select(i => new { From = pointsForLeftEye[i[0]], To = pointsForLeftEye[i[1]] }))
                g.DrawLine(edge.From.X, edge.From.Y, edge.To.X, edge.To.Y, Colors.Lime);
            foreach (var point in pointsForLeftEye.Where(i => i.Z >= 0))
                g.FillCircle(point.X, point.Y, (float)(PointSize * height / point.W), Colors.Red);

            foreach (var edge in Edges.Select(i => new { From = pointsForRightEye[i[0]], To = pointsForRightEye[i[1]] }))
                g.DrawLine(edge.From.X + half, edge.From.Y, edge.To.X + half, edge.To.Y, Colors.Lime);
            foreach (var point in pointsForRightEye.Where(i => i.Z >= 0))
                g.FillCircle(point.X + half, point.Y, (float)(PointSize * height / point.W), Colors.Red);
        }
    }

    class Cube3DPerspectiveScene : RegularScene
    {
        public Cube3DPerspectiveScene()
        {
            Points.AddRange(new[]
            {
                Matrix.Column(-1, -1, -1, 1),
                Matrix.Column(-1, 1, -1, 1),
                Matrix.Column(1, 1, -1, 1),
                Matrix.Column(1, -1, -1, 1),
                Matrix.Column(-1, -1, 1, 1),
                Matrix.Column(-1, 1, 1, 1),
                Matrix.Column(1, 1, 1, 1),
                Matrix.Column(1, -1, 1, 1),
            });
            Edges.AddRange(new[]
            {
                new[] {0, 1},
                new[] {1, 2},
                new[] {2, 3},
                new[] {3, 0},
                new[] {4, 5},
                new[] {5, 6},
                new[] {6, 7},
                new[] {7, 4},
                new[] {0, 4},
                new[] {1, 5},
                new[] {2, 6},
                new[] {3, 7},
            });
        }


        protected override void Update(double width, double height, double t)
        {
            var angle = (Math.PI * 2) * t;

            var world = Matrix.RotationMatrix(0, 2, 3, angle / 5) * Matrix.RotationMatrix(1, 2, 3, angle / 7);
            var view = Matrix.TranslationMatrix(0, 0, 3);
            var projection = Matrix.PerspectiveProjectionMatrix(Math.PI / 3, width / height);
            TransformationMatrix = projection * view * world;
        }
    }

    class Cube3DOrthographicScene : RegularScene
    {
        public Cube3DOrthographicScene()
        {
            Points.AddRange(new[]
            {
                Matrix.Column(-1, -1, -1, 1),
                Matrix.Column(-1, 1, -1, 1),
                Matrix.Column(1, 1, -1, 1),
                Matrix.Column(1, -1, -1, 1),
                Matrix.Column(-1, -1, 1, 1),
                Matrix.Column(-1, 1, 1, 1),
                Matrix.Column(1, 1, 1, 1),
                Matrix.Column(1, -1, 1, 1),
            });
            Edges.AddRange(new[]
            {
                new[] {0, 1},
                new[] {1, 2},
                new[] {2, 3},
                new[] {3, 0},
                new[] {4, 5},
                new[] {5, 6},
                new[] {6, 7},
                new[] {7, 4},
                new[] {0, 4},
                new[] {1, 5},
                new[] {2, 6},
                new[] {3, 7},
            });
        }


        protected override void Update(double width, double height, double t)
        {
            var angle = (Math.PI * 2) * t;

            var world = Matrix.RotationMatrix(0, 2, 3, angle / 5) * Matrix.RotationMatrix(1, 2, 3, angle / 7);
            var view = Matrix.TranslationMatrix(0, 0, 3) * Matrix.ScaleMatrix(0.2f, 0.2f, 0.2f);
            var projection = Matrix.Identity(4);
            TransformationMatrix = projection * view * world;
        }
    }

    class Cube3DPerspectiveStereopairScene : StereopairScene
    {
        public Cube3DPerspectiveStereopairScene()
        {
            Points.AddRange(new[]
            {
                Matrix.Column(-1, -1, -1, 1),
                Matrix.Column(-1, 1, -1, 1),
                Matrix.Column(1, 1, -1, 1),
                Matrix.Column(1, -1, -1, 1),
                Matrix.Column(-1, -1, 1, 1),
                Matrix.Column(-1, 1, 1, 1),
                Matrix.Column(1, 1, 1, 1),
                Matrix.Column(1, -1, 1, 1),
            });
            Edges.AddRange(new[]
            {
                new[] {0, 1},
                new[] {1, 2},
                new[] {2, 3},
                new[] {3, 0},
                new[] {4, 5},
                new[] {5, 6},
                new[] {6, 7},
                new[] {7, 4},
                new[] {0, 4},
                new[] {1, 5},
                new[] {2, 6},
                new[] {3, 7},
            });
        }

        protected override void Update(double width, double height, double dt)
        {
            var angle = (Math.PI * 2) * dt;

            var world = Matrix.RotationMatrix(0, 2, 3, angle / 5) * Matrix.RotationMatrix(1, 2, 3, angle / 7);
            var projection = Matrix.PerspectiveProjectionMatrix(Math.PI / 4, width / height / 2);

            const double eyeOffset = 0.1;
            const double eyeZ = -5;

            var leftView = Matrix.LookAtMatrix(Matrix.Column(-eyeOffset, 0, eyeZ), Matrix.Column(0, 0, 0));
            var rightView = Matrix.LookAtMatrix(Matrix.Column(eyeOffset, 0, eyeZ), Matrix.Column(0, 0, 0));

            LeftEyeTransformationMatrix = projection * leftView * world;
            RightEyeTransformationMatrix = projection * rightView * world;
        }
    }

    class Cube4DScene : RegularScene
    {
        public Cube4DScene()
        {
            Points.AddRange(new[]
            {
                Matrix.Column(-1, -1, -1, -1, 1), // 0
                Matrix.Column(+1, -1, -1, -1, 1), // 1
                Matrix.Column(-1, +1, -1, -1, 1), // 2
                Matrix.Column(+1, +1, -1, -1, 1), // 3
                Matrix.Column(-1, -1, +1, -1, 1), // 4
                Matrix.Column(+1, -1, +1, -1, 1), // 5
                Matrix.Column(-1, +1, +1, -1, 1), // 6
                Matrix.Column(+1, +1, +1, -1, 1), // 7

                Matrix.Column(-1, -1, -1, +1, 1), // 8
                Matrix.Column(+1, -1, -1, +1, 1), // 9
                Matrix.Column(-1, +1, -1, +1, 1), // 10
                Matrix.Column(+1, +1, -1, +1, 1), // 11
                Matrix.Column(-1, -1, +1, +1, 1), // 12
                Matrix.Column(+1, -1, +1, +1, 1), // 13
                Matrix.Column(-1, +1, +1, +1, 1), // 14
                Matrix.Column(+1, +1, +1, +1, 1), // 15
            });
            Edges.AddRange(new[]
            {
                new[] {0, 1},
                new[] {0, 2},
                new[] {0, 4},
                new[] {0, 8},
                new[] {1, 3},
                new[] {1, 5},
                new[] {1, 9},
                new[] {2, 3},
                new[] {2, 6},
                new[] {2, 10},
                new[] {3, 7},
                new[] {3, 11},
                new[] {4, 5},
                new[] {4, 6},
                new[] {4, 12},
                new[] {5, 7},
                new[] {5, 13},
                new[] {6, 7},
                new[] {6, 14},
                new[] {7, 15},
                new[] {8, 9},
                new[] {8, 10},
                new[] {8, 12},
                new[] {9, 11},
                new[] {9, 13},
                new[] {10, 11},
                new[] {10, 14},
                new[] {11, 15},
                new[] {12, 13},
                new[] {12, 14},
                new[] {13, 15},
                new[] {14, 15},
            });
        }

        protected override void Update(double width, double height, double dt)
        {
            //var angle = (Math.PI * 2) * dt * 0.5f;

            //var world =
            //    Matrix.RotationMatrix(0, 1, 4, angle / 2.134123) *
            //    Matrix.RotationMatrix(0, 2, 4, angle / 5.32141) *
            //    Matrix.RotationMatrix(0, 3, 4, angle / 11.54142) *
            //    Matrix.RotationMatrix(1, 2, 4, angle / 17.41251) *
            //    Matrix.RotationMatrix(1, 3, 4, angle / 29.51245) *
            //    Matrix.RotationMatrix(2, 3, 4, angle / 37.512412) *
            //    Matrix.Identity(5);

            var world =
                Matrix.RotationMatrix(0, 1, 4, Parameters["A"]) *
                Matrix.RotationMatrix(0, 2, 4, Parameters["B"]) *
                Matrix.RotationMatrix(0, 3, 4, Parameters["C"]) *
                Matrix.RotationMatrix(1, 2, 4, Parameters["D"]) *
                Matrix.RotationMatrix(1, 3, 4, Parameters["E"]) *
                Matrix.RotationMatrix(2, 3, 4, Parameters["F"]);

            var view = Matrix.TranslationMatrix(0, 0, 3) * Matrix.OrthographicProjectionMatrix(4, 3);
            var projection = Matrix.PerspectiveProjectionMatrix(Math.PI / 3, width / height);
            TransformationMatrix = projection * view * world;
        }
    }

    class Cube4DStereopairScene : StereopairScene
    {
        public Cube4DStereopairScene()
        {
            Points.AddRange(new[]
            {
                Matrix.Column(-1, -1, -1, -1, 1), // 0
                Matrix.Column(+1, -1, -1, -1, 1), // 1
                Matrix.Column(-1, +1, -1, -1, 1), // 2
                Matrix.Column(+1, +1, -1, -1, 1), // 3
                Matrix.Column(-1, -1, +1, -1, 1), // 4
                Matrix.Column(+1, -1, +1, -1, 1), // 5
                Matrix.Column(-1, +1, +1, -1, 1), // 6
                Matrix.Column(+1, +1, +1, -1, 1), // 7

                Matrix.Column(-1, -1, -1, +1, 1), // 8
                Matrix.Column(+1, -1, -1, +1, 1), // 9
                Matrix.Column(-1, +1, -1, +1, 1), // 10
                Matrix.Column(+1, +1, -1, +1, 1), // 11
                Matrix.Column(-1, -1, +1, +1, 1), // 12
                Matrix.Column(+1, -1, +1, +1, 1), // 13
                Matrix.Column(-1, +1, +1, +1, 1), // 14
                Matrix.Column(+1, +1, +1, +1, 1), // 15
            });
            Edges.AddRange(new[]
            {
                new[] {0, 1},
                new[] {0, 2},
                new[] {0, 4},
                new[] {0, 8},
                new[] {1, 3},
                new[] {1, 5},
                new[] {1, 9},
                new[] {2, 3},
                new[] {2, 6},
                new[] {2, 10},
                new[] {3, 7},
                new[] {3, 11},
                new[] {4, 5},
                new[] {4, 6},
                new[] {4, 12},
                new[] {5, 7},
                new[] {5, 13},
                new[] {6, 7},
                new[] {6, 14},
                new[] {7, 15},
                new[] {8, 9},
                new[] {8, 10},
                new[] {8, 12},
                new[] {9, 11},
                new[] {9, 13},
                new[] {10, 11},
                new[] {10, 14},
                new[] {11, 15},
                new[] {12, 13},
                new[] {12, 14},
                new[] {13, 15},
                new[] {14, 15},
            });
        }

        protected override void Update(double width, double height, double dt)
        {
            var angle = (Math.PI * 2) * dt * 0.5f;

            //var world =
            //    Matrix.RotationMatrix(0, 1, 4, angle / 2.134123) *
            //    Matrix.RotationMatrix(0, 2, 4, angle / 5.32141) *
            //    Matrix.RotationMatrix(0, 3, 4, angle / 11.54142) *
            //    Matrix.RotationMatrix(1, 2, 4, angle / 17.41251) *
            //    Matrix.RotationMatrix(1, 3, 4, angle / 29.51245) *
            //    Matrix.RotationMatrix(2, 3, 4, angle / 37.512412) *
            //    Matrix.Identity(5);

            var world =
                Matrix.RotationMatrix(0, 1, 4, Values["A"]) *
                Matrix.RotationMatrix(0, 2, 4, Values["B"]) *
                Matrix.RotationMatrix(0, 3, 4, Values["C"]) *
                Matrix.RotationMatrix(1, 2, 4, Values["D"]) *
                Matrix.RotationMatrix(1, 3, 4, Values["E"]) *
                Matrix.RotationMatrix(2, 3, 4, Values["F"]);


            const double eyeOffset = 0.1;
            const double eyeZ = -5;

            var leftView = Matrix.LookAtMatrix(Matrix.Column(-eyeOffset, 0, eyeZ), Matrix.Column(0, 0, 0)) * Matrix.OrthographicProjectionMatrix(4, 3);
            var rightView = Matrix.LookAtMatrix(Matrix.Column(eyeOffset, 0, eyeZ), Matrix.Column(0, 0, 0)) * Matrix.OrthographicProjectionMatrix(4, 3);

            var projection = Matrix.PerspectiveProjectionMatrix(Math.PI / 3, width / height / 2);
            LeftEyeTransformationMatrix = projection * leftView * world;
            RightEyeTransformationMatrix = projection * rightView * world;
        }
    }
}
