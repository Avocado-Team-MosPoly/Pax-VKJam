mergeInto(LibraryManager.library, 
    {
        //  Структура названий функций: UnityPlugin + Название функции в Unity
            UnityPluginRequestJs: function () 
            {
                FromUnityToJs ();
            }

            
    }
);
mergeInto(LibraryManager.library, 
    {
        //  Структура названий функций: UnityPlugin + Название функции в Unity
            UnityPluginRequestUserData: function () 
            {
                JSRequestUserData ();
            }

            
    }
);