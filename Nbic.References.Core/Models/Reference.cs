namespace Nbic.References.Core.Models;

/// <summary>
/// Represents a Reference - article , book, person, url etc.
/// </summary>
public partial class Reference
{
    public Reference()
    {
        ReferenceUsage = new HashSet<ReferenceUsage>();
    }

    /// <summary>
    /// Internal Id for the reference
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The application the reference was registered via
    /// </summary>
    public int? ApplicationId { get; set; }

    /// <summary>
    /// A reference to the user registering the reference
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// The author of the publication/reference
    /// </summary>
    public string? Author { get; set; }

    /// <summary>
    /// the year of the publication/reference
    /// </summary>
    public string? Year { get; set; }

    /// <summary>
    /// The title of the publication/reference
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// An summary of the content of the reference
    /// </summary>
    public string? Summary { get; set; }

    /// <summary>
    /// The journal of the publication/reference
    /// </summary>
    public string? Journal { get; set; }

    /// <summary>
    /// Volume in the publication/journal
    /// </summary>
    public string? Volume { get; set; }

    /// <summary>
    /// Pages of the publication/journal
    /// </summary>
    public string? Pages { get; set; }

    /// <summary>
    /// A bibliographic reference for the resource.
    /// </summary>
    public string? Bibliography { get; set; }

    /// <summary>
    /// Last name of a person referenced
    /// </summary>
    /// <remarks>
    /// Used in context with Middlename and firstname - other fields not required
    /// </remarks>
    public string? Lastname { get; set; }

    /// <summary>
    /// Middle name of a person referenced
    /// </summary>
    public string? Middlename { get; set; }

    /// <summary>
    /// First name of a person references
    /// </summary>
    public string? Firstname { get; set; }

    /// <summary>
    /// An reference to an URL - may be standalone but then in context of an title
    /// </summary>
    public string? Url { get; set; }

    /// <summary>
    /// AN reference to an URL - may be standalone but then in context of an title
    /// </summary>
    public string? Keywords { get; set; }

    //public string ImportXml { get; set; }
    
    /// <summary>
    /// A free format representation of the reference
    /// </summary>
    public string? ReferenceString { get; set; }
    
    /// <summary>
    /// The edit date
    /// </summary>
    public DateTime EditDate { get; set; }

    /// <summary>
    /// Readonly formatted/standardized presentation of the reference
    /// </summary>
    public string ReferencePresentation
    {
        get
        {
            return FormatReference(this);
        }
    }

    /// <summary>
    /// Readonly category of reference type "Reference", "Publication", "Person","Url" ....
    /// </summary>
    public string ReferenceType
    {
        get
        {
            return GetReferenceType(this);
        }
    }

    /// <summary>
    /// A list of usages in different applications
    /// </summary>
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
        if (start != null && reference.Bibliography != null && (!string.IsNullOrEmpty(reference.Author) && start.Contains(reference.Author) || reference.Bibliography.Length > formattedString.Length))
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
