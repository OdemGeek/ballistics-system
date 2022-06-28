using System.Collections;
using System.Collections.Generic;
using UnityEditor;
#if UNITY_EDITOR
using UnityEngine;
#endif

namespace OdemIdea.Ballistics
{
    [AddComponentMenu("Ballistics/BallisticsManager")]
    public class BallisticsManager : MonoBehaviour
    {
        [SerializeField]
        private BallisticsData m_ballisticsData;
        public bool debug = true;
        public static BallisticsManager instance;

        public static bool initialized
        {
            get 
            { 
                if (instance == null) return false;
                return instance.m_ballisticsData != null;
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            instance = this;
            if (m_ballisticsData != null) return;
            Initialize();
        }

        public static void Initialize()
        {
            string[] guids = AssetDatabase.FindAssets("t:" + typeof(BallisticsData).Name);
            if (guids.Length > 0)
            {
                instance.m_ballisticsData = AssetDatabase.LoadAssetAtPath<BallisticsData>(AssetDatabase.GUIDToAssetPath(guids[0]));
            }
            else
            {
                BallisticsData asset = ScriptableObject.CreateInstance<BallisticsData>();
                asset.materials = new BallisticsMaterial[]
                {
                    new BallisticsMaterial() { name = "Default", density = 14f},
                    new BallisticsMaterial() { name = "Wood", density = 0.8f},
                    new BallisticsMaterial() { name = "Steel", density = 7.8f}
                };

                AssetDatabase.CreateAsset(asset, "Assets/MaterialsData.asset");
                AssetDatabase.SaveAssets();

                //EditorUtility.FocusProjectWindow();

                //Selection.activeObject = asset;

                instance.m_ballisticsData = asset;
            }
        }
#endif

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (instance != this)
            {
                DestroyImmediate(instance);
            }
        }

        public BallisticsData ballisticsData()
        {
            return m_ballisticsData;
        }

        public static int GetIndexByName(string Name)
        {
            return instance.GetIndexByNameP(Name);
        }

        private int GetIndexByNameP(string Name)
        {
            for (int i = 0; i < m_ballisticsData.materials.Length; i++)
            {
                if (m_ballisticsData.materials[i].name == Name) return i;
            }
            return -1;
        }

        public static BallisticsMaterial GetMaterial(string Name)
        {
            BallisticsMaterial? bm = instance.GetMaterialP(Name);
            return bm == null ? GetMaterial(0) : bm.Value;
        }

        private BallisticsMaterial? GetMaterialP(string Name)
        {
            foreach (BallisticsMaterial m in m_ballisticsData.materials)
            {
                if (m.name == Name)
                {
                    return m;
                }
            }
            return null;
        }

        public static BallisticsMaterial GetMaterial(int id)
        {
            return instance.GetMaterialP(id);
        }

        private BallisticsMaterial GetMaterialP(int id)
        {
            id = Mathf.Clamp(id, 0, m_ballisticsData.materials.Length - 1);
            return m_ballisticsData.materials[id];
        }
    }
}
