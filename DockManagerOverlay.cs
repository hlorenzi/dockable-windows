using System.Windows.Forms;


namespace DockableWindows
{
    class DockManagerOverlay : Control
    {
        public DockManagerOverlay()
        {
            this.Dock = DockStyle.Fill;
        }


        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            // Do not paint background.
        }
    }
}
