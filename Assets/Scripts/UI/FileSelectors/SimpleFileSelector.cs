using SimpleFileBrowser;
using System.Collections;
using System.IO;
using System.Linq;

namespace UI.FileSelectors
{
    public class SimpleFileSelector : FileSelectorBase
    {
        public override void OpenDialog()
        {
            FileBrowser.SetFilters(true, new FileBrowser.Filter("Midi files", ".mid", ".midi"));
            FileBrowser.SetDefaultFilter(".mid");
            StartCoroutine(ShowLoadDialogCoroutine());
        }

        IEnumerator ShowLoadDialogCoroutine()
        {
            yield return FileBrowser.WaitForLoadDialog(FileBrowser.PickMode.Files, false, Path.GetDirectoryName(GetLastLocation()), "Open midi file", "Open");

            if (!FileBrowser.Success) yield break;
            var result = FileBrowser.Result.First();
            SaveLastLocation(result);
            Value = result;
        }
    }
}
