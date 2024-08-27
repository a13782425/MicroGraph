//using UnityEditor;

//namespace MicroGraph.Editor
//{
//    [CustomEditor(typeof(MicroGraphConfigModel))]
//    internal class MicroGraphConfigModelInspector : UnityEditor.Editor
//    {
//        public override void OnInspectorGUI()
//        {
//            EditorGUI.BeginChangeCheck();
//            serializedObject.UpdateIfRequiredOrScript();
//            SerializedProperty iterator = serializedObject.GetIterator();
//            bool enterChildren = true;
//            while (iterator.NextVisible(enterChildren))
//            {
//                using (new EditorGUI.DisabledScope("EditorFrame" != iterator.propertyPath))
//                {
//                    EditorGUILayout.PropertyField(iterator, true);
//                }

//                enterChildren = false;
//            }

//            serializedObject.ApplyModifiedProperties();
//            EditorGUI.EndChangeCheck();
//            //using (new EditorGUI.DisabledScope(true))
//            //{
//            //    base.OnInspectorGUI();
//            //}
//        }
//    }
//}