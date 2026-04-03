#pragma once
#include "cocos2d.h"

class StoreScene : public cocos2d::Scene {
public:
    static cocos2d::Scene* createScene();
    bool init() override;
    CREATE_FUNC(StoreScene);
};
