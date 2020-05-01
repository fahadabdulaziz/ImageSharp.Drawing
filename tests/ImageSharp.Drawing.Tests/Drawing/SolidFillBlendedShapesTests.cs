// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.
using System;
using System.Collections.Generic;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Drawing.Tests.TestUtilities.ImageComparison;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Xunit;

// ReSharper disable InconsistentNaming
namespace SixLabors.ImageSharp.Drawing.Tests.Drawing
{
    [GroupOutput("Drawing")]
    public class SolidFillBlendedShapesTests
    {
        public static IEnumerable<object[]> modes = GetAllModeCombinations();

        private static IEnumerable<object[]> GetAllModeCombinations()
        {
            foreach (object composition in Enum.GetValues(typeof(PixelAlphaCompositionMode)))
            {
                foreach (object blending in Enum.GetValues(typeof(PixelColorBlendingMode)))
                {
                    yield return new object[] { blending, composition };
                }
            }
        }

        [Theory]
        [WithBlankImages(nameof(modes), 250, 250, PixelTypes.Rgba32)]
        public void _1DarkBlueRect_2BlendHotPinkRect<TPixel>(
            TestImageProvider<TPixel> provider,
            PixelColorBlendingMode blending,
            PixelAlphaCompositionMode composition)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            using (Image<TPixel> img = provider.GetImage())
            {
                int scaleX = img.Width / 100;
                int scaleY = img.Height / 100;
                img.Mutate(
                    x => x.Fill(
                            Color.DarkBlue,
                            new Rectangle(0 * scaleX, 40 * scaleY, 100 * scaleX, 20 * scaleY)
                            )
                        .Fill(
                            new GraphicsOptions { Antialias = true, ColorBlendingMode = blending, AlphaCompositionMode = composition },
                            Color.HotPink,
                            new Rectangle(20 * scaleX, 0 * scaleY, 30 * scaleX, 100 * scaleY))
                    );

                VerifyImage(provider, blending, composition, img);
            }
        }

        [Theory]
        [WithBlankImages(nameof(modes), 250, 250, PixelTypes.Rgba32)]
        public void _1DarkBlueRect_2BlendHotPinkRect_3BlendTransparentEllipse<TPixel>(
            TestImageProvider<TPixel> provider,
            PixelColorBlendingMode blending,
            PixelAlphaCompositionMode composition)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            using (Image<TPixel> img = provider.GetImage())
            {
                int scaleX = img.Width / 100;
                int scaleY = img.Height / 100;
                img.Mutate(
                    x => x.Fill(
                        Color.DarkBlue,
                        new Rectangle(0 * scaleX, 40 * scaleY, 100 * scaleX, 20 * scaleY)));
                img.Mutate(
                    x => x.Fill(
                        new GraphicsOptions { Antialias = true, ColorBlendingMode = blending, AlphaCompositionMode = composition },
                        Color.HotPink,
                        new Rectangle(20 * scaleX, 0 * scaleY, 30 * scaleX, 100 * scaleY)));
                img.Mutate(
                    x => x.Fill(
                        new GraphicsOptions { Antialias = true, ColorBlendingMode = blending, AlphaCompositionMode = composition },
                        Color.Transparent,
                        new EllipsePolygon(40 * scaleX, 50 * scaleY, 50 * scaleX, 50 * scaleY))
                    );

                VerifyImage(provider, blending, composition, img);
            }
        }

        [Theory]
        [WithBlankImages(nameof(modes), 250, 250, PixelTypes.Rgba32)]
        public void _1DarkBlueRect_2BlendHotPinkRect_3BlendSemiTransparentRedEllipse<TPixel>(
            TestImageProvider<TPixel> provider,
            PixelColorBlendingMode blending,
            PixelAlphaCompositionMode composition)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            using (Image<TPixel> img = provider.GetImage())
            {
                int scaleX = (img.Width / 100);
                int scaleY = (img.Height / 100);
                img.Mutate(
                    x => x.Fill(
                        Color.DarkBlue,
                        new Rectangle(0 * scaleX, 40, 100 * scaleX, 20 * scaleY)));
                img.Mutate(
                    x => x.Fill(
                        new GraphicsOptions { Antialias = true, ColorBlendingMode = blending, AlphaCompositionMode = composition },
                        Color.HotPink,
                        new Rectangle(20 * scaleX, 0, 30 * scaleX, 100 * scaleY)));

                Color transparentRed = Color.Red.WithAlpha(0.5f);

                img.Mutate(
                    x => x.Fill(
                        new GraphicsOptions { Antialias = true, ColorBlendingMode = blending, AlphaCompositionMode = composition },
                        transparentRed,
                        new EllipsePolygon(40 * scaleX, 50 * scaleY, 50 * scaleX, 50 * scaleY))
                    );

                VerifyImage(provider, blending, composition, img); ;
            }
        }

        [Theory]
        [WithBlankImages(nameof(modes), 250, 250, PixelTypes.Rgba32)]
        public void _1DarkBlueRect_2BlendBlackEllipse<TPixel>(
            TestImageProvider<TPixel> provider,
            PixelColorBlendingMode blending,
            PixelAlphaCompositionMode composition)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            using (Image<TPixel> dstImg = provider.GetImage(), srcImg = provider.GetImage())
            {
                int scaleX = dstImg.Width / 100;
                int scaleY = dstImg.Height / 100;

                dstImg.Mutate(
                    x => x.Fill(
                        Color.DarkBlue,
                        new Rectangle(0 * scaleX, 40 * scaleY, 100 * scaleX, 20 * scaleY)));

                srcImg.Mutate(
                    x => x.Fill(
                        Color.Black,
                        new EllipsePolygon(40 * scaleX, 50 * scaleY, 50 * scaleX, 50 * scaleY)));

                dstImg.Mutate(
                    x => x.DrawImage(srcImg, new GraphicsOptions { Antialias = true, ColorBlendingMode = blending, AlphaCompositionMode = composition }));

                VerifyImage(provider, blending, composition, dstImg);
            }
        }

        private static void VerifyImage<TPixel>(
            TestImageProvider<TPixel> provider,
            PixelColorBlendingMode blending,
            PixelAlphaCompositionMode composition,
            Image<TPixel> img)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            img.DebugSave(
                provider,
                new { composition, blending },
                appendPixelTypeToFileName: false,
                appendSourceFileOrDescription: false);

            var comparer = ImageComparer.TolerantPercentage(0.01f, 3);
            img.CompareFirstFrameToReferenceOutput(comparer,
                provider,
                new { composition, blending },
                appendPixelTypeToFileName: false,
                appendSourceFileOrDescription: false);
        }
    }
}
