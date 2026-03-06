#include "ExampleScene.h"

USING_NS_CC;

Scene* ExampleScene::createScene() {
    return ExampleScene::create();
}

bool ExampleScene::init() {
    if (!Scene::init()) {
        return false;
    }

    auto visibleSize = Director::getInstance()->getVisibleSize();
    auto origin = Director::getInstance()->getVisibleOrigin();

    _statusLabel = Label::createWithSystemFont("IntelliVerseX Example", "Arial", 24);
    _statusLabel->setPosition(Vec2(
        origin.x + visibleSize.width / 2,
        origin.y + visibleSize.height / 2
    ));
    this->addChild(_statusLabel);

    this->scheduleUpdate();
    initializeSDK();

    return true;
}

void ExampleScene::update(float dt) {
    auto& mgr = IntelliVerseX::IVXManager::getInstance();
    if (mgr.isInitialized()) {
        mgr.tick();
    }
}

void ExampleScene::onExit() {
    this->unscheduleUpdate();
    Scene::onExit();
}

void ExampleScene::initializeSDK() {
    setStatus("Initializing SDK...");

    IntelliVerseX::IVXConfig config;
    config.nakamaHost = "127.0.0.1";
    config.nakamaPort = 7350;
    config.nakamaServerKey = "defaultkey";
    config.enableDebugLogs = true;

    auto& mgr = IntelliVerseX::IVXManager::getInstance();
    mgr.initialize(config);

    if (!mgr.isInitialized()) {
        setStatus("ERROR: SDK failed to initialize");
        return;
    }

    setStatus("Authenticating...");
    mgr.authenticateDevice("",
        [this]() { onAuthSuccess(); },
        [this](const IntelliVerseX::IVXError& err) { onError(err); }
    );
}

void ExampleScene::onAuthSuccess() {
    auto& mgr = IntelliVerseX::IVXManager::getInstance();
    std::string uid = mgr.getUserId();
    setStatus("Authenticated: " + uid + "\nFetching profile...");

    mgr.fetchProfile(
        [this](const IntelliVerseX::IVXProfile& profile) { onProfileLoaded(profile); },
        [this](const IntelliVerseX::IVXError& err) { onError(err); }
    );

    mgr.fetchWallet(
        [this](const std::string& wallet) { onWalletLoaded(wallet); },
        [this](const IntelliVerseX::IVXError& err) { onError(err); }
    );
}

void ExampleScene::onProfileLoaded(const IntelliVerseX::IVXProfile& profile) {
    std::string info =
        "Profile Loaded!\n"
        "User: " + profile.username + "\n"
        "Display: " + profile.displayName + "\n"
        "ID: " + profile.userId;
    setStatus(info);
    cocos2d::log("[ExampleScene] %s", info.c_str());
}

void ExampleScene::onWalletLoaded(const std::string& walletJson) {
    cocos2d::log("[ExampleScene] Wallet: %s", walletJson.c_str());
}

void ExampleScene::onError(const IntelliVerseX::IVXError& error) {
    std::string msg = "Error (" + std::to_string(error.code) + "): " + error.message;
    setStatus(msg);
    cocos2d::log("[ExampleScene] %s", msg.c_str());
}

void ExampleScene::setStatus(const std::string& text) {
    if (_statusLabel) {
        _statusLabel->setString(text);
    }
}
