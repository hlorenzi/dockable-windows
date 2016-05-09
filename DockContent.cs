using System.Windows.Forms;


namespace DockableWindows
{
    public class DockContent : Control
    {
        private const int titleHeight = 16;

        private DockManager dockManager;
        private TableLayoutPanel tableLayout;
        private Panel contentLayout;
        private Button popOutButton;


        public new ControlCollection Controls
        {
            get
            {
                return this.contentLayout.Controls;
            }
        }


        public DockContent(DockManager dockManager)
        {
            this.dockManager = dockManager;

            this.Dock = DockStyle.Fill;

            this.tableLayout = new TableLayoutPanel();
            this.tableLayout.Dock = DockStyle.Fill;
            this.tableLayout.ColumnCount = 1;
            this.tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 1));
            this.tableLayout.RowCount = 2;
            this.tableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, titleHeight));
            this.tableLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize, 1));

            this.tableLayout.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;

            this.popOutButton = new Button();
            this.popOutButton.Text = "^";
            this.popOutButton.Anchor = AnchorStyles.Right;
            this.popOutButton.SetBounds(0, 0, titleHeight, titleHeight);
            this.popOutButton.Margin = new Padding(0);
            this.popOutButton.Click += PopOut;
            this.tableLayout.Controls.Add(this.popOutButton, 0, 0);

            this.contentLayout = new Panel();
            this.contentLayout.Dock = DockStyle.Fill;
            this.tableLayout.Controls.Add(this.contentLayout, 0, 1);

            base.Controls.Add(this.tableLayout);
        }


        private void PopOut(object sender, System.EventArgs e)
        {
            this.dockManager.PopContentOut(this);
        }
    }
}
