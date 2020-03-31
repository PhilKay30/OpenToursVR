using System.Windows.Media;

namespace Mapping.CustomControls
{
    public class ConfirmButton : AbstractButton
    {
        protected override Brush GetMainColour()
        {
            return Brushes.LightGreen;
        }

        protected override Brush GetHoverColour()
        {
            return Brushes.DarkSeaGreen;
        }
    }
}
