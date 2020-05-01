// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Numerics;
using SixLabors.ImageSharp.PixelFormats;

namespace SixLabors.ImageSharp.Drawing.Processing
{
    /// <summary>
    /// Base class for Gradient brushes
    /// </summary>
    public abstract class GradientBrush : IBrush
    {
        /// <inheritdoc cref="IBrush"/>
        /// <param name="repetitionMode">Defines how the colors are repeated beyond the interval [0..1]</param>
        /// <param name="colorStops">The gradient colors.</param>
        protected GradientBrush(
            GradientRepetitionMode repetitionMode,
            params ColorStop[] colorStops)
        {
            this.RepetitionMode = repetitionMode;
            this.ColorStops = colorStops;
        }

        /// <summary>
        /// Gets how the colors are repeated beyond the interval [0..1].
        /// </summary>
        protected GradientRepetitionMode RepetitionMode { get; }

        /// <summary>
        /// Gets the list of color stops for this gradient.
        /// </summary>
        protected ColorStop[] ColorStops { get; }

        /// <inheritdoc />
        public abstract BrushApplicator<TPixel> CreateApplicator<TPixel>(
            Configuration configuration,
            GraphicsOptions options,
            ImageFrame<TPixel> source,
            RectangleF region)
            where TPixel : unmanaged, IPixel<TPixel>;

        /// <summary>
        /// Base class for gradient brush applicators
        /// </summary>
        internal abstract class GradientBrushApplicator<TPixel> : BrushApplicator<TPixel>
            where TPixel : unmanaged, IPixel<TPixel>
        {
            private static readonly TPixel Transparent = Color.Transparent.ToPixel<TPixel>();

            private readonly ColorStop[] colorStops;

            private readonly GradientRepetitionMode repetitionMode;

            /// <summary>
            /// Initializes a new instance of the <see cref="GradientBrushApplicator{TPixel}"/> class.
            /// </summary>
            /// <param name="configuration">The configuration instance to use when performing operations.</param>
            /// <param name="options">The graphics options.</param>
            /// <param name="target">The target image.</param>
            /// <param name="colorStops">An array of color stops sorted by their position.</param>
            /// <param name="repetitionMode">Defines if and how the gradient should be repeated.</param>
            protected GradientBrushApplicator(
                Configuration configuration,
                GraphicsOptions options,
                ImageFrame<TPixel> target,
                ColorStop[] colorStops,
                GradientRepetitionMode repetitionMode)
                : base(configuration, options, target)
            {
                this.colorStops = colorStops; // TODO: requires colorStops to be sorted by position - should that be checked?
                this.repetitionMode = repetitionMode;
            }

            /// <inheritdoc/>
            internal override TPixel this[int x, int y]
            {
                get
                {
                    float positionOnCompleteGradient = this.PositionOnGradient(x + 0.5f, y + 0.5f);

                    switch (this.repetitionMode)
                    {
                        case GradientRepetitionMode.None:
                            // do nothing. The following could be done, but is not necessary:
                            // onLocalGradient = Math.Min(0, Math.Max(1, onLocalGradient));
                            break;
                        case GradientRepetitionMode.Repeat:
                            positionOnCompleteGradient %= 1;
                            break;
                        case GradientRepetitionMode.Reflect:
                            positionOnCompleteGradient %= 2;
                            if (positionOnCompleteGradient > 1)
                            {
                                positionOnCompleteGradient = 2 - positionOnCompleteGradient;
                            }

                            break;
                        case GradientRepetitionMode.DontFill:
                            if (positionOnCompleteGradient > 1 || positionOnCompleteGradient < 0)
                            {
                                return Transparent;
                            }

                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    (ColorStop from, ColorStop to) = this.GetGradientSegment(positionOnCompleteGradient);

                    if (from.Color.Equals(to.Color))
                    {
                        return from.Color.ToPixel<TPixel>();
                    }
                    else
                    {
                        float onLocalGradient = (positionOnCompleteGradient - from.Ratio) / (to.Ratio - from.Ratio);
                        return new Color(Vector4.Lerp((Vector4)from.Color, (Vector4)to.Color, onLocalGradient)).ToPixel<TPixel>();
                    }
                }
            }

            /// <summary>
            /// calculates the position on the gradient for a given point.
            /// This method is abstract as it's content depends on the shape of the gradient.
            /// </summary>
            /// <param name="x">The x-coordinate of the point.</param>
            /// <param name="y">The y-coordinate of the point.</param>
            /// <returns>
            /// The position the given point has on the gradient.
            /// The position is not bound to the [0..1] interval.
            /// Values outside of that interval may be treated differently,
            /// e.g. for the <see cref="GradientRepetitionMode" /> enum.
            /// </returns>
            protected abstract float PositionOnGradient(float x, float y);

            private (ColorStop from, ColorStop to) GetGradientSegment(
                float positionOnCompleteGradient)
            {
                ColorStop localGradientFrom = this.colorStops[0];
                ColorStop localGradientTo = default;

                // TODO: ensure colorStops has at least 2 items (technically 1 would be okay, but that's no gradient)
                foreach (ColorStop colorStop in this.colorStops)
                {
                    localGradientTo = colorStop;

                    if (colorStop.Ratio > positionOnCompleteGradient)
                    {
                        // we're done here, so break it!
                        break;
                    }

                    localGradientFrom = localGradientTo;
                }

                return (localGradientFrom, localGradientTo);
            }
        }
    }
}
