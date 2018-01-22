using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IAGrim.UI.Misc.CEF {
    class SetZoomEvent : EventArgs {
        public double ZoomLevel { get; set; }
    }
}
