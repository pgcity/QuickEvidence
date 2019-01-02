using System.Windows;
using System.Windows.Input;

namespace QuickEvidence.ViewModels
{
    public interface IGetPosition
    {
        Point GetPositionFromViewBox(MouseEventArgs arg);
        Point GetPositionFromScrollViewer(MouseEventArgs arg);
    }
}
