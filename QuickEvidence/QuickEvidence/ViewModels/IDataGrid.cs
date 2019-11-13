using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickEvidence.ViewModels
{
    public interface IDataGrid
    {
        bool ScrollToItem(FileItemViewModel item);

        void SetCurrentCell(FileItemViewModel item);

        void CommitEdit();

        void CancelEdit();
    }
}
