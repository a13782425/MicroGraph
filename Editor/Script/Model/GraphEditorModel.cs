using MicroGraph.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MicroGraph.Editor
{
    ///// <summary>
    ///// 图编辑器信息
    ///// </summary>
    //[Serializable]
    //internal sealed class GraphEditorModel
    //{
    //    private readonly static DateTime MINI_TIME = new DateTime(2023, 1, 1);
    //    public BaseMicroGraph target { get; set; }
        
    //    [SerializeField]
    //    private string a = "";
    //    /// <summary>
    //    /// 标题
    //    /// </summary>
    //    public string LogicName { get => a; set => a = value; }
    //    /// <summary>
    //    /// 当前图的节点编辑器数据
    //    /// </summary>
    //    [SerializeField]
    //    private List<NodeEditorModel> b = new List<NodeEditorModel>();
    //    /// <summary>
    //    /// 当前图的节点编辑器数据
    //    /// </summary>
    //    public List<NodeEditorModel> NodeDatas => b;

    //    /// <summary>
    //    /// 当前图的分组编辑器数据
    //    /// </summary>
    //    [SerializeField]
    //    private List<GroupEditorModel> c = new List<GroupEditorModel>();
    //    /// <summary>
    //    /// 当前图的分组编辑器数据
    //    /// </summary>
    //    public List<GroupEditorModel> GroupDatas => c;
    //    /// <summary>
    //    /// 最后一次Format位置
    //    /// </summary>
    //    [SerializeField]
    //    private string d = "";
    //    /// <summary>
    //    /// 最后一次Format位置
    //    /// </summary>
    //    [SerializeField]
    //    public string LastFormatPath { get => d; set => d = value; }
    //    /// <summary>
    //    /// 当前图坐标
    //    /// </summary>
    //    [SerializeField]
    //    private Vector3 e = Vector3.zero;
    //    /// <summary>
    //    /// 当前图坐标
    //    /// </summary>
    //    [SerializeField]
    //    public Vector3 Pos { get => e; set => e = value; }

    //    /// <summary>
    //    /// 当前图的缩放
    //    /// </summary>
    //    [SerializeField]
    //    private Vector3 f = Vector3.one;
    //    /// <summary>
    //    /// 当前图的缩放
    //    /// </summary>
    //    public Vector3 Scale { get => f; set => f = value; }

    //    /// <summary>
    //    /// 创建时间
    //    /// </summary>
    //    [SerializeField]
    //    private int g = 0;
    //    /// <summary>
    //    /// 创建时间DateTime.Now.ToString("yyyy.MM.dd");
    //    /// </summary>
    //    public DateTime CreateTime { get => MINI_TIME.AddMinutes(g); set => g = (int)(value - MINI_TIME).TotalMinutes; }

    //    /// <summary>
    //    /// 修改时间
    //    /// </summary>
    //    [SerializeField]
    //    private int h = 0;
    //    /// <summary>
    //    /// 修改时间
    //    /// </summary>
    //    public DateTime ModifyTime { get => MINI_TIME.AddMinutes(h); set => h = (int)(value - MINI_TIME).TotalMinutes; }

    //    /// <summary>
    //    /// 描述
    //    /// </summary>
    //    [SerializeField]
    //    private string i = "";
    //    /// <summary>
    //    /// 描述
    //    /// </summary>
    //    public string Describe { get => i; set => i = value; }
    //}



    ///// <summary>
    ///// 图编辑器分组信息
    ///// </summary>
    //[Serializable]
    //internal sealed class GroupEditorModel
    //{
    //    [SerializeField]
    //    private string a = "New Group";
    //    public string Title { get => a; set => a = value; }
    //    [SerializeField]
    //    private Color b = new Color(0, 0, 0, 0.3f);
    //    public Color Color { get => b; set => b = value; }
    //    [SerializeField]
    //    private Vector2 c;
    //    public Vector2 Pos { get => c; set => c = value; }
    //    [SerializeField]
    //    private Vector2 d;
    //    public Vector2 Size { get => d; set => d = value; }
    //    [SerializeField]
    //    private List<int> e = new List<int>();
    //    public List<int> Nodes => e;
    //}

    ///// <summary>
    ///// 图编辑器节点信息
    ///// </summary>
    //[Serializable]
    //internal sealed class NodeEditorModel
    //{
    //    public BaseMicroNode target { get; set; }

    //    [SerializeField]
    //    private int a;
    //    public int OnlyId { get => a; set => a = value; }

    //    [SerializeField]
    //    private string b;
    //    public string Title { get => b; set => b = value; }

    //    [SerializeField]
    //    private Vector2 c;
    //    public Vector2 Pos { get => c; set => c = value; }
    //    /// <summary>
    //    /// 是否上锁
    //    /// </summary>
    //    [SerializeField]
    //    private bool d;
    //    public bool IsLock { get => d; set => d = value; }
    //    /// <summary>
    //    /// 节点描述
    //    /// </summary>
    //    [SerializeField]
    //    private string e;
    //    public string Describe { get => e; set => e = value; }

    //}
}
