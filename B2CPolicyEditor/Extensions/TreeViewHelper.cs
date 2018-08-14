using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace B2CPolicyEditor.Extensions
{
    public class TreeViewHelper
    {
        // Declare our attached property, it needs to be a DependencyProperty so
        // we can bind to it from oout ViewMode.
        public static readonly DependencyProperty TreeViewSelectedItemProperty =
            DependencyProperty.RegisterAttached(
            "TreeViewSelectedItem",
            typeof(object),
            typeof(TreeViewHelper),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                new PropertyChangedCallback(TreeViewSelectedItemChanged)));

        // We need a Get method for our new property
        public static object GetTreeViewSelectedItem(DependencyObject dependencyObject)
        {
            return (object)dependencyObject.GetValue(TreeViewSelectedItemProperty);
        }

        // As well as a Set method for our new property
        public static void SetTreeViewSelectedItem(
          DependencyObject dependencyObject, object value)
        {
            dependencyObject.SetValue(TreeViewSelectedItemProperty, value);
        }

        // This is the handler for when our new property's value changes
        // When our property is set to a non null value we need to add an event handler
        // for the TreeView's SelectedItemChanged event
        private static void TreeViewSelectedItemChanged(DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs e)
        {
            TreeView tv = dependencyObject as TreeView;

            if (e.NewValue == null && e.OldValue != null)
            {
                tv.SelectedItemChanged -=
                    new RoutedPropertyChangedEventHandler<object>(tv_SelectedItemChanged);
            }
            else if (e.NewValue != null && e.OldValue == null)
            {
                tv.SelectedItemChanged +=
                    new RoutedPropertyChangedEventHandler<object>(tv_SelectedItemChanged);
            }
        }

        // When TreeView.SelectedItemChanged fires, set our new property to the value
        static void tv_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            SetTreeViewSelectedItem((DependencyObject)sender, e.NewValue);
        }
    }
}
//        private static Dictionary<DependencyObject, TreeViewSelectedItemBehavior> behaviors = new Dictionary<DependencyObject, TreeViewSelectedItemBehavior>();

//        public static object GetSelectedItem(DependencyObject obj)
//        {
//            return (object)obj.GetValue(SelectedItemProperty);
//        }

//        public static void SetSelectedItem(DependencyObject obj, object value)
//        {
//            obj.SetValue(SelectedItemProperty, value);
//        }

//        // Using a DependencyProperty as the backing store for SelectedItem.  This enables animation, styling, binding, etc...
//        public static readonly DependencyProperty SelectedItemProperty =
//            DependencyProperty.RegisterAttached("SelectedItem", typeof(object), typeof(TreeViewHelper), new UIPropertyMetadata(null, SelectedItemChanged));

//        private static void SelectedItemChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
//        {
//            if (!(obj is TreeView))
//                return;

//            if (!behaviors.ContainsKey(obj))
//                behaviors.Add(obj, new TreeViewSelectedItemBehavior(obj as TreeView));

//            TreeViewSelectedItemBehavior view = behaviors[obj];
//            view.ChangeSelectedItem(e.NewValue);
//        }

//        private class TreeViewSelectedItemBehavior
//        {
//            TreeView view;
//            public TreeViewSelectedItemBehavior(TreeView view)
//            {
//                this.view = view;
//                view.SelectedItemChanged += (sender, e) => SetSelectedItem(view, e.NewValue);
//            }

//            internal void ChangeSelectedItem(object p)
//            {
//                TreeViewItem item = (TreeViewItem)view.ItemContainerGenerator.ContainerFromItem(p);
//                item.IsSelected = true;
//            }
//        }
//    }
//}
