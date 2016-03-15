using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using CodeFluent.Runtime.Utilities;
using SoftFluent.Windows;

namespace RowShareTool.Model
{
    public abstract class TreeItem : AutoObject, IComparable, IComparable<TreeItem>
    {
        private ObservableCollection<TreeItem> _children = new ObservableCollection<TreeItem>();
        private static readonly LazyItem Lazy = new LazyItem();

        private class LazyItem : TreeItem
        {
            public LazyItem()
                : base(null)
            {
            }
        }

        protected TreeItem(TreeItem parent)
            : this(parent, false)
        {
        }

        protected TreeItem(TreeItem parent, bool hasLazy)
        {
            Parent = parent;
            if (hasLazy)
            {
                _children.Add(Lazy);
            }
        }

        [Browsable(false)]
        [JsonUtilities(IgnoreWhenSerializing = true)]
        public TreeItem Parent { get; private set; }

        public virtual string Url
        {
            get
            {
                return null;
            }
        }

        public virtual string DisplayName
        {
            get
            {
                return GetProperty<string>();
            }
            set
            {
                SetProperty(value);
            }
        }

        [Browsable(false)]
        public virtual bool IsExpanded
        {
            get
            {
                return GetProperty<bool>();
            }
            set
            {
                if (SetProperty(value, false, false))
                {
                    if (IsExpanded && Parent != null)
                    {
                        Parent.IsExpanded = true;
                    }

                    LazyLoadChildren();
                }
            }
        }

        [Browsable(false)]
        public virtual bool IsSelected
        {
            get
            {
                return GetProperty<bool>();
            }
            set
            {
                SetProperty(value, false, false);
            }
        }

        public void LazyLoadChildren()
        {
            if (!HasLazyChild)
                return;

            Children.Remove(Lazy);
            LoadChildren();
        }

        public  void ChildrenClear()
        {
            if (HasLazyChild)
                return;

            Children.Clear();
        }

        public virtual bool Delete()
        {
            return false;
        }

        public virtual void Reload()
        {
        }

        public void ReloadChildren()
        {
            ChildrenClear();
            LoadChildren();
        }

        protected virtual void LoadChildren()
        {
        }

        [Browsable(false)]
        public ObservableCollection<TreeItem> Children
        {
            get
            {
                return _children;
            }
        }

        [Browsable(false)]
        public bool HasLazyChild
        {
            get
            {
                return Children.Count > 0 && Children[0] == Lazy;
            }
        }

        public override string ToString()
        {
            return DisplayName;
        }

        public virtual T Get<T>()
        {
            if (typeof(T).IsAssignableFrom(GetType()))
                return (T)(object)this;

            return Parent != null ? Parent.Get<T>() : default(T);
        }

        int IComparable.CompareTo(object obj)
        {
            return CompareTo(obj as TreeItem);
        }

        public virtual int CompareTo(TreeItem other)
        {
            if (other == null)
                throw new ArgumentNullException("other");

            if (DisplayName == null)
            {
                if (other.DisplayName == null)
                    return 0;

                return 1;
            }
            else if (other.DisplayName == null)
                return -1;

            return DisplayName.CompareTo(other.DisplayName);
        }
    }
}
