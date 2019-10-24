using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Interactivity;

namespace LSMEmprunts
{

    /// <summary>
    /// This behavior add a "Tagged" boolean attached property to all the Items of the associated ItemsControl. Default value of this property is false
    /// When the TaggedItems property is set, the "Tagged" AP of each item is set to true for items whose DataContext is in the TaggedItems collection.
    /// </summary>
    /// <remarks>
    /// If The TaggedItems property is an observable collection, then the "Tagged" AP is kepy in sync as its content changes.
    /// It is intended that the "Tagged" attached property is bound to a visual element in the Item visual representation.
    /// </remarks>
    public class ItemsControlTagBehavior : Behavior<ItemsControl>
    {
        private static class Boxes
        {
            internal static object FalseBox = false;
            internal static object TrueBox = true;
        }

        #region Tagged Attached property
        public static readonly DependencyProperty TaggedProperty = DependencyProperty.RegisterAttached("Tagged", typeof(bool), typeof(ItemsControlTagBehavior),
            new FrameworkPropertyMetadata(Boxes.FalseBox, FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static bool GetTagged(FrameworkElement target)
        {
            return (bool)target.GetValue(TaggedProperty);
        }

        public static void SetTagged(FrameworkElement target, bool value)
        {
            target.SetValue(TaggedProperty, value ? Boxes.TrueBox : Boxes.FalseBox);
        }
        #endregion

        #region TaggedItems property
        public static readonly DependencyProperty TaggedItemsProperty = DependencyProperty.Register(nameof(TaggedItems), typeof(IEnumerable), typeof(ItemsControlTagBehavior),
            new PropertyMetadata(null, OnTaggedItemsPropertyChanged));

        private static void OnTaggedItemsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var me = (ItemsControlTagBehavior)d;
            if (e.OldValue is INotifyCollectionChanged olcColl)
            {
                olcColl.CollectionChanged -= me.OnTaggedItemsChanged;
            }
            if (e.NewValue is INotifyCollectionChanged newColl)
            {
                newColl.CollectionChanged += me.OnTaggedItemsChanged;
            }
            me.UpdateViewFromModel();
        }

        public IEnumerable TaggedItems
        {
            get => (IEnumerable)GetValue(TaggedItemsProperty);
            set => SetValue(TaggedItemsProperty, value);
        }
        #endregion

        protected override void OnAttached()
        {
            base.OnAttached();
            if (TaggedItems is INotifyCollectionChanged coll)
            {
                coll.CollectionChanged += OnTaggedItemsChanged;
            }

            (AssociatedObject.Items as INotifyCollectionChanged).CollectionChanged += OnControlItemsChanged;

            AssociatedObject.ItemContainerGenerator.StatusChanged += OnItemsContainerGeneratorStatusChanged;

            UpdateViewFromModel();
        }

        protected override void OnDetaching()
        {
            (AssociatedObject.Items as INotifyCollectionChanged).CollectionChanged -= OnControlItemsChanged;
            AssociatedObject.ItemContainerGenerator.StatusChanged -= OnItemsContainerGeneratorStatusChanged;

            if (TaggedItems is INotifyCollectionChanged coll)
            {
                coll.CollectionChanged -= OnTaggedItemsChanged;
            }
            base.OnDetaching();
        }

        private void OnTaggedItemsChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Add:
                case NotifyCollectionChangedAction.Replace:
                    foreach (var newSourceItem in args.NewItems)
                    {
                        var container = AssociatedObject.ItemContainerGenerator.ContainerFromItem(newSourceItem) as FrameworkElement;
                        if (container != null)
                        {
                            SetTagged(container, true);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (var removedItem in args.OldItems)
                    {
                        var container = AssociatedObject.ItemContainerGenerator.ContainerFromItem(removedItem) as FrameworkElement;
                        if (container != null)
                        {
                            SetTagged(container, false);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    UpdateViewFromModel();
                    break;
            }
        }

        private void OnItemsContainerGeneratorStatusChanged(object sender, EventArgs e)
        {
            if (AssociatedObject.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
            {
                UpdateViewFromModel();
            }
        }

        private void OnControlItemsChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            if (TaggedItems == null)
            {
                return;
            }

            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Add:
                case NotifyCollectionChangedAction.Replace:
                    foreach (FrameworkElement newContainer in args.NewItems)
                    {
                        SetTagged(newContainer, TaggedItems.Cast<object>().Any(e => e == newContainer.DataContext));
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (FrameworkElement oldContainer in args.OldItems)
                    {
                        SetTagged(oldContainer, false);
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    UpdateViewFromModel();
                    break;
            }
        }

        private void UpdateViewFromModel()
        {
            if (TaggedItems != null && AssociatedObject?.Items != null)
            {
                //update Tagged property for all items of the ItemsControl
                for (var i = 0; i < AssociatedObject.Items.Count; ++i)
                {
                    var container = AssociatedObject.ItemContainerGenerator.ContainerFromIndex(i) as FrameworkElement;
                    if (container != null)
                    {
                        SetTagged(container, TaggedItems.Cast<object>().Any(e => e == container.DataContext));
                    }
                }
            }
        }
    }
}
