using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Reversi.GUI
{
    public class EngineManager
    {
        public Dictionary<string, ThinkingEngineBase.IThinkingEngine> EngineMap { get; } = new Dictionary<string, ThinkingEngineBase.IThinkingEngine>();
        public Dictionary<string,string> pathMap = new Dictionary<string,string>();
        /// <summary>
        /// リフレクションによるプラグインライブラリの読み込み
        /// </summary>
        /// <param name="path"></param>
        public void Register(string path)
        {
            #region 参考 http://...
            if (path == "")
                return;
            var asm = Assembly.LoadFrom(path);
            var types = asm.GetTypes();
            foreach (var t in asm.GetTypes())
            {
                if (t.IsInterface) continue;
                if (t.IsAbstract) continue;
                var engine = Activator.CreateInstance(t) as ThinkingEngineBase.IThinkingEngine;
                if (engine != null)
                {
                    EngineMap.Add(engine.Name, engine);
                    pathMap.Add(engine.Name, path);
                    break;
                }
            }
            #endregion
        }
        public void SaveToFile(string path)
        {
            System.IO.File.WriteAllText(path,string.Join(",\n",pathMap.Values));
        }
        public static EngineManager FromFile(string path)
        {
            var array = System.IO.File.ReadAllText(path).Replace("\n","").Split(',');
            var manager = new EngineManager();
            foreach (var item in array)
            {
                manager.Register(item);
            }
            return manager;
        }
    }
}
