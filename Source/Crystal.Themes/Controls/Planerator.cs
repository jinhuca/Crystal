// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Windows.Media.Media3D;

namespace Crystal.Themes.Controls;

/// <summary>
///   Based on Greg Schechter's Planerator
///   http://blogs.msdn.com/b/greg_schechter/archive/2007/10/26/enter-the-planerator-dead-simple-3d-in-wpf-with-a-stupid-name.aspx
/// </summary>
[ContentProperty(nameof(Child))]
public class Planerator : FrameworkElement
{
  /// <summary>Identifies the <see cref="RotationX"/> dependency property.</summary>
  public static readonly DependencyProperty RotationXProperty
    = DependencyProperty.Register(nameof(RotationX),
      typeof(double),
      typeof(Planerator),
      new UIPropertyMetadata(0.0, (d, args) => ((Planerator)d).UpdateRotation()));

  public double RotationX
  {
    get => (double)GetValue(RotationXProperty);
    set => SetValue(RotationXProperty, value);
  }

  /// <summary>Identifies the <see cref="RotationY"/> dependency property.</summary>
  public static readonly DependencyProperty RotationYProperty
    = DependencyProperty.Register(nameof(RotationY),
      typeof(double),
      typeof(Planerator),
      new UIPropertyMetadata(0.0, (d, args) => ((Planerator)d).UpdateRotation()));

  public double RotationY
  {
    get => (double)GetValue(RotationYProperty);
    set => SetValue(RotationYProperty, value);
  }

  /// <summary>Identifies the <see cref="RotationZ"/> dependency property.</summary>
  public static readonly DependencyProperty RotationZProperty
    = DependencyProperty.Register(nameof(RotationZ),
      typeof(double),
      typeof(Planerator),
      new UIPropertyMetadata(0.0, (d, args) => ((Planerator)d).UpdateRotation()));

  public double RotationZ
  {
    get => (double)GetValue(RotationZProperty);
    set => SetValue(RotationZProperty, value);
  }

  /// <summary>Identifies the <see cref="FieldOfView"/> dependency property.</summary>
  public static readonly DependencyProperty FieldOfViewProperty
    = DependencyProperty.Register(nameof(FieldOfView),
      typeof(double),
      typeof(Planerator),
      new UIPropertyMetadata(45.0, (d, args) => ((Planerator)d).Update3D(), (d, val) => Math.Min(Math.Max((double)val, 0.5), 179.9)));

  public double FieldOfView
  {
    get => (double)GetValue(FieldOfViewProperty);
    set => SetValue(FieldOfViewProperty, value);
  }

  // clamp to a meaningful range

  private static readonly Point3D[] Mesh =
  {
    new Point3D(0, 0, 0), new Point3D(0, 1, 0), new Point3D(1, 1, 0),
    new Point3D(1, 0, 0)
  };

  private static readonly Point[] TexCoords =
  {
    new Point(0, 1), new Point(0, 0), new Point(1, 0),
    new Point(1, 1)
  };

  private static readonly int[] Indices = { 0, 2, 1, 0, 3, 2 };
  private static readonly Vector3D XAxis = new Vector3D(1, 0, 0);
  private static readonly Vector3D YAxis = new Vector3D(0, 1, 0);
  private static readonly Vector3D ZAxis = new Vector3D(0, 0, 1);
  private readonly QuaternionRotation3D quaternionRotation = new QuaternionRotation3D();
  private readonly RotateTransform3D rotationTransform = new RotateTransform3D();
  private readonly ScaleTransform3D scaleTransform = new ScaleTransform3D();
  private Decorator? logicalChild;
  private FrameworkElement? originalChild;
  private Viewport3D? viewport3D;
  private FrameworkElement? visualChild;
  private Viewport2DVisual3D? frontModel;

  public FrameworkElement? Child
  {
    get => originalChild;
    set
    {
      if (originalChild == value)
      {
        return;
      }

      RemoveVisualChild(visualChild);
      RemoveLogicalChild(logicalChild);

      // Wrap child with special decorator that catches layout invalidations. 
      originalChild = value;
      logicalChild = new LayoutInvalidationCatcher { Child = originalChild };
      visualChild = CreateVisualChild();

      AddVisualChild(visualChild);

      // Need to use a logical child here to make sure databinding operations get down to it,
      // since otherwise the child appears only as the Visual to a Viewport2DVisual3D, which 
      // doesn't have databinding operations pass into it from above.
      AddLogicalChild(logicalChild);
      InvalidateMeasure();
    }
  }

  protected override int VisualChildrenCount => visualChild == null ? 0 : 1;

  protected override Size MeasureOverride(Size availableSize)
  {
    if (logicalChild != null)
    {
      // Measure based on the size of the logical child, since we want to align with it.
      logicalChild.Measure(availableSize);
      var result = logicalChild.DesiredSize;
      visualChild?.Measure(result);
      return result;
    }

    return new Size(0, 0);
  }

  protected override Size ArrangeOverride(Size finalSize)
  {
    if (logicalChild != null)
    {
      logicalChild.Arrange(new Rect(finalSize));
      visualChild?.Arrange(new Rect(finalSize));
      Update3D();
    }

    return base.ArrangeOverride(finalSize);
  }

  protected override Visual? GetVisualChild(int index)
  {
    return visualChild;
  }

  private FrameworkElement CreateVisualChild()
  {
    var simpleQuad = new MeshGeometry3D
    {
      Positions = new Point3DCollection(Mesh),
      TextureCoordinates = new PointCollection(TexCoords),
      TriangleIndices = new Int32Collection(Indices)
    };

    // Front material is interactive, back material is not.
    Material frontMaterial = new DiffuseMaterial(Brushes.White);
    frontMaterial.SetValue(Viewport2DVisual3D.IsVisualHostMaterialProperty, true);

    var vb = new VisualBrush(logicalChild);
    SetCachingForObject(vb); // big perf wins by caching!!
    Material backMaterial = new DiffuseMaterial(vb);

    rotationTransform.Rotation = quaternionRotation;
    var xfGroup = new Transform3DGroup { Children = { scaleTransform, rotationTransform } };

    var backModel = new GeometryModel3D { Geometry = simpleQuad, Transform = xfGroup, BackMaterial = backMaterial };
    var m3dGroup = new Model3DGroup
    {
      Children =
      {
        new DirectionalLight(Colors.White, new Vector3D(0, 0, -1)),
        new DirectionalLight(Colors.White, new Vector3D(0.1, -0.1, 1)),
        backModel
      }
    };

    // Non-interactive Visual3D consisting of the backside, and two lights.
    var mv3d = new ModelVisual3D { Content = m3dGroup };

    if (frontModel != null)
    {
      frontModel.Visual = null;
    }

    // Interactive frontside Visual3D
    frontModel = new Viewport2DVisual3D
    {
      Geometry = simpleQuad,
      Visual = logicalChild,
      Material = frontMaterial,
      Transform = xfGroup
    };

    // Cache the brush in the VP2V3 by setting caching on it.  Big perf wins.
    SetCachingForObject(frontModel);

    // Scene consists of both the above Visual3D's.
    viewport3D = new Viewport3D { ClipToBounds = false, Children = { mv3d, frontModel } };

    UpdateRotation();

    return viewport3D;
  }

  public void Refresh()
  {
    if (logicalChild != null)
    {
      // #3720 I didn't find a better solution to update the child after changing accent/theme
      logicalChild.Child = null;
      logicalChild.Child = Child;

      InvalidateVisual();
      InvalidateMeasure();
    }
  }

  private void SetCachingForObject(DependencyObject d)
  {
    RenderOptions.SetCachingHint(d, CachingHint.Cache);
    RenderOptions.SetCacheInvalidationThresholdMinimum(d, 0.5);
    RenderOptions.SetCacheInvalidationThresholdMaximum(d, 2.0);
  }

  private void UpdateRotation()
  {
    var qx = new Quaternion(XAxis, RotationX);
    var qy = new Quaternion(YAxis, RotationY);
    var qz = new Quaternion(ZAxis, RotationZ);

    quaternionRotation.Quaternion = qx * qy * qz;
  }

  private void Update3D()
  {
    if (viewport3D is null
        || logicalChild is null)
    {
      return;
    }

    // Use GetDescendantBounds for sizing and centering since DesiredSize includes layout whitespace, whereas GetDescendantBounds 
    // is tighter
    Rect logicalBounds = VisualTreeHelper.GetDescendantBounds(logicalChild);
    double w = logicalBounds.Width;
    double h = logicalBounds.Height;

    // Create a camera that looks down -Z, with up as Y, and positioned right halfway in X and Y on the element, 
    // and back along Z the right distance based on the field-of-view is the same projected size as the 2D content
    // that it's looking at.  See http://blogs.msdn.com/greg_schechter/archive/2007/04/03/camera-construction-in-parallaxui.aspx
    // for derivation of this camera.
    double fovInRadians = FieldOfView * (Math.PI / 180);
    double zValue = w / Math.Tan(fovInRadians / 2) / 2;
    viewport3D.Camera = new PerspectiveCamera(new Point3D(w / 2, h / 2, zValue), -ZAxis, YAxis, FieldOfView);

    scaleTransform.ScaleX = w;
    scaleTransform.ScaleY = h;
    rotationTransform.CenterX = w / 2;
    rotationTransform.CenterY = h / 2;
  }
}