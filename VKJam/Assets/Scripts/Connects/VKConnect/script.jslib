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

    UnityPluginRequestInviteOldPlayer: function (id, lobby_key) 
    {
        JSRequestInviteOldPlayer(id, lobby_key);
    },

    UnityPluginRequestRepost: function () 
    {
        JSRequestRepost();
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
});