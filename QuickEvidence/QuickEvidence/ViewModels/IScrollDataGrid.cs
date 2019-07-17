using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickEvidence.ViewModels
{
    public interface IScrollDataGrid
    {
        bool ScrollToItem(FileItemViewModel item);
    }
}
