using Microsoft.AspNetCore.Http.HttpResults;
using System.ComponentModel.DataAnnotations;

namespace BugTracker.Models
{
    public class Invite
    {
        private DateTime _inviteDate;
        private DateTime _joinDate;

        public int Id { get; set; }

        [DataType(DataType.Date)]
        public DateTime InviteDate { get => _inviteDate.ToLocalTime(); set => value.ToUniversalTime(); }

        [DataType(DataType.Date)]
        public DateTime JoinDate { get => _joinDate.ToLocalTime(); set => value.ToUniversalTime(); }

        public Guid CompanyToken { get; set; }

        public int CompanyId { get; set; }

        public int ProjectId { get; set; }

        [Required]
        public string? InvitorId { get; set; }

        public string? InviteeId { get; set; }

        [Required]
        public string? InviteeEmail { get; set; }

        [Required]
        public string? InviteeFirstName { get; set; }

        [Required]
        public string? InviteeLastName { get; set; }

        public string? Message { get; set; }
        public bool IsValid { get; set; }

        //Navigation Properties

        public virtual Company? Company { get; set; }

        public virtual Project? Project { get; set; }

        public virtual BTUser? Invitor { get; set; }

        public virtual BTUser? Invitee { get; set; }

    }
}
