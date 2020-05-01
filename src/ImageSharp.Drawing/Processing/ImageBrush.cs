// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Buffers;

using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Memory;
using SixLabors.ImageSharp.PixelFormats;

namespace SixLabors.ImageSharp.Drawing.Processing
{
    /// <summary>
    /// Provides an implementation of an image brush for painting images within areas.
    /// </summary>
    public class ImageBrush : IBrush
    {
        /// <summary>
        /// The image to paint.
        /// </summary>
        private readonly Image image;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageBrush"/> class.
        /// </summary>
        /// <param name="image">The image.</param>
        public ImageBrush(Image image)
        {
            this.image = image;
        }

        /// <inheritdoc />
        public BrushApplicator<TPixel> CreateApplicator<TPixel>(
            Configuration configuration,
            GraphicsOptions options,
            ImageFrame<TPixel> source,
            RectangleF region)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            if (this.image is Image<TPixel> specificImage)
            {
                return new ImageBrushApplicator<TPixel>(configuration, options, source, specificImage, region, false);
            }

            specificImage = this.image.CloneAs<TPixel>();

            return new ImageBrushApplicator<TPixel>(configuration, options, source, specificImage, region, true);
        }

        /// <summary>
        /// The image brush applicator.
        /// </summary>
        private class ImageBrushApplicator<TPixel> : BrushApplicator<TPixel>
            where TPixel : unmanaged, IPixel<TPixel>
        {
            private ImageFrame<TPixel> sourceFrame;

            private Image<TPixel> sourceImage;

            private readonly bool shouldDisposeImage;

            /// <summary>
            /// The y-length.
            /// </summary>
            private readonly int yLength;

            /// <summary>
            /// The x-length.
            /// </summary>
            private readonly int xLength;

            /// <summary>
            /// The Y offset.
            /// </summary>
            private readonly int offsetY;

            /// <summary>
            /// The X offset.
            /// </summary>
            private readonly int offsetX;

            private bool isDisposed;

            /// <summary>
            /// Initializes a new instance of the <see cref="ImageBrushApplicator{TPixel}"/> class.
            /// </summary>
            /// <param name="configuration">The configuration instance to use when performing operations.</param>
            /// <param name="options">The graphics options.</param>
            /// <param name="target">The target image.</param>
            /// <param name="image">The image.</param>
            /// <param name="region">The region.</param>
            /// <param name="shouldDisposeImage">Whether to dispose the image on disposal of the applicator.</param>
            public ImageBrushApplicator(
                Configuration configuration,
                GraphicsOptions options,
                ImageFrame<TPixel> target,
                Image<TPixel> image,
                RectangleF region,
                bool shouldDisposeImage)
                : base(configuration, options, target)
            {
                this.sourceImage = image;
                this.sourceFrame = image.Frames.RootFrame;
                this.shouldDisposeImage = shouldDisposeImage;
                this.xLength = image.Width;
                this.yLength = image.Height;
                this.offsetY = (int)MathF.Max(MathF.Floor(region.Top), 0);
                this.offsetX = (int)MathF.Max(MathF.Floor(region.Left), 0);
            }

            /// <inheritdoc/>
            internal override TPixel this[int x, int y]
            {
                get
                {
                    int srcX = (x - this.offsetX) % this.xLength;
                    int srcY = (y - this.offsetY) % this.yLength;
                    return this.sourceFrame[srcX, srcY];
                }
            }

            /// <inheritdoc />
            protected override void Dispose(bool disposing)
            {
                if (this.isDisposed)
                {
                    return;
                }

                if (disposing && this.shouldDisposeImage)
                {
                    this.sourceImage?.Dispose();
                }

                this.sourceImage = null;
                this.sourceFrame = null;
                this.isDisposed = true;
            }

            /// <inheritdoc />
            internal override void Apply(Span<float> scanline, int x, int y)
            {
                // Create a span for colors
                MemoryAllocator allocator = this.Configuration.MemoryAllocator;
                using (IMemoryOwner<float> amountBuffer = allocator.Allocate<float>(scanline.Length))
                using (IMemoryOwner<TPixel> overlay = allocator.Allocate<TPixel>(scanline.Length))
                {
                    Span<float> amountSpan = amountBuffer.Memory.Span;
                    Span<TPixel> overlaySpan = overlay.Memory.Span;

                    int sourceY = (y - this.offsetY) % this.yLength;
                    int offsetX = x - this.offsetX;
                    Span<TPixel> sourceRow = this.sourceFrame.GetPixelRowSpan(sourceY);

                    for (int i = 0; i < scanline.Length; i++)
                    {
                        amountSpan[i] = scanline[i] * this.Options.BlendPercentage;

                        int sourceX = (i + offsetX) % this.xLength;
                        overlaySpan[i] = sourceRow[sourceX];
                    }

                    Span<TPixel> destinationRow = this.Target.GetPixelRowSpan(y).Slice(x, scanline.Length);
                    this.Blender.Blend(
                        this.Configuration,
                        destinationRow,
                        destinationRow,
                        overlaySpan,
                        amountSpan);
                }
            }
        }
    }
}
