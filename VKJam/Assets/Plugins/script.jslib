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

    UnityPluginRequest_ShowInterstitialAd: function ()
    {
        JSRequest_ShowInterstitialAd();
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
        var temp = UTF8ToString(lobby_key);
        JSRequestInviteOldPlayer(id, temp);
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

    UnityPluginRequestCheckSubscriptionVKGroup: function () 
    {
        JSRequestCheckSubscriptionVKGroup();
    },

    UnityPluginIsMobilePlatform: function () 
    {
        JSIsMobilePlatform();
    },

    UnityPluginCopyTextToClipboard: function (text) 
    {
        var temp = UTF8ToString(text);
        JSCopyTextToClipboard(temp);
    }
});