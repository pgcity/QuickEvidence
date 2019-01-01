using System.Windows;
using System.Windows.Input;

namespace QuickEvidence.ViewModels
{
    public interface IGetPosition
    {
        Point GetPosition(MouseEventArgs arg);
    }
}
