using System;
using System.Windows.Forms;


namespace DockableWindows
{
    public class DockFloatingForm : Form
    {
        protected DockManager dockManager;


        public DockFloatingForm(DockManager dockManager)
        {
            this.dockManager = dockManager;
            this.FormBorderStyle = FormBorderStyle.SizableToolWindow;
            this.ShowInTaskbar = false;
            this.LocationChanged += OnDragMove;
            this.ResizeEnd += OnDragEnd;
        }


        public DockContent RetrieveContentAndClose()
        {
            var content = (DockContent)this.Controls[0];
            this.Controls.Remove(content);
            this.Close();
            return content;
        }


        public void OnDragMove(object sender, EventArgs e)
        {
            this.Opacity = 0.25;
            this.dockManager.AnchorAttemptMove();
        }


        public void OnDragEnd(object sender, EventArgs e)
        {
            this.Opacity = 1;
            this.dockManager.AnchorAttemptEnd(this);
        }


        public void DragEnd()
        {
            this.Opacity = 1;
        }
    }
}
