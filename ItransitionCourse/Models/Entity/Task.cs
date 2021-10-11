using Microsoft.AspNetCore.Identity;
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
        [MaxLength(100)]
        [Display(Name = "Title")]
        public string Title { get; set; }
        [MaxLength(5000)]
        [Display(Name = "Task")]
        public string TaskText { get; set; }
        [MaxLength(100)]
        public string Answer1 { get; set; }
        public string Answer2 { get; set; }
        public string Answer3 { get; set; }
        public string Image1 { get; set; }
        public string Image2 { get; set; }
        public string Image3 { get; set; }
        public string UserId { get; set; }
        public IdentityUser User { get; set; }
        public DateTime CreationDate { get; set; }
    }
}
