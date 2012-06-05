using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using PixelLab.Common;
using PixelLab.Wpf.Demo.Core;

namespace PixelLab.Wpf.Demo.FlipTile3D
{
    public class FlipTile3D : WrapperElement<Viewport3D>
    {
        public FlipTile3D()
            : base(new Viewport3D())
        {
            Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(setup3D));

            //perf improvement. Clipping in 3D is expensive. Avoid if you can!
            WrappedElement.ClipToBounds = false;

            m_listener.Rendering += tick;
            m_listener.WireParentLoadedUnloaded(this);
        }

        #region render/layout overrides

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            drawingContext.DrawRectangle(Brushes.Transparent, null, new Rect(this.RenderSize));
        }

        #endregion

        #region mouse overrides
        protected override void OnMouseMove(System.Windows.Input.MouseEventArgs e)
        {
            updateLastMouse(e.GetPosition(this));
        }
        protected override void OnMouseLeave(MouseEventArgs e)
        {
            updateLastMouse(new Point(double.NaN, double.NaN));
        }

        #endregion

        #region implementation

        private void updateLastMouse(Point point)
        {
            if (point != m_lastMouse)
            {
                m_lastMouse = point;
                m_listener.StartListening();
            }
        }

        private void tileClicked(FlipTile tileData)
        {
            m_isFlipped = !m_isFlipped;
            if (m_isFlipped)
            {
                m_backMaterial.Brush = tileData.FrontBrush;
            }
        }

        private void setup3D()
        {
            PerspectiveCamera camera = new PerspectiveCamera(
                new Point3D(0, 0, 3.73), //position
                new Vector3D(0, 0, -1), //lookDirection
                new Vector3D(0, 1, 0), //upDirection
                30 //FOV
                );

            WrappedElement.Camera = camera;

            Model3DGroup everything = new Model3DGroup();

            Model3DGroup lights = new Model3DGroup();
            DirectionalLight whiteLight = new DirectionalLight(Colors.White, new Vector3D(0, 0, -1));
            lights.Children.Add(whiteLight);

            everything.Children.Add(lights);

            ModelVisual3D model = new ModelVisual3D();

            double tileSizeX = 2.0 / c_xCount;
            double startX = -((double)c_xCount) / 2 * tileSizeX + tileSizeX / 2;
            double startY = -((double)c_yCount) / 2 * tileSizeX + tileSizeX / 2;

            int index;

            Size tileTextureSize = new Size(1.0 / c_xCount, 1.0 / c_yCount);

            //so, tiles are numbers, left-to-right (ascending x), bottom-to-top (ascending y)
            for (int y = 0; y < c_yCount; y++)
            {
                for (int x = 0; x < c_xCount; x++)
                {
                    index = y * c_xCount + x;

                    Rect backTextureCoordinates = new Rect(
                        x * tileTextureSize.Width,

                        // this will give you a headache. Exists since we are going
                        // from bottom bottomLeft of 3D space (negative Y is down),
                        // but texture coor are negative Y is up
                        1 - y * tileTextureSize.Height - tileTextureSize.Height,

                        tileTextureSize.Width, tileTextureSize.Height);

                    m_tiles[index] = new FlipTile(
                        getMaterial(index),
                        new Size(tileSizeX, tileSizeX),
                        new Point(startX + x * tileSizeX, startY + y * tileSizeX),
                        m_backMaterial,
                        backTextureCoordinates);

                    m_tiles[index].Click += (sender, args) => tileClicked((FlipTile)sender);

                    WrappedElement.Children.Add(m_tiles[index]);
                }
            }

            model.Content = everything;

            WrappedElement.Children.Add(model);

            //start the per-frame tick for the physics
            m_listener.StartListening();
        }

        private void tick(object sender, EventArgs e)
        {
            bool keepTicking = false;
            if (m_tiles[0] != null)
            {
                Vector mouseFixed = fixMouse(m_lastMouse, this.RenderSize);
                for (int i = 0; i < m_tiles.Length; i++)
                {
                    keepTicking = m_tiles[i].TickData(mouseFixed, m_isFlipped) || keepTicking;
                }
            }
            if (!keepTicking)
            {
                m_listener.StopListening();
            }
        }

        private DiffuseMaterial getMaterial(int index)
        {
            return m_materials[index % m_materials.Count];
        }

        private readonly ReadOnlyCollection<DiffuseMaterial> m_materials = GetSamplePictures();

        private static ReadOnlyCollection<DiffuseMaterial> GetSamplePictures()
        {
            IList<string> files = SampleImageHelper.GetPicturePaths().ToList();
            if (files.Count > 0)
            {
                return files
                  .Select(file =>
                  {
                      BitmapImage bitmapImage = new BitmapImage();
                      bitmapImage.BeginInit();
                      bitmapImage.UriSource = new Uri(file);
                      bitmapImage.DecodePixelWidth = 320;
                      bitmapImage.DecodePixelHeight = 240;
                      bitmapImage.EndInit();
                      bitmapImage.Freeze();

                      ImageBrush imageBrush = new ImageBrush(bitmapImage);
                      imageBrush.Stretch = Stretch.UniformToFill;
                      imageBrush.ViewportUnits = BrushMappingMode.Absolute;
                      imageBrush.Freeze();

                      return new DiffuseMaterial(imageBrush);
                  })
                  .ToReadOnlyCollection();
            }
            else
            {
                return (new Brush[] { Brushes.LightBlue, Brushes.Pink, Brushes.LightGray, Brushes.Yellow, Brushes.Orange, Brushes.LightGreen })
                            .Select(brush => new DiffuseMaterial(brush))
                            .ToReadOnlyCollection();
            }
        }

        private Point m_lastMouse = new Point(double.NaN, double.NaN);
        private bool m_isFlipped;

        private readonly FlipTile[] m_tiles = new FlipTile[c_xCount * c_yCount];
        private readonly DiffuseMaterial m_backMaterial = new DiffuseMaterial();
        private readonly CompositionTargetRenderingListener m_listener =
            new CompositionTargetRenderingListener();

        private static Vector fixMouse(Point mouse, Size size)
        {
            Debug.Assert(size.Width >= 0 && size.Height >= 0);
            double scale = Math.Max(size.Width, size.Height) / 2;

            // Translate y going down to y going up
            mouse.Y = -mouse.Y + size.Height;

            mouse.Y -= size.Height / 2;
            mouse.X -= size.Width / 2;

            Vector v = new Vector(mouse.X, mouse.Y);

            v /= scale;

            return v;
        }

        private const int c_xCount = 7, c_yCount = 7;

        #endregion
    }
}