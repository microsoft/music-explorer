using Microsoft.Phone.Controls;
using System.Windows.Controls;

namespace MusicExplorer
{
    public class TiltGrid : Grid
    {
        public TiltGrid()
        {
            if (!TiltEffect.TiltableItems.Contains(typeof(TiltGrid)))
            {
                TiltEffect.TiltableItems.Add(typeof(TiltGrid));
            }
        }
    }
}
