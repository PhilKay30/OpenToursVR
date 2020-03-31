using System.Windows.Media;

namespace Mapping.CustomControls
{
    public class MenuButton : AbstractButton
    {
        protected override Brush GetMainColour()
        {
            return Brushes.Lavender;
        }

        protected override Brush GetHoverColour()
        {
            return Brushes.Plum;
        }
    }
}
