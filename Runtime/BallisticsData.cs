using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OdemIdea.Ballistics
{
    [CreateAssetMenu(fileName = "MaterialsData", menuName = "ScriptableObjects/MaterialsData", order = 1)]
    public class BallisticsData : ScriptableObject
    {
        public BallisticsMaterial[] materials;
    }

    [System.Serializable]
    public struct BallisticsMaterial
    {
        public string name;
        [Tooltip("gr/cm")]
        public float density;

        public BallisticsMaterial(string _name, float _density)
        {
            name = _name;
            density = _density;
        }
    }
}
