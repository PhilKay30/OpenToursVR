using System.Windows.Media;

namespace Mapping.CustomControls
{
    public class NavigationButton : AbstractButton
    {
        protected override Brush GetMainColour()
        {
            return Brushes.LightSkyBlue;
        }

        protected override Brush GetHoverColour()
        {
            return Brushes.CornflowerBlue;
        }
    }
}
