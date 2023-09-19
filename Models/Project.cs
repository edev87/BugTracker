using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BugTracker.Models
{
    public class Project
    {

        private DateTime _startDate;
        private DateTime _endDate;
        private DateTime _created;
        public int Id { get; set; }

        public int CompanyId { get; set; }

        [Required]
        public string? Name { get; set; }


        [Required]
        public string? Description { get; set; }

        [DataType(DataType.Date)]
        public DateTime Created { get { return _created; } set { _created = value.ToUniversalTime(); }  }

        [DataType(DataType.Date)]
        public DateTime StartDate { get { return _startDate; } set { _startDate = value.ToUniversalTime(); } }

        [DataType(DataType.Date)]
        public DateTime EndDate { get { return _endDate; } set { _endDate = value.ToUniversalTime(); } }

        public int ProjectPriorityId { get; set; }

        [NotMapped]
        public IFormFile? ImageFile { get; set; } //doesn't get stored in db but the 2 below props do
        public byte[]? ImageData { get; set; }
        public string? ImageType { get; set; }
        public bool Archived { get; set; }

        //Navigation Properties

        public virtual Company? Company { get; set; }

        public virtual ProjectPriority? ProjectPriority { get; set; }

        public virtual ICollection<BTUser> Members { get; set; } = new HashSet<BTUser>();

        public virtual ICollection<Ticket> Tickets { get; set; } = new HashSet<Ticket>();


    }
}
