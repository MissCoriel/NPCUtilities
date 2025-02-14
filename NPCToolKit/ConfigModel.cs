using StardewModdingAPI.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPCToolKit
{
    class ConfigModel
    {
        public KeybindList GrabLocation { get; set; } = KeybindList.Parse("LeftControl + L");
        public KeybindList GetAllInfo { get; set; } = KeybindList.Parse("LeftControl + G");
    }
}
