using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ItransitionCourse.Models
{
    public class TaskViewModel
    {
        public string UserName { get; set; }
        public string UserId { get; set; }
        public int TaskId { get; set; }
        public string Title { get; set; }
        public string TaskText { get; set; }
        public string Theme { get; set; }
        public string Image { get; set; }
        public string Answer1 { get; set; }
        public string Answer2 { get; set; }
        public string Answer3 { get; set; }
        public bool CorrectAnswer { get; set; }
        public bool HasAnAnswer { get; set; }

        public DateTime CreationDate { get; set; }
    }
}
