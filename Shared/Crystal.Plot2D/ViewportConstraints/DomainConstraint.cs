using Crystal.Plot2D.Common;

namespace Crystal.Plot2D.ViewportConstraints;

/// <summary>
/// Represents a constraint which returns data rectangle, intersected with specified data domain.
/// </summary>
public sealed class DomainConstraint : ViewportConstraint {
  /// <summary>
  /// Initializes a new instance of the <see cref="DomainConstraint"/> class.
  /// </summary>
  public DomainConstraint() { }

  /// <summary>
  /// Initializes a new instance of the <see cref="DomainConstraint"/> class with given domain property.
  /// </summary>
  /// <param name="domain">
  /// The domain.
  /// </param>
  public DomainConstraint(DataRect domain) {
    Domain = domain;
  }

  private DataRect domain = new(xMin: -1, yMin: -1, width: 2, height: 2);

  /// <summary>
  /// Gets or sets the domain.
  /// </summary>
  /// <value>
  /// The domain.
  /// </value>
  public DataRect Domain {
    get => domain;
    set {
      if(domain != value) {
        domain = value;
        RaiseChanged();
      }
    }
  }

  /// <summary>
  /// Applies the specified old data rect.
  /// </summary>
  /// <param name="oldDataRect">The old data rect.</param>
  /// <param name="newDataRect">The new data rect.</param>
  /// <param name="viewport">The viewport.</param>
  /// <returns></returns>
  public override DataRect Apply(DataRect oldDataRect, DataRect newDataRect, Viewport2D viewport) {
    var res = domain;
    if(domain.IsEmpty) {
      res = newDataRect;
    }
    else if(newDataRect.IntersectsWith(rect: domain)) {
      res = newDataRect;
      if(newDataRect.Size == oldDataRect.Size) {
        if(res.XMin < domain.XMin) {
          res.XMin = domain.XMin;
        }

        if(res.YMin < domain.YMin) {
          res.YMin = domain.YMin;
        }

        if(res.XMax > domain.XMax) {
          res.XMin += domain.XMax - res.XMax;
        }

        if(res.YMax > domain.YMax) {
          res.YMin += domain.YMax - res.YMax;
        }
      }
      else {
        res = DataRect.Intersect(rect1: newDataRect, rect2: domain);
      }
    }

    return res;
  }
}
