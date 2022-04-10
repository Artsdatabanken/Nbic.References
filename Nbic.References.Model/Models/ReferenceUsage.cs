using System;
using System.Text.Json.Serialization;

namespace Nbic.References.Public.Models;

public class ReferenceUsage
{
    public Guid ReferenceId { get; set; }
    public int ApplicationId { get; set; }
    public Guid UserId { get; set; }


    [JsonIgnore]
    public virtual Reference Reference { get; set; }
}
