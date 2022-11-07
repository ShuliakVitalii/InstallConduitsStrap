using System.Windows.Media.Imaging;
using System.Reflection;

namespace InstallConduitsStrap.Common
{
    public class IconManager
    {
        /// <summary>
        /// Gets the icon from reource assembly.
        /// </summary>
        /// <param iconName="iconName">The iconName.</param>
        /// <returns></returns>
        public static BitmapImage GetIcon(string iconName) 
        {

            // Create the resource reader stream.
            var stream = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream(typeof(App).Namespace+".Icons."
                                                                + iconName);

            //Construct and return icon.
            var icon = new BitmapImage();
            icon.BeginInit();
            icon.StreamSource = stream;
            icon.EndInit();
                   

            //Return constructed BitmapImage.
            return icon;
        }
    }
}