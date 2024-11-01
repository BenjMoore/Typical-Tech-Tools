using System;
using System.ComponentModel.DataAnnotations;

namespace TypicalTechTools.Models
{
    public class Comment
    {
        public int CommentId { get; set; }

        [Required(ErrorMessage = "Comment text is required")]
        [StringLength(500, ErrorMessage = "Comment text cannot exceed 500 characters")]
        public string CommentText { get; set; }

        [Required]
        public string ProductCode { get; set; }

        public string UserID { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}
