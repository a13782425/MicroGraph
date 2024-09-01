using MicroGraph.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using Random = System.Random;
using UnityObject = UnityEngine.Object;

namespace MicroGraph.Editor
{
    /// <summary>
    /// 逻辑图工具类
    /// </summary>
    public static partial class MicroGraphUtils
    {
        /// <summary>
        /// 标准时间
        /// </summary>
        public readonly static DateTime STANDARD_TIME = new DateTime(2023, 1, 1);
        /// <summary>
        /// 窗口最小大小
        /// </summary>
        public readonly static Vector2 MIN_SIZE;
        /// <summary>
        /// 编辑器路径
        /// </summary>
        public readonly static string EDITOR_PATH_ROOT;
        public readonly static string EDITOR_RESOURCE_PATH = "__MicroGraph";
        public readonly static string CONFIG_PATH_ROOT = "../Library/MicroGraph";

        /// <summary>
        /// 通知时间
        /// </summary>
        public const float NOTIFICATION_TIME = 2F;
        /// <summary>
        /// 版本号
        /// </summary>
        public static string Versions => MicroGraphGlobalConfigModel.VERSIONS;

        /// <summary>
        /// 微图编辑器下的配置
        /// </summary>
        private static MicroGraphGlobalConfigModel _allEditorConfig = null;
        /// <summary>
        /// 微图编辑器下的配置
        /// </summary>
        internal static MicroGraphGlobalConfigModel EditorConfig => _allEditorConfig;

        private static Font _currentFont = null;
        /// <summary>
        /// 当前字体
        /// </summary>
        internal static Font CurrentFont
        {
            get
            {
                if (_currentFont != null)
                {
                    return _currentFont;
                }
                RefreshFont();
                return _currentFont;
            }
        }

        /// <summary>
        /// 微图编辑器下的配置文件路径
        /// </summary>
        private const string EDITOR_CONFIG_FILE = "MicroGraphConfig";

        static MicroGraphUtils()
        {
            Type lgType = typeof(MicroGraphWindow);
            string[] guids = AssetDatabase.FindAssets(lgType.Name);
            foreach (var item in guids)
            {

                string path = AssetDatabase.GUIDToAssetPath(item);
                MonoScript monoScript = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                if (monoScript != null && monoScript.GetClass() == lgType)
                {
                    DirectoryInfo info = Directory.GetParent(Path.GetDirectoryName(path));
                    string fullPath = Path.Combine("Packages", Path.GetRelativePath("Packages", info.ToString()));
                    string[] strs = fullPath.Split(new char[] { '@' }, StringSplitOptions.RemoveEmptyEntries);
                    if (strs.Length == 1)
                        EDITOR_PATH_ROOT = Path.Combine(strs[0], "Resources", "__MicroGraph");
                    else
                        EDITOR_PATH_ROOT = Path.Combine(strs[0], "Editor", "Resources", "__MicroGraph");
                    break;
                }
            }

            MIN_SIZE = new Vector2(640, 480);
            CONFIG_PATH_ROOT = Path.Combine(Application.dataPath, CONFIG_PATH_ROOT);
            if (!Directory.Exists(CONFIG_PATH_ROOT))
                Directory.CreateDirectory(CONFIG_PATH_ROOT);
            string file = Path.Combine(CONFIG_PATH_ROOT, EDITOR_CONFIG_FILE);
            if (!File.Exists(file))
            {
                _allEditorConfig = new MicroGraphGlobalConfigModel();
                SaveConfig();
            }
            else
            {
                string str = File.ReadAllText(file, Encoding.UTF8);
                try
                {
                    _allEditorConfig = JsonUtility.FromJson<MicroGraphGlobalConfigModel>(str);
                }
                catch (Exception)
                {
                    Debug.LogError("微图配置文件错误，正在还原配置文件...");
                    _allEditorConfig = new MicroGraphGlobalConfigModel();
                    SaveConfig();
                    Debug.LogError("微图配置文件错误，配置文件还原完成");
                }
            }
            if (string.IsNullOrWhiteSpace(_allEditorConfig.SavePath))
            {
                _allEditorConfig.SavePath = Application.dataPath;
            }
            _allEditorConfig.OverviewConfig.Initialize();
            RefreshFont();
            EditorApplication.playModeStateChanged += s_onPlayModeStateChanged;
        }


    }
    //公共方法
    partial class MicroGraphUtils
    {
        /// <summary>
        /// 打开一个微图
        /// </summary>
        /// <returns></returns>
        public static bool OpenMicroGraph(string assetPath)
        {
            try
            {
                BaseMicroGraph baseMicroGraph = AssetDatabase.LoadAssetAtPath<BaseMicroGraph>(assetPath);
                return OpenMicroGraph(baseMicroGraph);
            }
            catch (Exception)
            {
                Debug.LogError("路径异常:" + assetPath);
                return false;
            }
        }
        /// <summary>
        /// 打开一个微图
        /// </summary>
        /// <returns></returns>
        public static bool OpenMicroGraph(BaseMicroGraph graph)
        {
            if (graph == null)
                return false;
            MicroGraphWindow.ShowMicroGraph(graph.OnlyId);
            return true;
        }

        /// <summary>
        /// 获取一个时间距离标准时间的秒数
        /// </summary>
        /// <returns></returns>
        public static int GetSeconds()
        {
            return (int)(DateTime.Now - STANDARD_TIME).TotalSeconds;
        }
        /// <summary>
        /// 获取一个时间距离标准时间的毫秒数
        /// </summary>
        /// <returns></returns>
        public static int GetMilliseconds()
        {
            return (int)(DateTime.Now - STANDARD_TIME).TotalMilliseconds;
        }
        /// <summary>
        /// 格式化一个时间
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static string FormatTime(DateTime time)
        {
            return time.ToString("yyyy.MM.dd HH:mm");
        }
        /// <summary>
        /// 获取图的颜色
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Color GetColor(Type type)
        {
            return GetColor(type.FullName);
        }
        /// <summary>
        /// 获取图的颜色
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Color GetColor(string str)
        {
            int temp = str.GetHashCode();
            Random random = new Random(temp);
            int count = random.Next(10, 50);
            float h = 0;
            float s = 0;
            for (int i = 0; i < count; i++)
            {
                h = (float)random.NextDouble();
                s = (float)random.NextDouble();
            }
            s = (0.5f - 0.2f) * s + 0.2f;
            return Color.HSVToRGB(h, s, 1);
        }
        /// <summary>
        /// 获取一个随机颜色
        /// </summary>
        /// <returns></returns>
        public static Color GetRandomColor() => MicroGraphUtils.GetColor(Guid.NewGuid().ToString());
    }
    //内部方法
    partial class MicroGraphUtils
    {
        /// <summary>
        /// 加载资源文件夹下的资源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <returns></returns>
        internal static T LoadRes<T>(string path) where T : UnityEngine.Object
        {
            return Resources.Load<T>(Path.Combine(EDITOR_RESOURCE_PATH, path));
        }
        /// <summary>
        /// 卸载一个UnityObject
        /// </summary>
        /// <param name="obj"></param>
        internal static void UnloadObject(BaseMicroGraph obj)
        {
            if (obj != null)
            {
                List<MicroGraphWindow> panels = Resources.FindObjectsOfTypeAll(typeof(MicroGraphWindow)).OfType<MicroGraphWindow>().ToList();
                bool isUse = false;
                foreach (var item in panels)
                {
                    if (item.graphId == obj.OnlyId)
                    {
                        isUse = true;
                        break;
                    }
                }
                if (!isUse)
                    Resources.UnloadAsset(obj);
            }
        }
        /// <summary>
        /// 获取微图数据
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static BaseMicroGraph GetMicroGraph(string path)
        {
            return AssetDatabase.LoadAssetAtPath<BaseMicroGraph>(path);
        }
        /// <summary>
        /// 创建逻辑图 
        /// </summary>
        /// <param name="graphType"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static BaseMicroGraph CreateLogicGraph(Type graphType, string path)
        {
            BaseMicroGraph graph = ScriptableObject.CreateInstance(graphType) as BaseMicroGraph;
            string file = Path.GetFileNameWithoutExtension(path);
            graph.name = file;
            graph.editorInfo.Title = file;
            graph.editorInfo.CreateTime = DateTime.Now;
            graph.editorInfo.ShowGrid = MicroGraphUtils.EditorConfig.DefaultOpenGrid;
            graph.editorInfo.CanZoom = MicroGraphUtils.EditorConfig.DefaultOpenZoom;
            AssetDatabase.CreateAsset(graph, path);
            AssetDatabase.Refresh();
            return graph;
        }
        /// <summary>
        /// 创建逻辑图 
        /// </summary>
        /// <param name="graphType"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static BaseMicroGraph CreateLogicGraph(GraphCategoryModel configData)
        {
            if (!Directory.Exists(EditorConfig.SavePath))
                EditorConfig.SavePath = Application.dataPath;
            string path = EditorUtility.SaveFilePanel("创建逻辑图", EditorConfig.SavePath, "MicroGraph", "asset");
            if (string.IsNullOrEmpty(path))
            {
                EditorUtility.DisplayDialog("错误", "路径为空", "确定");
                return null;
            }
            if (File.Exists(path))
            {
                EditorUtility.DisplayDialog("错误", "创建文件已存在", "确定");
                return null;
            }
            path = FileUtil.GetProjectRelativePath(path);
            if (EditorConfig.RecordSavePath)
                EditorConfig.SavePath = Path.GetDirectoryName(path);
            BaseMicroGraph graph = CreateLogicGraph(configData.GraphType, path);
            return graph;
        }
        /// <summary>
        /// 删除逻辑图
        /// </summary>
        /// <param name="graph"></param>
        /// <returns></returns>
        internal static bool RemoveGraph(BaseMicroGraph graph)
        {
            return RemoveGraph(AssetDatabase.GetAssetPath(graph));
        }
        /// <summary>
        /// 删除逻辑图
        /// </summary>
        /// <param name="graph"></param>
        /// <returns></returns>
        internal static bool RemoveGraph(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
                AssetDatabase.Refresh();
            }
            return true;
        }
        /// <summary>
        /// 保存config
        /// </summary>
        public static void SaveConfig()
        {
            File.WriteAllText(Path.Combine(CONFIG_PATH_ROOT, EDITOR_CONFIG_FILE), JsonUtility.ToJson(_allEditorConfig), Encoding.UTF8);
        }

        /// <summary>
        /// 标题合法性
        /// </summary>
        /// <returns></returns>
        public static bool TitleValidity(string titleStr, int length = 30)
        {
            if (string.IsNullOrWhiteSpace(titleStr))
                return false;
            int utf8ByteCount = Encoding.UTF8.GetByteCount(titleStr);
            return utf8ByteCount > 0 && utf8ByteCount < length;
        }
        /// <summary>
        /// 变量名合法性
        /// </summary>
        /// <returns></returns>
        public static bool VariableValidity(string varName)
        {
            varName = varName.Trim();
            if (string.IsNullOrWhiteSpace(varName))
            {
                return false;
            }
            char[] strs = varName.ToArray();
            if (strs.Length > 20)
            {
                return false;
            }
            bool result = true;
            int length = 0;
            while (length < strs.Length)
            {
                char c = strs[length];
                if ((c < 'A' || c > 'Z') && (c < 'a' || c > 'z') && c != '_')
                {
                    if (length == 0)
                    {
                        result = false;
                        goto End;
                    }
                    else if (c < '0' || c > '9')
                    {
                        result = false;
                        goto End;
                    }

                }
                length++;
            }
        End:
            return result;
        }
        /// <summary>
        /// 获取Lambda表达式成员名
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        internal static string GetMemberName(LambdaExpression expression)
        {
            if (expression == null)
                throw new ArgumentNullException("expression");

            var body = expression.Body as MemberExpression;
            if (body == null)
            {
                Debug.LogError(string.Format("Invalid expression:{0}", expression));
                return null;
            }
            if (!(body.Expression is ParameterExpression))
            {
                Debug.LogError(string.Format("Invalid expression:{0}", expression));
                return null;
            }
            return body.Member.Name;
        }
        private static void s_onPlayModeStateChanged(PlayModeStateChange change)
        {
            switch (change)
            {
                case PlayModeStateChange.EnteredEditMode:
                    MicroGraphEventListener.OnEventAll(MicroGraphEventIds.EDITOR_PLAY_MODE_CHANGED, change);
                    break;
                case PlayModeStateChange.EnteredPlayMode:
                    MicroGraphEventListener.OnEventAll(MicroGraphEventIds.EDITOR_PLAY_MODE_CHANGED, change);
                    break;
                default:
                    break;
            }
        }
        internal static void RefreshFont()
        {
            if (_currentFont != null)
            {
                EditorUtility.UnloadUnusedAssetsImmediate();
                _currentFont = null;
            }
            if (!string.IsNullOrWhiteSpace(EditorConfig.EditorFont) && File.Exists(EditorConfig.EditorFont))
            {
                _currentFont = new Font(Path.GetFullPath(EditorConfig.EditorFont));
                if (_currentFont == null)
                {
#if UNITY_2022_1_OR_NEWER
                    _currentFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
#else
                    _currentFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
#endif
                }
            }
            else
            {
                EditorConfig.EditorFont = "";
#if UNITY_2022_1_OR_NEWER
                _currentFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
#else
                _currentFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
#endif
            }
        }
    }
    partial class MicroGraphUtils
    {
        [MenuItem("Tools/微图/打开窗口", priority = 99)]
        private static void OpenGraphWindow()
        {
            MicroGraphWindow.OpenWindow();
        }
        [MenuItem("Tools/微图/节点信息", priority = 198)]
        private static void NodeUsageRate()
        {
            EditorWindow.GetWindow<MicroNodeInfoWindow>();
        }
        [MenuItem("Tools/微图/修复索引", priority = 199)]
        private static void FixedGroupIndex()
        {
            UnityObject[] panels = Resources.FindObjectsOfTypeAll(typeof(MicroGraphWindow));
            if (panels.Length > 0)
            {
                EditorUtility.DisplayDialog("提示", "请关闭所有微图窗口后重试", "确定");
                return;
            }
            MicroGraphProvider.GraphSummaryList.Clear();
            string[] guids = AssetDatabase.FindAssets("t:BaseMicroGraph");
            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                BaseMicroGraph logicGraph = AssetDatabase.LoadAssetAtPath<BaseMicroGraph>(assetPath);
                if (logicGraph == null)
                    continue;
                MicroGraphEditorInfo editorInfo = logicGraph.editorInfo;
                GraphSummaryModel graphCache = new GraphSummaryModel();
                graphCache.GraphClassName = logicGraph.GetType().FullName;
                graphCache.AssetPath = assetPath;
                graphCache.OnlyId = logicGraph.OnlyId;
                graphCache.SetEditorInfo(editorInfo);
                MicroGraphProvider.GraphSummaryList.Add(graphCache);
            }
        }
        [OnOpenAsset(0)]
        private static bool OnBaseGraphOpened(int instanceID, int line)
        {
            var asset = EditorUtility.InstanceIDToObject(instanceID) as BaseMicroGraph;

            if (asset != null)
            {
                MicroGraphWindow.ShowMicroGraph(asset.OnlyId);
                return true;
            }
            return false;
        }
    }
}
