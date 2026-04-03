#include "GameBootstrap.h"

#include "Config.h"
#include "MainMenuScene.h"

#include "ivx/ivx_manager.hpp"

USING_NS_CC;

Scene* GameBootstrap::createScene()
{
	return GameBootstrap::create();
}

bool GameBootstrap::init()
{
	if (!Scene::init())
		return false;

	auto label = Label::createWithSystemFont("Bootstrapping IVX…", "Arial", 22);
	label->setPosition(Director::getInstance()->getVisibleSize().width / 2,
	                   Director::getInstance()->getVisibleSize().height / 2);
	addChild(label);
	return true;
}

void GameBootstrap::onEnterTransitionDidFinish()
{
	Scene::onEnterTransitionDidFinish();

	ivx::IVXManager::shared()->configure(GameConfig::GAME_ID, GameConfig::SERVER_HOST,
	                                     GameConfig::SERVER_PORT, GameConfig::SERVER_KEY);
	const bool ok = ivx::IVXManager::shared()->authenticateGuest();
	if (ok)
	{
		ivx::IVXManager::shared()->loadHiroSystems();
		ivx::IVXManager::shared()->trackEvent("session_start", { { "game_id", GameConfig::GAME_ID } });
	}
	Director::getInstance()->replaceScene(MainMenuScene::createScene());
}
