// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System.Numerics;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Drawing.Processing.Processors.Drawing;
using SixLabors.ImageSharp.Drawing.Tests.Processing;
using SixLabors.ImageSharp.Drawing.Tests.TestUtilities;
using Xunit;

namespace SixLabors.ImageSharp.Drawing.Tests.Drawing.Paths
{
    public class DrawPathCollection : BaseImageOperationsExtensionTest
    {
        private static readonly GraphicsOptionsComparer graphicsOptionsComparer = new GraphicsOptionsComparer();

        GraphicsOptions nonDefault = new GraphicsOptions { Antialias = false };
        Color color = Color.HotPink;
        Pen pen = Pens.Solid(Color.HotPink, 1);
        IPath path1 = new Path(new LinearLineSegment(new PointF[] {
                    new Vector2(10,10),
                    new Vector2(20,10),
                    new Vector2(20,10),
                    new Vector2(30,10),
                }));
        IPath path2 = new Path(new LinearLineSegment(new PointF[] {
                    new Vector2(10,10),
                    new Vector2(20,10),
                    new Vector2(20,10),
                    new Vector2(30,10),
                }));

        IPathCollection pathCollection;

        public DrawPathCollection()
        {
            this.pathCollection = new PathCollection(this.path1, this.path2);
        }

        [Fact]
        public void CorrectlySetsBrushAndPath()
        {
            this.operations.Draw(this.pen, this.pathCollection);

            for (int i = 0; i < 2; i++)
            {
                FillRegionProcessor processor = this.Verify<FillRegionProcessor>(i);

                Assert.Equal(new GraphicsOptions(), processor.Options, graphicsOptionsComparer);

                ShapePath region = Assert.IsType<ShapePath>(processor.Region);

                // path is converted to a polygon before filling
                Assert.IsType<ComplexPolygon>(region.Shape);

                Assert.Equal(this.pen.StrokeFill, processor.Brush);
            }
        }

        [Fact]
        public void CorrectlySetsBrushPathOptions()
        {
            this.operations.Draw(this.nonDefault, this.pen, this.pathCollection);

            for (int i = 0; i < 2; i++)
            {
                FillRegionProcessor processor = this.Verify<FillRegionProcessor>(i);

                Assert.Equal(this.nonDefault, processor.Options, graphicsOptionsComparer);

                ShapePath region = Assert.IsType<ShapePath>(processor.Region);
                Assert.IsType<ComplexPolygon>(region.Shape);

                Assert.Equal(this.pen.StrokeFill, processor.Brush);
            }
        }

        [Fact]
        public void CorrectlySetsColorAndPath()
        {
            this.operations.Draw(this.color, 1, this.pathCollection);

            for (int i = 0; i < 2; i++)
            {
                FillRegionProcessor processor = this.Verify<FillRegionProcessor>(i);

                Assert.Equal(new GraphicsOptions(), processor.Options, graphicsOptionsComparer);

                ShapePath region = Assert.IsType<ShapePath>(processor.Region);
                Assert.IsType<ComplexPolygon>(region.Shape);

                SolidBrush brush = Assert.IsType<SolidBrush>(processor.Brush);
                Assert.Equal(this.color, brush.Color);
            }
        }

        [Fact]
        public void CorrectlySetsColorPathAndOptions()
        {
            this.operations.Draw(this.nonDefault, this.color, 1, this.pathCollection);

            for (int i = 0; i < 2; i++)
            {
                FillRegionProcessor processor = this.Verify<FillRegionProcessor>(i);

                Assert.Equal(this.nonDefault, processor.Options, graphicsOptionsComparer);

                ShapePath region = Assert.IsType<ShapePath>(processor.Region);
                Assert.IsType<ComplexPolygon>(region.Shape);

                SolidBrush brush = Assert.IsType<SolidBrush>(processor.Brush);
                Assert.Equal(this.color, brush.Color);
            }
        }
    }
}
