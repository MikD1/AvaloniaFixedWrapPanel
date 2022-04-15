using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
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
            var children = Children;
            var curLineSize = new UVSize();
            var panelSize = new UVSize();
            var uvConstraint = new UVSize(constraint.Width, constraint.Height);

            var childConstraint = new Size(itemWidth, constraint.Height);

            for (int i = 0, count = children.Count; i < count; i++)
            {
                var child = children[i];
                if (child != null)
                {
                    // Flow passes its own constraint to children
                    child.Measure(childConstraint);

                    // This is the size of the child in UV space
                    var sz = new UVSize(itemWidth, child.DesiredSize.Height);

                    if (MathUtilities.GreaterThan(curLineSize.U + sz.U,
                            uvConstraint.U)) // Need to switch to another line
                    {
                        panelSize.U = Max(curLineSize.U, panelSize.U);
                        panelSize.V += curLineSize.V;
                        curLineSize = sz;

                        if (MathUtilities.GreaterThan(sz.U,
                                uvConstraint
                                    .U)) // The element is wider then the constraint - give it a separate line                    
                        {
                            panelSize.U = Max(sz.U, panelSize.U);
                            panelSize.V += sz.V;
                            curLineSize = new UVSize();
                        }
                    }
                    else // Continue to accumulate a line
                    {
                        curLineSize.U += sz.U;
                        curLineSize.V = Max(sz.V, curLineSize.V);
                    }
                }
            }

            // The last line size, if any should be added
            panelSize.U = Max(curLineSize.U, panelSize.U);
            panelSize.V += curLineSize.V;

            // Go from UV space to W/H space
            return new Size(panelSize.Width, panelSize.Height);
        }

        /// <inheritdoc/>
        protected override Size ArrangeOverride(Size finalSize)
        {
            double itemWidth = finalSize.Width / ItemsPerLine;
            var children = Children;
            int firstInLine = 0;
            double accumulatedV = 0;
            double itemU = itemWidth;
            var curLineSize = new UVSize();
            var uvFinalSize = new UVSize(finalSize.Width, finalSize.Height);
            bool useItemU = true;

            for (int i = 0; i < children.Count; i++)
            {
                var child = children[i];
                if (child != null)
                {
                    var sz = new UVSize(itemWidth, child.DesiredSize.Height);

                    if (MathUtilities.GreaterThan(curLineSize.U + sz.U,
                            uvFinalSize.U)) // Need to switch to another line
                    {
                        ArrangeLine(accumulatedV, curLineSize.V, firstInLine, i, useItemU, itemU);

                        accumulatedV += curLineSize.V;
                        curLineSize = sz;

                        if (MathUtilities.GreaterThan(sz.U,
                                uvFinalSize
                                    .U)) // The element is wider then the constraint - give it a separate line                    
                        {
                            // Switch to next line which only contain one element
                            ArrangeLine(accumulatedV, sz.V, i, ++i, useItemU, itemU);

                            accumulatedV += sz.V;
                            curLineSize = new UVSize();
                        }

                        firstInLine = i;
                    }
                    else // Continue to accumulate a line
                    {
                        curLineSize.U += sz.U;
                        curLineSize.V = Max(sz.V, curLineSize.V);
                    }
                }
            }

            // Arrange the last line, if any
            if (firstInLine < children.Count)
            {
                ArrangeLine(accumulatedV, curLineSize.V, firstInLine, children.Count, useItemU, itemU);
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
                    var childSize = new UVSize(child.DesiredSize.Width, child.DesiredSize.Height);
                    double layoutSlotU = useItemU ? itemU : childSize.U;
                    child.Arrange(new Rect(u, v, layoutSlotU, lineV));
                    u += layoutSlotU;
                }
            }
        }

        private struct UVSize
        {
            internal UVSize(double width, double height)
            {
                U = V = 0d;
                Width = width;
                Height = height;
            }

            internal double U;
            internal double V;

            internal double Width
            {
                get => U;
                set => U = value;
            }

            internal double Height
            {
                get => V;
                set => V = value;
            }
        }
    }
}