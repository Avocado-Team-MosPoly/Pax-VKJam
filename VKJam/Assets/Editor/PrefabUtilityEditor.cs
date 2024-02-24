using UnityEditor;
using UnityEngine;

public static class PrefabUtilityEditor
{

    public static string test;
    [MenuItem("Tools/Configure Prefabs and Add to StoreSection")]
    public static void ConfigurePrefabsAndAddToStoreSection()
    {
        string[] folderPaths = new string[] 
        { 
            "Assets/Prefabs/Castom/Bestiary", "Assets/Prefabs/Castom/CandleLeft", "Assets/Prefabs/Castom/CandleRight", 
            "Assets/Prefabs/Castom/Clock", "Assets/Prefabs/Castom/Notebook", "Assets/Prefabs/Castom/Cauldron", 
            "Assets/Prefabs/Castom/Token" 
        };
        string[] sectionAssetPaths = new string[] 
        { 
            "Assets/Scripts/Shop/Data/Books.asset", "Assets/Scripts/Shop/Data/LeftItem.asset", "Assets/Scripts/Shop/Data/RightItem.asset", 
            "Assets/Scripts/Shop/Data/Watch.asset", "Assets/Scripts/Shop/Data/Books.asset", "Assets/Scripts/Shop/Data/Cauldron.asset",
            "Assets/Scripts/Shop/Data/Token.asset"
        };

        for (int i = 0; i < folderPaths.Length; i++)
        {
            string folderPath = folderPaths[i];
            StoreSection section = AssetDatabase.LoadAssetAtPath<StoreSection>(sectionAssetPaths[i]);

            if (section == null)
            {
                Debug.LogError("StoreSection not found at path: " + sectionAssetPaths[i]);
                continue;
            }

            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { folderPath });
            foreach (string guid in prefabGuids)
            {
                string prefabPath = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                DetecterModule Detec = prefab.GetComponent<DetecterModule>();
                if (Detec == null) { 
                    ModulePrefabSetter setter = prefab.GetComponent<ModulePrefabSetter>();
                
                    if (setter == null)
                    {
                        setter = prefab.AddComponent<ModulePrefabSetter>();
                        PrefabUtility.RecordPrefabInstancePropertyModifications(setter);
                    }
                
                    setter.ConfigureAndRemove();
                }

                WareData wareData = new WareData()
                {
                    Model = prefab
                };
                section.Add(wareData);
                EditorUtility.SetDirty(section);
            }
        }

        AssetDatabase.SaveAssets(); // ���������� ���� ���������
    }
}