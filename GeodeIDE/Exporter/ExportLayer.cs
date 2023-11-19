using GeodeIDE;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace VisualGeode.Exporter
{
    public class ExportLayer
    {
        public static string CreateLayerH(CCLayer layer)
        {
            string data = @$"#pragma once
#include <cocos2d.h>

using namespace geode::prelude;

class {layer.name} : public CCLayer {{
protected:

    bool init()
{{
auto dir = CCDirector::get();
this->setKeypadEnabled(true);
auto men = CCMenu::create();
men->setPosition({{0, 0}});

{generateInitCode(layer)}

addChild(men);
return true;
}};

    void keyBackClicked()
{{
    this->onGoBack(nullptr);
}};

void onGoBack(CCObject*) {{
    CCDirector::get()->popSceneWithTransition(0.5, PopTransition::kPopTransitionFade);
}}

public:
    static {layer.name}* create()
{{
auto ret = new {layer.name}();
            if (ret && ret->init()) {{
                ret->autorelease();
            }} else {{
                delete ret;
                ret = nullptr;
            }}
            return ret;
}};
    static CCScene* switchToScene()
{{
        auto scene = CCScene::create();
        scene->addChild({layer.name}::create());
        CCDirector::get()->pushScene(CCTransitionFade::create(.5f, scene));
        return scene;
    }};
}};";

            return data;

        }

        public static string generateInitCode(CCLayer layer)
        {
            string data = "";

            int n = 0;

            foreach (var node in layer.nodes)
            {
                switch (node.type)
                {
                    case CCLayer.CCNode.nType.Node:
                        if (node.spriteName != "")
                        {
                            data += generateSpriteCode(node, n);
                            data += generatePositionCode("spr" + n, node);

                            data += $"men->addChild(spr{n});\n";
                        }
                        break;

                    case CCLayer.CCNode.nType.Button:
                        if (node.spriteName != "")
                        {
                            data += generateSpriteCode(node, n);

                            data += $"auto btn{n} = CCMenuItemSpriteExtra::create(spr{n}, this, nullptr);\n";
                            data += generatePositionCode("btn" + n, node);

                            if (node.enabled)
                                data += $";\n";

                            data += $"men->addChild(btn{n});\n";
                        }
                        break;

                    default:
                        break;
                }

                n++;
            }

            return data;
        }

        public static string generatePositionCode(string obj, CCLayer.CCNode node)
        {
            string data = "";

            string x = (node.GetPositionForAlignment(false).X % 1) == 0 ? node.GetPositionForAlignment(false).X.ToString("0.0") : node.GetPositionForAlignment(false).X.ToString();
            string y = (node.GetPositionForAlignment(false).Y % 1) == 0 ? node.GetPositionForAlignment(false).Y.ToString("0.0") : node.GetPositionForAlignment(false).Y.ToString();

            //(dir->getWinSize().width - {node.size.Y + node.position.X}) / spr0->getTextureRect().size.width
            //120 / 61
            string w = ((node.size.X / 1920) % 1) == 0 ? (node.size.X / 1920).ToString("0.0") : (node.size.X / 1920).ToString();
            string h = ((node.size.Y / 1080) % 1) == 0 ? (node.size.Y / 1080).ToString("0.0") : (node.size.Y / 1080).ToString();

            switch (node.alignment)
            {
                case CCLayer.CCNode.Alignment.STRETCH:
                    data += $"{obj}->setScaleX((dir->getWinSize().width - {node.size.Y + node.position.X}) / spr0->getTextureRect().size.width - {node.size.X + node.position.Y});\n";
                    data += $"{obj}->setScaleY(dir->getWinSize().height / spr0->getTextureRect().size.height);\n";
                    data += $"{obj}->setAnchorPoint({{0,0}});\n";
                    data += $"{obj}->setPosition({{{node.position.X},{node.size.X}}});\n";
                    break;

                case CCLayer.CCNode.Alignment.BOTTOMLEFT:
                    //data += $"{obj}->setContentSize();\n";
                    data += $"{obj}->setAnchorPoint({{{node.anchor.X},{node.anchor.Y}}});\n";

                    data += $"{obj}->setContentSize({{{w}f * dir->getWinSize().width, {h}f * dir->getWinSize().height}});\n";

                    data += $"{obj}->setPosition({{{x}f * dir->getWinSize().width, {y}f * dir->getWinSize().height}});\n";
                    break;

                case CCLayer.CCNode.Alignment.TOPLEFT:
                    //data += $"{obj}->setContentSize();\n";
                    data += $"{obj}->setAnchorPoint({{{node.anchor.X},{node.anchor.Y}}});\n";

                    //data += $"{obj}->setContentSize({{{w}f * dir->getWinSize().width, {h}f * dir->getWinSize().height}});\n";
                    data += $"{obj}->setScaleX({w}f / {obj}->getContentSize().width * dir->getWinSize().width);\n";
                    data += $"{obj}->setScaleY({h}f / {obj}->getContentSize().height * dir->getWinSize().height);\n";

                    data += $"{obj}->setPosition({{{x}f * dir->getWinSize().width, dir->getWinSize().height - ({y}f * dir->getWinSize().height)}});\n";
                    break;

                case CCLayer.CCNode.Alignment.CENTER:
                    //data += $"{obj}->setContentSize();\n";
                    data += $"{obj}->setAnchorPoint({{{node.anchor.X},{node.anchor.Y}}});\n";
                    data += $"{obj}->setPosition({{dir->getWinSize().width / 2 + {node.GetPositionForAlignment(false).X}f * dir->getWinSize().width, dir->getWinSize().height / 2 + {node.GetPositionForAlignment(false).Y}f * dir->getWinSize().width}});\n";
                    break;

                default:
                    break;
            }

            return data;
        }

        public static string generateSpriteCode(CCLayer.CCNode node, int n)
        {
            string data = "";

            if (Program.gdSheetAssets.ContainsKey(node.spriteName))
            {
                if (node.cclayer9sprite)
                {
                    data += $"auto spr{n} = CCScale9Sprite::createWithSpriteFrameName(\"{node.spriteName}.png\");\n";
                }
                else
                {
                    data += $"auto spr{n} = CCSprite::createWithSpriteFrameName(\"{node.spriteName}.png\");\n";
                }
            }
            else
            {
                if (node.cclayer9sprite)
                {
                    data += $"auto spr{n} = CCScale9Sprite::create(\"{node.spriteName}.png\");\n";
                }
                else
                {
                    data += $"auto spr{n} = CCSprite::create(\"{node.spriteName}.png\");\n";
                }
            }

            data += $"spr{n}->setVisible({node.enabled.ToString().ToLower()});\n";

            return data;
        }
    }
}
