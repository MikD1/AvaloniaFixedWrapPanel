using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Utilities;
using static System.Math;

namespace AvaloniaFixedWrapPanelExample.Views
{
    public class FixedWrapPanel : Panel, INavigableContainer
    {
        public static readonly StyledProperty<int> ItemsPerLineProperty =
            AvaloniaProperty.Register<FixedWrapPanel, int>(nameof(ItemsPerLine), 3);

        static FixedWrapPanel()
        {
            AffectsMeasure<FixedWrapPanel>(ItemsPerLineProperty);
        }

        public double ItemsPerLine
        {
            get => GetValue(ItemsPerLineProperty);
            set => SetValue(ItemsPerLineProperty, value);
        }

        IInputElement? INavigableContainer.GetControl(NavigationDirection direction, IInputElement? from, bool wrap)
        {
            var children = Children;
            int index = from is not null ? Children.IndexOf((IControl) from) : -1;

            switch (direction)
            {
                case NavigationDirection.First:
                    index = 0;
                    break;
                case NavigationDirection.Last:
                    index = children.Count - 1;
                    break;
                case NavigationDirection.Next:
                    ++index;
                    break;
                case NavigationDirection.Previous:
                    --index;
                    break;
                case NavigationDirection.Left:
                    index -= 1;
                    break;
                case NavigationDirection.Right:
                    index += 1;
                    break;
                case NavigationDirection.Up:
                    index = -1;
                    break;
                case NavigationDirection.Down:
                    index = -1;
                    break;
            }

            if (index >= 0 && index < children.Count)
            {
                return children[index];
            }
            else
            {
                return null;
            }
        }

        protected override Size MeasureOverride(Size constraint)
        {
            double itemWidth = constraint.Width / ItemsPerLine;
            MutableSize currentLineSize = new();
            MutableSize panelSize = new();
            Size lineConstraint = new(constraint.Width, constraint.Height);
            Size childConstraint = new(itemWidth, constraint.Height);

            for (int i = 0, count = Children.Count; i < count; i++)
            {
                IControl child = Children[i];
                if (child is null)
                {
                    continue;
                }

                child.Measure(childConstraint);
                Size childSize = new(itemWidth, child.DesiredSize.Height);

                if (MathUtilities.GreaterThan(currentLineSize.Width + childSize.Width, lineConstraint.Width))
                {
                    // Need to switch to another line
                    panelSize.Width = Max(currentLineSize.Width, panelSize.Width);
                    panelSize.Height += currentLineSize.Height;
                    currentLineSize = new(childSize);
                }
                else
                {
                    // Continue to accumulate a line
                    currentLineSize.Width += childSize.Width;
                    currentLineSize.Height = Max(childSize.Height, currentLineSize.Height);
                }
            }

            // The last line size, if any should be added
            panelSize.Width = Max(currentLineSize.Width, panelSize.Width);
            panelSize.Height += currentLineSize.Height;

            return panelSize.ToSize();
        }

        /// <inheritdoc/>
        protected override Size ArrangeOverride(Size finalSize)
        {
            double itemWidth = finalSize.Width / ItemsPerLine;
            var children = Children;
            int firstInLine = 0;
            double accumulatedV = 0;
            double itemU = itemWidth;
            var curLineSize = new MutableSize();
            var uvFinalSize = new MutableSize(finalSize.Width, finalSize.Height);
            bool useItemU = true;

            for (int i = 0; i < children.Count; i++)
            {
                var child = children[i];
                if (child != null)
                {
                    var sz = new MutableSize(itemWidth, child.DesiredSize.Height);

                    if (MathUtilities.GreaterThan(curLineSize.Width + sz.Width,
                            uvFinalSize.Width)) // Need to switch to another line
                    {
                        ArrangeLine(accumulatedV, curLineSize.Height, firstInLine, i, useItemU, itemU);

                        accumulatedV += curLineSize.Height;
                        curLineSize = sz;

                        if (MathUtilities.GreaterThan(sz.Width,
                                uvFinalSize
                                    .Width)) // The element is wider then the constraint - give it a separate line                    
                        {
                            // Switch to next line which only contain one element
                            ArrangeLine(accumulatedV, sz.Height, i, ++i, useItemU, itemU);

                            accumulatedV += sz.Height;
                            curLineSize = new MutableSize();
                        }

                        firstInLine = i;
                    }
                    else // Continue to accumulate a line
                    {
                        curLineSize.Width += sz.Width;
                        curLineSize.Height = Max(sz.Height, curLineSize.Height);
                    }
                }
            }

            // Arrange the last line, if any
            if (firstInLine < children.Count)
            {
                ArrangeLine(accumulatedV, curLineSize.Height, firstInLine, children.Count, useItemU, itemU);
            }

            return finalSize;
        }

        private void ArrangeLine(double v, double lineV, int start, int end, bool useItemU, double itemU)
        {
            var children = Children;
            double u = 0;

            for (int i = start; i < end; i++)
            {
                var child = children[i];
                if (child != null)
                {
                    var childSize = new Size(child.DesiredSize.Width, child.DesiredSize.Height);
                    double layoutSlotU = useItemU ? itemU : childSize.Width;
                    child.Arrange(new Rect(u, v, layoutSlotU, lineV));
                    u += layoutSlotU;
                }
            }
        }

        private struct MutableSize
        {
            internal MutableSize(double width, double height)
            {
                Width = width;
                Height = height;
            }

            internal MutableSize(Size size)
            {
                Width = size.Width;
                Height = size.Height;
            }

            internal double Width;
            internal double Height;

            internal Size ToSize()
            {
                return new Size(Width, Height);
            }
        }
    }
}