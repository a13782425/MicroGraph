﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MicroGraph.Editor
{
    /// <summary>
    /// 预览视图的配置
    /// </summary>
    [Serializable]
    internal sealed class OverviewGraphConfig
    {
        [SerializeField]
        public bool ShowGrid = false;
        [SerializeField]
        public bool CanZoom = false;
        [SerializeField]
        public float Scale = 1f;
        [SerializeField]
        public Vector2 Pos = Vector2.zero;
        [SerializeField]
        public List<OverviewGroupInfo> GroupInfos = new List<OverviewGroupInfo>();
        [SerializeField]
        public List<OverviewFavoriteGroupInfo> FavoriteGroupInfos = new List<OverviewFavoriteGroupInfo>();
        internal void Initialize()
        {
            for (int i = GroupInfos.Count - 1; i >= 0; i--)
            {
                var groupInfo = GroupInfos[i];
                if (MicroGraphProvider.GraphCategoryList.FirstOrDefault(a => a.GraphType.FullName == groupInfo.GroupKey) == null)
                    GroupInfos.RemoveAt(i);
            }
        }
    }

    [Serializable]
    internal sealed class OverviewGroupInfo
    {
        [SerializeField]
        public string GroupKey;
        [SerializeField]
        public int ColumnCount = 0;
        [SerializeField]
        public Vector2 Pos = Vector2.zero;
    }
    [Serializable]
    internal sealed class OverviewFavoriteGroupInfo
    {
        [SerializeField]
        public string FavoriteName;
        [SerializeField]
        public int ColumnCount = 0;
        [SerializeField]
        public Vector2 Pos = Vector2.zero;
        [SerializeField]
        public Color Color = Color.black;
        [SerializeField]
        public List<string> Graphs = new List<string>();
    }
}
