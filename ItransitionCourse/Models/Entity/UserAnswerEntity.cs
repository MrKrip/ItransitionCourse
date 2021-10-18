using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ItransitionCourse.Models.Entity
{
    public class UserAnswerEntity
    {
        [Key]
        public int id { get; set; }
        [Required]
        public string UserID { get; set; }
        [Required]
        public int TaskId { get; set; }
        [Required]
        public string Answer { get; set; }
        public bool CorrectAnswer { get; set; }
    }
}
