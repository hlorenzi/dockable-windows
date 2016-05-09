using System.Windows.Forms;


namespace DockableWindows
{
    class FormTest : Form
    {
        public FormTest()
        {
            this.SuspendLayout();

            var random = new System.Random();

            var menuStrip = new MenuStrip();
            var spawnWindowItem = menuStrip.Items.Add("Spawn Window");


            var dockManager = new DockManager();
            dockManager.Dock = DockStyle.Fill;

            spawnWindowItem.Click += (sender, args) =>
            {
                var content = new DockContent(dockManager);
                dockManager.CreateFloatingForm(content);

                var panel = new Panel
                {
                    Dock = DockStyle.Fill,
                    BackColor = System.Drawing.Color.FromArgb(random.Next(255), random.Next(255), random.Next(255))
                };

                panel.Controls.Add(new Button { Text = "Hello, world!" });
                content.Controls.Add(panel);
            };


            this.Controls.Add(dockManager);
            this.Controls.Add(menuStrip);
            this.MainMenuStrip = menuStrip;

            this.Width = 900;
            this.Height = 600;
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
