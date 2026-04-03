#include "StoreScene.h"
#include "MainMenuScene.h"

USING_NS_CC;

Scene* StoreScene::createScene() {
    return StoreScene::create();
}

bool StoreScene::init() {
    if (!Scene::init()) return false;
    
    Size visible = Director::getInstance()->getVisibleSize();
    Vec2 origin = Director::getInstance()->getVisibleOrigin();
    
    auto label = Label::createWithSystemFont("Store", "Arial", 36);
    label->setPosition(origin + Vec2(visible.width/2, visible.height/2));
    addChild(label);

    auto backBtn = MenuItemFont::create("Back", [](Ref*) {
        Director::getInstance()->replaceScene(MainMenuScene::createScene());
    });
    backBtn->setPosition(visible.width/2, 50);
    
    auto menu = Menu::create(backBtn, nullptr);
    menu->setPosition(Vec2::ZERO);
    addChild(menu);

    return true;
}
