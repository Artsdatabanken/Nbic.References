namespace Nbic.References.Core.Models;

public partial class Reference
{
    public Reference()
    {
        ReferenceUsage = new HashSet<ReferenceUsage>();
    }

    public Guid Id { get; set; }
    public int? ApplicationId { get; set; }
    public Guid UserId { get; set; }
    public string? Author { get; set; }
    public string? Year { get; set; }
    public string? Title { get; set; }
    public string? Summary { get; set; }
    public string? Journal { get; set; }
    public string? Volume { get; set; }
    public string? Pages { get; set; }
    public string? Bibliography { get; set; }
    public string? Lastname { get; set; }
    public string? Middlename { get; set; }
    public string? Firstname { get; set; }
    public string? Url { get; set; }
    public string? Keywords { get; set; }
    //public string ImportXml { get; set; }
    public string? ReferenceString { get; set; }
    public DateTime EditDate { get; set; }
    public string ReferencePresentation
    {
        get
        {
            return FormatReference(this);
        }
    }
    public string ReferenceType
    {
        get
        {
            return GetReferenceType(this);
        }
    }

    public virtual ICollection<ReferenceUsage> ReferenceUsage { get; set; }

    private static string FormatReference(Reference reference)
    {
        if (reference == null)
        {
            throw new ArgumentNullException(nameof(reference));
        }

        if (!string.IsNullOrWhiteSpace(reference.ReferenceString))
        {
            return !string.IsNullOrWhiteSpace(reference.Url)
                ? $"{reference.ReferenceString.Trim()} {reference.Url}".Trim()
                : reference.ReferenceString.Trim();
        }

        string formattedString = FormatReferenceDetails(reference);

        if (!string.IsNullOrWhiteSpace(reference.Bibliography))
        {
            formattedString = AdjustFormattedStringWithBibliography(reference, formattedString);
        }

        if (!string.IsNullOrWhiteSpace(reference.Url))
        {
            formattedString = $"{formattedString} {reference.Url}".Trim();
        }

        return string.IsNullOrWhiteSpace(formattedString) || formattedString.Trim() == "."
            ? GetFallbackFormattedString(reference)
            : formattedString.Trim();
    }


    private static string FormatReferenceDetails(Reference reference)
    {
        return string.Concat(
            !string.IsNullOrEmpty(reference.Author) ? reference.Author.Trim() : string.Empty,
            !string.IsNullOrEmpty(reference.Year) ? $" {AddParanteseIfMissing(reference.Year.Trim())}" : string.Empty,
            (!string.IsNullOrEmpty(reference.Author) || !string.IsNullOrEmpty(reference.Year)) ? ". " : string.Empty,
            !string.IsNullOrEmpty(reference.Title) ? AddPointIfMissing(reference.Title.Trim()) : string.Empty,
            !string.IsNullOrEmpty(reference.Journal) ? $" {reference.Journal.Trim()}" : string.Empty,
            !string.IsNullOrEmpty(reference.Volume) ? $" {reference.Volume.Trim()}" : string.Empty,
            (!string.IsNullOrEmpty(reference.Volume) && !string.IsNullOrEmpty(reference.Pages)) ? ": " : " ",
            !string.IsNullOrEmpty(reference.Pages) ? AddPointIfMissing(reference.Pages.Trim()) : string.Empty
        ).Trim();
    }

    private static string AdjustFormattedStringWithBibliography(Reference reference, string formattedString)
    {
        var start = reference.Bibliography;
        if (!string.IsNullOrEmpty(reference.Author) && start.Contains(reference.Author) || reference.Bibliography.Length > formattedString.Length)
        {
            return start;
        }
        return formattedString;
    }

    private static string GetFallbackFormattedString(Reference reference)
    {
        return reference switch
        {
            { Author: { Length: > 0 } } => reference.Author,
            { Lastname: { Length: > 0 } } or { Firstname: { Length: > 0 } } => $"{reference.Lastname ?? string.Empty}, {reference.Firstname ?? string.Empty}{(!string.IsNullOrEmpty(reference.Middlename) ? $" {reference.Middlename}" : string.Empty)}",
            { Url: { Length: > 0 } } => reference.Url,
            _ => "(Uten tittel)"
        };
    }


    private static string GetReferenceType(Reference reference)
    {
        return reference switch
        {
            { ReferenceString: { Length: > 0 } } => "Reference",
            { Author: { Length: > 0 } } or { Year: { Length: > 0 } } or { Volume: { Length: > 0 } } or { Pages: { Length: > 0 } } => "Publication",
            { Firstname: { Length: > 0 } } or { Middlename: { Length: > 0 } } or { Lastname: { Length: > 0 } } => "Person",
            { Url: { Length: > 0 } } => "Url",
            _ => "All"
        };
    }

    private static string AddParanteseIfMissing(string item)
    {
        if (!string.IsNullOrWhiteSpace(item) && !item.StartsWith("("))
        {
            return $"({item})";
        }

        return item;
    }
    private static string AddPointIfMissing(string item)
    {
        if (!string.IsNullOrWhiteSpace(item) && !item.EndsWith("."))
        {
            return $"{item}.";
        }

        return item;
    }
}
