// Copyright (c) 2016, Andreas Grimme

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sidi.Treemapping
{
    public class TreeLayout
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public TreeLayout()
        {
        }

        public TreeNode Tag;
        public RectangleD Rectangle;
        public Color Color;
    }

    public static class TreeLayoutExtensions
    {
        public static ITree<TreeLayout> GetNodeAt(this ITree<TreeLayout> tree, System.Windows.Point point)
        {
            if (tree.Data.Rectangle.Contains(point))
            {
                return tree.Children
                    .Concat(new[] { tree })
                    .First(_ => _.Data.Rectangle.Contains(point));
            }
            else
            {
                throw new ArgumentOutOfRangeException("point", point, "outside of rectangle");
            }
        }

        static ITree<TreeLayout> Columns(this TreeNode tree, RectangleD bounds)
        {
            double s = 0;
            double sizeInv = 1.0 / tree.Size * bounds.Width;
            double p0 = s * sizeInv + bounds.Left;

            var layout = new Tree<TreeLayout>
            {
                Data = new TreeLayout
                {
                    Rectangle = bounds,
                    Tag = tree
                }
            };

            foreach (var n in tree.Nodes)
            {
                s += n.Size;
                double p1 = s * sizeInv + bounds.Left;
                new Tree<TreeLayout>
                {
                    Data = new TreeLayout
                    {
                        Rectangle = RectangleF.FromLTRB((float)p0, (float)bounds.Top, (float)p1, (float)bounds.Bottom),
                        Tag = n
                    },
                    Parent = layout,
                };
                p0 = p1;
            }

            return layout;
        }

        static ITree<TreeLayout> Rows(this TreeNode tree, RectangleD bounds)
        {
            double s = 0;
            double sizeInv = 1.0 / tree.Size * bounds.Height;
            double p0 = s * sizeInv + bounds.Top;

            var layout = new Tree<TreeLayout>
            {
                Data = new TreeLayout
                {
                    Rectangle = bounds,
                    Tag = tree
                }
            };

            foreach (var n in tree.Nodes)
            {
                s += n.Size;
                double p1 = s * sizeInv + bounds.Top;

                new Tree<TreeLayout>
                {
                    Data = new TreeLayout
                    {
                        Rectangle = RectangleD.FromLTRB(bounds.Left, p0, bounds.Right, p1),
                        Tag = n
                    },
                    Parent = layout,
                };
                p0 = p1;
            }

            return layout;
        }

        public static Tree<TreeLayout> Squarify(this TreeNode tree, RectangleD bounds)
        {
            var rectangles = tree.Nodes
                .OrderByDescending(x => x.Size)
                .Select(n => new TreeLayout { Tag = n })
                .ToArray();

            RectangleD remaining = bounds;

            // align rectangles so that the aspect ratio is optimal
            for (int offset = 0; offset < rectangles.Length; )
            {
                var aspectRatio = Double.MaxValue;
                double ar;
                int count = 1;
                RectangleD newRemaining;
                for (; offset + count < rectangles.Length; ++count)
                {
                    Squarify(rectangles, offset, count, remaining, out newRemaining, out ar);
                    if (ar > aspectRatio)
                    {
                        --count;
                        break;
                    }

                    aspectRatio = ar;
                }
                Squarify(rectangles, offset, count, remaining, out newRemaining, out ar);
                remaining = newRemaining;
                offset += count;
            }

            var layout = new Tree<TreeLayout>
            {
                Data = new TreeLayout
                {
                    Rectangle = bounds,
                    Tag = tree,
                }
            };

            foreach (var i in rectangles)
            {
                i.Tag.Squarify(i.Rectangle).Parent = layout;
            }
            return layout;
        }

        /// <summary>
        /// Squarify count childs beginning at offset and return the worst aspect ratio
        /// </summary>
        /// <param name="childs"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <param name="bounds"></param>
        /// <param name="remaining"></param>
        /// <param name="worstAspectRatio"></param>
        static void Squarify(TreeLayout[] childs, int offset, int count, RectangleD bounds, out RectangleD remaining, out double worstAspectRatio)
        {
            double size = 0;
            for (int i = offset; i < childs.Length; ++i)
            {
                size += childs[i].Tag.Size;
            }

            var isRow = bounds.Width < bounds.Height;

            double rowSize = 0;
            for (int i = offset; i < offset + count; ++i)
            {
                rowSize += childs[i].Tag.Size;
            }
            var rowWidth = isRow ? bounds.Width : bounds.Height;
            var availableRowHeight = isRow ? bounds.Height : bounds.Width;
            var rowHeight = (availableRowHeight * rowSize / size);
            var columnPerSize = rowWidth / rowSize;

            worstAspectRatio = 1.0;
            double s = 0;
            for (int i = offset; i < offset + count; ++i)
            {
                var s1 = s + childs[i].Tag.Size;
                if (isRow)
                {
                    childs[i].Rectangle = RectangleD.FromLTRB(
                        bounds.Left + s * columnPerSize,
                        bounds.Top,
                        bounds.Left + s1 * columnPerSize,
                        bounds.Top + rowHeight);
                }
                else
                {
                    childs[i].Rectangle = RectangleD.FromLTRB(
                        bounds.Left,
                        (bounds.Top + s * columnPerSize),
                        bounds.Left + rowHeight,
                        (bounds.Top + s1 * columnPerSize));
                }
                s = s1;
                worstAspectRatio = Math.Max(childs[i].Rectangle.GetAspectRatio(), worstAspectRatio);
            }

            if (isRow)
            {
                remaining = RectangleD.FromLTRB(
                        bounds.Left,
                        bounds.Top + rowHeight,
                        bounds.Right,
                        bounds.Bottom);
            }
            else
            {
                remaining = RectangleD.FromLTRB(
                        bounds.Left + rowHeight,
                        bounds.Top,
                        bounds.Right,
                        bounds.Bottom);
            }
        }
    }
}
