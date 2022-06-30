using UnityEngine;
using UnityEditor;

namespace OdemIdea.Ballistics
{
    [CustomEditor(typeof(MaterialHolder))]
    public class MaterialHolderEditor : Editor
    {
        SerializedProperty materialName;

        void OnEnable()
        {
            materialName = serializedObject.FindProperty("materialName");
        }

        public override void OnInspectorGUI()
        {
            if (!BallisticsManager.initialized)
            {
                if (FindObjectsOfType(typeof(BallisticsManager)).Length == 0)
                {
                    EditorGUILayout.LabelField("I think you should setup your MaterialsData first");
                    Debug.LogWarning("You didn't have a ballistics manager, but it doesn't matter, we created it for you automatically");
                    GameObject go = new GameObject("BallisticsManager");
                    go.AddComponent<BallisticsManager>();
                }
                if (GUILayout.Button("Create data file"))
                {
                    BallisticsManager.Initialize();
                }
                return;
            }
            serializedObject.Update();
            string[] options = new string[BallisticsManager.instance.ballisticsData().materials.Length];
            if (options.Length == 0)
            {
                EditorGUILayout.LabelField("I think you should setup your MaterialsData to have some materials");
                
            }
            else
            {
                for (int i = 0; i < options.Length; i++)
                {
                    options[i] = BallisticsManager.instance.ballisticsData().materials[i].name;
                }

                int index = EditorGUILayout.Popup(BallisticsManager.GetIndexByName(materialName.stringValue), options);
                ((MaterialHolder)target).materialName = BallisticsManager.GetMaterial(index).name;
            }
            if (GUILayout.Button("Select data file"))
            {
                string[] guids = AssetDatabase.FindAssets("t:" + typeof(BallisticsData).Name);
                EditorUtility.FocusProjectWindow();
                Selection.activeObject = AssetDatabase.LoadAssetAtPath<BallisticsData>(AssetDatabase.GUIDToAssetPath(guids[0]));
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}
