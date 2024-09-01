using System;
using System.Collections.Generic;

namespace MicroGraph.Editor
{
    /// <summary>
    /// 微图的撤销操作
    /// </summary>
    public sealed class MicroGraphViewUndo
    {
        private MicroRecordOperateData _originData;

        private DateTime _originTime = DateTime.Now;

        private LinkedList<MicroRecordOperateData> _undoDatas = new LinkedList<MicroRecordOperateData>();
        private LinkedList<MicroRecordOperateData> _redoDatas = new LinkedList<MicroRecordOperateData>();
        private MicroRecordOperateData _curData;

        private BaseMicroGraphView _graphView;

        private bool isBusy = false;
        public void Initialize(BaseMicroGraphView graphView)
        {
            this._graphView = graphView;
        }

        public void AddCommand(IMicroGraphRecordCommand command)
        {
            if (isBusy)
                return;
            if (_undoDatas.Count >= MicroGraphUtils.EditorConfig.UndoStep)
                _undoDatas.RemoveFirst();
            MicroRecordOperateData operateData = default;
            if (_undoDatas.Count > 0)
            {
                operateData = _undoDatas.Last.Value;
                if (operateData.RecordId != GetRecordId())
                {
                    operateData = new MicroRecordOperateData();
                    operateData.View = _graphView;
                    operateData.RecordId = GetRecordId();
                    _redoDatas.Clear();
                    _undoDatas.AddLast(operateData);
                }
            }
            else
            {
                operateData = new MicroRecordOperateData();
                operateData.View = _graphView;
                operateData.RecordId = GetRecordId();
                _redoDatas.Clear();
                _undoDatas.AddLast(operateData);
            }
            operateData.AddCommand(command);
        }

        public void Undo()
        {
            if (_undoDatas.Count == 0)
                return;
            isBusy = true;
            _graphView.View.ClearSelection();
            MicroRecordOperateData operateData = _undoDatas.Last.Value;
            _undoDatas.RemoveLast();
            _redoDatas.AddLast(operateData);
            var record = operateData.Record;
            while (record != null)
            {
                record.RecordCommand.Undo(operateData.View);
                record = record.Next;
            }
            if (_graphView.View.selection.Count > 0)
                _graphView.View.DeleteSelection();
            isBusy = false;
        }

        public void Redo()
        {
            if (_redoDatas.Count == 0)
                return;
            isBusy = true;
            _graphView.View.ClearSelection();
            MicroRecordOperateData operateData = _redoDatas.Last.Value;
            _redoDatas.RemoveLast();
            _undoDatas.AddLast(operateData);
            var record = operateData.Record;
            while (record != null)
            {
                record.RecordCommand.Redo(operateData.View);
                record = record.Next;
            }
            if (_graphView.View.selection.Count > 0)
                _graphView.View.DeleteSelection();
            isBusy = false;
        }

        private int GetRecordId()
        {
            double millisecond = (DateTime.Now - _originTime).TotalMilliseconds * 0.01;
            return (int)millisecond;
        }
    }
}
