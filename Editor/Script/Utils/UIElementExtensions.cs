using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

namespace MicroGraph.Editor
{
    [Flags]
    internal enum CustomPseudoStates
    {
        Active = 1,
        Hover = 2,
        Checked = 8,
        Disabled = 0x20,
        Focus = 0x40,
        Root = 0x80
    }
    public static partial class UIElementExtensions
    {
        private static PropertyInfo s_pseudoStateProp;
        private static Type s_pseudoStateType;

        private static MethodInfo s_setPropertyMethod;
        private static MethodInfo s_getPropertyMethod;
        private static MethodInfo s_hasPropertyMethod;

        static UIElementExtensions()
        {
            s_pseudoStateProp = typeof(VisualElement).GetProperty("pseudoStates", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            s_getPropertyMethod = typeof(VisualElement).GetMethod("GetProperty", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            s_setPropertyMethod = typeof(VisualElement).GetMethod("SetProperty", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            s_hasPropertyMethod = typeof(VisualElement).GetMethod("HasProperty", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            s_pseudoStateType = s_pseudoStateProp.PropertyType;
        }

        /// <summary>
        /// 设置字段组件的默认样式
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="element"></param>
        public static void SetBaseFieldStyle<T>(this BaseField<T> element)
        {
            if (element == null)
                return;
            element.style.minHeight = 24;
            element.style.marginTop = 2;
            element.style.marginRight = 2;
            element.style.marginLeft = 2;
            element.style.marginBottom = 2;
            element.style.unityTextAlign = TextAnchor.MiddleLeft;
            element.labelElement.style.minWidth = 50;
            element.labelElement.style.fontSize = 12;
        }
        /// <summary>
        /// 设置是否显示
        /// </summary>
        /// <param name="element"></param>
        /// <param name="display"></param>
        public static void SetDisplay(this VisualElement element, bool display)
        {
            if (element == null)
                return;
            element.style.display = display ? DisplayStyle.Flex : DisplayStyle.None;
        }

        internal static void AddStyleSheet(this VisualElement element, string stylePath)
        {
            if (element == null)
                return;
            element.styleSheets.Add(MicroGraphUtils.LoadRes<StyleSheet>(stylePath));
        }

        internal static void RemoveStyleSheet(this VisualElement element, string stylePath)
        {
            if (element == null)
                return;
            element.styleSheets.Remove(MicroGraphUtils.LoadRes<StyleSheet>(stylePath));
        }

        internal static BaseMicroNodeView.InternalNodeView GetFirstAncestorNodeView(this INodeFieldElement element)
        {
            if (element is VisualElement visual)
                return visual.GetFirstAncestorOfType<BaseMicroNodeView.InternalNodeView>();
            return null;
        }

        /// <summary>
        /// 获取伪状态
        /// </summary>
        /// <param name="ve"></param>
        /// <returns></returns>
        internal static CustomPseudoStates GetPseudoStates(this VisualElement ve)
        {
            object value = s_pseudoStateProp.GetValue(ve);
            int result = (int)Convert.ChangeType(value, typeof(int));
            return (CustomPseudoStates)result;
        }

        /// <summary>
        /// 设置伪状态
        /// </summary>
        /// <param name="ve"></param>
        /// <param name="pseudoState"></param>
        internal static void SetPseudoStates(this VisualElement ve, int pseudoState)
        {
            object result = Enum.ToObject(s_pseudoStateType, pseudoState);
            s_pseudoStateProp.SetValue(ve, result);
        }
        /// <summary>
        /// 设置伪状态
        /// </summary>
        /// <param name="ve"></param>
        /// <param name="pseudoState"></param>
        internal static void SetPseudoStates(this VisualElement ve, CustomPseudoStates pseudoState)
        {
            object result = Enum.ToObject(s_pseudoStateType, (int)pseudoState);
            s_pseudoStateProp.SetValue(ve, result);
        }


        internal static object GetPropertyEx(this VisualElement ve, PropertyName key)
        {
            return s_getPropertyMethod.Invoke(ve, new object[] { key });
        }
        internal static void SetPropertyEx(this VisualElement ve, PropertyName key, object value)
        {
            s_setPropertyMethod.Invoke(ve, new object[] { key, value });
        }
        internal static bool HasPropertyEx(this VisualElement ve, PropertyName key)
        {
            return (bool)s_hasPropertyMethod.Invoke(ve, new object[] { key });
        }
    }
}
