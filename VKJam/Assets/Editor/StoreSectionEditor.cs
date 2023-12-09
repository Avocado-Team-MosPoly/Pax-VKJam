using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(storeSection))]
public class StoreSectionEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        storeSection section = (storeSection)target;
        if (section.products != null)
        {
            for (int i = 0; i < section.products.Count; i++)
            {
                WareData wareData = section.products[i];

                GUILayout.BeginHorizontal();

                GUILayout.Label(wareData.Data.productName);

                if (GUILayout.Button("Set in Choosen Custom"))
                {
                    wareData.ChooseThis();
                }

                GUILayout.EndHorizontal();
            }
        }
    }
}