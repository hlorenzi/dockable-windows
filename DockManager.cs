using System.Windows.Forms;
using System.Collections.Generic;


namespace DockableWindows
{
    public class DockManager : Control
    {
        private DockRegion rootRegion;
        private DockManagerOverlay overlay;


        public DockManager()
        {
            this.rootRegion = new DockRegion(this);
            this.Controls.Add(this.rootRegion);

            this.overlay = new DockManagerOverlay();
            this.overlay.Hide();
            this.overlay.Paint += PaintOverlay;
            this.Controls.Add(this.overlay);
        }


        public void CreateFloatingForm(DockContent content)
        {
            var poppedForm = new DockFloatingForm(this);
            poppedForm.Owner = this.FindForm();
            poppedForm.Controls.Add(content);
            poppedForm.Show();
            poppedForm.DragEnd();
        }


        public void PopContentOut(DockContent content)
        {
            var panel = this.rootRegion.GetRegionWithContent(content);
            if (panel == null)
                return;

            var panelBounds = panel.Bounds;
            var panelPosition = panel.PointToScreen(new System.Drawing.Point(0, 0));

            DockContent poppedContent;

            var splitPanel = panel.ParentRegion;
            if (splitPanel != null)
                poppedContent = splitPanel.Unsplit(splitPanel.GetIfRightOrBottomSplitHasContent(content));
            else
                poppedContent = panel.RetrieveContent();

            var poppedForm = new DockFloatingForm(this);
            poppedForm.Owner = this.FindForm();
            poppedForm.Controls.Add(poppedContent);
            poppedForm.Show();

            poppedForm.SetBounds(panelPosition.X, panelPosition.Y, panelBounds.Width, panelBounds.Height);
            poppedForm.DragEnd();
        }


        public void DockContentIn(DockContent content, DockRegion region, bool vertical, bool rightOrBottom)
        {
            if (!region.IsSplit && !region.HasContent)
            {
                region.SetContent(content);
            }
            else
            {
                region.Split(vertical, !rightOrBottom);

                if (rightOrBottom)
                    region.SplitRegion2.SetContent(content);
                else
                    region.SplitRegion1.SetContent(content);
            }
        }


        public void AnchorAttemptMove()
        {
            if (!this.attemptingToAnchor)
                RebuildAnchors();

            this.attemptingToAnchor = true;
            CheckAnchorsAtMouse();

            if (this.hoveringAnchor != this.hoveringAnchorLast ||
                this.hoveringRegion != this.hoveringRegionLast)
            {
                this.hoveringAnchorLast = this.hoveringAnchor;
                this.hoveringRegionLast = this.hoveringRegion;

                this.overlay.Hide();
                this.Refresh();
                this.overlay.BringToFront();
                this.overlay.Show();
            }
        }


        public void AnchorAttemptEnd(DockFloatingForm floatWindow)
        {
            if (this.hoveringAnchor >= 0)
            {
                var region = (this.hoveringRegion ?? this.rootRegion);
                var anchor = this.hoverAnchors[this.hoveringAnchor];

                var content = floatWindow.RetrieveContentAndClose();
                DockContentIn(content, region, anchor.vertical, anchor.rightOrBottom);
            }

            this.attemptingToAnchor = false;
            this.overlay.Hide();
            this.Refresh();
        }


        private class AnchorSpot
        {
            public DockRegion region;
            public int xVisible, yVisible, widthVisible, heightVisible;
            public int x, y, width, height;
            public bool vertical;
            public bool rightOrBottom;
        }


        private bool attemptingToAnchor = false;
        private List<AnchorSpot> hoverAnchors = new List<AnchorSpot>();
        private int hoveringAnchor = -1;
        private int hoveringAnchorLast = -1;
        private DockRegion hoveringRegion = null;
        private DockRegion hoveringRegionLast = null;


        private void RebuildAnchors()
        {
            this.hoverAnchors = new List<AnchorSpot>();
            this.hoveringAnchor = -1;
            this.hoveringAnchorLast = -1;
            this.hoveringRegion = null;
            this.hoveringRegionLast = null;

            var anchorSize = 30;
            var anchorOffset = 40;
            var anchorsX = new int[] { 0, -1, 1, 0 };
            var anchorsY = new int[] { -1, 0, 0, 1 };

            for (var i = 0; i < 4; i++)
            {
                this.hoverAnchors.Add(new AnchorSpot
                {
                    region = null,
                    vertical = (i == 0 || i == 3),
                    rightOrBottom = (i == 2 || i == 3),
                    xVisible = 0,
                    yVisible = 0,
                    widthVisible = this.Width,
                    heightVisible = this.Height,
                    x = this.Width / 2 + (anchorsX[i] * this.Width / 2) - anchorSize / 2,
                    y = this.Height / 2 + (anchorsY[i] * this.Height / 2) - anchorSize / 2,
                    width = anchorSize,
                    height = anchorSize
                });
            }

            foreach (var region in this.rootRegion.EnumerateLeafRegions())
            {
                var rect = region.GetScreenRectangle();
                var point = PointToClient(new System.Drawing.Point(rect.X, rect.Y));

                var centerX = point.X + rect.Width / 2;
                var centerY = point.Y + rect.Height / 2;
                for (var i = 0; i < 4; i++)
                {
                    this.hoverAnchors.Add(new AnchorSpot
                    {
                        region = region,
                        vertical = (i == 0 || i == 3),
                        rightOrBottom = (i == 2 || i == 3),
                        xVisible = point.X,
                        yVisible = point.Y,
                        widthVisible = rect.Width,
                        heightVisible = rect.Height,
                        x = centerX + anchorsX[i] * anchorOffset - anchorSize / 2,
                        y = centerY + anchorsY[i] * anchorOffset - anchorSize / 2,
                        width = anchorSize,
                        height = anchorSize
                    });
                }
            }
        }


        private void CheckAnchorsAtMouse()
        {
            var mousePos = PointToClient(System.Windows.Forms.Cursor.Position);
            this.hoveringAnchor = -1;
            this.hoveringRegion = null;

            for (var i = 0; i < this.hoverAnchors.Count; i++)
            {
                var anchor = this.hoverAnchors[i];

                if (mousePos.X >= anchor.xVisible && mousePos.Y >= anchor.yVisible &&
                    mousePos.X <= anchor.xVisible + anchor.widthVisible &&
                    mousePos.Y <= anchor.yVisible + anchor.heightVisible)
                {
                    this.hoveringRegion = anchor.region;

                    if (mousePos.X >= anchor.x && mousePos.Y >= anchor.y &&
                        mousePos.X <= anchor.x + anchor.width &&
                        mousePos.Y <= anchor.y + anchor.height)
                    {
                        this.hoveringAnchor = i;
                        break;
                    }
                }
            }
        }


        private void PaintOverlay(object sender, PaintEventArgs e)
        {
            var mousePos = PointToClient(System.Windows.Forms.Cursor.Position);

            for (var i = 0; i < this.hoverAnchors.Count; i++)
            {
                var anchor = this.hoverAnchors[i];

                if (!(mousePos.X >= anchor.xVisible && mousePos.Y >= anchor.yVisible &&
                    mousePos.X <= anchor.xVisible + anchor.widthVisible &&
                    mousePos.Y <= anchor.yVisible + anchor.heightVisible))
                    continue;

                var mouseOnTop = (this.hoveringAnchor == i);

                var margin = (mouseOnTop ? 3 : 0);

                e.Graphics.FillRectangle(
                    (mouseOnTop ? System.Drawing.Brushes.Blue : System.Drawing.Brushes.LightGray),
                    anchor.x - margin, anchor.y - margin,
                    anchor.width + margin * 2, anchor.height + margin * 2);

                e.Graphics.DrawRectangle(new System.Drawing.Pen(System.Drawing.Color.Black, 2),
                    anchor.x - margin, anchor.y - margin,
                    anchor.width + margin * 2, anchor.height + margin * 2);
            }
        }


        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.FillRectangle(System.Drawing.Brushes.Red, 0, 0, this.Width, this.Height);
            base.OnPaint(e);
        }
    }
}
