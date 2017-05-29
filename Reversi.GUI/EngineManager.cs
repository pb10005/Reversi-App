using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;
using System.Xml.Serialization;

namespace Reversi.GUI
{
    /// <summary>
    /// 思考エンジンを管理するクラス
    /// </summary>
    public class EngineManager
    {
        public Dictionary<string, ThinkingEngineBase.IThinkingEngine> EngineMap { get; } = new Dictionary<string, ThinkingEngineBase.IThinkingEngine>();
        public List<EnginePath> pathList = new List<EnginePath>();
        /// <summary>
        /// リフレクションによるプラグインライブラリの読み込み
        /// </summary>
        /// <param name="path"></param>
        public void Register(string path)
        {
            #region 参考 http://qiita.com/rita0222/items/609583c31cb7f0132086
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
                    pathList.Add(new EnginePath(engine.Name, path));
                    break;
                }
            }
            #endregion
        }
        public void Remove(string name)
        {
            pathList.RemoveAll(x=>x.Name==name);
            EngineMap.Remove(name);
        }
        public void SaveToFile(string path)
        {
            using(var stream = new FileStream(path,FileMode.OpenOrCreate))
            {
                var serializer = new XmlSerializer(typeof(List<EnginePath>));
                serializer.Serialize(stream,pathList);
            }
        }
        public static EngineManager FromFile(string path)
        {
            using(var stream = new FileStream(path, FileMode.Open))
            {
                var manager = new EngineManager();
                var serializer = new XmlSerializer(typeof(List<EnginePath>));
                var list = (List<EnginePath>)serializer.Deserialize(stream);
                foreach (var item in list)
                {
                    manager.Register(item.Path);
                }
                return manager;
            }
            
        }
    }
}
