using System.Windows.Forms;

namespace PUBG_MAPHACK
{
    public partial class hidden : Form
    {
        public hidden()
        {
            InitializeComponent();          
            this.WindowState = FormWindowState.Minimized;
            this.ShowInTaskbar = false;
            this.WindowState = FormWindowState.Normal; // Seems to do the trick in windows 8
        }
    }
}
