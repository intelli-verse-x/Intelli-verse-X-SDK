// ============================================================================
// IVXWebGLAds.jslib
// WebGL JavaScript plugin for IntelliVerse-X SDK Ads
// 
// Copyright (c) IntelliVerseX. All rights reserved.
// Version: 2.0.0
// 
// Supports:
// - Google AdSense (display, banner, interstitial)
// - Applixir (rewarded video - bridges to existing ApplixirSDK)
// ============================================================================

var IVXWebGLAdsPlugin = {

    // ========================================================================
    // AdSense Functions
    // ========================================================================

    InitializeAdSenseJS: function(publisherIdPtr, autoAds) {
        var publisherId = UTF8ToString(publisherIdPtr);
        
        console.log('[IVXWebGLAds] Initializing AdSense:', publisherId);
        
        // Store config
        window.ivxAdsensePublisherId = publisherId;
        window.ivxAdsenseAutoAds = autoAds;
        window.ivxAdContainers = {};
        
        // Load AdSense script if not already loaded
        if (!window.ivxAdsenseLoaded) {
            var script = document.createElement('script');
            script.src = 'https://pagead2.googlesyndication.com/pagead/js/adsbygoogle.js?client=' + publisherId;
            script.async = true;
            script.crossOrigin = 'anonymous';
            script.onload = function() {
                window.ivxAdsenseLoaded = true;
                console.log('[IVXWebGLAds] AdSense script loaded');
            };
            script.onerror = function() {
                console.error('[IVXWebGLAds] Failed to load AdSense script');
            };
            document.head.appendChild(script);
        }
    },

    ShowAdSenseBannerJS: function(unitNamePtr, slotIdPtr, sizePtr) {
        var unitName = UTF8ToString(unitNamePtr);
        var slotId = UTF8ToString(slotIdPtr);
        var size = UTF8ToString(sizePtr);
        
        console.log('[IVXWebGLAds] Showing AdSense banner:', unitName, slotId, size);
        
        // Remove existing container if any
        var existingContainer = document.getElementById('ivx-ad-' + unitName);
        if (existingContainer) {
            existingContainer.remove();
        }
        
        // Create ad container
        var container = document.createElement('div');
        container.id = 'ivx-ad-' + unitName;
        container.style.cssText = 'position:fixed;z-index:9999;left:50%;transform:translateX(-50%);';
        
        // Position based on unit name
        if (unitName.toLowerCase().includes('top')) {
            container.style.top = '0';
        } else {
            container.style.bottom = '0';
        }
        
        // Create ins element for AdSense
        var ins = document.createElement('ins');
        ins.className = 'adsbygoogle';
        ins.style.cssText = 'display:block;';
        ins.setAttribute('data-ad-client', window.ivxAdsensePublisherId);
        ins.setAttribute('data-ad-slot', slotId);
        
        if (size === 'auto') {
            ins.setAttribute('data-ad-format', 'auto');
            ins.setAttribute('data-full-width-responsive', 'true');
        } else {
            var dimensions = size.split('x');
            if (dimensions.length === 2) {
                ins.style.width = dimensions[0] + 'px';
                ins.style.height = dimensions[1] + 'px';
            }
        }
        
        container.appendChild(ins);
        document.body.appendChild(container);
        window.ivxAdContainers[unitName] = container;
        
        // Push ad
        try {
            (window.adsbygoogle = window.adsbygoogle || []).push({});
            console.log('[IVXWebGLAds] AdSense banner pushed');
        } catch (e) {
            console.error('[IVXWebGLAds] AdSense push error:', e);
        }
    },

    HideAdSenseBannerJS: function(unitNamePtr) {
        var unitName = UTF8ToString(unitNamePtr);
        
        console.log('[IVXWebGLAds] Hiding AdSense banner:', unitName);
        
        var container = window.ivxAdContainers ? window.ivxAdContainers[unitName] : null;
        if (container) {
            container.style.display = 'none';
        }
        
        var element = document.getElementById('ivx-ad-' + unitName);
        if (element) {
            element.style.display = 'none';
        }
    },

    RefreshAdSenseBannerJS: function(unitNamePtr) {
        var unitName = UTF8ToString(unitNamePtr);
        
        console.log('[IVXWebGLAds] Refreshing AdSense banner:', unitName);
        
        // Re-push ad for refresh
        try {
            (window.adsbygoogle = window.adsbygoogle || []).push({});
        } catch (e) {
            console.error('[IVXWebGLAds] AdSense refresh error:', e);
        }
    },

    ShowAdSenseInterstitialJS: function(unitNamePtr, slotIdPtr, rewarded) {
        var unitName = UTF8ToString(unitNamePtr);
        var slotId = UTF8ToString(slotIdPtr);
        
        console.log('[IVXWebGLAds] Showing AdSense interstitial:', unitName, rewarded ? '(rewarded)' : '');
        
        // AdSense doesn't have true interstitials for web
        // We simulate with a large overlay ad
        var overlay = document.createElement('div');
        overlay.id = 'ivx-interstitial-overlay';
        overlay.style.cssText = 'position:fixed;top:0;left:0;width:100%;height:100%;background:rgba(0,0,0,0.9);z-index:99999;display:flex;flex-direction:column;align-items:center;justify-content:center;';
        
        // Close button
        var closeBtn = document.createElement('button');
        closeBtn.textContent = rewarded ? 'Watch to earn reward' : 'Close';
        closeBtn.style.cssText = 'position:absolute;top:10px;right:10px;padding:10px 20px;font-size:16px;cursor:pointer;background:#333;color:white;border:none;border-radius:5px;';
        
        if (rewarded) {
            closeBtn.disabled = true;
            var countdown = 5;
            var timer = setInterval(function() {
                countdown--;
                closeBtn.textContent = 'Close in ' + countdown + 's';
                if (countdown <= 0) {
                    clearInterval(timer);
                    closeBtn.textContent = 'Close & Get Reward';
                    closeBtn.disabled = false;
                    closeBtn.onclick = function() {
                        overlay.remove();
                        // Notify Unity
                        if (window.unityInstance) {
                            window.unityInstance.SendMessage('IVXWebGLAdsCallback', 'OnAdSenseInterstitialCompleted', unitName + ':true');
                        }
                    };
                }
            }, 1000);
        } else {
            closeBtn.onclick = function() {
                overlay.remove();
                if (window.unityInstance) {
                    window.unityInstance.SendMessage('IVXWebGLAdsCallback', 'OnAdSenseInterstitialCompleted', unitName + ':true');
                }
            };
        }
        
        // Ad container
        var adContainer = document.createElement('div');
        adContainer.style.cssText = 'width:90%;max-width:728px;height:auto;background:#fff;padding:20px;border-radius:10px;';
        
        var ins = document.createElement('ins');
        ins.className = 'adsbygoogle';
        ins.style.cssText = 'display:block;width:100%;height:250px;';
        ins.setAttribute('data-ad-client', window.ivxAdsensePublisherId);
        ins.setAttribute('data-ad-slot', slotId);
        ins.setAttribute('data-ad-format', 'rectangle');
        
        adContainer.appendChild(ins);
        overlay.appendChild(closeBtn);
        overlay.appendChild(adContainer);
        document.body.appendChild(overlay);
        
        try {
            (window.adsbygoogle = window.adsbygoogle || []).push({});
        } catch (e) {
            console.error('[IVXWebGLAds] AdSense interstitial error:', e);
        }
    },

    // ========================================================================
    // Applixir Functions
    // ========================================================================

    InitializeApplixirJS: function(zoneIdPtr, testMode) {
        var zoneId = UTF8ToString(zoneIdPtr);
        
        console.log('[IVXWebGLAds] Initializing Applixir:', zoneId, testMode ? '(test mode)' : '');
        
        window.ivxApplixirZoneId = zoneId;
        window.ivxApplixirTestMode = testMode;
        window.ivxApplixirReady = false;
        
        // Applixir SDK should be loaded separately via their script
        // We just store the config and check if it's ready
        if (window.ApplixirApp) {
            window.ivxApplixirReady = true;
            console.log('[IVXWebGLAds] Applixir SDK already loaded');
        } else {
            // Try to load Applixir SDK
            console.log('[IVXWebGLAds] Waiting for Applixir SDK...');
            
            // Check periodically for SDK load
            var checkInterval = setInterval(function() {
                if (window.ApplixirApp) {
                    window.ivxApplixirReady = true;
                    clearInterval(checkInterval);
                    console.log('[IVXWebGLAds] Applixir SDK detected');
                }
            }, 1000);
            
            // Stop checking after 30 seconds
            setTimeout(function() {
                clearInterval(checkInterval);
            }, 30000);
        }
    },

    ShowApplixirRewardedAdJS: function(unitNamePtr, skipDelay) {
        var unitName = UTF8ToString(unitNamePtr);
        
        console.log('[IVXWebGLAds] Showing Applixir rewarded ad:', unitName);
        
        window.ivxCurrentApplixirUnit = unitName;
        
        if (!window.ApplixirApp) {
            console.error('[IVXWebGLAds] Applixir SDK not loaded');
            // Notify Unity of failure
            if (window.unityInstance) {
                window.unityInstance.SendMessage('IVXWebGLAdsCallback', 'OnApplixirAdCompleted', unitName + ':false');
            }
            return;
        }
        
        try {
            // Set up callbacks
            window.ivxApplixirAdCallback = function(status) {
                console.log('[IVXWebGLAds] Applixir ad status:', status);
                
                var success = (status === 'ad-watched' || status === 'ad-rewarded');
                
                if (window.unityInstance) {
                    window.unityInstance.SendMessage('IVXWebGLAdsCallback', 'OnApplixirAdCompleted', unitName + ':' + success);
                }
            };
            
            // Open Applixir player
            window.ApplixirApp.openPlayer();
            
        } catch (e) {
            console.error('[IVXWebGLAds] Applixir show error:', e);
            if (window.unityInstance) {
                window.unityInstance.SendMessage('IVXWebGLAdsCallback', 'OnApplixirAdCompleted', unitName + ':false');
            }
        }
    },

    IsApplixirAdReadyJS: function() {
        return window.ApplixirApp != null && window.ivxApplixirReady;
    }
};

mergeInto(LibraryManager.library, IVXWebGLAdsPlugin);
