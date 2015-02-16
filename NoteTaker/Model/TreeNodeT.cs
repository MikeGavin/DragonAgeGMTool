using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scrivener.Model
{
    /// <summary>
    /// Source: http://stackoverflow.com/a/11879200
    /// </summary>
    /// <typeparam name="T"></typeparam>
    class TreeNode<T>  
    {
        ObservableCollection<TreeNode<T>> Children;

        T Item { get; set; }

        public TreeNode(T item)
        {
            Item = item;
        }

        public TreeNode<T> AddChild(T item)
        {
            TreeNode<T> nodeItem = new TreeNode<T>(item);
            Children.Add(nodeItem);
            return nodeItem;
        }
    }
}
