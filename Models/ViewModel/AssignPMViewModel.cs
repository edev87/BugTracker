﻿using Microsoft.AspNetCore.Mvc.Rendering;

namespace BugTracker.Models.ViewModel
{
    public class AssignPMViewModel
    {
        public int ProjectId { get; set; }
        public string? ProjectName { get; set; }
        public SelectList? PMList { get; set; }
        public string? PMId { get; set; }
    }
}
