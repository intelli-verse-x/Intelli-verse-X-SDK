#pragma once
#include "cocos2d.h"

class EnergyScene : public cocos2d::Scene {
public:
    static cocos2d::Scene* createScene();
    bool init() override;
    CREATE_FUNC(EnergyScene);
};
