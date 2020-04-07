using System;
namespace ApacheLogViewer
{
    public partial class LogViewerWindow : Gtk.Window
    {
        public LogViewerWindow() :
                base(Gtk.WindowType.Toplevel)
        {
            this.Build();
        }
    }
}
