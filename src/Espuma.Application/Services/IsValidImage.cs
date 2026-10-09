using SixLabors.ImageSharp;

namespace Espuma.Application.Services
{
    public interface IIsValidImage
    {
        bool ValidateImage(Stream imageStream);
    }
    public class IsValidImage : IIsValidImage
    {
        public bool ValidateImage(Stream imageStream)
        {
            try
            {
                var originalPosition = imageStream.Position;
                imageStream.Position = 0;

                //NO HAY QUE USAR USING O DISPOSE, DISPONER DEL STREAM EN ESTE PUNTO PROVOCA UNA EXCEPCIÓN.
                var image = Image.Identify(imageStream);

                imageStream.Position = originalPosition;
                return image != null;
            }
            catch
            {
                return false;
            }
        }
    }
}
