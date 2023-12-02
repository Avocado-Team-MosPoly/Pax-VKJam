mergeInto(LibraryManager.library, {
    UnityPluginRequestJs: function () 
    {
        FromUnityToJs();
    },

    UnityPluginRequestUserData: function () 
    {
        JSRequestUserData();
    },

    UnityPluginRequestAds: function () 
    {
        JSRequestShowAds();
    },

    UnityPluginRequestInviteNewPlayer: function () 
    {
        JSRequestInviteNewPlayer();
    },

    UnityPluginRequestInviteOldPlayer: function () 
    {
        JSRequestInviteOldPlayer();
    },

    UnityPluginRequestRepost: function () 
    {
        JSRequestRepost();
    },

    UnityPluginRequestBuyTry: function (id) 
    {
        JSRequestBuyTry(id);
    }
});