using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using UnityPopupWindow = UnityEditor.PopupWindow;

namespace MicroGraph.Editor
{
    public struct SearchPopupContent
    {
        public enum ItemType
        {
            Category,
            Separator,
            Item
        }

        public struct Category
        {
            public string name;
        }

        public struct ValueItem
        {
            public string value;

            public string displayName;

            public string categoryName;
        }

        internal struct Item
        {
            public string value;

            public string displayName;

            public string categoryName;

            public ItemType itemType;
        }

        private List<Item> m_Items;

        internal List<Item> Items => m_Items ?? (m_Items = new List<Item>());

        public void AppendCategory(Category category)
        {
            Items.Add(new Item
            {
                itemType = ItemType.Category,
                displayName = category.name,
                categoryName = category.name
            });
        }

        public void AppendValue(ValueItem value)
        {
            Items.Add(new Item
            {
                itemType = ItemType.Item,
                value = value.value,
                displayName = string.IsNullOrWhiteSpace(value.displayName) ? value.value : string.Empty,
                categoryName = string.IsNullOrWhiteSpace(value.categoryName) ? value.value : string.Empty
            });
        }

        public void AppendSeparator()
        {
            Items.Add(new Item
            {
                itemType = ItemType.Separator
            });
        }

        public void AppendContent(SearchPopupContent content)
        {
            foreach (Item item in content.Items)
            {
                Items.Add(new Item
                {
                    value = item.value,
                    categoryName = item.categoryName,
                    itemType = item.itemType,
                    displayName = item.displayName
                });
            }
        }
    }

    public class SearchPopupField : BaseField<string>
    {

        private class WindowContent : PopupWindowContent
        {
            const string k_UssPath = "Uss/Element/SearchPopupContent";
            const string k_SelectionContextKey = "SearchPopupField.SelectionContext";
            const string k_BaseClass = "unity-search-popup-field";
            const string k_Category = k_BaseClass + "__category";
            const string k_Item = k_BaseClass + "__item";
            const string k_ItemInCategory = k_BaseClass + "__category-item";
            const string k_Separator = k_BaseClass + "__separator";
            const string k_SearchField = k_BaseClass + "__search-field";

            class SelectionContext
            {
                public int index;
                public SearchPopupContent.Item item;
                public WindowContent content;
            }

            static readonly UnityEngine.Pool.ObjectPool<TextElement> s_CategoryPool = new UnityEngine.Pool.ObjectPool<TextElement>(() =>
            {
                var category = new TextElement();
                category.AddToClassList(k_Category);
                return category;
            }, null, te =>
            {
                te.style.display = DisplayStyle.Flex;
            });

            static readonly UnityEngine.Pool.ObjectPool<TextElement> s_ItemPool = new UnityEngine.Pool.ObjectPool<TextElement>(() =>
            {
                var value = new TextElement();
                value.style.display = DisplayStyle.Flex;
                value.AddToClassList(k_Item);
                return value;
            }, null, te =>
            {
                CustomPseudoStates pseudoStates = te.GetPseudoStates();
                pseudoStates &= ~CustomPseudoStates.Checked;
                te.SetPseudoStates(pseudoStates);
                te.style.display = DisplayStyle.Flex;
                te.RemoveFromClassList(k_ItemInCategory);
            });

            static readonly UnityEngine.Pool.ObjectPool<VisualElement> s_SeparatorPool = new UnityEngine.Pool.ObjectPool<VisualElement>(() =>
            {
                var separator = new VisualElement();
                separator.AddToClassList(k_Separator);
                return separator;
            }, null, ve =>
            {
                ve.style.display = DisplayStyle.Flex;
            });

            readonly List<SearchPopupContent.Item> m_Items = new List<SearchPopupContent.Item>();

            Vector2 m_Size;
            string m_CurrentActiveValue;
            int m_SelectedIndex = -1;
            ScrollView m_ScrollView;
            KeyboardNavigationManipulator m_NavigationManipulator;

            public event Action<string> onSelectionChanged;

            public void Show(Rect rect, string currentValue, IEnumerable<SearchPopupContent.Item> items)
            {
                m_CurrentActiveValue = currentValue;

                m_Items.Clear();
                m_Items.AddRange(items);

                m_Size = new Vector2(rect.width, 22 * m_Items.Count + 36);
                m_Size.y = Mathf.Min(240, m_Size.y);
                UnityPopupWindow.Show(rect, this);
            }

            public override void OnOpen()
            {
                editorWindow.rootVisualElement.AddStyleSheet(k_UssPath);
                editorWindow.rootVisualElement.focusable = true;

                editorWindow.rootVisualElement.AddToClassList(k_BaseClass);
                editorWindow.rootVisualElement.AddManipulator(m_NavigationManipulator = new KeyboardNavigationManipulator(Apply));
                var searchField = new ToolbarSearchField();
                searchField.AddToClassList(k_SearchField);
                searchField.RegisterCallback<AttachToPanelEvent>(evt =>
                {
                    ((VisualElement)evt.target)?.Focus();
                });

                searchField.RegisterCallback<KeyDownEvent>(evt =>
                {
                    switch (evt.keyCode)
                    {
                        case KeyCode.UpArrow:
                        case KeyCode.DownArrow:
                        case KeyCode.PageDown:
                        case KeyCode.PageUp:
                            evt.StopPropagation();
                            m_ScrollView.Focus();
                            break;
                        case KeyCode.Return:
                        case KeyCode.KeypadEnter:
                            evt.StopPropagation();
                            if (string.IsNullOrWhiteSpace(searchField.value) || m_SelectedIndex < 0)
                            {
                                m_ScrollView.Focus();
                                return;
                            }

                            onSelectionChanged?.Invoke(m_Items[m_SelectedIndex].value);
                            editorWindow.Close();
                            break;
                    }
                });

                editorWindow.rootVisualElement.RegisterCallback<KeyDownEvent>(evt =>
                {
                    searchField.Focus();
                });

                searchField.RegisterValueChangedCallback(OnSearchChanged);
                editorWindow.rootVisualElement.Add(searchField);

                m_ScrollView = new ScrollView();
                m_ScrollView.RegisterCallback<GeometryChangedEvent, ScrollView>((evt, sv) =>
                {
                    if (m_SelectedIndex >= 0)
                        sv.ScrollTo(sv[m_SelectedIndex]);
                }, m_ScrollView);

                var selectionWasSet = false;
                for (var i = 0; i < m_Items.Count; ++i)
                {
                    var property = m_Items[i];
                    var element = GetPooledItem(property, i);
                    m_ScrollView.Add(element);

                    if (selectionWasSet)
                        continue;

                    if (property.itemType != SearchPopupContent.ItemType.Item || property.value != m_CurrentActiveValue)
                        continue;

                    m_SelectedIndex = i;
                    element.SetPseudoStates(element.GetPseudoStates() | CustomPseudoStates.Checked);
                    selectionWasSet = true;
                }
                editorWindow.rootVisualElement.RegisterCallback<KeyDownEvent>(evt =>
                {
                    if (evt.keyCode == KeyCode.F && evt.actionKey)
                    {
                        searchField.Focus();
                    }
                }, TrickleDown.TrickleDown);

                editorWindow.rootVisualElement.Add(m_ScrollView);
            }

            public override void OnClose()
            {
                editorWindow?.rootVisualElement.RemoveManipulator(m_NavigationManipulator);

                // Return to pool
                for (var i = 0; i < m_Items.Count; ++i)
                {
                    switch (m_Items[i].itemType)
                    {
                        case SearchPopupContent.ItemType.Category:
                            s_CategoryPool.Release((TextElement)m_ScrollView[i]);
                            break;
                        case SearchPopupContent.ItemType.Separator:
                            s_SeparatorPool.Release(m_ScrollView[i]);
                            break;
                        case SearchPopupContent.ItemType.Item:
                            s_ItemPool.Release((TextElement)m_ScrollView[i]);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                m_ScrollView.Clear();
            }

            bool SetSelection(int index)
            {
                if (index < 0 || index >= m_ScrollView.childCount)
                {
                    if (m_SelectedIndex >= 0)
                    {
                        var previous = m_ScrollView[m_SelectedIndex];
                        previous.SetPseudoStates(previous.GetPseudoStates() & (~CustomPseudoStates.Checked));
                    }

                    m_SelectedIndex = -1;
                    return false;
                }

                if (m_SelectedIndex >= 0)
                {
                    var previous = m_ScrollView[m_SelectedIndex];
                    previous.SetPseudoStates(previous.GetPseudoStates() & (~CustomPseudoStates.Checked));
                }

                m_SelectedIndex = index;
                var next = m_ScrollView[m_SelectedIndex];
                next.SetPseudoStates(next.GetPseudoStates() | CustomPseudoStates.Checked);
                m_ScrollView.ScrollTo(next);
                return true;
            }

            void ResetSearch()
            {
                for (var i = 0; i < m_ScrollView.childCount; ++i)
                {
                    var element = m_ScrollView[i];
                    element.style.display = DisplayStyle.Flex;
                }
            }

            void OnSearchChanged(ChangeEvent<string> evt)
            {
                var searchString = evt.newValue;
                if (string.IsNullOrEmpty(searchString))
                {
                    ResetSearch();
                    return;
                }

                for (var i = 0; i < m_Items.Count; ++i)
                {
                    var item = m_Items[i];
                    var element = m_ScrollView[i];

                    switch (item.itemType)
                    {
                        case SearchPopupContent.ItemType.Category:
                            {
                                var categoryIndex = i;
                                var shouldDisplayCategory = false;
                                // Manually iterate through the item of the current category
                                for (; i + 1 < m_Items.Count; ++i)
                                {
                                    var sub = i + 1;
                                    var categoryItem = m_Items[sub];
                                    var categoryElement = m_ScrollView[sub];
                                    if (categoryItem.itemType == SearchPopupContent.ItemType.Item &&
                                        categoryItem.categoryName == item.displayName)
                                    {
                                        if (categoryItem.displayName.IndexOf(searchString, StringComparison.OrdinalIgnoreCase) >= 0 ||
                                            categoryItem.value?.IndexOf(searchString, StringComparison.OrdinalIgnoreCase) >= 0)
                                        {
                                            shouldDisplayCategory = true;
                                            categoryElement.style.display = DisplayStyle.Flex;
                                        }
                                        else
                                        {
                                            categoryElement.style.display = DisplayStyle.None;
                                        }
                                    }
                                    else
                                        break;
                                }

                                m_ScrollView[categoryIndex].style.display = shouldDisplayCategory ? DisplayStyle.Flex : DisplayStyle.None;
                                break;
                            }
                        case SearchPopupContent.ItemType.Item:
                            if (item.displayName.IndexOf(searchString, StringComparison.OrdinalIgnoreCase) >= 0 ||
                                item.value?.IndexOf(searchString, StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                element.style.display = DisplayStyle.Flex;
                            }
                            else
                            {
                                element.style.display = DisplayStyle.None;
                            }

                            break;
                        case SearchPopupContent.ItemType.Separator:
                            m_ScrollView[i].style.display = DisplayStyle.None;
                            break;
                    }
                }

                // Check if previous selection is still visible, otherwise select the first shown item
                if (m_SelectedIndex >= 0 && m_ScrollView[m_SelectedIndex].style.display == DisplayStyle.Flex)
                    return;

                if (!SelectFirstDisplayedItem())
                    SetSelection(-1);
            }

            VisualElement GetPooledItem(SearchPopupContent.Item item, int index)
            {
                switch (item.itemType)
                {
                    case SearchPopupContent.ItemType.Category:
                        var category = s_CategoryPool.Get();
                        category.text = item.displayName;
                        return category;
                    case SearchPopupContent.ItemType.Separator:
                        return s_SeparatorPool.Get();
                    case SearchPopupContent.ItemType.Item:
                        var element = s_ItemPool.Get();
                        element.text = item.displayName;
                        element.tooltip = item.value;

                        var context = (SelectionContext)element.GetPropertyEx(k_SelectionContextKey);
                        if (null == context)
                        {
                            context = new SelectionContext();
                            element.SetPropertyEx(k_SelectionContextKey, context);
                            element.RegisterCallback<PointerUpEvent>(OnItemSelected);
                        }

                        context.index = index;
                        context.item = item;
                        context.content = this;

                        if (!string.IsNullOrWhiteSpace(item.categoryName))
                            element.AddToClassList(k_ItemInCategory);

                        return element;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            void OnItemSelected(PointerUpEvent evt)
            {
                var e = (VisualElement)evt.target;
                var ctx = (SelectionContext)e.GetPropertyEx(k_SelectionContextKey);
                // We must go through the context here, because the elements are pooled and the closure would bind on
                // the previous time the element was used.
                ctx.content.SetSelection(ctx.index);
                ctx.content.onSelectionChanged?.Invoke(ctx.item.value);
                ctx.content.editorWindow.Close();
            }

            bool SelectFirstDisplayedItem()
            {
                for (var i = 0; i < m_Items.Count; ++i)
                {
                    if (m_Items[i].itemType == SearchPopupContent.ItemType.Item && m_ScrollView[i].style.display == DisplayStyle.Flex)
                        return SetSelection(i);
                }

                return false;
            }

            bool SelectLastDisplayedItem()
            {
                for (var i = m_Items.Count - 1; i >= 0; --i)
                {
                    if (m_Items[i].itemType == SearchPopupContent.ItemType.Item && m_ScrollView[i].style.display == DisplayStyle.Flex)
                        return SetSelection(i);
                }

                return false;
            }

            bool SelectNextDisplayedItem(int offset = 1)
            {
                var current = m_SelectedIndex;
                var initialIndex = Mathf.Clamp(m_SelectedIndex + offset, 0, m_Items.Count - 1);
                for (var i = initialIndex; i < m_Items.Count; ++i)
                {
                    if (m_Items[i].itemType == SearchPopupContent.ItemType.Item &&
                        m_ScrollView[i].style.display == DisplayStyle.Flex
                        && i != current)
                        return SetSelection(i);
                }

                return false;
            }

            bool SelectPreviousDisplayedItem(int offset = 1)
            {
                var current = m_SelectedIndex;
                var initialIndex = Mathf.Clamp(m_SelectedIndex - offset, 0, m_Items.Count - 1);
                for (var i = initialIndex; i >= 0; --i)
                {
                    if (m_Items[i].itemType == SearchPopupContent.ItemType.Item &&
                        m_ScrollView[i].style.display == DisplayStyle.Flex &&
                        i != current)
                    {
                        return SetSelection(i);
                    }
                }

                return false;
            }

            void Apply(KeyboardNavigationOperation op, EventBase sourceEvent)
            {
                if (!Apply(op))
                    return;

                sourceEvent.StopImmediatePropagation();
            }

            bool Apply(KeyboardNavigationOperation op)
            {
                switch (op)
                {
                    case KeyboardNavigationOperation.None:
                    case KeyboardNavigationOperation.SelectAll:
                        break;

                    case KeyboardNavigationOperation.Cancel:
                        editorWindow.Close();
                        break;
                    case KeyboardNavigationOperation.Submit:
                        if (m_SelectedIndex < 0)
                            return false;

                        onSelectionChanged?.Invoke(m_Items[m_SelectedIndex].value);
                        editorWindow.Close();
                        break;
                    case KeyboardNavigationOperation.Previous:
                        {
                            return SelectPreviousDisplayedItem() ||
                                   SelectLastDisplayedItem();
                        }
                    case KeyboardNavigationOperation.Next:
                        {
                            return SelectNextDisplayedItem() ||
                                   SelectFirstDisplayedItem();
                        }
                    case KeyboardNavigationOperation.PageUp:
                        {
                            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                            return SelectPreviousDisplayedItem(10) ||
                                   SelectFirstDisplayedItem() ||
                                   true;
                        }
                    case KeyboardNavigationOperation.PageDown:
                        {
                            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                            return SelectNextDisplayedItem(10) ||
                                   SelectLastDisplayedItem() ||
                                   true;
                        }
                    case KeyboardNavigationOperation.Begin:
                        {
                            SelectFirstDisplayedItem();
                            return true;
                        }
                    case KeyboardNavigationOperation.End:
                        {
                            SelectLastDisplayedItem();
                            return true;
                        }
                }

                return false;
            }

            public override void OnGUI(Rect rect)
            {
                // Intentionally left empty.
            }

            public override Vector2 GetWindowSize()
            {
                return m_Size;
            }
        }

        public new class UxmlFactory : UxmlFactory<SearchPopupField, UxmlTraits>
        {
        }

        public new class UxmlTraits : BaseField<string>.UxmlTraits
        {
        }

        private class PopupTextElement : TextElement
        {
            protected override Vector2 DoMeasure(float desiredWidth, MeasureMode widthMode, float desiredHeight, MeasureMode heightMode)
            {
                string text = this.text;
                if (string.IsNullOrEmpty(text))
                {
                    text = " ";
                }
                return MeasureTextSize(text, desiredWidth, widthMode, desiredHeight, heightMode);
            }
        }

        private const string k_UssClassNameBasePopupField = "unity-base-popup-field";

        private const string k_TextUssClassNameBasePopupField = "unity-base-popup-field__text";

        private const string k_ArrowUssClassNameBasePopupField = "unity-base-popup-field__arrow";

        private const string k_LabelUssClassNameBasePopupField = "unity-base-popup-field__label";

        private const string k_InputUssClassNameBasePopupField = "unity-base-popup-field__input";

        private const string k_UssClassNamePopupField = "unity-popup-field";

        private const string k_LabelUssClassNamePopupField = "unity-popup-field__label";

        private const string k_InputUssClassNamePopupField = "unity-popup-field__input";

        private readonly TextElement m_Input;
        private VisualElement m_VisualInput;

        public Func<SearchPopupContent> getContent;

        public SearchPopupField()
            : this(null)
        {
        }

        public SearchPopupField(string label)
            : base(label, new VisualElement() { name = "SearchPopupVisualInput" })
        {
            AddToClassList("unity-base-popup-field");
            base.labelElement.AddToClassList("unity-base-popup-field__label");
            m_Input = new PopupTextElement
            {
                pickingMode = PickingMode.Ignore
            };
            m_Input.AddToClassList("unity-base-popup-field__text");
            m_VisualInput = this.Q("SearchPopupVisualInput");
            m_VisualInput.AddToClassList("unity-base-popup-field__input");
            m_VisualInput.Add(m_Input);
            VisualElement visualElement = new VisualElement();
            visualElement.AddToClassList("unity-base-popup-field__arrow");
            visualElement.pickingMode = PickingMode.Ignore;
            m_VisualInput.Add(visualElement);
            AddToClassList("unity-popup-field");
            base.labelElement.AddToClassList("unity-popup-field__label");
            m_VisualInput.AddToClassList("unity-popup-field__input");
        }

#if UNITY_2022_3_OR_NEWER
        [EventInterest(new Type[]
        {
        typeof(KeyDownEvent),
        typeof(MouseDownEvent)
        })]
#endif
        protected override void ExecuteDefaultActionAtTarget(EventBase evt)
        {
            base.ExecuteDefaultActionAtTarget(evt);
            if (evt == null)
            {
                return;
            }
            bool flag = false;
            if (evt is KeyDownEvent keyDownEvent)
            {
                if (keyDownEvent.keyCode == KeyCode.Space || keyDownEvent.keyCode == KeyCode.KeypadEnter || keyDownEvent.keyCode == KeyCode.Return)
                {
                    flag = true;
                }
            }
            else if (evt is MouseDownEvent { button: 0 } mouseDownEvent && m_VisualInput.ContainsPoint(m_VisualInput.WorldToLocal(mouseDownEvent.mousePosition)))
            {
                flag = true;
            }
            if (flag)
            {
                ShowMenu();
                evt.StopPropagation();
            }
        }

        private void ShowMenu()
        {
            WindowContent windowContent = new WindowContent();
            windowContent.onSelectionChanged += delegate (string selected)
            {
                value = selected;
            };
            SearchPopupContent popupContent = getContent?.Invoke() ?? default(SearchPopupContent);
            windowContent.Show(m_VisualInput.worldBound, value, popupContent.Items);
        }

        public override void SetValueWithoutNotify(string newValue)
        {
            base.SetValueWithoutNotify(newValue);
            ((INotifyValueChanged<string>)m_Input).SetValueWithoutNotify(value);
        }
    }
}
