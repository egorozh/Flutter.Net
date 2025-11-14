using System.Diagnostics;
using Avalonia;
using Avalonia.Media;
using Flutter.Foundation;
using Flutter.Painting;
using Flutter.UI;

namespace Flutter.Rendering;

/// <summary>
/// Displays its children in a one-dimensional array.
/// </summary>
public class RenderFlex : RenderBox, IRenderBoxContainerDefaultsMixin<RenderBox, FlexParentData>
{
    private readonly RenderBoxContainerDefaultsMixin<RenderBox, FlexParentData> _mixin1;

    public RenderFlex(
        List<RenderBox>? children,
        Axis direction = Axis.Horizontal,
        MainAxisSize mainAxisSize = MainAxisSize.Max,
        MainAxisAlignment mainAxisAlignment = MainAxisAlignment.Start,
        CrossAxisAlignment crossAxisAlignment = CrossAxisAlignment.Center,
        TextDirection? textDirection = null,
        VerticalDirection verticalDirection = VerticalDirection.Down,
        TextBaseline? textBaseline = null,
        double spacing = 0.0)
    {
        _direction = direction;
        _mainAxisSize = mainAxisSize;
        _mainAxisAlignment = mainAxisAlignment;
        _crossAxisAlignment = crossAxisAlignment;
        _textDirection = textDirection;
        _verticalDirection = verticalDirection;
        _textBaseline = textBaseline;
        _spacing = spacing;

        Debug.Assert(spacing >= 0.0);

        _mixin1 = new RenderBoxContainerDefaultsMixin<RenderBox, FlexParentData>(this);

        if (children != null)
            AddAll(children);
    }


    #region Properties

    private Axis _direction;

    /// The direction to use as the main axis.
    public Axis Direction
    {
        get => _direction;
        set
        {
            _direction = value;
            MarkNeedsLayout();
        }
    }

    private MainAxisSize _mainAxisSize;

    public MainAxisSize MainAxisSize
    {
        get => _mainAxisSize;
        set
        {
            _mainAxisSize = value;
            MarkNeedsLayout();
        }
    }

    private MainAxisAlignment _mainAxisAlignment;

    public MainAxisAlignment MainAxisAlignment
    {
        get => _mainAxisAlignment;
        set
        {
            _mainAxisAlignment = value;
            MarkNeedsLayout();
        }
    }

    private CrossAxisAlignment _crossAxisAlignment;

    public CrossAxisAlignment CrossAxisAlignment
    {
        get => _crossAxisAlignment;
        set
        {
            _crossAxisAlignment = value;
            MarkNeedsLayout();
        }
    }

    private TextBaseline? _textBaseline;

    /// If aligning items according to their baseline, which baseline to use.
    ///
    /// Must not be null if [crossAxisAlignment] is [CrossAxisAlignment.baseline].
    public TextBaseline? TextBaseline
    {
        get => _textBaseline;
        set
        {
            Debug.Assert(_crossAxisAlignment != CrossAxisAlignment.Baseline || value != null);

            if (_textBaseline != value)
            {
                _textBaseline = value;

                MarkNeedsLayout();
            }
        }
    }

    private TextDirection? _textDirection;

    private TextDirection? TextDirection
    {
        get => _textDirection;
        set
        {
            if (_textDirection != value)
            {
                _textDirection = value;
                MarkNeedsLayout();
            }
        }
    }

    private VerticalDirection _verticalDirection;

    public VerticalDirection VerticalDirection
    {
        get => _verticalDirection;
        set
        {
            if (_verticalDirection != value)
            {
                _verticalDirection = value;
                MarkNeedsLayout();
            }
        }
    }


    // Set during layout if overflow occurred on the main axis.
    private double _overflow = 0;

    // Check whether any meaningful overflow is present. Values below an epsilon
    // are treated as not overflowing.
    public bool _hasOverflow => _overflow > Constants.PrecisionErrorTolerance;


    private double _spacing;

    public double Spacing
    {
        get => _spacing;
        set
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (_spacing == value)
                return;

            _spacing = value;

            MarkNeedsLayout();
        }
    }

    private bool IsBaselineAligned
    {
        get
        {
            return CrossAxisAlignment switch
            {
                CrossAxisAlignment.Baseline => Direction switch
                {
                    Axis.Horizontal => true,
                    Axis.Vertical => false,
                    _ => throw new ArgumentOutOfRangeException()
                },

                CrossAxisAlignment.Start => false,
                CrossAxisAlignment.Center => false,
                CrossAxisAlignment.End => false,
                CrossAxisAlignment.Stretch => false,

                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }

    #endregion

    public override void SetupParentData(RenderObject child)
    {
        if (child.parentData is not FlexParentData)
        {
            child.parentData = new FlexParentData();
        }
    }

    protected override void PerformLayout()
    {
        var constraints = Constraints;

        var sizes = _computeSizes(
            constraints: constraints,
            layoutChild: ChildLayoutHelper.layoutChild,
            getBaseline: ChildLayoutHelper.getBaseline
        );

        double crossAxisExtent = sizes.axisSize.crossAxisExtent;

        Size = sizes.axisSize.ToSize(Direction);

        _overflow = Math.Max(0.0, -sizes.mainAxisFreeSpace);

        double remainingSpace = Math.Max(0.0, sizes.mainAxisFreeSpace);
        bool flipMainAxis = _flipMainAxis;
        bool flipCrossAxis = _flipCrossAxis;

        (double leadingSpace, double betweenSpace) = _distributeSpace(
            MainAxisAlignment,
            remainingSpace,
            ChildCount,
            flipMainAxis,
            Spacing
        );

        Func<RenderBox, RenderBox?> nextChild = flipMainAxis ? ChildBefore : ChildAfter;
        RenderBox? topLeftChild = flipMainAxis ? LastChild : FirstChild;

        double? baselineOffset = sizes.baselineOffset;

        Debug.Assert(
            baselineOffset == null ||
            (CrossAxisAlignment == CrossAxisAlignment.Baseline && Direction == Axis.Horizontal)
        );

        // Position all children in visual order: starting from the top-left child and
        // work towards the child that's farthest away from the origin.
        double childMainPosition = leadingSpace;

        for (RenderBox? child = topLeftChild; child != null; child = nextChild(child))
        {
            double? childBaselineOffset = null;

            bool baselineAlign =
                baselineOffset != null &&
                (childBaselineOffset = child.GetDistanceToBaseline(TextBaseline!.Value, onlyReal: true)) !=
                null;

            double childCrossPosition = baselineAlign
                ? baselineOffset!.Value - childBaselineOffset!.Value
                : _getChildCrossAxisOffset(CrossAxisAlignment,
                    crossAxisExtent - _getCrossSize(child.Size),
                    flipCrossAxis
                );

            var childParentData = (FlexParentData)child.parentData!;

            childParentData.offset = Direction switch
            {
                Axis.Horizontal => new Point(childMainPosition, childCrossPosition),
                Axis.Vertical => new Point(childCrossPosition, childMainPosition),

                _ => throw new ArgumentOutOfRangeException()
            };

            childMainPosition += _getMainSize(child.Size) + betweenSpace;
        }
    }

    public override void Paint(PaintingContext ctx, Point offset)
    {
        if (!_hasOverflow)
        {
            DefaultPaint(ctx, offset);
            return;
        }

        // There's no point in drawing the children if we're empty.
        if (Size.IsEmpty)
        {
            return;
        }
        
        DefaultPaint(ctx, offset);
    }

    private static int _getFlex(RenderBox child)
    {
        var childParentData = (FlexParentData)child.parentData!;

        return childParentData.flex ?? 0;
    }

    private static FlexFit _getFit(RenderBox child)
    {
        var childParentData = (FlexParentData)child.parentData!;

        return childParentData.fit ?? FlexFit.Tight;
    }

    private BoxConstraints _constraintsForNonFlexChild(BoxConstraints constraints)
    {
        bool fillCrossAxis = CrossAxisAlignment switch
        {
            CrossAxisAlignment.Stretch => true,
            _ => false,
        };

        return _direction switch
        {
            Axis.Horizontal =>
                fillCrossAxis
                    ? BoxConstraints.TightFor(height: constraints.MaxHeight)
                    : new BoxConstraints(MaxHeight: constraints.MaxHeight),
            Axis.Vertical =>
                fillCrossAxis
                    ? BoxConstraints.TightFor(width: constraints.MaxWidth)
                    : new BoxConstraints(MaxWidth: constraints.MaxWidth),

            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private double _getCrossSize(Size size) => _direction switch
    {
        Axis.Horizontal => size.Height,
        Axis.Vertical => size.Width,

        _ => throw new ArgumentOutOfRangeException()
    };


    private double _getMainSize(Size size) => _direction switch
    {
        Axis.Horizontal => size.Width,
        Axis.Vertical => size.Height,

        _ => throw new ArgumentOutOfRangeException()
    };

    // flipMainAxis is used to decide whether to lay out
    // left-to-right/top-to-bottom (false), or right-to-left/bottom-to-top
    // (true). Returns false in cases when the layout direction does not matter
    // (for instance, there is no child).
    private bool _flipMainAxis =>
        FirstChild != null &&
        Direction switch
        {
            Axis.Horizontal => TextDirection switch
            {
                null => false,
                UI.TextDirection.Ltr => false,
                UI.TextDirection.Rtl => true,

                _ => throw new ArgumentOutOfRangeException()
            },
            Axis.Vertical => VerticalDirection switch
            {
                VerticalDirection.Down => false,
                VerticalDirection.Up => true,
                _ => throw new ArgumentOutOfRangeException()
            },
            _ => throw new ArgumentOutOfRangeException()
        };

    private bool _flipCrossAxis =>
        FirstChild != null &&
        Direction switch
        {
            Axis.Vertical => TextDirection switch
            {
                null => false,
                UI.TextDirection.Ltr => false,
                UI.TextDirection.Rtl => true,
                _ => throw new ArgumentOutOfRangeException()
            },
            Axis.Horizontal => VerticalDirection switch
            {
                VerticalDirection.Down => false,
                VerticalDirection.Up => true,

                _ => throw new ArgumentOutOfRangeException()
            },
        };

    private BoxConstraints _constraintsForFlexChild(
        RenderBox child,
        BoxConstraints constraints,
        double maxChildExtent
    )
    {
        Debug.Assert(_getFlex(child) > 0.0);
        Debug.Assert(maxChildExtent >= 0.0);

        double minChildExtent = _getFit(child) switch
        {
            FlexFit.Tight => maxChildExtent,
            FlexFit.Loose => 0.0,

            _ => throw new ArgumentOutOfRangeException()
        };

        bool fillCrossAxis = CrossAxisAlignment switch
        {
            CrossAxisAlignment.Stretch => true,
            CrossAxisAlignment.Start => false,
            CrossAxisAlignment.Center => false,
            CrossAxisAlignment.End => false,
            CrossAxisAlignment.Baseline => false,

            _ => throw new ArgumentOutOfRangeException()
        };

        return _direction switch
        {
            Axis.Horizontal => new BoxConstraints(
                MinWidth: minChildExtent,
                MaxWidth: maxChildExtent,
                MinHeight: fillCrossAxis ? constraints.MaxHeight : 0.0,
                MaxHeight: constraints.MaxHeight
            ),
            Axis.Vertical => new BoxConstraints(
                MinWidth: fillCrossAxis ? constraints.MaxWidth : 0.0,
                MaxWidth: constraints.MaxWidth,
                MinHeight: minChildExtent,
                MaxHeight: maxChildExtent
            ),

            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private _LayoutSizes _computeSizes(
        BoxConstraints constraints,
        Func<RenderBox, BoxConstraints, Size> layoutChild,
        Func<RenderBox, BoxConstraints, TextBaseline, double?> getBaseline)
    {
        //assert(_debugHasNecessaryDirections);

        // Determine used flex factor, size inflexible items, calculate free space.
        double maxMainSize = _getMainSize(constraints.Biggest);
        bool canFlex = double.IsFinite(maxMainSize);

        var nonFlexChildConstraints = _constraintsForNonFlexChild(constraints);

        // Null indicates the children are not baseline aligned.
        TextBaseline? textBaseline = IsBaselineAligned
            ? TextBaseline ??
              throw new Exception(
                  "To use CrossAxisAlignment.baseline, you must also specify which baseline to use using the \"textBaseline\" argument."
              )
            : null;

        // The first pass lays out non-flex children and computes total flex.
        int totalFlex = 0;
        RenderBox? firstFlexChild = null;

        var accumulatedAscentDescent = _AscentDescent.None;

        // Initially, accumulatedSize is the sum of the spaces between children in the main axis.
        _AxisSize accumulatedSize = new(new Size(Spacing * (ChildCount - 1), 0.0));

        for (RenderBox? child = FirstChild; child != null; child = ChildAfter(child))
        {
            int flex;

            if (canFlex && (flex = _getFlex(child)) > 0)
            {
                totalFlex += flex;
                firstFlexChild ??= child;
            }
            else
            {
                var childSize = _AxisSize.FromSize(
                    size: layoutChild(child, nonFlexChildConstraints),
                    direction: Direction
                );

                accumulatedSize += childSize;

                // Baseline-aligned children contributes to the cross axis extent separately.
                double? baselineOffset = textBaseline == null
                    ? null
                    : getBaseline(child, nonFlexChildConstraints, textBaseline.Value);

                accumulatedAscentDescent += _AscentDescent.Create(
                    baselineOffset: baselineOffset,
                    crossSize: childSize.crossAxisExtent
                );
            }
        }

        Debug.Assert((totalFlex == 0) == (firstFlexChild == null));
        Debug.Assert(
            firstFlexChild == null || canFlex
        ); // If we are given infinite space there's no need for this extra step.

        // The second pass distributes free space to flexible children.
        double flexSpace = Math.Max(0.0, maxMainSize - accumulatedSize.mainAxisExtent);
        double spacePerFlex = flexSpace / totalFlex;
        for (
            RenderBox? child = firstFlexChild;
            child != null && totalFlex > 0;
            child = ChildAfter(child)
        )
        {
            int flex = _getFlex(child);
            if (flex == 0)
            {
                continue;
            }

            totalFlex -= flex;
            Debug.Assert(double.IsFinite(spacePerFlex));

            double maxChildExtent = spacePerFlex * flex;

            Debug.Assert(_getFit(child) == FlexFit.Loose || maxChildExtent < double.PositiveInfinity);

            BoxConstraints childConstraints = _constraintsForFlexChild(
                child,
                constraints,
                maxChildExtent
            );

            var childSize = _AxisSize.FromSize(
                size: layoutChild(child, childConstraints),
                direction: Direction
            );

            accumulatedSize += childSize;
            double? baselineOffset = textBaseline == null
                ? null
                : getBaseline(child, childConstraints, textBaseline.Value);

            accumulatedAscentDescent += _AscentDescent.Create(
                baselineOffset: baselineOffset,
                crossSize: childSize.crossAxisExtent
            );
        }

        Debug.Assert(totalFlex == 0);

        // The overall height of baseline-aligned children contributes to the cross axis extent.
        accumulatedSize += accumulatedAscentDescent.AscentDescent switch
        {
            null => _AxisSize.Empty,
            var (ascent, descent) => _AxisSize.Create(
                mainAxisExtent: 0,
                crossAxisExtent: ascent + descent
            )
        };

        double idealMainSize = MainAxisSize switch
        {
            MainAxisSize.Max when double.IsFinite(maxMainSize) => maxMainSize,
            MainAxisSize.Max or MainAxisSize.Min => accumulatedSize.mainAxisExtent,

            _ => throw new ArgumentOutOfRangeException()
        };

        var constrainedSize = _AxisSize.Create(
            mainAxisExtent: idealMainSize,
            crossAxisExtent: accumulatedSize.crossAxisExtent
        ).ApplyConstraints(constraints, Direction);

        return new _LayoutSizes(
            axisSize: constrainedSize,
            mainAxisFreeSpace: constrainedSize.mainAxisExtent - accumulatedSize.mainAxisExtent,
            baselineOffset: accumulatedAscentDescent.BaselineOffset,
            spacePerFlex: firstFlexChild == null ? null : spacePerFlex
        );
    }

    private static (double leadingSpace, double betweenSpace) _distributeSpace(
        MainAxisAlignment mainAxisAlignment,
        double freeSpace,
        int itemCount,
        bool flipped,
        double spacing
    )
    {
        Debug.Assert(itemCount >= 0);

        return mainAxisAlignment switch
        {
            MainAxisAlignment.Start => flipped ? (freeSpace, spacing) : (0.0, spacing),

            MainAxisAlignment.End => _distributeSpace(
                MainAxisAlignment.Start,
                freeSpace,
                itemCount,
                !flipped,
                spacing
            ),
            MainAxisAlignment.SpaceBetween when itemCount < 2 => _distributeSpace(
                MainAxisAlignment.Start,
                freeSpace,
                itemCount,
                flipped,
                spacing
            ),
            MainAxisAlignment.SpaceAround when itemCount == 0 => _distributeSpace(
                MainAxisAlignment.Start,
                freeSpace,
                itemCount,
                flipped,
                spacing
            ),

            MainAxisAlignment.Center => (freeSpace / 2.0, spacing),
            MainAxisAlignment.SpaceBetween => (0.0, freeSpace / (itemCount - 1) + spacing),
            MainAxisAlignment.SpaceAround => (freeSpace / itemCount / 2, freeSpace / itemCount + spacing),
            MainAxisAlignment.SpaceEvenly => (
                freeSpace / (itemCount + 1),
                freeSpace / (itemCount + 1) + spacing
            ),

            _ => throw new ArgumentOutOfRangeException(nameof(mainAxisAlignment), mainAxisAlignment, null)
        };
    }

    private static double _getChildCrossAxisOffset(
        CrossAxisAlignment crossAxisAlignment,
        double freeSpace,
        bool flipped)
    {
        // This method should not be used to position baseline-aligned children.
        return crossAxisAlignment switch
        {
            CrossAxisAlignment.Stretch => 0.0,
            CrossAxisAlignment.Baseline => 0.0,
            CrossAxisAlignment.Start => flipped ? freeSpace : 0.0,
            CrossAxisAlignment.Center => freeSpace / 2,
            CrossAxisAlignment.End => _getChildCrossAxisOffset(CrossAxisAlignment.Start,
                freeSpace,
                !flipped
            ),

            _ => throw new ArgumentOutOfRangeException(nameof(crossAxisAlignment), crossAxisAlignment, null)
        };
    }


    private struct _AxisSize(Size size)
    {
        private readonly Size _size = size;

        public static readonly _AxisSize Empty = new(new Size());

        public double mainAxisExtent => _size.Width;

        public double crossAxisExtent => _size.Height;

        public static _AxisSize Create(double mainAxisExtent, double crossAxisExtent)
        {
            return new _AxisSize(new Size(mainAxisExtent, crossAxisExtent));
        }

        public static _AxisSize FromSize(Size size, Axis direction)
        {
            return new _AxisSize(_convert(size, direction));
        }

        public Size ToSize(Axis direction) => _convert(_size, direction);

        public _AxisSize ApplyConstraints(BoxConstraints constraints, Axis direction)
        {
            var effectiveConstraints = direction switch
            {
                Axis.Horizontal => constraints,
                Axis.Vertical => constraints.Flipped,

                _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
            };

            return new _AxisSize(effectiveConstraints.Constrain(_size));
        }

        private static Size _convert(Size size, Axis direction)
        {
            return direction switch
            {
                Axis.Horizontal => size,
                Axis.Vertical => size.Flipped,

                _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
            };
        }

        public static _AxisSize operator +(_AxisSize a, _AxisSize b)
        {
            return new _AxisSize(
                new Size(
                    a._size.Width + b._size.Width,
                    Math.Max(a._size.Height, b._size.Height)
                ));
        }
    }

    private struct _AscentDescent
    {
        private readonly (double ascent, double descent)? _ascentDescent;

        private _AscentDescent((double ascent, double descent)? ascentDescent)
        {
            _ascentDescent = ascentDescent;
        }

        public double? BaselineOffset => AscentDescent?.ascent;


        public (double ascent, double descent)? AscentDescent => _ascentDescent;


        public static readonly _AscentDescent None = new _AscentDescent(null);

        public static _AscentDescent Create(double? baselineOffset, double crossSize)
        {
            return !baselineOffset.HasValue
                ? None
                : new _AscentDescent((baselineOffset.Value, crossSize));
        }

        public static _AscentDescent operator +(_AscentDescent a, _AscentDescent b)
        {
            if (a.AscentDescent is not null)
            {
                if (b.AscentDescent is not null)
                {
                    return new _AscentDescent((
                        Math.Max(a.AscentDescent.Value.ascent, b.AscentDescent.Value.ascent),
                        Math.Max(a.AscentDescent.Value.descent, b.AscentDescent.Value.descent)));
                }

                return a;
            }

            return b;
        }
    }

    private struct _LayoutSizes
    {
        public _LayoutSizes(
            _AxisSize axisSize,
            double mainAxisFreeSpace,
            double? baselineOffset,
            double? spacePerFlex
        )
        {
            this.axisSize = axisSize;
            this.mainAxisFreeSpace = mainAxisFreeSpace;
            this.baselineOffset = baselineOffset;
            this.spacePerFlex = spacePerFlex;

            Debug.Assert(!spacePerFlex.HasValue || double.IsFinite(spacePerFlex.Value));
        }

        // The  constrained _AxisSize of the RenderFlex.
        public _AxisSize axisSize;

        // The free space along the main axis. If the value is positive, the free space
        // will be distributed according to the [MainAxisAlignment] specified. A
        // negative value indicates the RenderFlex overflows along the main axis.
        public double mainAxisFreeSpace;

        // Null if the RenderFlex is not baseline aligned, or none of its children has
        // a valid baseline of the given [TextBaseline] type.
        public double? baselineOffset;

        // The allocated space for flex children.
        public double? spacePerFlex;
    }


    #region Mixins

    public int ChildCount => _mixin1.ChildCount;

    public RenderBox? FirstChild => _mixin1.FirstChild;

    public RenderBox? LastChild => _mixin1.LastChild;

    public void AddAll(List<RenderBox> children) => _mixin1.AddAll(children);

    public RenderBox? ChildBefore(RenderBox child) => _mixin1.ChildBefore(child);

    public RenderBox? ChildAfter(RenderBox child) => _mixin1.ChildAfter(child);

    public void DefaultPaint(PaintingContext ctx, Point offset) => _mixin1.DefaultPaint(ctx, offset);

    #endregion
}