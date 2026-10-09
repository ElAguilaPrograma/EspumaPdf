namespace Espuma.Application.Common
{
    /// <summary>A page of the source document (0-based) placed in the output with an extra rotation.</summary>
    public readonly record struct PageSelection(int SourceIndex, PageRotation Rotation = PageRotation.None);
}
