using System.Text.Json.Serialization;

namespace Nbic.References.Core.Models;

/// <summary>
/// Signals usage in different applications
/// </summary>
public class ReferenceUsage
{
    /// <summary>
    /// A reference by id
    /// </summary>
    public Guid ReferenceId { get; set; }

    /// <summary>
    ///  A reference to the application
    /// </summary>
    public int ApplicationId { get; set; }

    /// <summary>
    /// A reference to the user who have assossiated the reference to data in a specific application
    /// </summary>
    public Guid UserId { get; set; }


    [JsonIgnore]
    public virtual Reference Reference { get; set; } = null!;
}
