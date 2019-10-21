using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sitecore.HttpStatus.Windows.ViewModel
{
    public interface IFolder
    {
        string ContentPath { get; }
        string FolderLabel { get; }
        List<IFolder> Folders { get; }
    }
}
