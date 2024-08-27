using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace MicroGraph.Editor
{
    /// <summary>
    /// 创建图搜索窗口
    /// </summary>
    internal sealed class OverviewCreateGraphWindow : ScriptableObject, ISearchWindowProvider
    {
        public event Func<SearchTreeEntry, SearchWindowContext, bool> onSelectHandler;
        private List<SearchTreeEntry> _entries = new List<SearchTreeEntry>();
        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            _entries.Clear();
            _entries.Add(new SearchTreeGroupEntry(new GUIContent("创建逻辑图")));
            foreach (GraphCategoryModel item in MicroGraphProvider.GraphCategoryList)
            {
                _entries.Add(new SearchTreeEntry(new GUIContent(item.GraphName)) { level = 1, userData = item });
            }
            return _entries;
        }
        
        public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
        {
            bool? res = onSelectHandler?.Invoke(searchTreeEntry, context);
            return res.HasValue ? res.Value : false;
        }
    }
}
