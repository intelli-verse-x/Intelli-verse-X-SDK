var ApplixirPlugin = {

    adStatusCallbackX: function (status) {
        if (window.applixirVerbosity > 2) {
            console.log('[ApplixirWebGL] Ad Status:', status);
        }
        
        var buffer = stringToNewUTF8(status.type);
        {{{ makeDynCall('vi', 'window.applixirCallback') }}} (buffer);
    },
    
    adErrorCallbackX: function (error) {
            if (window.applixirVerbosity > 2) {
                console.log('[ApplixirWebGL] Ad Error:', error);
            }
            
            var buffer = stringToNewUTF8(error.toString());
            {{{ makeDynCall('vi', 'window.applixirCallback') }}} (buffer);
    },

    ShowVideo__deps: [
        'adStatusCallbackX',
        'adErrorCallbackX'
    ],
    
    ShowVideo: function (userId, callback, errorCallback, verbosity) {
        console.log(userId);
        if(window.ApplixirApp == null){
            var buffer = stringToNewUTF8("initialisationFailed");
            {{{ makeDynCall('vi', 'callback') }}} (buffer);
            return; 
        }
        window.applixirCallback = callback;
        window.applixirErrorCallback = errorCallback;
        window.applixirVerbosity = verbosity;
        window.adStatusCallbackX = _adStatusCallbackX;
        window.adErrorCallbackX = _adErrorCallbackX;
        window.ApplixirApp.openPlayer();
    }
};

mergeInto(LibraryManager.library, ApplixirPlugin);