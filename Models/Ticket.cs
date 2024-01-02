using Microsoft.AspNetCore.Http.HttpResults;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace BugTracker.Models
{
    public class Ticket
    {
        private DateTime _created;
        private DateTime _updated;
        public int Id
        {
            get; set;
        }

        [Required]
        public string? Title { get; set; }
        [Required]
        public string? Description { get; set; }

        [DataType(DataType.Date)]
        public DateTime Created { get { return _created; } set { _created = value.ToUniversalTime(); } }

        [DataType(DataType.Date)]
        public DateTime Updated { get { return _updated; } set { _updated = value.ToUniversalTime(); } }


        public bool Archived { get; set; }

        public int ProjectId { get; set; }

        public bool ArchivedByProject { get; set; }

        [Display(Name = "Type")]
        public int TicketTypeId { get; set; }

        [Display(Name = "Status")]
        public int TicketStatusId { get; set; }

        [Display(Name = "Priority")]
        public int TicketPriorityId { get; set; }

        public string? DeveloperUserId { get; set; }

        [Required]
        public string? SubmitterUserId { get; set; }

        //Navigation Properties
        public virtual Project? Project {get; set;}

        public virtual TicketPriority? TicketPriority { get; set; }

        public virtual TicketType? TicketType { get; set; }

        public virtual TicketStatus? TicketStatus { get; set; }

        public virtual BTUser? DeveloperUser { get; set; }

        public virtual BTUser? SubmitterUser { get; set; }

        public virtual ICollection<TicketComment> Comments { get; set; } = new HashSet<TicketComment>();
        public virtual ICollection<TicketAttachment> Attachments { get; set; } = new HashSet<TicketAttachment>();

        public virtual ICollection<TicketHistory> History { get; set; } = new HashSet<TicketHistory>();
    }

}
