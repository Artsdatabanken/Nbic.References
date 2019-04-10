using System;
using System.Collections.Generic;

namespace Nbic.References.Public.Models
{
    public partial class ReferenceUsage
    {
        public Guid ReferenceId { get; set; }
        public int ApplicationId { get; set; }
        public int UserId { get; set; }

        public virtual Reference Reference { get; set; }
    }
}
