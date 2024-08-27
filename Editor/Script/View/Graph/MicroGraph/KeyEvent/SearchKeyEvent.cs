using UnityEngine;
using UnityEngine.UIElements;

namespace MicroGraph.Editor
{
    /// <summary>
    /// 搜索
    /// </summary>
    public sealed class SearchKeyEvent : BaseMicroGraphKeyEvent
    {
        public override KeyCode Code => KeyCode.F;

        private MicroSearchView _searchView;
        public override bool Execute(KeyDownEvent evt, BaseMicroGraphView graphView)
        {
            if (_searchView == null)
            {
                _searchView = new MicroSearchView(graphView);
                graphView.View.Add(_searchView);
            }
            _searchView.Search();
            return true;
        }
    }
}
