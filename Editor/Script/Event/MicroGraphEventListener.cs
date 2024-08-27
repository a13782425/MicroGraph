using System;
using System.Collections.Generic;
using UnityEngine;

namespace MicroGraph.Editor
{
    /// <summary>
    /// 事件委托
    /// </summary>
    /// <param name="args"></param>
    /// <returns>返回false可中断当前消息</returns>
    public delegate bool MessageEventHandler(object args);
    /// <summary>
    /// 微图事件监听者
    /// </summary>
    public class MicroGraphEventListener
    {
        #region Static

        private static List<MicroGraphEventListener> _allListeners = new List<MicroGraphEventListener>();

        public static void RegisterListener(MicroGraphEventListener listener)
        {
            if (listener == null)
                return;
            _allListeners.Add(listener);
        }
        public static void UnregisterListener(MicroGraphEventListener listener)
        {
            if (listener == null)
                return;
            _allListeners.Remove(listener);
        }
        public static void OnEventAll(int messageId, object args = null)
        {
            for (int i = _allListeners.Count - 1; i >= 0; i--)
            {
                _allListeners[i].OnEvent(messageId, args);
            }
        } 

        #endregion

        private Dictionary<int, MessageDto> _allMsg = new Dictionary<int, MessageDto>();
        private Queue<List<MessageEventHandler>> _cachePool = new Queue<List<MessageEventHandler>>();
        /// <summary>
        /// 注册事件
        /// 同一个事件回调在一个事件ID中只能注册一次
        /// </summary>
        /// <param name="messageId">事件ID</param>
        /// <param name="callback">事件回调</param>
        public void AddListener(int messageId, MessageEventHandler callback)
        {
            if (!_allMsg.ContainsKey(messageId))
            {
                _allMsg[messageId] = new MessageDto(messageId, this);
            }
            _allMsg[messageId].Add(callback);
        }
        /// <summary>
        /// 移除一个事件监听
        /// </summary>
        /// <param name="messageId"></param>
        /// <param name="callback"></param>
        public void RemoveListener(int messageId, MessageEventHandler callback)
        {
            if (!_allMsg.ContainsKey(messageId))
            {
                _allMsg[messageId] = new MessageDto(messageId, this);
            }
            _allMsg[messageId].Remove(callback);
        }
        /// <summary>
        /// 派发一个事件
        /// </summary>
        /// <param name="messageId"></param>
        /// <param name="args"></param>
        public void OnEvent(int messageId, object args = null)
        {
            if (_allMsg.ContainsKey(messageId))
            {
                _allMsg[messageId].OnEvent(args);
            }
        }

        /// <summary>
        /// 弹出一个list
        /// </summary>
        private List<MessageEventHandler> DequeueList()
        {
            if (_cachePool.Count > 0)
            {
                return _cachePool.Dequeue();
            }
            return new List<MessageEventHandler>();
        }

        /// <summary>
        /// 压入一个list
        /// </summary>
        private void EnqueueList(List<MessageEventHandler> list)
        {
            list.Clear();
            _cachePool.Enqueue(list);
        }

        private class MessageDto
        {
            public readonly int Id;
            private bool _isExecute = false;
            private HashSet<MessageEventHandler> _messageEvent = new HashSet<MessageEventHandler>();
            private List<MessageEventHandler> _waitAddList = null;
            private List<MessageEventHandler> _waitDelList = null;
            private MicroGraphEventListener listener;
            public MessageDto(int id, MicroGraphEventListener listener)
            {
                this.Id = id;
                this.listener = listener;
            }

            public void Add(MessageEventHandler messageEvent)
            {
                if (_messageEvent.Contains(messageEvent))
                {
                    return;
                }
                if (_isExecute)
                {
                    if (_waitAddList == null)
                    {
                        _waitAddList = listener.DequeueList();
                    }
                    //事件正在执行
                    _waitAddList.Add(messageEvent);
                }
                else
                {
                    //事件没有执行
                    _messageEvent.Add(messageEvent);
                }
            }
            public void Remove(MessageEventHandler messageEvent)
            {
                if (!_messageEvent.Contains(messageEvent))
                {
                    return;
                }
                if (_isExecute)
                {
                    if (_waitDelList == null)
                    {
                        _waitDelList = listener.DequeueList();
                    }
                    //事件正在执行
                    _waitDelList.Add(messageEvent);
                }
                else
                {
                    //事件没有执行
                    _messageEvent.Remove(messageEvent);
                }
            }
            public void OnEvent(object args)
            {
                _isExecute = true;
                foreach (var item in _messageEvent)
                {
                    try
                    {
                        bool result = item.Invoke(args);
                        if (!result)
                        {
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError(ex.Message);
                    }
                }
                _isExecute = false;
                if (_waitAddList != null)
                {
                    foreach (var item in _waitAddList)
                    {
                        this.Add(item);
                    }
                    listener.EnqueueList(_waitAddList);
                    _waitAddList = null;
                }
                if (_waitDelList != null)
                {
                    foreach (var item in _waitDelList)
                    {
                        this.Remove(item);
                    }
                    listener.EnqueueList(_waitDelList);
                    _waitDelList = null;
                }

            }
        }
    }
}
