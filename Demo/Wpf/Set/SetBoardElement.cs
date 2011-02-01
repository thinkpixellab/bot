using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using PixelLab.Common;
#if CONTRACTS_FULL
using System.Diagnostics.Contracts;
#else
using PixelLab.Contracts;
#endif

namespace PixelLab.Wpf.Demo.Set
{
    public class SetBoardElement : WrapperElement<Viewport3D>, IDisposable
    {
        static SetBoardElement()
        {
            EventManager.RegisterClassHandler(
                typeof(SetBoardElement),
                SetCard3D.ClickEvent,
                new RoutedEventHandler(
                delegate(object sender, RoutedEventArgs e)
                {
                    ((SetBoardElement)sender).OnCardClick(e);
                }));
        }

        public SetBoardElement()
            : base(new Viewport3D())
        {
            int row = 0, column = 0;

            Vector3D offset = new Vector3D(
                -s_setCard3DSize.Width * 1.5 - 1.5 * c_setCardMargin,
                s_setCard3DSize.Height + c_setCardMargin,
                0);

            Vector3D delta = new Vector3D(s_setCard3DSize.Width + c_setCardMargin, -(s_setCard3DSize.Height + c_setCardMargin), 0);

            for (int i = 0; i < m_cards.Count; i++)
            {
                row = i / 4;
                column = i % 4;

                m_cards[i] = new SetCard3D();
                m_cards[i].TranslateTransform3D.SetToVector(offset + new Vector3D(delta.X * column, delta.Y * row, 0));

                this.WrappedElement.Children.Add(m_cards[i]);
            }

            WrappedElement.ClipToBounds = false;

            // camera to ue
            PerspectiveCamera camera = new PerspectiveCamera();
            camera.Position = new Point3D(0, 0, 500);
            WrappedElement.Camera = camera;

            ModelVisual3D modelView3DPointLight1 = new ModelVisual3D();
            modelView3DPointLight1.Content = new PointLight(Color.FromRgb(255, 255, 255), new Point3D(-100, 200, 500));
            WrappedElement.Children.Add(modelView3DPointLight1);

            ModelVisual3D modelView3DPointLight2 = new ModelVisual3D();
            modelView3DPointLight2.Content = new PointLight(Color.FromRgb(45, 45, 50), new Point3D(100, -200, 500));
            WrappedElement.Children.Add(modelView3DPointLight2);

            m_listener.Rendering += m_listener_Rendering;
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            base.MeasureOverride(s_setBoardElementSize);
            return s_setBoardElementSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            base.ArrangeOverride(s_setBoardElementSize);
            return s_setBoardElementSize;
        }

        public void ProvideGame(SetGame set)
        {
            Contract.Requires<ArgumentNullException>(set != null);

            VerifyAccess();
            if (m_set != null)
            {
                throw new InvalidOperationException("Can only set the game once.");
            }

            m_set = set;
            m_set.PropertyChanged += delegate(object sender, PropertyChangedEventArgs e)
            {
                resetMaterials();
            };

            resetMaterials();
        }

        public void ResetCards(bool animate)
        {
            m_cards.ForEach(card => card.IsChecked = false);

            if (animate)
            {
                m_cards.ForEach(card => card.CurrentZVelocity = -50);
            }

            m_listener.StartListening();
        }

        protected virtual void OnCardClick(RoutedEventArgs e)
        {
            SetCard3D card = (SetCard3D)e.OriginalSource;

            if (m_set.CanPlay)
            {
                m_listener.StartListening();

                if (m_checkedCards.Contains(card) && !card.IsChecked)
                {
                    m_checkedCards.Remove(card);

                    card.CurrentZVelocity = -10;
                }
                else if (!m_checkedCards.Contains(card) && card.IsChecked)
                {
                    m_checkedCards.Add(card);

                    card.CurrentZVelocity = -20;
                }

                Debug.Assert(m_checkedCards.Count <= 3);

                if (m_checkedCards.Count == 3)
                {
                    int index0 = m_cards.IndexOf(m_checkedCards[0]);
                    int index1 = m_cards.IndexOf(m_checkedCards[1]);
                    int index2 = m_cards.IndexOf(m_checkedCards[2]);

                    Debug.Assert(index0 != index1
                        && index0 != index2
                        && index1 != index2);

                    bool goodPlay = m_set.TryPlay(index0, index1, index2);

                    m_checkedCards.Clear();
                    m_cards[index0].IsChecked = false;
                    m_cards[index1].IsChecked = false;
                    m_cards[index2].IsChecked = false;

                    double velocity = goodPlay ? -50 : -20;

                    m_cards[index0].CurrentZVelocity = velocity;
                    m_cards[index1].CurrentZVelocity = velocity;
                    m_cards[index2].CurrentZVelocity = velocity;
                }
            } // if can play
            else
            {
                card.IsChecked = false;
            }
        } //*** void OnCardClick

        protected override void OnMouseMove(MouseEventArgs e)
        {
            m_listener.StartListening();
        }

        public void Dispose()
        {
            if (!m_listener.IsDisposed)
            {
                m_listener.Dispose();
            }
        }

        #region Implementation

        private void m_listener_Rendering(object sender, EventArgs e)
        {
            bool stillAnimating = false;

            foreach (SetCard3D card in m_cards)
            {
                double zTranslateTarget = (card.IsChecked) ? 50 : 0;
                zTranslateTarget += (card.IsMouseOver) ? -10 : 0;

                double attractionFactor = .05;
                attractionFactor *= 1 + (card.Variation - .5);

                double newTranslate, newVelocity;
                stillAnimating = GeoHelper.Animate(card.TranslateTransform3D.OffsetZ, card.CurrentZVelocity, zTranslateTarget,
                    attractionFactor, .3, 1000, .00001, .00001,
                    out newTranslate, out newVelocity) || stillAnimating;

                card.CurrentZVelocity = newVelocity;
                card.TranslateTransform3D.OffsetZ = newTranslate;
            }

            if (!stillAnimating)
            {
                m_listener.StopListening();
            }
        }

        private void resetMaterials()
        {
            if (m_set != null)
            {
                for (int i = 0; i < m_cards.Count; i++)
                {
                    SetCard card = m_set[i];
                    m_cards[i].SetCard(card);
                }
            }
        }

        private SetGame m_set;

        private readonly IList<SetCard3D> m_cards = new SetCard3D[12];
        private readonly List<SetCard3D> m_checkedCards = new List<SetCard3D>();

        private readonly Size s_setBoardElementSize = new Size(400, 420);
        private readonly Size s_setCard3DSize = new Size(100, 140);

        private readonly CompositionTargetRenderingListener m_listener = new CompositionTargetRenderingListener();

        private const double c_setCardMargin = 5;

        private class SetCard3D : ToggleButton3D
        {
            public SetCard3D()
            {
                DiffuseMaterial material = new DiffuseMaterial(Brushes.LightGray);

                this.Visual3DModel = m_geometry = GetQuad(
                    new Point3D(-50, -70, 0), new Point3D(50, -70, 0), new Point3D(50, 70, 0), new Point3D(-50, 70, 0),
                    material, material, new Rect(0, 0, 1, 1));

                this.Transform = TranslateTransform3D;
            }

            public void SetCard(SetCard card)
            {
                if (m_card != card)
                {
                    m_card = card;

                    if (m_card != null)
                    {
                        Drawing drawing = SetCardDrawingFactory.GetFullCardDrawing(m_card);
                        DrawingBrush brush = new DrawingBrush(drawing);
                        DiffuseMaterial material = new DiffuseMaterial(brush);
                        material.Freeze();

                        m_geometry.Material = material;
                    }
                    else
                    {
                        m_geometry.Material = new DiffuseMaterial(Brushes.Transparent);
                    }
                }
            }

            public double CurrentZVelocity;

            public readonly double Variation = Util.Rnd.NextDouble();

            private static GeometryModel3D GetQuad(
                Point3D bottomLeft, Point3D bottomRight, Point3D topRight, Point3D topLeft,
                Material material, Material backMaterial, Rect textureCoordinates)
            {
                MeshGeometry3D mesh = new MeshGeometry3D();
                mesh.Positions.Add(bottomLeft);
                mesh.Positions.Add(bottomRight);
                mesh.Positions.Add(topRight);
                mesh.Positions.Add(topLeft);

                mesh.TriangleIndices.Add(0);
                mesh.TriangleIndices.Add(1);
                mesh.TriangleIndices.Add(2);

                mesh.TriangleIndices.Add(2);
                mesh.TriangleIndices.Add(3);
                mesh.TriangleIndices.Add(0);

                mesh.TextureCoordinates.Add(textureCoordinates.BottomLeft);
                mesh.TextureCoordinates.Add(textureCoordinates.BottomRight);
                mesh.TextureCoordinates.Add(textureCoordinates.TopRight);
                mesh.TextureCoordinates.Add(textureCoordinates.TopLeft);

                GeometryModel3D gm3d = new GeometryModel3D(mesh, material);
                gm3d.BackMaterial = backMaterial;

                return gm3d;
            }

            private readonly GeometryModel3D m_geometry;

            private SetCard m_card;

            public readonly TranslateTransform3D TranslateTransform3D = new TranslateTransform3D();
        }

        #endregion
    }

    public class ToggleButton3D : ButtonBase3D
    {
        public bool IsChecked
        {
            get { return (bool)GetValue(IsCheckedProperty); }
            set { SetValue(IsCheckedProperty, value); }
        }

        public static readonly DependencyProperty IsCheckedProperty =
            DependencyProperty.Register("IsChecked", typeof(bool), typeof(ToggleButton3D), new PropertyMetadata(false));

        protected override void OnClick()
        {
            OnToggle();
            base.OnClick();
        }

        protected virtual void OnToggle()
        {
            IsChecked = !IsChecked;
        }
    }
}