#pragma once

#include "cocos2d.h"

/**
 * Hub UI: tab buttons, wallet label, placeholder feature panels.
 */
class MainMenuScene : public cocos2d::Scene
{
public:
	static cocos2d::Scene* createScene();

	bool init() override;

	CREATE_FUNC(MainMenuScene);
};
