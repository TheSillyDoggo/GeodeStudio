using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace VisualGeode
{
    public class GeneralNodes
    {
        public class setAnimationInterval : NodeGraph.Node
        {
            public setAnimationInterval()
            {
                colour = new Avalonia.Media.Color(255, 230, 0, 0);
                name = "setAnimationInterval";
                description = "Set the interval that the game runs at. Also known as fps";
            }
        }

        public class setDisplayStats : NodeGraph.Node
        {
            public setDisplayStats()
            {
                colour = new Avalonia.Media.Color(255, 230, 0, 0);
                name = "setDisplayStats";
                description = "Set if it should display the fps and deltaTime (time between frames) in the bottom left";
            }
        }

        public class setPaused : NodeGraph.Node
        {
            public setPaused()
            {
                colour = new Avalonia.Media.Color(255, 230, 0, 0);
                name = "setPaused";
                description = "Set if the game runs it's update method";
            }
        }

        public class pushScene : NodeGraph.Node
        {
            public pushScene()
            {
                colour = new Avalonia.Media.Color(255, 230, 0, 0);
                name = "pushScene";
                description = "Switches to a scene with a transition";
            }
        }

        public class popScene : NodeGraph.Node
        {
            public popScene()
            {
                colour = new Avalonia.Media.Color(255, 230, 0, 0);
                name = "popScene";
                description = "Switches to the last scene";
            }
        }

        public class addScene : NodeGraph.Node
        {
            public addScene()
            {
                colour = new Avalonia.Media.Color(255, 230, 0, 0);
                name = "addLayer";
                description = "Adds a layer to the front of the current scene";
            }
        }


    }
}
