using Sirenix.OdinInspector;
using UnityEngine;

namespace GraveyardHunter.Data
{
    [System.Serializable]
    public class SkinData
    {
        public string SkinName;
        public int Price;
        public Color PrimaryColor;
        public Color SecondaryColor;

        [PreviewField]
        public Sprite Icon;

        public GameObject ModelPrefab;
    }
}
