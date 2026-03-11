#pragma once

#include "cocos2d.h"
#include "IntelliVerseX/IVXManager.h"
#include "IntelliVerseX/IVXTypes.h"

/**
 * Example Cocos2d-x scene demonstrating IntelliVerseX SDK usage:
 *   - Initializes the SDK
 *   - Authenticates with a device ID
 *   - Fetches and displays the player profile
 *
 * Add this scene to your AppDelegate or scene manager to test.
 */
class ExampleScene : public cocos2d::Scene {
public:
    static cocos2d::Scene* createScene();

    virtual bool init() override;
    virtual void update(float dt) override;
    virtual void onExit() override;

    CREATE_FUNC(ExampleScene);

private:
    cocos2d::Label* _statusLabel = nullptr;

    void initializeSDK();
    void onAuthSuccess();
    void onProfileLoaded(const IntelliVerseX::IVXProfile& profile);
    void onWalletLoaded(const std::string& walletJson);
    void onError(const IntelliVerseX::IVXError& error);
    void setStatus(const std::string& text);
};
