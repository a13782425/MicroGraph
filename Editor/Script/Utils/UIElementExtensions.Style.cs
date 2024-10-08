﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

namespace MicroGraph.Editor
{
    partial class UIElementExtensions
    {
        public static T Stylize<T>(this T ve, Action<IStyle> process) where T : VisualElement
        {
            process(ve.style);
            return ve;
        }

        public static T DeferredStylize<T>(this T ve, Action<IStyle> process, string query = "") where T : VisualElement
        {
            ve.schedule.Execute(() =>
            {
                if (!string.IsNullOrWhiteSpace(query))
                    process(ve.Q(query).style);
                else
                    process(ve.style);
            });
            return ve;
        }

        public static IEnumerable<T> StylizeGroup<T>(this IEnumerable<T> elements, Action<IStyle, int> process)
            where T : VisualElement
        {
            var arr = elements.ToArray();
            for (var i = 0; i < arr.Length; i++) process(arr[i].style, i);

            return elements;
        }
        public static T FirstOrSpecified<T>(this IEnumerable<T> elements, T @default, Func<T, bool> query)
        {
            var array = elements as T[] ?? elements.ToArray();
            return array.Any(query) ? array.First(query) : @default;
        }
	}
}
