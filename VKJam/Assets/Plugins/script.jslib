mergeInto(LibraryManager.library, {
    UnityPluginRequestJs: function () 
    {
        FromUnityToJs();
    },

    UnityPluginRequestRepost: function () 
    {
        JSRequestRepost();
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

    UnityPluginRequestInviteOldPlayer: function (id, lobby_key) 
    {
        JSRequestInviteOldPlayer(id, lobby_key);
    },

    UnityPluginRequestBuyTry: function (id) 
    {
        JSRequestBuyTry(id);
    },

    UnityPluginRequestGetFriends: function () 
    {
        JSRequestGetFriends();
    },

    UnityPluginRequestJoinGroup: function () 
    {
        JSRequestJoinGroup();
    },

    UnityPluginIsMobilePlatform: function () 
    {
        JSIsMobilePlatform();
    }
});