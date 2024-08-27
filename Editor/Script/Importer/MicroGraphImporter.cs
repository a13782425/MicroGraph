using MicroGraph.Runtime;
using System.Drawing;
using System.IO;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine;

namespace MicroGraph.Editor
{
    /// <summary>
    /// 每次资源变化调用
    /// </summary>
    internal sealed class MicroGraphImporter : AssetPostprocessor
    {
        /// <summary>
        /// 所有的资源的导入，删除，移动，都会调用此方法，注意，这个方法是static的
        /// </summary>
        /// <param name="importedAsset">导入或者发生改变的资源</param>
        /// <param name="deletedAssets">删除的资源</param>
        /// <param name="movedAssets">移动后资源路径</param>
        /// <param name="movedFromAssetPaths">移动前资源路径</param>
        public static void OnPostprocessAllAssets(string[] importedAsset, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            bool hasAsset = false;
            GraphAssetsChangedEventArgs refreshViewEvent = new GraphAssetsChangedEventArgs();
            //foreach (var str in movedFromAssetPaths)
            //{
            //    //移动前资源路径
            //}
            foreach (var str in movedAssets)
            {
                //移动后资源路径
                if (Path.GetExtension(str) == ".asset")
                {
                    var logicGraph = AssetDatabase.LoadAssetAtPath<BaseMicroGraph>(str);
                    if (logicGraph != null)
                    {
                        //bool res = MicroGraphProvider.MoveSummaryModel(str, logicGraph);
                        //if (res)
                        //{
                        refreshViewEvent.moveGraphs.Add(str);
                        //}
                        hasAsset = true;
                    }
                }
            }
            foreach (string str in importedAsset)
            {
                //导入或者发生改变的资源
                if (Path.GetExtension(str) == ".asset")
                {
                    var logicGraph = AssetDatabase.LoadAssetAtPath<BaseMicroGraph>(str);
                    if (logicGraph != null)
                    {
                        GraphSummaryModel summary = MicroGraphProvider.GetGraphSummary(logicGraph.OnlyId);
                        if (summary != null)
                        {
                            //当前逻辑图ID已存在,需要判断路径是否相同
                            if (summary.AssetPath == str)
                                //如果路径相同则不操作
                                continue;
                        }
                        //bool res = MicroGraphProvider.AddSummaryModel(str, logicGraph);
                        //if (res)
                        //{
                        refreshViewEvent.addGraphs.Add(str);
                        //}
                        hasAsset = true;
                        //if (!hasAsset)
                        //    hasAsset = res;
                    }
                }
            }
            foreach (string str in deletedAssets)
            {
                if (Path.GetExtension(str) == ".asset")
                {
                    //bool res = MicroGraphProvider.DeleteSummaryModel(str);
                    //if (res)
                    refreshViewEvent.deletedGraphs.Add(str);
                    //if (!hasAsset)
                    //    hasAsset = res;
                    hasAsset = true;
                }
            }
            if (hasAsset)
            {
                MicroGraphProvider.RefreshGraphSummary();
                MicroGraphEventListener.OnEventAll(MicroGraphEventIds.GRAPH_ASSETS_CHANGED, refreshViewEvent);
            }
        }
    }
}
