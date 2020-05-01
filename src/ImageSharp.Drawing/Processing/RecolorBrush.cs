// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Buffers;
using System.Numerics;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Memory;
using SixLabors.ImageSharp.PixelFormats;

namespace SixLabors.ImageSharp.Drawing.Processing
{
    /// <summary>
    /// Provides an implementation of a brush that can recolor an image
    /// </summary>
    public class RecolorBrush : IBrush
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RecolorBrush" /> class.
        /// </summary>
        /// <param name="sourceColor">Color of the source.</param>
        /// <param name="targetColor">Color of the target.</param>
        /// <param name="threshold">The threshold as a value between 0 and 1.</param>
        public RecolorBrush(Color sourceColor, Color targetColor, float threshold)
        {
            this.SourceColor = sourceColor;
            this.Threshold = threshold;
            this.TargetColor = targetColor;
        }

        /// <summary>
        /// Gets the threshold.
        /// </summary>
        public float Threshold { get; }

        /// <summary>
        /// Gets the source color.
        /// </summary>
        public Color SourceColor { get; }

        /// <summary>
        /// Gets the target color.
        /// </summary>
        public Color TargetColor { get; }

        /// <inheritdoc />
        public BrushApplicator<TPixel> CreateApplicator<TPixel>(
            Configuration configuration,
            GraphicsOptions options,
            ImageFrame<TPixel> source,
            RectangleF region)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            return new RecolorBrushApplicator<TPixel>(
                configuration,
                options,
                source,
                this.SourceColor.ToPixel<TPixel>(),
                this.TargetColor.ToPixel<TPixel>(),
                this.Threshold);
        }

        /// <summary>
        /// The recolor brush applicator.
        /// </summary>
        private class RecolorBrushApplicator<TPixel> : BrushApplicator<TPixel>
            where TPixel : unmanaged, IPixel<TPixel>
        {
            /// <summary>
            /// The source color.
            /// </summary>
            private readonly Vector4 sourceColor;

            /// <summary>
            /// The threshold.
            /// </summary>
            private readonly float threshold;

            private readonly TPixel targetColorPixel;

            /// <summary>
            /// Initializes a new instance of the <see cref="RecolorBrushApplicator{TPixel}" /> class.
            /// </summary>
            /// <param name="configuration">The configuration instance to use when performing operations.</param>
            /// <param name="options">The options</param>
            /// <param name="source">The source image.</param>
            /// <param name="sourceColor">Color of the source.</param>
            /// <param name="targetColor">Color of the target.</param>
            /// <param name="threshold">The threshold .</param>
            public RecolorBrushApplicator(
                Configuration configuration,
                GraphicsOptions options,
                ImageFrame<TPixel> source,
                TPixel sourceColor,
                TPixel targetColor,
                float threshold)
                : base(configuration, options, source)
            {
                this.sourceColor = sourceColor.ToVector4();
                this.targetColorPixel = targetColor;

                // Lets hack a min max extremes for a color space by letting the IPackedPixel clamp our values to something in the correct spaces :)
                var maxColor = default(TPixel);
                maxColor.FromVector4(new Vector4(float.MaxValue));
                var minColor = default(TPixel);
                minColor.FromVector4(new Vector4(float.MinValue));
                this.threshold = Vector4.DistanceSquared(maxColor.ToVector4(), minColor.ToVector4()) * threshold;
            }

            /// <inheritdoc />
            internal override TPixel this[int x, int y]
            {
                get
                {
                    // Offset the requested pixel by the value in the rectangle (the shapes position)
                    TPixel result = this.Target[x, y];
                    var background = result.ToVector4();
                    float distance = Vector4.DistanceSquared(background, this.sourceColor);
                    if (distance <= this.threshold)
                    {
                        float lerpAmount = (this.threshold - distance) / this.threshold;
                        return this.Blender.Blend(
                            result,
                            this.targetColorPixel,
                            lerpAmount);
                    }

                    return result;
                }
            }

            /// <inheritdoc />
            internal override void Apply(Span<float> scanline, int x, int y)
            {
                MemoryAllocator memoryAllocator = this.Configuration.MemoryAllocator;

                using (IMemoryOwner<float> amountBuffer = memoryAllocator.Allocate<float>(scanline.Length))
                using (IMemoryOwner<TPixel> overlay = memoryAllocator.Allocate<TPixel>(scanline.Length))
                {
                    Span<float> amountSpan = amountBuffer.Memory.Span;
                    Span<TPixel> overlaySpan = overlay.Memory.Span;

                    for (int i = 0; i < scanline.Length; i++)
                    {
                        amountSpan[i] = scanline[i] * this.Options.BlendPercentage;

                        int offsetX = x + i;

                        // No doubt this one can be optimized further but I can't imagine its
                        // actually being used and can probably be removed/internalized for now
                        overlaySpan[i] = this[offsetX, y];
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
