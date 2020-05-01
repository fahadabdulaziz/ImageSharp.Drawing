// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Memory;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

using Xunit;

namespace SixLabors.ImageSharp.Drawing.Tests.Drawing
{
    public class FillPatternBrushTests
    {
        private void Test(string name, Rgba32 background, IBrush brush, Rgba32[,] expectedPattern)
        {
            string path = TestEnvironment.CreateOutputDirectory("Drawing", "FillPatternBrushTests");
            using (var image = new Image<Rgba32>(20, 20))
            {
                image.Mutate(x => x.Fill(background).Fill(brush));

                image.Save($"{path}/{name}.png");

                Buffer2D<Rgba32> sourcePixels = image.GetRootFramePixelBuffer();
                // lets pick random spots to start checking
                var r = new Random();
                var expectedPatternFast = new DenseMatrix<Rgba32>(expectedPattern);
                int xStride = expectedPatternFast.Columns;
                int yStride = expectedPatternFast.Rows;
                int offsetX = r.Next(image.Width / xStride) * xStride;
                int offsetY = r.Next(image.Height / yStride) * yStride;
                for (int x = 0; x < xStride; x++)
                {
                    for (int y = 0; y < yStride; y++)
                    {
                        int actualX = x + offsetX;
                        int actualY = y + offsetY;
                        Rgba32 expected = expectedPatternFast[y, x]; // inverted pattern
                        Rgba32 actual = sourcePixels[actualX, actualY];
                        if (expected != actual)
                        {
                            Assert.True(false, $"Expected {expected} but found {actual} at ({actualX},{actualY})");
                        }
                    }
                }

                image.Mutate(x => x.Resize(80, 80, KnownResamplers.NearestNeighbor));
                image.Save($"{path}/{name}x4.png");
            }
        }

        [Fact]
        public void ImageShouldBeFloodFilledWithPercent10()
        {
            this.Test(
                "Percent10",
                Color.Blue,
                Brushes.Percent10(Color.HotPink, Color.LimeGreen),
                new Rgba32[,]
                    {
                        { Color.HotPink, Color.LimeGreen, Color.LimeGreen, Color.LimeGreen },
                        { Color.LimeGreen, Color.LimeGreen, Color.LimeGreen, Color.LimeGreen },
                        { Color.LimeGreen, Color.LimeGreen, Color.HotPink, Color.LimeGreen },
                        { Color.LimeGreen, Color.LimeGreen, Color.LimeGreen, Color.LimeGreen }
                    });
        }

        [Fact]
        public void ImageShouldBeFloodFilledWithPercent10Transparent()
        {
            this.Test(
                "Percent10_Transparent",
                Color.Blue,
                Brushes.Percent10(Color.HotPink),
                new Rgba32[,]
                    {
                        { Color.HotPink, Color.Blue, Color.Blue, Color.Blue },
                        { Color.Blue, Color.Blue, Color.Blue, Color.Blue },
                        { Color.Blue, Color.Blue, Color.HotPink, Color.Blue },
                        { Color.Blue, Color.Blue, Color.Blue, Color.Blue }
                    });
        }

        [Fact]
        public void ImageShouldBeFloodFilledWithPercent20()
        {
            this.Test(
                "Percent20",
                Color.Blue,
                Brushes.Percent20(Color.HotPink, Color.LimeGreen),
                new Rgba32[,]
                    {
                        { Color.HotPink, Color.LimeGreen, Color.LimeGreen, Color.LimeGreen },
                        { Color.LimeGreen, Color.LimeGreen, Color.HotPink, Color.LimeGreen },
                        { Color.HotPink, Color.LimeGreen, Color.LimeGreen, Color.LimeGreen },
                        { Color.LimeGreen, Color.LimeGreen, Color.HotPink, Color.LimeGreen }
                    });
        }

        [Fact]
        public void ImageShouldBeFloodFilledWithPercent20_transparent()
        {
            this.Test(
                "Percent20_Transparent",
                Color.Blue,
                Brushes.Percent20(Color.HotPink),
                new Rgba32[,]
                    {
                        { Color.HotPink, Color.Blue, Color.Blue, Color.Blue },
                        { Color.Blue, Color.Blue, Color.HotPink, Color.Blue },
                        { Color.HotPink, Color.Blue, Color.Blue, Color.Blue },
                        { Color.Blue, Color.Blue, Color.HotPink, Color.Blue }
                    });
        }

        [Fact]
        public void ImageShouldBeFloodFilledWithHorizontal()
        {
            this.Test(
                "Horizontal",
                Color.Blue,
                Brushes.Horizontal(Color.HotPink, Color.LimeGreen),
                new Rgba32[,]
                    {
                        { Color.LimeGreen, Color.LimeGreen, Color.LimeGreen, Color.LimeGreen },
                        { Color.HotPink, Color.HotPink, Color.HotPink, Color.HotPink },
                        { Color.LimeGreen, Color.LimeGreen, Color.LimeGreen, Color.LimeGreen },
                        { Color.LimeGreen, Color.LimeGreen, Color.LimeGreen, Color.LimeGreen }
                    });
        }

        [Fact]
        public void ImageShouldBeFloodFilledWithHorizontal_transparent()
        {
            this.Test(
                "Horizontal_Transparent",
                Color.Blue,
                Brushes.Horizontal(Color.HotPink),
                new Rgba32[,]
                    {
                        { Color.Blue, Color.Blue, Color.Blue, Color.Blue },
                        { Color.HotPink, Color.HotPink, Color.HotPink, Color.HotPink },
                        { Color.Blue, Color.Blue, Color.Blue, Color.Blue },
                        { Color.Blue, Color.Blue, Color.Blue, Color.Blue }
                    });
        }

        [Fact]
        public void ImageShouldBeFloodFilledWithMin()
        {
            this.Test(
                "Min",
                Color.Blue,
                Brushes.Min(Color.HotPink, Color.LimeGreen),
                new Rgba32[,]
                    {
                        { Color.LimeGreen, Color.LimeGreen, Color.LimeGreen, Color.LimeGreen },
                        { Color.LimeGreen, Color.LimeGreen, Color.LimeGreen, Color.LimeGreen },
                        { Color.LimeGreen, Color.LimeGreen, Color.LimeGreen, Color.LimeGreen },
                        { Color.HotPink, Color.HotPink, Color.HotPink, Color.HotPink }
                    });
        }

        [Fact]
        public void ImageShouldBeFloodFilledWithMin_transparent()
        {
            this.Test(
                "Min_Transparent",
                Color.Blue,
                Brushes.Min(Color.HotPink),
                new Rgba32[,]
                    {
                        { Color.Blue, Color.Blue, Color.Blue, Color.Blue },
                        { Color.Blue, Color.Blue, Color.Blue, Color.Blue },
                        { Color.Blue, Color.Blue, Color.Blue, Color.Blue },
                        { Color.HotPink, Color.HotPink, Color.HotPink, Color.HotPink },
                    });
        }

        [Fact]
        public void ImageShouldBeFloodFilledWithVertical()
        {
            this.Test(
                "Vertical",
                Color.Blue,
                Brushes.Vertical(Color.HotPink, Color.LimeGreen),
                new Rgba32[,]
                    {
                        { Color.LimeGreen, Color.HotPink, Color.LimeGreen, Color.LimeGreen },
                        { Color.LimeGreen, Color.HotPink, Color.LimeGreen, Color.LimeGreen },
                        { Color.LimeGreen, Color.HotPink, Color.LimeGreen, Color.LimeGreen },
                        { Color.LimeGreen, Color.HotPink, Color.LimeGreen, Color.LimeGreen }
                    });
        }

        [Fact]
        public void ImageShouldBeFloodFilledWithVertical_transparent()
        {
            this.Test(
                "Vertical_Transparent",
                Color.Blue,
                Brushes.Vertical(Color.HotPink),
                new Rgba32[,]
                    {
                        { Color.Blue, Color.HotPink, Color.Blue, Color.Blue },
                        { Color.Blue, Color.HotPink, Color.Blue, Color.Blue },
                        { Color.Blue, Color.HotPink, Color.Blue, Color.Blue },
                        { Color.Blue, Color.HotPink, Color.Blue, Color.Blue }
                    });
        }

        [Fact]
        public void ImageShouldBeFloodFilledWithForwardDiagonal()
        {
            this.Test(
                "ForwardDiagonal",
                Color.Blue,
                Brushes.ForwardDiagonal(Color.HotPink, Color.LimeGreen),
                new Rgba32[,]
                    {
                        { Color.LimeGreen, Color.LimeGreen, Color.LimeGreen, Color.HotPink },
                        { Color.LimeGreen, Color.LimeGreen, Color.HotPink, Color.LimeGreen },
                        { Color.LimeGreen, Color.HotPink, Color.LimeGreen, Color.LimeGreen },
                        { Color.HotPink, Color.LimeGreen, Color.LimeGreen, Color.LimeGreen }
                    });
        }

        [Fact]
        public void ImageShouldBeFloodFilledWithForwardDiagonal_transparent()
        {
            this.Test(
                "ForwardDiagonal_Transparent",
                Color.Blue,
                Brushes.ForwardDiagonal(Color.HotPink),
                new Rgba32[,]
                    {
                        { Color.Blue, Color.Blue, Color.Blue, Color.HotPink },
                        { Color.Blue, Color.Blue, Color.HotPink, Color.Blue },
                        { Color.Blue, Color.HotPink, Color.Blue, Color.Blue },
                        { Color.HotPink, Color.Blue, Color.Blue, Color.Blue }
                    });
        }

        [Fact]
        public void ImageShouldBeFloodFilledWithBackwardDiagonal()
        {
            this.Test(
                "BackwardDiagonal",
                Color.Blue,
                Brushes.BackwardDiagonal(Color.HotPink, Color.LimeGreen),
                new Rgba32[,]
                    {
                        { Color.HotPink, Color.LimeGreen, Color.LimeGreen, Color.LimeGreen },
                        { Color.LimeGreen, Color.HotPink, Color.LimeGreen, Color.LimeGreen },
                        { Color.LimeGreen, Color.LimeGreen, Color.HotPink, Color.LimeGreen },
                        { Color.LimeGreen, Color.LimeGreen, Color.LimeGreen, Color.HotPink }
                    });
        }

        [Fact]
        public void ImageShouldBeFloodFilledWithBackwardDiagonal_transparent()
        {
            this.Test(
                "BackwardDiagonal_Transparent",
                Color.Blue,
                Brushes.BackwardDiagonal(Color.HotPink),
                new Rgba32[,]
                    {
                        { Color.HotPink, Color.Blue, Color.Blue, Color.Blue },
                        { Color.Blue, Color.HotPink, Color.Blue, Color.Blue },
                        { Color.Blue, Color.Blue, Color.HotPink, Color.Blue },
                        { Color.Blue, Color.Blue, Color.Blue, Color.HotPink }
                    });
        }
    }
}
