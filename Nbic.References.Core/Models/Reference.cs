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
            throw new ArgumentNullException("reference");
        }

        if (!string.IsNullOrWhiteSpace(reference.ReferenceString))
        {
            if (!string.IsNullOrWhiteSpace(reference.Url))
            {
                return (reference.ReferenceString.Trim() + " " + reference.Url).Trim();

            }
            return reference.ReferenceString.Trim();
        }

        string formatedString;
        try
        {
            formatedString =
                ((!string.IsNullOrEmpty(reference.Author) ? reference.Author.Trim() : string.Empty) +
                 (!string.IsNullOrEmpty(reference.Year) ? (" " + AddParanteseIfMissing(reference.Year.Trim())) : string.Empty) +
                 ((!string.IsNullOrEmpty(reference.Author) || !string.IsNullOrEmpty(reference.Year))
                      ? ". "
                      : string.Empty) +
                 (!string.IsNullOrEmpty(reference.Title) ? AddPointIfMissing(reference.Title.Trim()) : string.Empty) +
                 (!string.IsNullOrEmpty(reference.Journal) ? (" " + reference.Journal.Trim()) : string.Empty) +
                 (!string.IsNullOrEmpty(reference.Volume) ? (" " + reference.Volume.Trim()) : string.Empty) +
                 ((!string.IsNullOrEmpty(reference.Volume) && !string.IsNullOrEmpty(reference.Pages)) ? ": " : " ") +
                 (!string.IsNullOrEmpty(reference.Pages) ? AddPointIfMissing(reference.Pages.Trim()) : string.Empty)).Trim();
            if (!string.IsNullOrWhiteSpace(reference.Bibliography))
            {
                //maybe try remove stuff from this instead...
                var start = reference.Bibliography;
                //var doo = false;
                if (!string.IsNullOrEmpty(reference.Author) && start.Contains(reference.Author) || reference.Bibliography.Length > formatedString.Length)
                {
                    formatedString = start;
                }
            }

            if (!string.IsNullOrWhiteSpace(reference.Url))
            {
                formatedString = (formatedString + " " + reference.Url).Trim();

            }
        }
        catch (Exception ex)
        {
            formatedString = "Feil i formatering av referanse!" + ex.Message;
        }

        if (String.IsNullOrEmpty(formatedString) || formatedString.Trim() == ".")
        {
            if (!String.IsNullOrEmpty(reference.Author))
            {
                formatedString = reference.Author;
            }
            else if (!String.IsNullOrEmpty(reference.Lastname) || !String.IsNullOrEmpty(reference.Firstname))
            {
                formatedString = (reference.Lastname ?? string.Empty) + ", " + (reference.Firstname ?? string.Empty) +
                                 (!string.IsNullOrEmpty(reference.Middlename)
                                      ? (" " + reference.Middlename)
                                      : string.Empty);
            }
            else if (!String.IsNullOrEmpty(reference.Url))
            {
                formatedString = reference.Url;
            }
            else
            {
                formatedString = "(Uten tittel)";
            }
        }

        return formatedString.Trim();
    }

    private static string GetReferenceType(Reference reference)
    {
        string type;

        if (!String.IsNullOrEmpty(reference.ReferenceString))
        {
            type = "Reference";
        }
        else if (!String.IsNullOrEmpty(reference.Author) || !String.IsNullOrEmpty(reference.Year) ||
            !String.IsNullOrEmpty(reference.Volume) || !String.IsNullOrEmpty(reference.Pages))
        {
            type = "Publication";
        }
        else if (!String.IsNullOrEmpty(reference.Firstname) | !String.IsNullOrEmpty(reference.Middlename) |
                 !String.IsNullOrEmpty(reference.Lastname))
        {
            type = "Person";
        }
        else if (!String.IsNullOrEmpty(reference.Url))
        {
            type = "Url";
        }
        else
        {
            type = "All";
        }

        return type;
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
