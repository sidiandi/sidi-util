// Copyright (c) 2016, Andreas Grimme

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sidi.TreeMap
{
    internal class Layout
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public Layout()
        {
        }

        public ITree Tree;
        public RectangleD Rectangle;
        public double Size;
        public Color Color;
    }

    internal static class TreeLayoutExtensions
    {
        public static ITree<Layout> GetNodeAt(this ITree<Layout> tree, System.Windows.Point point)
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

        public static void Squarify(this ITree<Layout> tree, RectangleD bounds)
        {
            tree.Data.Rectangle = bounds;
            var rectangles = tree.Children
                .Select(_ => _.Data)
                .OrderByDescending(x => x.Size)
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

            foreach (var i in tree.Children)
            {
                i.Squarify(i.Data.Rectangle);
            }
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
        static void Squarify(Layout[] childs, int offset, int count, RectangleD bounds, out RectangleD remaining, out double worstAspectRatio)
        {
            double size = 0;
            for (int i = offset; i < childs.Length; ++i)
            {
                size += childs[i].Size;
            }

            var isRow = bounds.Width < bounds.Height;

            double rowSize = 0;
            for (int i = offset; i < offset + count; ++i)
            {
                rowSize += childs[i].Size;
            }

            var rowWidth = isRow ? bounds.Width : bounds.Height;
            var availableRowHeight = isRow ? bounds.Height : bounds.Width;
            var rowHeight = size == 0 ? 0 : (availableRowHeight * rowSize / size);
            var columnPerSize = rowSize == 0 ? 0 : (rowWidth / rowSize);

            worstAspectRatio = 1.0;
            double s = 0;
            for (int i = offset; i < offset + count; ++i)
            {
                var s1 = s + childs[i].Size;
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

        public static ITree<Layout> CreateLayoutTree(ITree tree, Func<ITree, Color> GetColor, Func<ITree, double> GetSize)
        {
            return CreateLayoutTree(null, tree, GetColor, GetSize);
        }

        static Tree<Layout> CreateLayoutTree(Tree<Layout> parent, ITree tree, Func<ITree, Color> GetColor, Func<ITree, double> GetSize)
        {
            var lt = new Tree<Layout>
            {
                Data = new Layout
                {
                    Tree = tree
                },
                Parent = parent
            };

            double size = 0;
            if (!tree.Children.Any())
            {
                lt.Data.Color = GetColor(tree);
                size = GetSize(tree);
            }
            else
            {
                foreach (var c in tree.Children)
                {
                    var cl = CreateLayoutTree(lt, c, GetColor, GetSize);
                    size += cl.Data.Size;
                }
            }
            lt.Data.Size = size;

            return lt;
        }
    }
}
