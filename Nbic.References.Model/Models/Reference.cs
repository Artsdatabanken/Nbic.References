using System;
using System.Collections.Generic;

namespace Nbic.References.Public.Models
{
    public partial class RfReference
    {
        public RfReference()
        {
            RfReferenceUsage = new HashSet<RfReferenceUsage>();
        }

        public Guid PkReferenceId { get; set; }
        public int? ApplicationId { get; set; }
        public int? FkUserId { get; set; }
        public string Author { get; set; }
        public string Year { get; set; }
        public string Title { get; set; }
        public string Summary { get; set; }
        public string Journal { get; set; }
        public string Volume { get; set; }
        public string Pages { get; set; }
        public string Bibliography { get; set; }
        public string Lastname { get; set; }
        public string Middlename { get; set; }
        public string Firstname { get; set; }
        public string Url { get; set; }
        public string Keywords { get; set; }
        public string ImportXml { get; set; }
        public DateTime EditDate { get; set; }

        public virtual ICollection<RfReferenceUsage> RfReferenceUsage { get; set; }
    }
}