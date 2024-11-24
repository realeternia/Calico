using System.Windows.Forms;

namespace MemoHippo.UIS
{
    public class DoubleBufferedPanel : Panel
    {
        public DoubleBufferedPanel()
        {
            this.DoubleBuffered = true; 
        }
    }
}
