﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using SiliconStudio.Presentation.Collections;
using SiliconStudio.Presentation.Extensions;

namespace SiliconStudio.Presentation.Controls
{
    public class TreeView : ItemsControl
    {
        // the space where items will be realized if virtualization is enabled. This is set by virtualizingtreepanel.
        internal VirtualizingTreePanel.VerticalArea RealizationSpace = new VirtualizingTreePanel.VerticalArea();
        internal VirtualizingTreePanel.SizesCache CachedSizes = new VirtualizingTreePanel.SizesCache();
        private bool updatingSelection;
        private bool allowedSelectionChanges;
        private bool mouseDown;
        private object lastShiftRoot;
        TreeViewItem editedItem;

        public static DependencyProperty SelectedItemProperty = DependencyProperty.Register("SelectedItem", typeof(object), typeof(TreeView), new FrameworkPropertyMetadata(null, OnSelectedItemPropertyChanged));

        public static DependencyPropertyKey SelectedItemsProperty = DependencyProperty.RegisterReadOnly("SelectedItems", typeof(IList), typeof(TreeView), new FrameworkPropertyMetadata(null, OnSelectedItemsPropertyChanged));

        public static DependencyProperty SelectionModeProperty = DependencyProperty.Register("SelectionMode", typeof(SelectionMode), typeof(TreeView), new FrameworkPropertyMetadata(SelectionMode.Extended, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectionModeChanged));

        public static readonly DependencyProperty IsVirtualizingProperty = DependencyProperty.Register("IsVirtualizing", typeof(bool), typeof(TreeView), new PropertyMetadata(false));

        internal static bool IsControlKeyDown => (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;

        internal static bool IsShiftKeyDown => (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift;

        private bool isInitialized;
        private ScrollViewer scroller;

        static TreeView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TreeView), new FrameworkPropertyMetadata(typeof(TreeView)));

            FrameworkElementFactory vPanel = new FrameworkElementFactory(typeof(VirtualizingTreePanel));
            vPanel.SetValue(Panel.IsItemsHostProperty, true);
            ItemsPanelTemplate vPanelTemplate = new ItemsPanelTemplate { VisualTree = vPanel };
            ItemsPanelProperty.OverrideMetadata(typeof(TreeView), new FrameworkPropertyMetadata(vPanelTemplate));

            KeyboardNavigation.DirectionalNavigationProperty.OverrideMetadata(typeof(TreeView), new FrameworkPropertyMetadata(KeyboardNavigationMode.Contained));
            KeyboardNavigation.TabNavigationProperty.OverrideMetadata(typeof(TreeView), new FrameworkPropertyMetadata(KeyboardNavigationMode.None));
        }

        public TreeView()
        {
            SelectedItems = new NonGenericObservableListWrapper<object>(new ObservableList<object>());
        }

        public bool IsVirtualizing { get { return (bool)GetValue(IsVirtualizingProperty); } set { SetValue(IsVirtualizingProperty, value); } }

        /// <summary>
        /// Gets the last selected item.
        /// </summary>
        public object SelectedItem { get { return GetValue(SelectedItemProperty); } set { SetValue(SelectedItemProperty, value); } }

        /// <summary>
        /// Gets the list of selected items.
        /// </summary>
        public IList SelectedItems { get { return (IList)GetValue(SelectedItemsProperty.DependencyProperty); } private set { SetValue(SelectedItemsProperty, value); } }

        public SelectionMode SelectionMode { get { return (SelectionMode)GetValue(SelectionModeProperty); } set { SetValue(SelectionModeProperty, value); } }

        internal ScrollViewer ScrollViewer => scroller ?? (scroller = (ScrollViewer)Template.FindName("scroller", this));

        internal bool AllowMultipleSelection => SelectionMode != SelectionMode.Single;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (!isInitialized)
            {
                Loaded += OnLoaded;
                Unloaded += OnUnLoaded;
                OnLoaded(this, new RoutedEventArgs(LoadedEvent));
            }
        }

        // TODO: This method has been implemented with a lot of fail and retry, and should be cleaned.
        // TODO: Also, it is probably close to work with virtualization, but it needs some testing
        public bool BringItemToView(object item, Func<object, object> getParent)
        {
            // Useful link: https://msdn.microsoft.com/en-us/library/ff407130%28v=vs.110%29.aspx
            if (item == null) throw new ArgumentNullException(nameof(item));
            if (getParent == null) throw new ArgumentNullException(nameof(getParent));
            if (IsVirtualizing)
                throw new InvalidOperationException("BringItemToView cannot be used when the tree view is virtualizing.");

            TreeViewItem container = null;

            var path = new List<object> { item };
            var parent = getParent(item);
            while (parent != null)
            {
                path.Add(parent);
                parent = getParent(parent);
            }

            for (int i = path.Count - 1; i >= 0; --i)
            {
                if (container != null)
                    container = (TreeViewItem)container.ItemContainerGenerator.ContainerFromItem(path[i]);
                else
                    container = (TreeViewItem)ItemContainerGenerator.ContainerFromItem(path[i]);

                if (container == null)
                    return false;

                container.IsExpanded = true;
                container.ApplyTemplate();
                var itemsPresenter = (ItemsPresenter)container.Template.FindName("ItemsHost", container);
                if (itemsPresenter == null)
                {
                    // The Tree template has not named the ItemsPresenter, 
                    // so walk the descendents and find the child.
                    itemsPresenter = container.FindVisualChildOfType<ItemsPresenter>();
                    if (itemsPresenter == null)
                    {
                        container.UpdateLayout();
                        itemsPresenter = container.FindVisualChildOfType<ItemsPresenter>();
                    }
                }
                if (itemsPresenter == null)
                    return false;

                itemsPresenter.ApplyTemplate();
                var itemsHostPanel = (Panel)VisualTreeHelper.GetChild(itemsPresenter, 0);
                itemsHostPanel.UpdateLayout();
                itemsHostPanel.ApplyTemplate();

                // Ensure that the generator for this panel has been created.
                // ReSharper disable once UnusedVariable
                UIElementCollection children = itemsHostPanel.Children;
                container.BringIntoView();
            }
            return true;
        }

        internal virtual void SelectFromUiAutomation(TreeViewItem item)
        {
            SelectSingleItem(item);
            item.ForceFocus();
        }

        internal virtual void SelectPreviousFromKey()
        {
            List<TreeViewItem> items = TreeViewElementFinder.FindAll(this, true).ToList();
            TreeViewItem item = GetFocusedItem();
            item = GetPreviousItem(item, items);
            if (item == null) return;

            // if ctrl is pressed just focus it, so it can be selected by space. Otherwise select it.
            if (!IsControlKeyDown)
            {
                SelectSingleItem(item);
            }

            item.ForceFocus();
        }

        internal virtual void SelectNextFromKey()
        {
            TreeViewItem item = GetFocusedItem();
            item = TreeViewElementFinder.FindNext(item, true);
            if (item == null) return;

            // if ctrl is pressed just focus it, so it can be selected by space. Otherwise select it.
            if (!IsControlKeyDown)
            {
                SelectSingleItem(item);
            }

            item.ForceFocus();
        }

        internal virtual void SelectCurrentBySpace()
        {
            TreeViewItem item = GetFocusedItem();
            SelectSingleItem(item);
            item.ForceFocus();
        }

        internal virtual void SelectFromProperty(TreeViewItem item, bool isSelected)
        {
            // we do not check if selection is allowed, because selecting on that way is no user action.
            // Hopefully the programmer knows what he does...
            if (isSelected)
            {
                ModifySelection(new List<object>(1) { item.DataContext }, new List<object>());
                item.ForceFocus();
            }
            else
            {
                ModifySelection(new List<object>(), new List<object>(1) { item.DataContext });
            }
        }

        internal virtual void SelectFirst()
        {
            var item = TreeViewElementFinder.FindFirst(this, true);
            if (item != null)
            {
                SelectSingleItem(item);
                item.ForceFocus();
            }
        }

        internal virtual void SelectLast()
        {
            var item = TreeViewElementFinder.FindLast(this, true);
            if (item != null)
            {
                SelectSingleItem(item);
                item.ForceFocus();
            }
        }

        internal virtual void ClearObsoleteItems(IList items)
        {
            updatingSelection = true;
            foreach (var itemToUnSelect in items)
            {
                SelectedItems.Remove(itemToUnSelect);
                if (SelectedItem == itemToUnSelect)
                    SelectedItem = null;
            }
            updatingSelection = false;

            if (SelectionMode != SelectionMode.Single && items.Contains(lastShiftRoot))
                lastShiftRoot = null;
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            StopEditing();

            mouseDown = e.ChangedButton == MouseButton.Left;

            TreeViewItem item = GetTreeViewItemUnderMouse(e.GetPosition(this));
            if (item == null)
                return;
            if (e.ChangedButton != MouseButton.Right || item.ContextMenu == null)
                return;
            if (item.IsEditing)
                return;

            SelectSingleItem(item);

            item.ForceFocus();
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);
            if (mouseDown)
            {
                TreeViewItem item = GetTreeViewItemUnderMouse(e.GetPosition(this));
                if (item == null) return;
                if (e.ChangedButton != MouseButton.Left) return;
                if (item.IsEditing) return;

                SelectSingleItem(item);

                item.ForceFocus();
            }
            mouseDown = false;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // Ensure everything is unloaded before reloading!
            OnUnLoaded(sender, e);
            isInitialized = true;
        }

        private void OnUnLoaded(object sender, RoutedEventArgs e)
        {
            scroller = null;
        }

        internal void StartEditing(TreeViewItem item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            StopEditing();
            editedItem = item;
        }

        internal void StopEditing()
        {
            if (editedItem == null)
                return;

            Keyboard.Focus(editedItem);
            editedItem.ForceFocus();
            editedItem = null;
        }

        internal IEnumerable<TreeViewItem> GetChildren(TreeViewItem item)
        {
            if (item == null) yield break;
            for (int i = 0; i < item.Items.Count; i++)
            {
                var child = item.ItemContainerGenerator.ContainerFromIndex(i) as TreeViewItem;
                if (child != null) yield return child;
            }
        }

        internal TreeViewItem GetNextItem(TreeViewItem item, List<TreeViewItem> items)
        {
            int indexOfCurrent = items.IndexOf(item);

            for (int i = indexOfCurrent + 1; i < items.Count; i++)
            {
                if (items[i].IsVisible)
                {
                    return items[i];
                }
            }

            return null;
        }

        internal IEnumerable<TreeViewItem> GetNodesToSelectBetween(TreeViewItem firstNode, TreeViewItem lastNode)
        {
            var allNodes = TreeViewElementFinder.FindAll(this, false).ToList();
            var firstIndex = allNodes.IndexOf(firstNode);
            var lastIndex = allNodes.IndexOf(lastNode);

            if (firstIndex >= allNodes.Count)
            {
                throw new InvalidOperationException(
                   "First node index " + firstIndex + "greater or equal than count " + allNodes.Count + ".");
            }

            if (lastIndex >= allNodes.Count)
            {
                throw new InvalidOperationException(
                   "Last node index " + lastIndex + " greater or equal than count " + allNodes.Count + ".");
            }

            var nodesToSelect = new List<TreeViewItem>();

            if (lastIndex == firstIndex)
            {
                return new List<TreeViewItem> { firstNode };
            }

            if (lastIndex > firstIndex)
            {
                for (int i = firstIndex; i <= lastIndex; i++)
                {
                    if (allNodes[i].IsVisible)
                    {
                        nodesToSelect.Add(allNodes[i]);
                    }
                }
            }
            else
            {
                for (int i = firstIndex; i >= lastIndex; i--)
                {
                    if (allNodes[i].IsVisible)
                    {
                        nodesToSelect.Add(allNodes[i]);
                    }
                }
            }

            return nodesToSelect;
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);
            //Send down the IsVirtualizing property if it's set on this element.
            TreeViewItem.IsVirtualizingPropagationHelper(this, element);
        }

        internal TreeViewItem GetPreviousItem(TreeViewItem item, List<TreeViewItem> items)
        {
            int indexOfCurrent = items.IndexOf(item);
            for (int i = indexOfCurrent - 1; i >= 0; i--)
            {
                if (items[i].IsVisible)
                {
                    return items[i];
                }
            }

            return null;
        }

        public TreeViewItem GetTreeViewItemFor(object item)
        {
            foreach (var treeViewItem in TreeViewElementFinder.FindAll(this, false))
            {
                if (item == treeViewItem.DataContext)
                {
                    return treeViewItem;
                }
            }

            return null;
        }

        internal IEnumerable<TreeViewItem> GetTreeViewItemsFor(IEnumerable objects)
        {
            if (objects == null)
            {
                yield break;
            }
            var items = objects.Cast<object>().ToList();
            foreach (var treeViewItem in TreeViewElementFinder.FindAll(this, false))
            {
                if (items.Contains(treeViewItem.DataContext))
                {
                    yield return treeViewItem;
                }
            }

        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new TreeViewItem();
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is TreeViewItem;
        }

        private static void OnSelectedItemPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var treeView = (TreeView)d;
            if (!treeView.updatingSelection)
            {
                if (treeView.SelectedItems.Count == 1 && treeView.SelectedItems[0] == e.NewValue)
                    return;

                treeView.updatingSelection = true;
                if (treeView.SelectedItems.Count > 0)
                {
                    foreach (var oldItem in treeView.SelectedItems.Cast<object>().ToList())
                    {
                        var item = treeView.GetTreeViewItemFor(oldItem);
                        if (item != null)
                            item.IsSelected = false;
                    }
                    treeView.SelectedItems.Clear();
                }
                if (e.NewValue != null)
                {
                    var item = treeView.GetTreeViewItemFor(e.NewValue);
                    if (item != null)
                        item.IsSelected = true;
                    treeView.SelectedItems.Add(e.NewValue);
                }
                treeView.updatingSelection = false;
            }
        }

        private static void OnSelectedItemsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TreeView treeView = (TreeView)d;
            if (e.OldValue != null)
            {
                INotifyCollectionChanged collection = e.OldValue as INotifyCollectionChanged;
                if (collection != null)
                {
                    collection.CollectionChanged -= treeView.OnSelectedItemsChanged;
                }
            }

            if (e.NewValue != null)
            {
                INotifyCollectionChanged collection = e.NewValue as INotifyCollectionChanged;
                if (collection != null)
                {
                    collection.CollectionChanged += treeView.OnSelectedItemsChanged;
                }
            }
        }

        private static void OnSelectionModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var treeView = (TreeView)d;
            var newValue = (SelectionMode)e.NewValue;
            if (newValue == SelectionMode.Multiple)
                throw new NotSupportedException("SelectionMode.Multiple is not yet supported. Please use SelectionMode.Single or SelectionMode.Multiple.Extended.");

            if (newValue != SelectionMode.Single)
            {
                var selectedItem = treeView.SelectedItem;
                treeView.updatingSelection = true;
                for (int i = treeView.SelectedItems.Count - 1; i >= 0; --i)
                {
                    if (treeView.SelectedItems[i] != selectedItem)
                    {
                        var item = treeView.GetTreeViewItemFor(treeView.SelectedItems[i]);
                        if (item != null)
                            item.IsSelected = false;
                        treeView.SelectedItems.RemoveAt(i);
                    }
                }
                treeView.updatingSelection = false;
            }
        }

        private void OnSelectedItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (updatingSelection)
                return;

            if (SelectionMode == SelectionMode.Single && !allowedSelectionChanges)
                throw new InvalidOperationException("Can only change SelectedItems collection in multiple selection modes. Use SelectedItem in single select modes.");

            updatingSelection = true;

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    object last = null;
                    foreach (var item in GetTreeViewItemsFor(e.NewItems))
                    {
                        item.IsSelected = true;

                        last = item.DataContext;
                    }

                    SelectedItem = last;
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (var item in GetTreeViewItemsFor(e.OldItems))
                    {
                        item.IsSelected = false;
                        if (item.DataContext == SelectedItem)
                        {
                            SelectedItem = SelectedItems.Count > 0 ? SelectedItems[SelectedItems.Count - 1] : null;
                        }
                    }

                    break;
                case NotifyCollectionChangedAction.Reset:
                    foreach (var item in TreeViewElementFinder.FindAll(this, false))
                    {
                        if (item.IsSelected)
                        {
                            item.IsSelected = false;
                        }
                    }

                    SelectedItem = null;
                    break;
                default:
                    throw new InvalidOperationException();
            }
            updatingSelection = false;
        }

        /// <summary>
        ///     This method is invoked when the Items property changes.
        /// </summary>
        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Remove:
                    ClearObsoleteItems(e.OldItems);
                    break;

                case NotifyCollectionChangedAction.Reset:
                case NotifyCollectionChangedAction.Replace:
                case NotifyCollectionChangedAction.Add:
                case NotifyCollectionChangedAction.Move:
                    break;

                default:
                    throw new NotSupportedException();
            }
        }

        protected void SelectSingleItem(TreeViewItem item)
        {
            // selection with SHIFT is not working in virtualized mode. Thats because the Items are not visible.
            // Therefore the children cannot be found/selected.
            if (SelectionMode != SelectionMode.Single && IsShiftKeyDown && SelectedItems.Count > 0 && !IsVirtualizing)
            {
                SelectWithShift(item);
                return;
            }

            if (IsControlKeyDown)
            {
                ToggleItem(item);
            }
            else
            {
                ModifySelection(new List<object>(1) { item.DataContext }, new List<object>((IEnumerable<object>)SelectedItems));
            }
        }

        protected TreeViewItem GetFocusedItem()
        {
            return TreeViewElementFinder.FindAll(this, false).FirstOrDefault(x => x.IsFocused);
        }

        protected TreeViewItem GetTreeViewItemUnderMouse(Point positionRelativeToTree)
        {
            HitTestResult hitTestResult = VisualTreeHelper.HitTest(this, positionRelativeToTree);
            if (hitTestResult?.VisualHit == null)
                return null;

            var child = hitTestResult.VisualHit as FrameworkElement;

            do
            {
                if (child == null)
                    return null;

                var treeViewItem = child as TreeViewItem;
                if (treeViewItem != null)
                {
                    return treeViewItem.IsVisible ? treeViewItem : null;
                }

                if (child is TreeView)
                    return null;

                child = VisualTreeHelper.GetParent(child) as FrameworkElement;
            } while (child != null);

            return null;
        }

        private void ToggleItem(TreeViewItem item)
        {
            if (item.DataContext == null)
                return;

            var itemsToUnselect = SelectionMode == SelectionMode.Single ? new List<object>(SelectedItems.Cast<object>()) : new List<object>();
            if (SelectedItems.Contains(item.DataContext))
            {
                itemsToUnselect.Add(item.DataContext);
                ModifySelection(new List<object>(), itemsToUnselect);
            }
            else
            {
                ModifySelection(new List<object>(1) { item.DataContext }, itemsToUnselect);
            }
        }

        private void ModifySelection(List<object> itemsToSelect, List<object> itemsToUnselect)
        {
            //clean up any duplicate or unnecessery input
            // check for itemsToUnselect also in itemsToSelect
            foreach (var item in itemsToSelect)
            {
                itemsToUnselect.Remove(item);
            }

            // check for itemsToSelect already in SelectedItems
            foreach (var item in SelectedItems)
            {
                itemsToSelect.Remove(item);
            }

            // check for itemsToUnSelect not in SelectedItems
            foreach (var item in itemsToUnselect.Where(x => !SelectedItems.Contains(x)).ToList())
            {
                itemsToUnselect.Remove(item);
            }

            //check if there's anything to do.
            if (itemsToSelect.Count == 0 && itemsToUnselect.Count == 0)
                return;

            allowedSelectionChanges = true;
            // Unselect and then select items
            foreach (var itemToUnSelect in itemsToUnselect)
            {
                SelectedItems.Remove(itemToUnSelect);
            }

            ((NonGenericObservableListWrapper<object>)SelectedItems).AddRange(itemsToSelect);
            allowedSelectionChanges = false;

            if (itemsToUnselect.Contains(lastShiftRoot))
                lastShiftRoot = null;

            if (!(SelectedItems.Contains(lastShiftRoot) && IsShiftKeyDown))
                lastShiftRoot = itemsToSelect.LastOrDefault();
        }


        private void SelectWithShift(TreeViewItem item)
        {
            object firstSelectedItem;
            if (lastShiftRoot != null)
            {
                firstSelectedItem = lastShiftRoot;
            }
            else
            {
                firstSelectedItem = SelectedItems.Count > 0 ? SelectedItems[0] : null;
            }

            var shiftRootItem = GetTreeViewItemsFor(new List<object> { firstSelectedItem }).First();

            var itemsToSelect = GetNodesToSelectBetween(shiftRootItem, item).Select(x => x.DataContext).ToList();
            var itemsToUnSelect = ((IEnumerable<object>)SelectedItems).ToList();

            ModifySelection(itemsToSelect, itemsToUnSelect);
        }
    }
}