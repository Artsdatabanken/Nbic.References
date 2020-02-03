using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Nbic.References.Public.Models
{
    public partial class ReferenceUsage
    {
        public Guid ReferenceId { get; set; }
        public int ApplicationId { get; set; }
        public Guid UserId { get; set; }

        
        [JsonIgnore]
        public virtual Reference Reference { get; set; }
    }
}
