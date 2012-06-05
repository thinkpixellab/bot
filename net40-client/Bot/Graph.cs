using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Animation;
using PixelLab.Common;

namespace PixelLab.Wpf
{
    public class Graph : FrameworkElement
    {
        static Graph()
        {
            ClipToBoundsProperty.OverrideMetadata(typeof(Graph), new FrameworkPropertyMetadata(true));
        }

        public Graph()
        {
            m_listener.WireParentLoadedUnloaded(this);

            m_listener.Rendering += compositionTarget_rendering;

            m_nodeTemplateBinding = new Binding(NodeTemplateProperty.Name);
            m_nodeTemplateBinding.Source = this;

            m_nodeTemplateSelectorBinding = new Binding(NodeTemplateSelectorProperty.Name);
            m_nodeTemplateSelectorBinding.Source = this;

            m_nodePresenters = new List<GraphContentPresenter>();
        }

        #region overrides
        protected override Size MeasureOverride(Size availableSize)
        {
            handleChanges();
            m_measureInvalidated = true;

            m_listener.StartListening();

            Children.ForEach(gcp => gcp.Measure(GeoHelper.SizeInfinite));

            return new Size();
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            m_controlCenterPoint = (Point)(.5 * (Vector)finalSize);
            Children.ForEach(gcp => gcp.Arrange(new Rect(gcp.DesiredSize)));
            return finalSize;
        }

        protected override int VisualChildrenCount
        {
            get
            {
                return m_nodePresenters.Count + m_fadingGCPList.Count + ((m_centerGraphContentPresenter == null) ? 0 : 1);
            }
        }

        protected override Visual GetVisualChild(int index)
        {
            if (index < m_fadingGCPList.Count)
            {
                return m_fadingGCPList[index];
            }
            index -= m_fadingGCPList.Count;

            if (index < m_nodePresenters.Count)
            {
                return m_nodePresenters[index];
            }
            index -= m_nodePresenters.Count;

            if (index == 0)
            {
                return m_centerGraphContentPresenter;
            }
            else
            {
                throw new ArgumentException("not a valid index");
            }
        }
        protected override void OnRender(DrawingContext drawingContext)
        {
            if (LinePen != null && m_centerGraphContentPresenter != null)
            {
                var pen = LinePen;
                m_nodePresenters.ForEach(gcp => drawingContext.DrawLine(pen, m_centerGraphContentPresenter.ActualLocation, gcp.ActualLocation));
            }
        }
        #endregion

        #region properties

        #region CenterObject
        public object CenterObject
        {
            get { return GetValue(CenterObjectProperty); }
            set { SetValue(CenterObjectProperty, value); }
        }
        public static readonly DependencyProperty CenterObjectProperty = DependencyProperty.Register(
            "CenterObject", typeof(object), typeof(Graph), getCenterObjectPropertyMetadata());

        #region CenterObject Impl

        private static PropertyMetadata getCenterObjectPropertyMetadata()
        {
            FrameworkPropertyMetadata fpm = new FrameworkPropertyMetadata();
            fpm.AffectsMeasure = true;
            fpm.PropertyChangedCallback = new PropertyChangedCallback(CenterObjectPropertyChanged);
            return fpm;
        }

        private static void CenterObjectPropertyChanged(DependencyObject element, DependencyPropertyChangedEventArgs args)
        {
            ((Graph)element).CenterObjectPropertyChanged();
        }

        private void CenterObjectPropertyChanged()
        {
            m_centerChanged = true;
            resetNodesBinding();
        }

        #endregion

        #endregion

        #region NodesBindingPath
        public string NodesBindingPath
        {
            get
            {
                return (string)GetValue(NodesBindingPathProperty);
            }
            set
            {
                SetValue(NodesBindingPathProperty, value);
            }
        }
        public static readonly DependencyProperty NodesBindingPathProperty =
            DependencyProperty.Register("NodesBindingPath",
            typeof(string), typeof(Graph),
            new FrameworkPropertyMetadata(new PropertyChangedCallback(NodesBindingPathPropertyChanged)));

        private static void NodesBindingPathPropertyChanged(DependencyObject element, DependencyPropertyChangedEventArgs e)
        {
            Graph g = (Graph)element;
            g.resetNodesBinding();
        }
        #endregion

        #region NodeTemplate
        public DataTemplate NodeTemplate
        {
            get
            {
                return (DataTemplate)GetValue(NodeTemplateProperty);
            }
            set
            {
                SetValue(NodeTemplateProperty, value);
            }
        }

        public static readonly DependencyProperty NodeTemplateProperty = DependencyProperty.Register(
            "NodeTemplate", typeof(DataTemplate), typeof(Graph), new FrameworkPropertyMetadata(null));
        #endregion

        #region NodeTemplateSelector
        public DataTemplateSelector NodeTemplateSelector
        {
            get
            {
                return (DataTemplateSelector)GetValue(NodeTemplateSelectorProperty);
            }
            set
            {
                SetValue(NodeTemplateSelectorProperty, value);
            }
        }

        public static readonly DependencyProperty NodeTemplateSelectorProperty = DependencyProperty.Register(
            "NodeTemplateSelector", typeof(DataTemplateSelector), typeof(Graph), new FrameworkPropertyMetadata(null));
        #endregion

        #region Dampening
        public double Dampening
        {
            get
            {
                return (double)GetValue(DampeningProperty);
            }
            set
            {
                SetValue(DampeningProperty, value);
            }
        }

        public static readonly DependencyProperty DampeningProperty =
            DependencyProperty.Register("Dampening", typeof(double), typeof(Graph),
            new FrameworkPropertyMetadata(.9, null, new CoerceValueCallback(CoerceDampeningPropertyCallback)));

        private static object CoerceDampeningPropertyCallback(DependencyObject element, object baseValue)
        {
            return CoerceDampeningPropertyCallback((double)baseValue);
        }

        private static double CoerceDampeningPropertyCallback(double baseValue)
        {
            if (baseValue <= c_minDampening)
            {
                return c_minDampening;
            }
            else if (baseValue >= c_maxDampening)
            {
                return c_maxDampening;
            }
            else
            {
                return baseValue;
            }
        }
        #endregion

        #region Attraction
        public double Attraction
        {
            get { return (double)GetValue(AttractionProperty); }
            set
            {
                SetValue(AttractionProperty, value);
            }
        }

        public static readonly DependencyProperty AttractionProperty =
            DependencyProperty.Register("Attraction", typeof(double), typeof(Graph),
            new FrameworkPropertyMetadata(.4, null, new CoerceValueCallback(CoerceAttractionPropertyCallback)));

        private static object CoerceAttractionPropertyCallback(DependencyObject element, object baseValue)
        {
            return CoerceAttractionPropertyCallback((double)baseValue);
        }

        private static double CoerceAttractionPropertyCallback(double baseValue)
        {
            if (baseValue <= c_minDampening)
            {
                return c_minDampening;
            }
            else if (baseValue >= c_maxDampening)
            {
                return c_maxDampening;
            }
            else
            {
                return baseValue;
            }
        }

        #endregion

        #region Line brush

        public Pen LinePen
        {
            get { return (Pen)GetValue(LinePenProperty); }
            set { SetValue(LinePenProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LinePen.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LinePenProperty =
            DependencyProperty.Register("LinePen", typeof(Pen), typeof(Graph), new PropertyMetadata(GetPen()));

        private static Pen GetPen()
        {
            if (DefaultPen == null)
            {
                DefaultPen = new Pen(Brushes.Gray, 1);
                DefaultPen.Freeze();
            }
            return DefaultPen;
        }

        private static Pen DefaultPen;

        #endregion

        #region IsCenter

        private static readonly DependencyPropertyKey IsCentersCenterPropertyKey =
            DependencyProperty.RegisterAttachedReadOnly("IsCenter", typeof(bool), typeof(Graph), new FrameworkPropertyMetadata(false));

        public static readonly DependencyProperty IsCenterProperty = IsCentersCenterPropertyKey.DependencyProperty;

        public static bool GetIsCenter(ContentPresenter element) { return (bool)element.GetValue(IsCenterProperty); }
        private static void SetIsCenter(ContentPresenter element, bool value) { element.SetValue(IsCentersCenterPropertyKey, value); }

        #endregion

        #endregion

        #region Implementation

        private IEnumerable<GraphContentPresenter> Children
        {
            get
            {
                for (int i = 0; i < VisualChildrenCount; i++)
                {
                    yield return (GraphContentPresenter)GetVisualChild(i);
                }
            }
        }

        private void nodesChangedHandler(object sender, NotifyCollectionChangedEventArgs e)
        {
            VerifyAccess();
            InvalidateMeasure();
            m_nodeCollectionChanged = true;
        }

        private void resetNodesBinding()
        {
            if (NodesBindingPath == null)
            {
                BindingOperations.ClearBinding(this, NodesProperty);
            }
            else
            {
                Binding theBinding = GetBinding(NodesBindingPath, this.CenterObject);
                if (theBinding == null)
                {
                    BindingOperations.ClearBinding(this, NodesProperty);
                }
                else
                {
                    BindingOperations.SetBinding(this, NodesProperty, theBinding);
                }
            }
        }

        private void compositionTarget_rendering(object sender, EventArgs args)
        {
            bool _somethingInvalid = false;
            if (m_measureInvalidated || m_stillMoving)
            {
                if (m_measureInvalidated)
                {
                    m_milliseconds = Environment.TickCount;
                }

                #region CenterObject
                if (m_centerGraphContentPresenter != null)
                {
                    if (m_centerGraphContentPresenter.New)
                    {
                        m_centerGraphContentPresenter.ParentCenter = m_controlCenterPoint;
                        m_centerGraphContentPresenter.New = false;
                        _somethingInvalid = true;
                    }
                    else
                    {
                        Vector forceVector = GetAttractionForce(
                            ensureNonzeroVector((Vector)m_centerGraphContentPresenter.Location));

                        if (updateGraphCP(m_centerGraphContentPresenter, forceVector, Dampening, Attraction, m_controlCenterPoint))
                        {
                            _somethingInvalid = true;
                        }
                    }
                }
                #endregion

                Point centerLocationToUse = (m_centerGraphContentPresenter != null) ? m_centerGraphContentPresenter.Location : new Point();

                GraphContentPresenter gcp;
                for (int i = 0; i < m_nodePresenters.Count; i++)
                {
                    Vector forceVector = new Vector();
                    gcp = m_nodePresenters[i];

                    if (gcp.New)
                    {
                        gcp.New = false;
                        _somethingInvalid = true;
                    }

                    for (int j = 0; j < m_nodePresenters.Count; j++)
                    {
                        if (j != i)
                        {
                            Vector distance = ensureNonzeroVector(gcp.Location - m_nodePresenters[j].Location);
                            Vector repulsiveForce = GetRepulsiveForce(distance);

                            forceVector += repulsiveForce;
                        }
                    }

                    forceVector += GetSpringForce(ensureNonzeroVector(m_nodePresenters[i].Location - centerLocationToUse));

                    if (updateGraphCP(m_nodePresenters[i], forceVector, Dampening, Attraction, m_controlCenterPoint))
                    {
                        _somethingInvalid = true;
                    }
                }

                #region animate all of the fading ones away
                for (int i = 0; i < m_fadingGCPList.Count; i++)
                {
                    if (!m_fadingGCPList[i].WasCenter)
                    {
                        Vector centerDiff = ensureNonzeroVector(m_fadingGCPList[i].Location - centerLocationToUse);
                        centerDiff.Normalize();
                        centerDiff *= 20;
                        if (updateGraphCP(m_fadingGCPList[i], centerDiff, Dampening, Attraction, m_controlCenterPoint))
                        {
                            _somethingInvalid = true;
                        }
                    }
                }

                #endregion

                if (_somethingInvalid && belowMaxSettleTime())
                {
                    m_stillMoving = true;
                    InvalidateVisual();
                }
                else
                {
                    m_stillMoving = false;
                    m_listener.StopListening();
                }
                m_measureInvalidated = false;
            }
        }

        private bool belowMaxSettleTime()
        {
            Debug.Assert(m_milliseconds != int.MinValue);

            return s_maxSettleTime > TimeSpan.FromMilliseconds(Environment.TickCount - m_milliseconds);
        }

        private static Vector ensureNonzeroVector(Vector vector)
        {
            if (vector.Length > 0)
            {
                return vector;
            }
            else
            {
                return new Vector(Util.Rnd.NextDouble() - .5, Util.Rnd.NextDouble() - .5);
            }
        }

        private static bool updateGraphCP(GraphContentPresenter graphContentPresenter, Vector forceVector,
                            double coefficientOfDampening, double frameRate, Point parentCenter)
        {
            bool parentCenterChanged = (graphContentPresenter.ParentCenter != parentCenter);
            if (parentCenterChanged)
            {
                graphContentPresenter.ParentCenter = parentCenter;
            }

            //add system drag
            Debug.Assert(coefficientOfDampening > 0);
            Debug.Assert(coefficientOfDampening < 1);
            graphContentPresenter.Velocity *= (1 - coefficientOfDampening * frameRate);

            //add force
            graphContentPresenter.Velocity += (forceVector * frameRate);

            //apply terminalVelocity
            if (graphContentPresenter.Velocity.Length > s_terminalVelocity)
            {
                graphContentPresenter.Velocity *= (s_terminalVelocity / graphContentPresenter.Velocity.Length);
            }

            if (graphContentPresenter.Velocity.Length > s_minVelocity && forceVector.Length > s_minVelocity)
            {
                graphContentPresenter.Location += (graphContentPresenter.Velocity * frameRate);
                return true;
            }
            else
            {
                graphContentPresenter.Velocity = new Vector();
                return false || parentCenterChanged;
            }
        }

        private void beginRemoveAnimation(GraphContentPresenter graphContentPresenter, bool isCenter)
        {
            Debug.Assert(VisualTreeHelper.GetParent(graphContentPresenter) == this);

            this.InvalidateVisual();

            m_fadingGCPList.Add(graphContentPresenter);

            graphContentPresenter.IsHitTestVisible = false;
            if (isCenter)
            {
                graphContentPresenter.WasCenter = true;
            }

            ScaleTransform scaleTransform = graphContentPresenter.ScaleTransform;

            DoubleAnimation doubleAnimation = new DoubleAnimation(0, s_hideDuration);
            doubleAnimation.Completed +=
                delegate(object sender, EventArgs e)
                {
                    CleanUpGCP(graphContentPresenter);
                };
            doubleAnimation.FillBehavior = FillBehavior.Stop;
            doubleAnimation.Freeze();

            scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, doubleAnimation);
            scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, doubleAnimation);
            graphContentPresenter.BeginAnimation(OpacityProperty, doubleAnimation);
        }

        private void CleanUpGCP(GraphContentPresenter contentPresenter)
        {
            if (m_fadingGCPList.Contains(contentPresenter))
            {
                Debug.Assert(VisualTreeHelper.GetParent(contentPresenter) == this);

                this.RemoveVisualChild(contentPresenter);
                m_fadingGCPList.Remove(contentPresenter);
            }
        }

        private void handleChanges()
        {
            handleNodesChangedWiring();

            if (m_centerChanged && m_nodeCollectionChanged &&
                CenterObject != null &&
                m_centerGraphContentPresenter != null
                )
            {
                Debug.Assert(!CenterObject.Equals(m_centerDataInUse));
                Debug.Assert(m_centerGraphContentPresenter.Content == null || m_centerGraphContentPresenter.Content.Equals(m_centerDataInUse));

                m_centerDataInUse = CenterObject;

                //figure out if we can re-cycle one of the existing children as the center Node
                //if we can, newCenter != null
                GraphContentPresenter newCenterPresenter = null;
                for (int i = 0; i < m_nodePresenters.Count; i++)
                {
                    if (m_nodePresenters[i].Content.Equals(CenterObject))
                    {
                        //we should re-use this
                        newCenterPresenter = m_nodePresenters[i];
                        m_nodePresenters[i] = null;
                        break;
                    }
                }

                //figure out if we can re-cycle the exsting center as one of the new child nodes
                //if we can, newChild != null && newChildIndex == indexOf(data in Nodes)
                int newChildIndex = -1;
                GraphContentPresenter newChildPresenter = null;
                for (int i = 0; i < m_nodesInUse.Count; i++)
                {
                    if (m_nodesInUse[i] != null && m_centerGraphContentPresenter.Content != null && m_nodesInUse[i].Equals(m_centerGraphContentPresenter.Content))
                    {
                        newChildIndex = i;
                        newChildPresenter = m_centerGraphContentPresenter;
                        m_centerGraphContentPresenter = null;
                        break;
                    }
                }

                //now we potentially have a center (or not) and one edge(or not)
                GraphContentPresenter[] newChildren = new GraphContentPresenter[m_nodesInUse.Count];

                //we did all the work to see if the current cernter can be reused.
                //if it can, use it
                if (newChildPresenter != null)
                {
                    newChildren[newChildIndex] = newChildPresenter;
                }

                //now go through all the existing children and place them in newChildren
                //if they match
                for (int i = 0; i < m_nodesInUse.Count; i++)
                {
                    if (newChildren[i] == null)
                    {
                        for (int j = 0; j < m_nodePresenters.Count; j++)
                        {
                            if (m_nodePresenters[j] != null)
                            {
                                if (m_nodesInUse[i].Equals(m_nodePresenters[j].Content))
                                {
                                    Debug.Assert(newChildren[i] == null);
                                    newChildren[i] = m_nodePresenters[j];
                                    m_nodePresenters[j] = null;
                                    break;
                                }
                            }
                        }
                    }
                }

                //we've now reused everything we can
                if (m_centerGraphContentPresenter == null)
                {
                    //we didn't find anything to recycle
                    //create a new one
                    if (newCenterPresenter == null)
                    {
                        m_centerGraphContentPresenter = GetGraphContentPresenter(
                            CenterObject,
                            m_nodeTemplateBinding,
                            m_nodeTemplateSelectorBinding,
                            false
                            );
                        this.AddVisualChild(m_centerGraphContentPresenter);
                    }
                    else
                    { //we did find something to recycle. Use it.
                        m_centerGraphContentPresenter = newCenterPresenter;
                        Debug.Assert(VisualTreeHelper.GetParent(newCenterPresenter) == this);
                    }
                }
                else
                {
                    if (newCenterPresenter == null)
                    {
                        m_centerGraphContentPresenter.Content = CenterObject;
                    }
                    else
                    {
                        beginRemoveAnimation(m_centerGraphContentPresenter, true);
                        m_centerGraphContentPresenter = newCenterPresenter;
                        Debug.Assert(VisualTreeHelper.GetParent(newCenterPresenter) == this);
                    }
                }

                //go through all of the old CPs that are not being used and remove them
                m_nodePresenters
                  .Where(gcp => gcp != null)
                  .ForEach(gcp => beginRemoveAnimation(gcp, false));

                //go through and "fill in" all the new CPs
                for (int i = 0; i < m_nodesInUse.Count; i++)
                {
                    if (newChildren[i] == null)
                    {
                        GraphContentPresenter gcp = GetGraphContentPresenter(m_nodesInUse[i],
                            m_nodeTemplateBinding, m_nodeTemplateSelectorBinding, true);
                        this.AddVisualChild(gcp);
                        newChildren[i] = gcp;
                    }
                }

                m_nodePresenters.Clear();
                m_nodePresenters.AddRange(newChildren);

                m_centerChanged = false;
                m_nodeCollectionChanged = false;
            }
            else
            {
                if (m_centerChanged)
                {
                    m_centerDataInUse = CenterObject;
                    if (m_centerGraphContentPresenter != null)
                    {
                        Debug.Assert(m_centerDataInUse == null);
                        beginRemoveAnimation(m_centerGraphContentPresenter, true);
                        m_centerGraphContentPresenter = null;
                    }
                    if (m_centerDataInUse != null)
                    {
                        SetUpCleanCenter(m_centerDataInUse);
                    }
                    m_centerChanged = false;
                }

                if (m_nodeCollectionChanged)
                {
                    setupNodes(Nodes);

                    m_nodesInUse = Nodes;

                    m_nodeCollectionChanged = false;
                }
            }

#if DEBUG
      if (CenterObject != null) {
        CenterObject.Equals(m_centerDataInUse);
        Debug.Assert(m_centerGraphContentPresenter != null);
      }
      else {
        Debug.Assert(m_centerDataInUse == null);
      }
      if (Nodes != null) {
        Debug.Assert(m_nodePresenters != null);
        Debug.Assert(Nodes.Count == m_nodePresenters.Count);
        Debug.Assert(m_nodesInUse == Nodes);
      }
      else {
        Debug.Assert(m_nodesInUse == null);
        if (m_nodePresenters != null) {
          Debug.Assert(m_nodePresenters.Count == 0);
        }
      }
#endif

            Children.ForEach(gcp => SetIsCenter(gcp, gcp == m_centerGraphContentPresenter));
        }

        private void handleNodesChangedWiring()
        {
            if (m_nodesChanged)
            {
                INotifyCollectionChanged oldList = m_nodesInUse as INotifyCollectionChanged;
                if (oldList != null)
                {
                    oldList.CollectionChanged -= nodesChangedHandler;
                }

                INotifyCollectionChanged newList = Nodes as INotifyCollectionChanged;
                if (newList != null)
                {
                    newList.CollectionChanged += nodesChangedHandler;
                }

                m_nodesInUse = Nodes;
                m_nodesChanged = false;
            }
        }

        private void setupNodes(IList nodes)
        {
#if DEBUG
      for (int i = 0; i < m_nodePresenters.Count; i++) {
        Debug.Assert(m_nodePresenters[i] != null);
        Debug.Assert(VisualTreeHelper.GetParent(m_nodePresenters[i]) == this);
      }
#endif

            int nodesCount = (nodes == null) ? 0 : nodes.Count;

            GraphContentPresenter[] newNodes = new GraphContentPresenter[nodesCount];
            for (int i = 0; i < nodesCount; i++)
            {
                for (int j = 0; j < m_nodePresenters.Count; j++)
                {
                    if (m_nodePresenters[j] != null)
                    {
                        if (nodes[i] == m_nodePresenters[j].Content)
                        {
                            newNodes[i] = m_nodePresenters[j];
                            m_nodePresenters[j] = null;
                            break;
                        }
                    }
                }
            }

            for (int i = 0; i < m_nodePresenters.Count; i++)
            {
                if (m_nodePresenters[i] != null)
                {
                    beginRemoveAnimation(m_nodePresenters[i], false);
                    m_nodePresenters[i] = null;
                }
            }

            for (int i = 0; i < newNodes.Length; i++)
            {
                if (newNodes[i] == null)
                {
                    newNodes[i] = GetGraphContentPresenter(nodes[i],
                        m_nodeTemplateBinding, m_nodeTemplateSelectorBinding, true);
                    this.AddVisualChild(newNodes[i]);
                }
            }

#if DEBUG
      m_nodePresenters.ForEach(item => Debug.Assert(item == null));
      newNodes.CountForEach((item, i) => {
        Debug.Assert(item != null);
        Debug.Assert(VisualTreeHelper.GetParent(item) == this);
        Debug.Assert(item.Content == nodes[i]);
      });
#endif

            m_nodePresenters.Clear();
            m_nodePresenters.AddRange(newNodes);
        }

        private void SetUpCleanCenter(object newCenter)
        {
            Debug.Assert(m_centerGraphContentPresenter == null);

            m_centerGraphContentPresenter = GetGraphContentPresenter(newCenter, m_nodeTemplateBinding, m_nodeTemplateSelectorBinding, false);
            this.AddVisualChild(m_centerGraphContentPresenter);
        }

        #region private Nodes property

        private IList Nodes
        {
            get
            {
                return (IList)GetValue(NodesProperty);
            }
        }
        private static readonly DependencyProperty NodesProperty = DependencyProperty.Register(
            "Nodes", typeof(IList), typeof(Graph), getNodesPropertyMetadata());

        private static PropertyMetadata getNodesPropertyMetadata()
        {
            FrameworkPropertyMetadata fpm = new FrameworkPropertyMetadata();
            fpm.AffectsMeasure = true;
            fpm.PropertyChangedCallback = new PropertyChangedCallback(NodesPropertyChanged);
            return fpm;
        }

        private static void NodesPropertyChanged(DependencyObject element, DependencyPropertyChangedEventArgs args)
        {
            ((Graph)element).NodesPropertyChanged();
        }

        private void NodesPropertyChanged()
        {
            m_nodeCollectionChanged = true;
            m_nodesChanged = true;
        }

        #endregion

        private static GraphContentPresenter GetGraphContentPresenter(object content, BindingBase nodeTemplateBinding,
            BindingBase nodeTemplateSelectorBinding, bool offsetCenter)
        {
            GraphContentPresenter gcp =
                new GraphContentPresenter(content, nodeTemplateBinding, nodeTemplateSelectorBinding, offsetCenter);
            return gcp;
        }

        private object m_centerDataInUse;
        private IList m_nodesInUse;

        private bool m_centerChanged;
        private bool m_nodesChanged;
        private bool m_nodeCollectionChanged;

        private GraphContentPresenter m_centerGraphContentPresenter;

        private bool m_measureInvalidated;
        private bool m_stillMoving;
        private Point m_controlCenterPoint;

        private int m_milliseconds = int.MinValue;

        private readonly List<GraphContentPresenter> m_nodePresenters;

        private readonly List<GraphContentPresenter> m_fadingGCPList = new List<GraphContentPresenter>();

        private readonly Binding m_nodeTemplateBinding;
        private readonly Binding m_nodeTemplateSelectorBinding;

        private readonly CompositionTargetRenderingListener m_listener = new CompositionTargetRenderingListener();

        #region Static Stuff

        private static Binding GetBinding(string bindingPath, object source)
        {
            Binding newBinding = null;
            try
            {
                newBinding = new Binding(bindingPath);
                newBinding.Source = source;
                newBinding.Mode = BindingMode.OneWay;
            }
            catch (InvalidOperationException) { }
            return newBinding;
        }

        private static Point GetRandomPoint(Size range)
        {
            return new Point(Util.Rnd.NextDouble() * range.Width, Util.Rnd.NextDouble() * range.Height);
        }

        private static Rect GetCenteredRect(Size elementSize, Point center)
        {
            double x = center.X - elementSize.Width / 2;
            double y = center.Y - elementSize.Height / 2;

            return new Rect(x, y, elementSize.Width, elementSize.Height);
        }

        private static Vector GetSpringForce(Vector x)
        {
            Vector force = new Vector();
            //negative is attraction
            force += GetAttractionForce(x);
            //positive is repulsion
            force += GetRepulsiveForce(x);

            Debug.Assert(force.IsValid());

            return force;
        }

        private static Vector GetAttractionForce(Vector x)
        {
            Vector force = -.2 * Normalize(x) * x.Length;
            Debug.Assert(force.IsValid());
            return force;
        }

        private static Vector GetRepulsiveForce(Vector x)
        {
            Vector force = .1 * Normalize(x) / Math.Pow(x.Length / 1000, 2);
            Debug.Assert(force.IsValid());
            return force;
        }

        private static Vector Normalize(Vector v)
        {
            v.Normalize();
            Debug.Assert(v.IsValid());
            return v;
        }

        private static double GetForce(double x)
        {
            return GetSCurve((x + 100) / 200);
        }

        #region math
        private static double GetSCurve(double x)
        {
            return 0.5 + Math.Sin(Math.Abs(x * (Math.PI / 2)) - Math.Abs((x * (Math.PI / 2)) - (Math.PI / 2))) / 2;
        }
        #endregion

        private static readonly Duration s_hideDuration = new Duration(new TimeSpan(0, 0, 1));
        private static readonly Duration s_showDuration = new Duration(new TimeSpan(0, 0, 0, 0, 500));
        private static readonly TimeSpan s_maxSettleTime = new TimeSpan(0, 0, 8);

        private const double s_terminalVelocity = 150;
        private const double s_minVelocity = .05;

        private const double c_minDampening = .01, c_maxDampening = .99;

        #endregion

        #endregion

        private class GraphContentPresenter : ContentPresenter
        {
            public GraphContentPresenter(object content,
            BindingBase nodeTemplateBinding, BindingBase nodeTemplateSelectorBinding, bool offsetCenter)
                : base()
            {
                Content = content;

                SetBinding(ContentPresenter.ContentTemplateProperty, nodeTemplateBinding);
                SetBinding(ContentPresenter.ContentTemplateSelectorProperty, nodeTemplateSelectorBinding);

                ScaleTransform = new ScaleTransform();
                if (offsetCenter)
                {
                    m_translateTransform = new TranslateTransform(Util.Rnd.NextDouble() - .5, Util.Rnd.NextDouble() - .5);
                }
                else
                {
                    m_translateTransform = new TranslateTransform();
                }

                var transformGroup = new TransformGroup();
                transformGroup.Children.Add(ScaleTransform);
                transformGroup.Children.Add(m_translateTransform);

                this.RenderTransform = transformGroup;

                var doubleAnimation = new DoubleAnimation(.5, 1, s_showDuration);
                this.BeginAnimation(OpacityProperty, doubleAnimation);
                ScaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, doubleAnimation);
                ScaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, doubleAnimation);

                LayoutUpdated += (sender, args) =>
                {
                    ScaleTransform.CenterX = RenderSize.Width / 2;
                    ScaleTransform.CenterY = RenderSize.Height / 2;

                    m_centerVector = -.5 * (Vector)RenderSize;
                    updateTransform();
                };
            }

            public bool New = true;
            public Point Location
            {
                get { return m_location; }
                set
                {
                    if (m_location != value)
                    {
                        m_location = value;
                        updateTransform();
                    }
                }
            }

            public Point ParentCenter
            {
                get
                {
                    return m_parentCenter;
                }
                set
                {
                    if (m_parentCenter != value)
                    {
                        m_parentCenter = value;
                        updateTransform();
                    }
                }
            }

            public Point ActualLocation
            {
                get
                {
                    return new Point(m_location.X + m_parentCenter.X, m_location.Y + m_parentCenter.Y);
                }
            }

            public Vector Velocity;
            public bool WasCenter;
            public ScaleTransform ScaleTransform;

            #region Implementation

            private void updateTransform()
            {
                m_translateTransform.SetToVector(m_centerVector + (Vector)m_location + (Vector)m_parentCenter);
            }

            private Point m_location;
            private Vector m_centerVector;
            private Point m_parentCenter;

            private readonly TranslateTransform m_translateTransform;

            #endregion
        }
    }
}
