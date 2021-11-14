using SimpleFileBrowser;
using System.Collections;
using System.IO;
using System.Linq;

namespace UI.FolderSelectors
{
    public class SimpleFolderSelector : FolderSelectorBase
    {
        public override void BrowseFolder()
        {
            StartCoroutine(ShowFolderSaveDialogCourutine());
        }

        IEnumerator ShowFolderSaveDialogCourutine()
        {
            yield return FileBrowser.WaitForLoadDialog(FileBrowser.PickMode.Folders, false, Value, "Select output directory", "Select");

            if (!FileBrowser.Success) yield break;
            Value = FileBrowser.Result.First();
        }
    }
}
