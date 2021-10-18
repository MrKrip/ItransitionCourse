using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ItransitionCourse.Models
{
    public class ProfileViewModel
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public bool CanEdit { get; set; }
        public int CorrectTasks { get; set; }
        public IEnumerable<ProfileTask> Tasks { get; set; }
        public class ProfileTask
        {
            public int TaskId { get; set; }
            public string Title { get; set; }

        }
    }
}
