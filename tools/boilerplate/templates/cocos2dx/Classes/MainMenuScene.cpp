#include "MainMenuScene.h"

#include "Config.h"
#include "StoreScene.h"
#include "AchievementsScene.h"
#include "DailyRewardsScene.h"
#include "LeaderboardScene.h"
#include "SettingsScene.h"
#include "EnergyScene.h"

USING_NS_CC;

Scene* MainMenuScene::createScene()
{
	return MainMenuScene::create();
}

bool MainMenuScene::init()
{
	if (!Scene::init())
		return false;

	Size visible = Director::getInstance()->getVisibleSize();
	Vec2 origin = Director::getInstance()->getVisibleOrigin();

	auto wallet = Label::createWithSystemFont(
		StringUtils::format("Wallet · coins %lld · gems %lld",
		                    static_cast<long long>(GameConfig::INITIAL_COINS),
		                    static_cast<long long>(GameConfig::INITIAL_GEMS)),
		"Arial", 20);
	wallet->setAnchorPoint(Vec2(0, 1));
	wallet->setPosition(origin + Vec2(16, visible.height - 16));
	addChild(wallet);

	Vector<MenuItem*> items;
	float y = origin.y + visible.height / 2 + 80.f;

	auto createTab = [&](const std::string& name, float py, const std::function<void()>& onClick) {
	    auto btn = MenuItemFont::create(name, [onClick](Ref*) { onClick(); });
	    btn->setPosition(visible.width / 2, py);
	    items.pushBack(btn);
	};

	createTab("Home", y, [](){});
	createTab("Store", y - 40.f, [](){ Director::getInstance()->replaceScene(StoreScene::createScene()); });
	createTab("Achievements", y - 80.f, [](){ Director::getInstance()->replaceScene(AchievementsScene::createScene()); });
	createTab("Daily", y - 120.f, [](){ Director::getInstance()->replaceScene(DailyRewardsScene::createScene()); });
	createTab("Leaderboard", y - 160.f, [](){ Director::getInstance()->replaceScene(LeaderboardScene::createScene()); });
	createTab("Settings", y - 200.f, [](){ Director::getInstance()->replaceScene(SettingsScene::createScene()); });
	createTab("Energy", y - 240.f, [](){ Director::getInstance()->replaceScene(EnergyScene::createScene()); });

	auto menu = Menu::createWithArray(items);
	menu->setPosition(Vec2::ZERO);
	addChild(menu);

	return true;
}
