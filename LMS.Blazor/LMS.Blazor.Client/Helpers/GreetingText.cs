namespace LMS.Blazor.Client.Helpers;

/// <summary>
/// The greeting at the top of both dashboards, so the student and the teacher get the same style:
/// "Welcome, Karin". There is no time of day, so the text is the same on the server and in the browser.
/// </summary>
public static class GreetingText
{
    // "Welcome, Karin", or just "Welcome" when the name is unknown
    public static string For(string? userName)
    {
        var firstName = FirstNameOf(userName);
        return string.IsNullOrEmpty(firstName) ? "Welcome" : $"Welcome, {firstName}";
    }

    // The first word of the name: "Karin Sjöberg" -> "Karin". If the name is an e-mail address,
    // only the part before the @ is used: "teacher@lms.com" -> "Teacher".
    public static string FirstNameOf(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return string.Empty;

        var atIndex = name.IndexOf('@');
        var text = atIndex > 0 ? name[..atIndex] : name;
        var first = text.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? string.Empty;

        return first.Length == 0 ? first : char.ToUpperInvariant(first[0]) + first[1..];
    }
}