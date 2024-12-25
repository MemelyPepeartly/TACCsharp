using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TACCsharp.TACC.Models
{
    public struct SceneData
    {
        public string Character { get; set; }
        public string Dialogue { get; set; }
        public string Portrait { get; set; }
        public string Background { get; set; }
        public float Duration { get; set; }
    }

    public struct CutsceneData
    {
        public string CutsceneName { get; set; }
        public SceneData[] Scenes { get; set; }
    }
}
