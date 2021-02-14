using System;
using System.Collections.Generic;

using pdxpartyparrot.Core.UI;

namespace pdxpartyparrot.Core.DebugMenu
{
    public sealed class DebugMenuNode : IEquatable<DebugMenuNode>, IComparable<DebugMenuNode>
    {
        public Func<string> Title { get; }

        public DebugMenuNode Parent { get; }

        public Action RenderContentsAction { get; set; }

        public int Priority { get; private set; }

        private readonly List<DebugMenuNode> _children = new List<DebugMenuNode>();

        public DebugMenuNode(Func<string> title, int priority = 0)
        {
            Title = title;
            Priority = priority;
        }

        public DebugMenuNode(Func<string> title, DebugMenuNode parent, int priority = 0)
        {
            Title = title;
            Parent = parent;
            Priority = priority;
        }

        public void RenderNode()
        {
            if(GUIUtils.LayoutButton(Title())) {
                DebugMenuManager.Instance.SetCurrentNode(this);
            }
        }

        public void RenderContents()
        {
            foreach(DebugMenuNode child in _children) {
                child.RenderNode();
            }

            RenderContentsAction?.Invoke();
        }

        public DebugMenuNode AddNode(Func<string> title, int priority = 0)
        {
            DebugMenuNode node = new DebugMenuNode(title, this, priority);
            _children.Add(node);
            _children.Sort();
            return node;
        }

        public bool Equals(DebugMenuNode other)
        {
            if(Priority == other.Priority) {
                return Title().Equals(other.Title());
            }
            return false;
        }

        public int CompareTo(DebugMenuNode other)
        {
            if(Priority == other.Priority) {
                return Title().CompareTo(other.Title());
            }
            return other.Priority - Priority;
        }
    }
}
