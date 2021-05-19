using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Realestate_portal.Models.ViewModels
{
    public class NotesView
    {
        public int ID_Customer { get; set; }
        public int ID_note { get; set; }

        public string Text { get; set; }

        public DateTime DateDate { get; set; }

        public string Date { get; set; }

        public string Created_By { get; set; }

    }
}