using MicroGraph.Runtime;
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MicroGraph.Editor
{
    //// 确保自定义的绘制逻辑总是位于最前面
    //[InitializeOnLoad]
    //public class CustomAssetIcon
    //{
    //    private static Texture2D BackgroundTex;
    //    static CustomAssetIcon()
    //    {
    //        EditorApplication.delayCall -= m_delayCall;
    //        EditorApplication.delayCall += m_delayCall;
    //    }

    //    private static void m_delayCall()
    //    {
    //        BackgroundTex = MicroGraphUtils.LoadRes<Texture2D>("Texture/flow_chart");
    //        // 订阅Project窗口的hierarchyWindowItemOnGUI事件
    //        EditorApplication.projectWindowItemOnGUI -= ProjectWindowItemOnGUI;
    //        EditorApplication.projectWindowItemOnGUI += ProjectWindowItemOnGUI;
    //        EditorApplication.delayCall -= m_delayCall;
    //    }

    //    static void ProjectWindowItemOnGUI(string guid, Rect rect)
    //    {
    //        // 获取当前行对应的Asset对象
    //        string assetPath = AssetDatabase.GUIDToAssetPath(guid);
    //        Object asset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);

    //        if (asset is BaseMicroGraph) // 替换'MyCustomType'为你想要更换图标的类名称
    //        {
    //            //计算图标应该放置的位置 Rect
    //            Rect imageRect;
    //            if (rect.height > 20)
    //            {
    //                imageRect = new Rect(rect.x - 1, rect.y - 1, rect.width + 2, rect.width + 2);
    //            }
    //            else if (rect.x > 20)
    //            {
    //                imageRect = new Rect(rect.x - 1, rect.y - 1, rect.height + 2, rect.height + 2);
    //            }
    //            else
    //            {
    //                imageRect = new Rect(rect.x + 2, rect.y - 1, rect.height + 2, rect.height + 2);
    //            }
    //            bool isSelect = IsAssetSelected(guid);
    //            Color backgroundColor;
    //            if (isSelect)
    //            {
    //                backgroundColor = EditorPalette.GetSelectionColor(isSelect);
    //            }
    //            else
    //            {
    //                backgroundColor = EditorPalette.BackgroundColor;
    //            }
    //            EditorGUI.DrawRect(imageRect, backgroundColor);
    //            GUI.DrawTexture(imageRect, BackgroundTex, ScaleMode.StretchToFill);
    //            EditorApplication.RepaintProjectWindow();
    //            //EditorGUI.DrawRect(imageRect, EditorPalette.BackgroundColor);
    //            // 加载我们自定义图标
    //            //Texture2D customIcon = MicroGraphUtils.LoadRes<Texture2D>("Texture/flow_chart"); // 替换"MyCustomIcon.png"为路径和图标名称                
    //            //GUI.DrawTexture(imageRect, customIcon, ScaleMode.StretchToFill);
    //        }
    //    }
    //    private static bool IsAssetSelected(string assetGuid)
    //    {
    //        // Selection.assetGUIDs 返回包含所有当前选择资产的GUID数组
    //        return Selection.assetGUIDs.Contains(assetGuid);
    //    }
    //}
    [CustomEditor(typeof(BaseMicroGraph), true)]
    internal sealed class BaseMicroGraph_Inspector : UnityEditor.Editor
    {
        private BaseMicroGraph _logic;

        private bool _isEditor = false;

        /// <summary>
        /// 是否显示详情
        /// </summary>
        private bool _isShowDetail = false;
        void OnEnable()
        {
            _logic = target as BaseMicroGraph;
        }
        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("打开"))
            {
                MicroGraphWindow.ShowMicroGraph(_logic.OnlyId);
            }
            if (GUILayout.Button(_isShowDetail ? "关闭详情" : "显示详情,但可能导致崩溃"))
            {
                _isShowDetail = !_isShowDetail;
            }

            //if (_isShowDetail)
            {
                using (new EditorGUILayout.HorizontalScope("box"))
                {
                    _isEditor = EditorGUILayout.Toggle("编辑：", _isEditor);
                }

                UnityEditor.EditorGUI.BeginDisabledGroup(!_isEditor);
                base.OnInspectorGUI();
                UnityEditor.EditorGUI.EndDisabledGroup();
            }
        }
    }
}
