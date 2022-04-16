// This source file is adapted from the Avalonia WrapPanel. 
// (https://github.com/AvaloniaUI/Avalonia) 

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Utilities;
using static System.Math;

namespace AvaloniaFixedWrapPanel
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

        IInputElement INavigableContainer.GetControl(NavigationDirection direction, IInputElement? from, bool wrap)
        {
            int index = from is not null ? Children.IndexOf((IControl) from) : -1;
            switch (direction)
            {
                case NavigationDirection.First:
                    index = 0;
                    break;
                case NavigationDirection.Last:
                    index = Children.Count - 1;
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

            if (index >= 0 && index < Children.Count)
            {
                return Children[index];
            }

            return this;
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

        protected override Size ArrangeOverride(Size finalSize)
        {
            double itemWidth = finalSize.Width / ItemsPerLine;
            int firstInLine = 0;
            double accumulatedHeight = 0;
            var currentLineSize = new MutableSize();

            for (int i = 0; i < Children.Count; i++)
            {
                IControl child = Children[i];
                if (child == null)
                {
                    continue;
                }

                MutableSize itemSize = new(itemWidth, child.DesiredSize.Height);
                if (MathUtilities.GreaterThan(currentLineSize.Width + itemSize.Width, finalSize.Width))
                {
                    // Need to switch to another line
                    ArrangeLine(accumulatedHeight, currentLineSize.Height, firstInLine, i, itemWidth);
                    accumulatedHeight += currentLineSize.Height;
                    currentLineSize = itemSize;
                    firstInLine = i;
                }
                else
                {
                    // Continue to accumulate a line
                    currentLineSize.Width += itemSize.Width;
                    currentLineSize.Height = Max(itemSize.Height, currentLineSize.Height);
                }
            }

            if (firstInLine < Children.Count)
            {
                // Arrange the last line, if any
                ArrangeLine(accumulatedHeight, currentLineSize.Height, firstInLine, Children.Count, itemWidth);
            }

            return finalSize;
        }

        private void ArrangeLine(double y, double height, int start, int end, double width)
        {
            double x = 0;
            for (int i = start; i < end; i++)
            {
                IControl child = Children[i];
                if (child == null)
                {
                    continue;
                }

                child.Arrange(new Rect(x, y, width, height));
                x += width;
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