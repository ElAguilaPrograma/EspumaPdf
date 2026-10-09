namespace Espuma.Application.Common
{
    /// <summary>Clockwise rotation in degrees.</summary>
    public enum PageRotation
    {
        None = 0,
        Clockwise90 = 90,
        Rotate180 = 180,
        Clockwise270 = 270
    }

    public static class PageRotationExtensions
    {
        public static PageRotation RotateClockwise(this PageRotation rotation) =>
            (PageRotation)(((int)rotation + 90) % 360);

        public static PageRotation RotateCounterClockwise(this PageRotation rotation) =>
            (PageRotation)(((int)rotation + 270) % 360);
    }
}
