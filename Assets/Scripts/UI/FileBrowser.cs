using System;
using System.IO;
using TMPro;
using UnityEngine;

public delegate void FileOpenEventHandler(string path, Vector3 pos);
public class FileBrowser : MonoBehaviour
{
    public enum Usage
    {
        ModelImport,
        TexturePicker
    };
    public Usage usage = Usage.ModelImport;

    public GameObject fileListingPrefab;
    public Transform fileListingsParent;
    public TextMeshProUGUI pathLabel;

    private string _transferDir;

    private string _selectedFileName;
    private string _currentPath;
    public string CurrentPath
    {
        get => _currentPath;
        set
        {
            _currentPath = value;
            if (string.IsNullOrEmpty(_currentPath))
            {
                pathLabel.text = "NULL";
                return;
            }
            // Update the file listing
            // Clear out the old listings
            foreach (Transform child in fileListingsParent)
            {
                Destroy(child.gameObject);
            }
            if (value == "/storage/emulated/0/Download")
            {
                FileListing f1 = Instantiate(fileListingPrefab, fileListingsParent).GetComponent<FileListing>();
                f1.FileName = "(Coming soon)";
                FileListing f2 = Instantiate(fileListingPrefab, fileListingsParent).GetComponent<FileListing>();
                f2.FileName = "(I need to figure out how to handle the permissions.)";
                FileListing f3 = Instantiate(fileListingPrefab, fileListingsParent).GetComponent<FileListing>();
                f3.FileName = "(For now you need to use something else to move your files from Downloads to the app's TransferDirectory)";
                return;
            }
            DirectoryInfo directory = new(value);
            // We want both files and folders to show
            FileSystemInfo[] fileSystemInfos = directory.GetFileSystemInfos();
            foreach (FileSystemInfo fileSystemInfo in fileSystemInfos)
            {
                GameObject newListing = Instantiate(fileListingPrefab, fileListingsParent);
                FileListing fileListing = newListing.GetComponent<FileListing>();
                fileListing.FileName = fileSystemInfo.Name;
                if ((fileSystemInfo.Attributes & FileAttributes.Directory) == FileAttributes.Directory)
                {
                    // It's a directory
                    fileListing.Selected += (sender, e) =>
                    {
                        CurrentPath = Path.Combine(CurrentPath, fileListing.FileName);
                    };
                    fileListing.iconComponent.sprite = fileListing.folderIcon;
                }
                else
                {
                    // It's a file
                    // Subscribe to event so we know when button is pressed
                    fileListing.Selected += onFileListingSelected;
                    if (ModelLoadingService.IsRecognizedFormat(Path.GetExtension(fileListing.FileName)))
                    {
                        fileListing.iconComponent.sprite = fileListing.fileIcon;
                    }
                }


            }
            pathLabel.text = value;
        }
    }
    public string FullFilePath
    {
        get
        {
            return Path.Combine(CurrentPath, _selectedFileName);
        }
    }

    public event FileOpenEventHandler FileOpen;

    private void onFileListingSelected(object sender, EventArgs e)
    {
        var fileListing = (FileListing)sender;
        _selectedFileName = fileListing.FileName;
        // Highlight this button and unhighlight the others
        foreach (Transform child in fileListingsParent)
        {
            FileListing fl = child.GetComponent<FileListing>();
            if (fl == fileListing)
            {
                fl.IsHighlighted = true;
            }
            else
            {
                fl.IsHighlighted = false;
            }
        }
    }

    void Start()
    {
        GoToTransferDirectory();
    }

    void Update()
    {

    }

    public void GoToParentDirectory()
    {
        DirectoryInfo directory = new(CurrentPath);
        DirectoryInfo parentDirectory = directory.Parent;
        CurrentPath = parentDirectory.FullName;
    }

    public void GoToTransferDirectory()
    {
        _transferDir = Path.Combine(Application.persistentDataPath, Services.TransferDirName);
        string targetDirectory = _transferDir;
        if (!Directory.Exists(targetDirectory))
        {
            Directory.CreateDirectory(targetDirectory);
            Debug.Log("Created new directory: " + targetDirectory);
        }
        CurrentPath = targetDirectory;
    }

    public void GoToDownloadsDirectory()
    {
        string targetDir = "/storage/emulated/0/Download";
        if (Directory.Exists(targetDir))
        {
            CurrentPath = targetDir;
        }
        else
        {
            Debug.LogError($"weird, {targetDir} doesn't exist");
        }
    }

    public void OpenSelectedFile()
    {
        string fullFilePath = Path.Combine(CurrentPath, _selectedFileName);
        Debug.Log($"Opening file: {fullFilePath}");
        // let listener know what file was selected.
        FileOpen?.Invoke(fullFilePath, transform.position);
    }
}
