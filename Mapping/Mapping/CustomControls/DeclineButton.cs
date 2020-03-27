using System.Windows.Media;

namespace Mapping.CustomControls
{
    public class DeclineButton : AbstractButton
    {
        protected override Brush GetMainColour()
        {
            return Brushes.LightCoral;
        }

        protected override Brush GetHoverColour()
        {
            return Brushes.IndianRed;
        }
    }
}
