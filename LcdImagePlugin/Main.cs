using HarmonyLib;
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
            new Harmony("avaness.LcdImagePlugin").PatchAll(Assembly.GetExecutingAssembly());
        }

        public void Update()
        {

        }

    }
}
