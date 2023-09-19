using Microsoft.Data.SqlClient;
using Microsoft.Identity.Client.Platforms.Features.DesktopOs.Kerberos;
using System.ComponentModel.DataAnnotations;

namespace BugTracker.Models
{
    public class Notification
    {

        private DateTime _created;
        public int Id { get; set; }

        public int? ProjectId { get; set; }

        public int? TicketId { get; set; }

        [Required]
        public string? Title { get; set; }

        [Required]
        public string? Message { get; set; }

        [DataType(DataType.Date)]
        public DateTime Created { get => _created.ToLocalTime(); set => value.ToUniversalTime(); }

        [Required]
        public string? SenderId { get; set; }
        [Required]
        public string? RecipientId { get; set; }

        public bool HasBeenViewed { get; set; }

        public int NotificationTypeId { get; set; }

        //Navigation properties
        public virtual NotificationType? NotificationType { get; set; }

        public virtual Ticket? Ticket { get; set; }
        public virtual Project? Project { get; set; } 
        public virtual BTUser? Sender { get; set; }
        public virtual BTUser? Recipient { get; set; }


    }
}
