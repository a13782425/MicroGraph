﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MicroGraph.Editor
{
    public struct GridContainer<T>
    {
        public Vector2Int min;
        public Vector2Int size;
        public Vector2Int Max => min + size;
        public List<(Vector2Int position, Vector2Int size, T element)> elements;

        public GridContainer(List<(Vector2Int, Vector2Int, T)> elements)
        {
            this.elements = elements;
            var minPos = this.elements.Min(p => p.position);
            min = minPos;
            size = this.elements.Max(p => p.position - minPos + p.size);
        }

        public void AddElement(Vector2Int position, Vector2Int size, T element)
        {
            elements.Add((position, size, element));
            var minPos = min;
            for (var i = 0; i < elements.Count; i++) minPos = Vector2Int.Min(minPos, elements[i].position);
            min = minPos;
            var maxSize = Vector2Int.zero;
            for (var i = 0; i < elements.Count; i++)
                maxSize = Vector2Int.Max(this.size, elements[i].position - min + elements[i].size);
            this.size = maxSize;
        }

        public IEnumerable<(Vector2 position, Vector2 size, T element)> GetNormalized()
        {
            var ordered = elements.OrderBy(el => el.position, new Vector2IntComparer());
            var min = this.min;
            var size = this.size;
            return ordered.Select(el => (((Vector2)el.position - min) / size, (Vector2)el.size / size, el.element));
        }
        private class Vector2IntComparer : IComparer<Vector2Int>
        {
            public int Compare(Vector2Int a, Vector2Int b)
            {
                var xComparison = a.x.CompareTo(b.x);
                if (xComparison != 0) return xComparison;
                var yComparison = a.y.CompareTo(b.y);
                return yComparison;
            }
        }
    }
}
