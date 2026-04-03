#pragma once

#include "cocos2d.h"

/**
 * Entry scene: configures IVX, authenticates, then opens main menu.
 */
class GameBootstrap : public cocos2d::Scene
{
public:
	static cocos2d::Scene* createScene();

	bool init() override;
	void onEnterTransitionDidFinish() override;

	CREATE_FUNC(GameBootstrap);

private:
	void goMainMenu();
};
