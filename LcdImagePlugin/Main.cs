using HarmonyLib;
using Sandbox.Game.Entities.Blocks;
using System.Reflection;
using VRage.Plugins;

namespace avaness.LcdImagePlugin
{
    public class Main : IPlugin
    {
        public void Dispose()
        {

        }

        public void Init(object gameInstance)
        {
            Harmony harmony = new Harmony("LcdImagePlugin");
            harmony.PatchAll();
        }

        public void Update()
        {

        }

    }
}
