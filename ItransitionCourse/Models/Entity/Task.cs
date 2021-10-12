﻿using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ItransitionCourse.Models.Entity
{
    public class Task
    {
        [Key]
        public int TaskId { get; set; }
        [Required]
        [MaxLength(100)]
        [MinLength(1)]
        [Display(Name = "Title")]
        public string Title { get; set; }
        [Required]
        [MaxLength(5000)]
        [MinLength(1)]
        [Display(Name = "Task")]
        public string TaskText { get; set; }
        public string Theme { get; set; }
        [Required]
        [MaxLength(100)]
        [MinLength(1)]
        [Display(Name = "First Answer")]
        public string Answer1 { get; set; }
        [Display(Name = "Second(Optional)")]
        public string Answer2 { get; set; }
        [Display(Name = "Third(Optional)")]
        public string Answer3 { get; set; }
        public string Image1 { get; set; }
        public string Image2 { get; set; }
        public string Image3 { get; set; }
        public string UserId { get; set; }
        public IdentityUser User { get; set; }
        public DateTime CreationDate { get; set; }
    }
}
