using BugTracker.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BugTracker.Models
{
    public class TicketAttachment
    {
        private DateTime _created;

        public int Id { get; set; }
        public string? Description { get; set; }

        [DataType(DataType.Date)]
        public DateTime Created { get => _created.ToLocalTime(); set => value.ToUniversalTime(); }

        public int TicketId { get; set; }

        [Required]
        public string? BTUserId { get; set; }

        [NotMapped]
        [DisplayName("Select a file")]
        [DataType(DataType.Upload)]
        [MaxFileSize(1024 * 1024)]
        [AllowedExtensions(new string[] { ".jpg", ".png", ".doc", ".docx", ".xls", ".xlsx", ".pdf" })]
        public IFormFile? FormFile { get; set; } //doesn't get stored in db but the 2 below props do

        public byte[]? FileData { get; set; }

        public string? FileName { get; set; }

        public string? FileContentType { get; set; }

        public string? FileType { get; set; }

        //Navigation Properties

        public virtual Ticket? Ticket { get; set; }

        public virtual BTUser? BTUser { get; set; }
    }
}
