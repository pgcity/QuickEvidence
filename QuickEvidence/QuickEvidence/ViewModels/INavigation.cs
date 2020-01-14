using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickEvidence.ViewModels
{
    public delegate bool FileNameCheckFunc(string fileName, int startNo, out string Message);

    public interface INavigation
    {
        FileRenameWindowViewModel RenameMultipleFiles(string defaultFileName, FileNameCheckFunc checkFunc);
    }
}
