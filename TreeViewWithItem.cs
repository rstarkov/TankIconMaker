using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace TankIconMaker
{
    /// <summary>
    /// Same as a TreeView, except that items which implement the <see cref="IHasTreeViewItem"/> interface will
    /// automatically receive the corresponding TreeViewItem container. This saves a lot of effort building simple
    /// trees, compared to the intended approach of having the ViewModel re-implement TreeViewItem features.
    /// (http://stackoverflow.com/questions/616948/)
    /// </summary>
    public class TreeViewWithItem : TreeView
    {
        public TreeViewWithItem()
        {
            ItemContainerGenerator.StatusChanged += ItemContainerGenerator_StatusChanged;
        }

        private void ItemContainerGenerator_StatusChanged(object sender, EventArgs e)
        {
            var generator = sender as ItemContainerGenerator;
            if (generator.Status == GeneratorStatus.ContainersGenerated)
            {
                int i = 0;
                while (true)
                {
                    var container = generator.ContainerFromIndex(i);
                    if (container == null)
                        break;

                    var tvi = container as TreeViewItem;
                    if (tvi != null)
                        tvi.ItemContainerGenerator.StatusChanged += ItemContainerGenerator_StatusChanged;

                    var item = generator.ItemFromContainer(container) as IHasTreeViewItem;
                    if (item != null)
                        item.TreeViewItem = tvi;

                    i++;
                }
            }
        }
    }

    interface IHasTreeViewItem
    {
        TreeViewItem TreeViewItem { get; set; }
    }
}
