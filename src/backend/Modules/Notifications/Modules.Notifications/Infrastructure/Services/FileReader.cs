using System.Reflection;
using Modules.Notifications.Interfaces;

namespace Modules.Notifications.Infrastructure.Services;

public sealed class FileReader : IFileReader
{
    public string ReadFile(string fileName)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // Namespace + folder path + file name
        var resourceName = $"Modules.Notifications.Emails.{fileName}";

        using var stream =
            assembly.GetManifestResourceStream(resourceName)
            ?? throw new FileNotFoundException($"Embedded resource '{resourceName}' was not found.");

        using var reader = new StreamReader(stream);

        return reader.ReadToEnd();
    }
}
