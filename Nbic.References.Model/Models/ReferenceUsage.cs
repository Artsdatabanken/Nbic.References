using System;
using System.Collections.Generic;

namespace Nbic.References.Public.Models
{
    public partial class RfReferenceUsage
    {
        public Guid FkReferenceId { get; set; }
        public int FkApplicationId { get; set; }
        public int FkUserId { get; set; }

        public virtual RfReference FkReference { get; set; }
    }
}
