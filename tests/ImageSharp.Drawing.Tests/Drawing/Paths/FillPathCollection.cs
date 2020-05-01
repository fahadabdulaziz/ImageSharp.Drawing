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
    public class FillPathCollection : BaseImageOperationsExtensionTest
    {
        private static readonly GraphicsOptionsComparer graphicsOptionsComparer = new GraphicsOptionsComparer();

        GraphicsOptions nonDefault = new GraphicsOptions { Antialias = false };
        Color color = Color.HotPink;
        SolidBrush brush = Brushes.Solid(Color.HotPink);
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

        public FillPathCollection()
        {
            this.pathCollection = new PathCollection(this.path1, this.path2);
        }

        [Fact]
        public void CorrectlySetsBrushAndPath()
        {
            this.operations.Fill(this.brush, this.pathCollection);

            for (int i = 0; i < 2; i++)
            {
                FillRegionProcessor processor = this.Verify<FillRegionProcessor>(i);

                Assert.Equal(new GraphicsOptions(), processor.Options, graphicsOptionsComparer);

                ShapeRegion region = Assert.IsType<ShapeRegion>(processor.Region);

                // path is converted to a polygon before filling
                Polygon polygon = Assert.IsType<Polygon>(region.Shape);
                Assert.IsType<LinearLineSegment>(polygon.LineSegments[0]);

                Assert.Equal(this.brush, processor.Brush);
            }
        }

        [Fact]
        public void CorrectlySetsBrushPathOptions()
        {
            this.operations.Fill(this.nonDefault, this.brush, this.pathCollection);

            for (int i = 0; i < 2; i++)
            {
                FillRegionProcessor processor = this.Verify<FillRegionProcessor>(i);

                Assert.Equal(this.nonDefault, processor.Options, graphicsOptionsComparer);

                ShapeRegion region = Assert.IsType<ShapeRegion>(processor.Region);
                Polygon polygon = Assert.IsType<Polygon>(region.Shape);
                Assert.IsType<LinearLineSegment>(polygon.LineSegments[0]);

                Assert.Equal(this.brush, processor.Brush);
            }
        }

        [Fact]
        public void CorrectlySetsColorAndPath()
        {
            this.operations.Fill(this.color, this.pathCollection);

            for (int i = 0; i < 2; i++)
            {
                FillRegionProcessor processor = this.Verify<FillRegionProcessor>(i);

                Assert.Equal(new GraphicsOptions(), processor.Options, graphicsOptionsComparer);

                ShapeRegion region = Assert.IsType<ShapeRegion>(processor.Region);
                Polygon polygon = Assert.IsType<Polygon>(region.Shape);
                Assert.IsType<LinearLineSegment>(polygon.LineSegments[0]);

                SolidBrush brush = Assert.IsType<SolidBrush>(processor.Brush);
                Assert.Equal(this.color, brush.Color);
            }
        }

        [Fact]
        public void CorrectlySetsColorPathAndOptions()
        {
            this.operations.Fill(this.nonDefault, this.color, this.pathCollection);

            for (int i = 0; i < 2; i++)
            {
                FillRegionProcessor processor = this.Verify<FillRegionProcessor>(i);

                Assert.Equal(this.nonDefault, processor.Options, graphicsOptionsComparer);

                ShapeRegion region = Assert.IsType<ShapeRegion>(processor.Region);
                Polygon polygon = Assert.IsType<Polygon>(region.Shape);
                Assert.IsType<LinearLineSegment>(polygon.LineSegments[0]);

                SolidBrush brush = Assert.IsType<SolidBrush>(processor.Brush);
                Assert.Equal(this.color, brush.Color);
            }
        }
    }
}
