using MicroGraph.Runtime;
using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace MicroGraph.Editor
{
    /// <summary>
    /// 创建节点窗口
    /// </summary>
    internal class MicroCreateNodeWindow : ScriptableObject, ISearchWindowProvider
    {
        public event Func<SearchTreeEntry, SearchWindowContext, bool> onSelectHandler;
        private List<SearchTreeEntry> _searchTrees = new List<SearchTreeEntry>();
        private GraphCategoryModel _categoryInfo;
        private BaseMicroGraphView _baseMicroGraphView;
        private List<string> _groups = new List<string>();
        private List<NodeCategoryModel> nodeCategories = new List<NodeCategoryModel>();

        private List<MicroGroupEditorInfo> groupEditors = new List<MicroGroupEditorInfo>();

        private bool _isUnique = false;
        public void Initialize(BaseMicroGraphView baseMicroGraphView, GraphCategoryModel categoryInfo, bool isUniqueCreate = false)
        {
            _isUnique = isUniqueCreate;
            _baseMicroGraphView = baseMicroGraphView;
            _categoryInfo = categoryInfo;
            if (isUniqueCreate)
                nodeCategories.AddRange(_categoryInfo.UniqueNodeCategories);
            else
            {
                nodeCategories.AddRange(_categoryInfo.NodeCategories);
                foreach (var item in _categoryInfo.UniqueNodeCategories)
                {
                    nodeCategories.Remove(item);
                }
            }
        }
        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            _searchTrees.Clear();
            _searchTrees.Add(new SearchTreeGroupEntry(new GUIContent("创建节点")));
            if (!_isUnique)
                AddPackageTree();
            AddNodeTree();
            return _searchTrees;
        }

        private void AddPackageTree()
        {
            groupEditors.Clear();
            foreach (var item in _baseMicroGraphView.editorInfo.Groups)
            {
                if (!item.IsPackage)
                    continue;
                groupEditors.Add(item);
            }
            if (groupEditors.Count == 0)
                return;
            _searchTrees.Add(new SearchTreeGroupEntry(new GUIContent("节点包"), 1));
            foreach (var item in groupEditors)
            {
                _searchTrees.Add(new SearchTreeEntry(new GUIContent(item.Title)) { level = 2, userData = item });
            }
        }

        public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
        {
            bool? res = onSelectHandler?.Invoke(searchTreeEntry, context);
            return res.HasValue ? res.Value : false;
        }
        /// <summary>
        /// 添加节点树
        /// </summary>
        /// <param name="searchTrees"></param>
        private void AddNodeTree()
        {
            _groups.Clear();
            foreach (NodeCategoryModel nodeConfig in nodeCategories)
            {
                if (nodeConfig.EnableState != MicroNodeEnableState.Enabled)
                    continue;
                int createIndex = int.MaxValue;

                for (int i = 0; i < nodeConfig.NodeLayers.Length - 1; i++)
                {
                    string group = nodeConfig.NodeLayers[i];
                    if (i >= _groups.Count)
                    {
                        createIndex = i;
                        break;
                    }
                    if (_groups[i] != group)
                    {
                        _groups.RemoveRange(i, _groups.Count - i);
                        createIndex = i;
                        break;
                    }
                }
                for (int i = createIndex; i < nodeConfig.NodeLayers.Length - 1; i++)
                {
                    string group = nodeConfig.NodeLayers[i];
                    _groups.Add(group);
                    _searchTrees.Add(new SearchTreeGroupEntry(new GUIContent(group)) { level = i + 1 });
                }

                _searchTrees.Add(new SearchTreeEntry(new GUIContent(nodeConfig.NodeName)) { level = nodeConfig.NodeLayers.Length, userData = nodeConfig });
            }
        }
    }
}
