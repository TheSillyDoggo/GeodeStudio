using GeodeIDE;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
this->setKeypadEnabled(true);
{generateInitCode(layer)}

return true;
}};

    void keyBackClicked()
{{
}};

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
    static {layer.name}* scene();
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

                            data += $"addChild(spr{n});\n";
                        }
                        break;

                    case CCLayer.CCNode.nType.Button:
                        if (node.spriteName != "")
                        {
                            data += generateSpriteCode(node, n);

                            data += $"auto btn{n} = CCMenuItemSpriteExtra::create(spr{n}, this, nullptr);\n";

                            data += $"addChild(btn{n});\n";
                        }
                        break;

                    default:
                        break;
                }

                n++;
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
                    data += $"auto spr{n} = CCScale9Sprite::createWithSpriteFrameName(\"{node.spriteName}.png\");\n";
                }
            }
            else
            {
                if (node.cclayer9sprite)
                {
                    data += $"auto spr{n} = CCSprite::create(\"{node.spriteName}.png\");\n";
                }
                else
                {
                    data += $"auto spr{n} = CCSprite::create(\"{node.spriteName}.png\");\n";
                }
            }

            return data;
        }
    }
}
