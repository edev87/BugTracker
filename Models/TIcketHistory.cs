﻿using Microsoft.AspNetCore.Http.HttpResults;
using System.ComponentModel.DataAnnotations;

namespace BugTracker.Models
{
    public class TicketHistory
    {
        private DateTime _created;

        public int Id { get; set; }

        public int TicketId { get; set; }

        public string? PropertyName { get; set; }
        public string? Description { get; set; }

        [DataType(DataType.Date)]
        public DateTime Created { get => _created.ToLocalTime(); set => value.ToUniversalTime(); }

        public string?  OldValue { get; set; }

        public string? NewValue { get; set; }

        [Required]
        public string? UserId { get; set; }

        //Navigation Properties
        public virtual Ticket? Ticket { get; set; }

        public virtual BTUser? User { get; set; }

    }
}