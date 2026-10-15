namespace Domain.Models.Configurations;

/// <summary>
/// Settings for uploaded course documents, read from the "DocumentStorage" section in appsettings.json.
/// </summary>
public class DocumentStorageSettings
{
    public const string Section = "DocumentStorage";

    /// <summary>
    /// Folder where the files are saved. A relative path is resolved from the API's content root.
    /// It is deliberately outside wwwroot, so a file can only be fetched through the download endpoint
    /// and its role check, never by guessing a URL.
    /// </summary>
    public string UploadsPath { get; set; } = "Uploads";

    /// <summary>Largest file we accept, in megabytes.</summary>
    public int MaxFileSizeMb { get; set; } = 10;
}