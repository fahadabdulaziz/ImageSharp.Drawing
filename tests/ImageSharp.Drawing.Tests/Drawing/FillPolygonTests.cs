// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Numerics;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

using Xunit;

namespace SixLabors.ImageSharp.Drawing.Tests.Drawing
{
    [GroupOutput("Drawing")]
    public class FillPolygonTests
    {
        [Theory]
        [WithBasicTestPatternImages(250, 350, PixelTypes.Rgba32, "White", 1f, true)]
        [WithBasicTestPatternImages(250, 350, PixelTypes.Rgba32, "White", 0.6f, true)]
        [WithBasicTestPatternImages(250, 350, PixelTypes.Rgba32, "White", 1f, false)]
        [WithBasicTestPatternImages(250, 350, PixelTypes.Bgr24, "Yellow", 1f, true)]
        public void FillPolygon_Solid<TPixel>(TestImageProvider<TPixel> provider, string colorName, float alpha, bool antialias)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            PointF[] simplePath =
                {
                    new Vector2(10, 10), new Vector2(200, 150), new Vector2(50, 300)
                };
            Color color = TestUtils.GetColorByName(colorName).WithAlpha(alpha);

            var options = new GraphicsOptions { Antialias = antialias };

            string aa = antialias ? "" : "_NoAntialias";
            FormattableString outputDetails = $"{colorName}_A{alpha}{aa}";

            provider.RunValidatingProcessorTest(
                c => c.FillPolygon(options, color, simplePath),
                outputDetails,
                appendSourceFileOrDescription: false);
        }

        [Theory]
        [WithBasicTestPatternImages(200, 200, PixelTypes.Rgba32)]
        public void FillPolygon_Concave<TPixel>(TestImageProvider<TPixel> provider)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            var points = new PointF[]
                             {
                                 new Vector2(8, 8),
                                 new Vector2(64, 8),
                                 new Vector2(64, 64),
                                 new Vector2(120, 64),
                                 new Vector2(120, 120),
                                 new Vector2(8, 120)
                             };

            var color = Color.LightGreen;

            provider.RunValidatingProcessorTest(
                c => c.FillPolygon(color, points),
                appendSourceFileOrDescription: false,
                appendPixelTypeToFileName: false);
        }

        [Theory]
        [WithBasicTestPatternImages(250, 350, PixelTypes.Rgba32)]
        public void FillPolygon_Pattern<TPixel>(TestImageProvider<TPixel> provider)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            PointF[] simplePath =
                {
                    new Vector2(10, 10), new Vector2(200, 150), new Vector2(50, 300)
                };
            var color = Color.Yellow;

            var brush = Brushes.Horizontal(color);

            provider.RunValidatingProcessorTest(
                c => c.FillPolygon(brush, simplePath),
                appendSourceFileOrDescription: false);
        }

        [Theory]
        [WithBasicTestPatternImages(250, 350, PixelTypes.Rgba32, TestImages.Png.Ducky)]
        [WithBasicTestPatternImages(250, 350, PixelTypes.Rgba32, TestImages.Bmp.Car)]
        public void FillPolygon_ImageBrush<TPixel>(TestImageProvider<TPixel> provider, string brushImageName)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            PointF[] simplePath =
                {
                    new Vector2(10, 10), new Vector2(200, 50), new Vector2(50, 200)
                };

            using (var brushImage = Image.Load<TPixel>(TestFile.Create(brushImageName).Bytes))
            {
                var brush = new ImageBrush(brushImage);

                provider.RunValidatingProcessorTest(
                    c => c.FillPolygon(brush, simplePath),
                    System.IO.Path.GetFileNameWithoutExtension(brushImageName),
                    appendSourceFileOrDescription: false);
            }
        }

        [Theory]
        [WithBasicTestPatternImages(250, 250, PixelTypes.Rgba32)]
        public void Fill_RectangularPolygon<TPixel>(TestImageProvider<TPixel> provider)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            var polygon = new RectangularPolygon(10, 10, 190, 140);
            var color = Color.White;

            provider.RunValidatingProcessorTest(
                c => c.Fill(color, polygon),
                appendSourceFileOrDescription: false);
        }

        [Theory]
        [WithBasicTestPatternImages(200, 200, PixelTypes.Rgba32, 3, 50, 0f)]
        [WithBasicTestPatternImages(200, 200, PixelTypes.Rgba32, 3, 60, 20f)]
        [WithBasicTestPatternImages(200, 200, PixelTypes.Rgba32, 3, 60, -180f)]
        [WithBasicTestPatternImages(200, 200, PixelTypes.Rgba32, 5, 70, 0f)]
        [WithBasicTestPatternImages(200, 200, PixelTypes.Rgba32, 7, 80, -180f)]
        public void Fill_RegularPolygon<TPixel>(TestImageProvider<TPixel> provider, int vertices, float radius, float angleDeg)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            float angle = GeometryUtilities.DegreeToRadian(angleDeg);
            var polygon = new RegularPolygon(100, 100, vertices, radius, angle);
            var color = Color.Yellow;

            FormattableString testOutput = $"V({vertices})_R({radius})_Ang({angleDeg})";
            provider.RunValidatingProcessorTest(
                c => c.Fill(color, polygon),
                testOutput,
                appendSourceFileOrDescription: false,
                appendPixelTypeToFileName: false);
        }

        [Theory]
        [WithBasicTestPatternImages(200, 200, PixelTypes.Rgba32)]
        public void Fill_EllipsePolygon<TPixel>(TestImageProvider<TPixel> provider)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            var polygon = new EllipsePolygon(100, 100, 80, 120);
            var color = Color.Azure;

            provider.RunValidatingProcessorTest(
                c => c.Fill(color, polygon),
                appendSourceFileOrDescription: false,
                appendPixelTypeToFileName: false);
        }

        [Theory]
        [WithSolidFilledImages(60, 60, "Blue", PixelTypes.Rgba32)]
        public void Fill_IntersectionRules_OddEven<TPixel>(TestImageProvider<TPixel> provider)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            using (var img = provider.GetImage())
            {

                var poly = new Polygon(new LinearLineSegment(
                    new PointF(10, 30),
                    new PointF(10, 20),
                    new PointF(50, 20),
                    new PointF(50, 50),
                    new PointF(20, 50),
                    new PointF(20, 10),
                    new PointF(30, 10),
                    new PointF(30, 40),
                    new PointF(40, 40),
                    new PointF(40, 30),
                    new PointF(10, 30)));

                img.Mutate(c => c.Fill(
                    new ShapeGraphicsOptions
                    {
                        IntersectionRule = IntersectionRule.OddEven,
                    },
                    Color.HotPink,
                    poly));

                provider.Utility.SaveTestOutputFile(img);

                Assert.Equal(Color.Blue.ToPixel<TPixel>(), img[25, 25]);
            }
        }

        [Theory]
        [WithSolidFilledImages(60, 60, "Blue", PixelTypes.Rgba32)]
        public void Fill_IntersectionRules_Nonzero<TPixel>(TestImageProvider<TPixel> provider)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            Configuration.Default.MaxDegreeOfParallelism = 1;
            using (var img = provider.GetImage())
            {
                var poly = new Polygon(new LinearLineSegment(
                    new PointF(10, 30),
                    new PointF(10, 20),
                    new PointF(50, 20),
                    new PointF(50, 50),
                    new PointF(20, 50),
                    new PointF(20, 10),
                    new PointF(30, 10),
                    new PointF(30, 40),
                    new PointF(40, 40),
                    new PointF(40, 30),
                    new PointF(10, 30)));
                img.Mutate(c => c.Fill(
                    new ShapeGraphicsOptions
                    {
                        IntersectionRule = IntersectionRule.Nonzero,
                    },
                    Color.HotPink,
                    poly));

                provider.Utility.SaveTestOutputFile(img);
                Assert.Equal(Color.HotPink.ToPixel<TPixel>(), img[25, 25]);
            }
        }
    }
}
