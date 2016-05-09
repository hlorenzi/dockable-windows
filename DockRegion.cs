using System.Windows.Forms;
using System.Collections.Generic;


namespace DockableWindows
{
    public class DockRegion : Control
    {
        private DockManager dockManager;
        private DockContent content = null;
        private SplitContainer splitContainer = null;


        public DockRegion(DockManager dockManager)
        {
            this.dockManager = dockManager;
            this.Dock = DockStyle.Fill;
        }


        public bool HasContent
        {
            get
            {
                return (this.content != null);
            }
        }


        public bool IsSplit
        {
            get
            {
                return (this.splitContainer != null);
            }
        }


        public DockRegion SplitRegion1
        {
            get
            {
                if (!this.IsSplit)
                    throw new System.Exception("Region is not split.");

                return (DockRegion)this.splitContainer.Panel1.Controls[0];
            }
        }


        public DockRegion SplitRegion2
        {
            get
            {
                if (!this.IsSplit)
                    throw new System.Exception("Region is not split.");

                return (DockRegion)this.splitContainer.Panel2.Controls[0];
            }
        }


        public DockRegion ParentRegion
        {
            get
            {
                var parentSplitterPanel = (this.Parent as SplitterPanel);
                if (parentSplitterPanel == null)
                    return null;

                var parentSplitter = (parentSplitterPanel.Parent as SplitContainer);
                if (parentSplitter == null)
                    return null;

                return (parentSplitter.Parent as DockRegion);
            }
        }


        public void SetContent(DockContent content)
        {
            if (this.IsSplit)
                throw new System.Exception("Region is split; cannot receive content.");

            this.content = content;
            this.Controls.Add(content);
        }


        public DockContent RetrieveContent()
        {
            if (this.content == null)
                return null;

            var window = this.content;
            this.Controls.Remove(this.content);
            this.content = null;
            return window;
        }


        public void Split(bool vertical, bool newToTheLeftOrTop)
        {
            var innerSplitBefore = this.splitContainer;
            if (this.splitContainer != null)
            {
                this.Controls.Remove(this.splitContainer);
                this.splitContainer = null;
            }

            var contentBefore = this.content;
            if (this.content != null)
            {
                this.Controls.Remove(this.content);
                this.content = null;
            }

            this.splitContainer = new SplitContainer();
            this.splitContainer.Dock = DockStyle.Fill;
            this.splitContainer.Orientation = (vertical ? Orientation.Horizontal : Orientation.Vertical);
            this.Controls.Add(this.splitContainer);

            var splitRegion1 = new DockRegion(this.dockManager);
            var splitRegion2 = new DockRegion(this.dockManager);
            this.splitContainer.Panel1.Controls.Add(splitRegion1);
            this.splitContainer.Panel2.Controls.Add(splitRegion2);

            if (innerSplitBefore != null)
            {
                if (newToTheLeftOrTop)
                {
                    splitRegion2.splitContainer = innerSplitBefore;
                    splitRegion2.Controls.Add(innerSplitBefore);
                }
                else
                {
                    splitRegion1.splitContainer = innerSplitBefore;
                    splitRegion1.Controls.Add(innerSplitBefore);
                }
            }

            if (contentBefore != null)
            {
                if (newToTheLeftOrTop)
                    splitRegion2.SetContent(contentBefore);
                else
                    splitRegion1.SetContent(contentBefore);
            }
        }


        public DockContent Unsplit(bool retrieveRightOrBottom)
        {
            if (!this.IsSplit)
                throw new System.Exception("Region is not split.");

            var splitRegion1 = (DockRegion)this.splitContainer.Panel1.Controls[0];
            var splitRegion2 = (DockRegion)this.splitContainer.Panel2.Controls[0];

            this.Controls.Remove(this.splitContainer);
            this.splitContainer = null;

            if (retrieveRightOrBottom)
            {
                if (splitRegion1 != null)
                {
                    if (splitRegion1.content != null)
                        this.SetContent(splitRegion1.content);
                    else if (splitRegion1.IsSplit)
                    {
                        var innerSplit = splitRegion1.splitContainer;
                        splitRegion1.Controls.Remove(innerSplit);
                        this.splitContainer = innerSplit;
                        this.Controls.Add(this.splitContainer);
                    }
                }

                return (splitRegion2 != null ? splitRegion2.content : null);
            }
            else
            {
                if (splitRegion2.content != null)
                    this.SetContent(splitRegion2.content);
                else if (splitRegion2.IsSplit)
                {
                    var innerSplit = splitRegion2.splitContainer;
                    splitRegion2.Controls.Remove(innerSplit);
                    this.splitContainer = innerSplit;
                    this.Controls.Add(this.splitContainer);
                }

                return (splitRegion1 != null ? splitRegion1.content : null);
            }
        }


        public System.Drawing.Rectangle GetScreenRectangle()
        {
            var rect = this.Bounds;
            var point = PointToScreen(this.Location);

            return new System.Drawing.Rectangle(point.X, point.Y, rect.Width, rect.Height);
        }


        public IEnumerable<DockRegion> EnumerateLeafRegions()
        {
            if (!this.IsSplit)
                yield return this;
            else
            {
                var splitRegion1 = (DockRegion)this.splitContainer.Panel1.Controls[0];
                var splitRegion2 = (DockRegion)this.splitContainer.Panel2.Controls[0];

                if (splitRegion1 != null)
                {
                    foreach (var inner in splitRegion1.EnumerateLeafRegions())
                        yield return inner;
                }

                if (splitRegion2 != null)
                {
                    foreach (var inner in splitRegion2.EnumerateLeafRegions())
                        yield return inner;
                }
            }
        }


        public DockRegion GetRegionWithContent(DockContent content)
        {
            if (!this.IsSplit)
            {
                if (this.content == content)
                    return this;

                return null;
            }
            else
            {
                var splitRegion1 = (DockRegion)this.splitContainer.Panel1.Controls[0];
                var splitRegion2 = (DockRegion)this.splitContainer.Panel2.Controls[0];

                if (splitRegion1 != null)
                {
                    var inner = splitRegion1.GetRegionWithContent(content);
                    if (inner != null)
                        return inner;
                }

                if (splitRegion2 != null)
                {
                    var inner = splitRegion2.GetRegionWithContent(content);
                    if (inner != null)
                        return inner;
                }

                return null;
            }
        }


        public bool GetIfRightOrBottomSplitHasContent(DockContent content)
        {
            if (!this.IsSplit)
                throw new System.Exception("Region is not split.");

            var splitRegion1 = (DockRegion)this.splitContainer.Panel1.Controls[0];
            var splitRegion2 = (DockRegion)this.splitContainer.Panel2.Controls[0];

            if (splitRegion1 != null)
            {
                var inner = splitRegion1.GetRegionWithContent(content);
                if (inner != null)
                    return false;
            }

            if (splitRegion2 != null)
            {
                var inner = splitRegion2.GetRegionWithContent(content);
                if (inner != null)
                    return true;
            }

            throw new System.Exception("Region does not contain the given content.");
        }
    }
}
