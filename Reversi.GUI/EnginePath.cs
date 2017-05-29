using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reversi.GUI
{
    /// <summary>
    /// 思考エンジンのパスを表すクラス
    /// </summary>
    public class EnginePath
    {
        public EnginePath() { }
        public EnginePath(string name,string path)
        {
            Name = name;
            Path = path;
        }
        public string Name { get; set; }
        public string Path { get; set; }
    }
}
