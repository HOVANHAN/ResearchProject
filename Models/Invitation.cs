namespace ResearchProject.Models
{
	public class Invitation
	{
		public int Id { get; set; }

		public string SenderId { get; set; }
		public string ReceiverId { get; set; }
		public bool IsAccepted { get; set; }
        public bool IsRejected { get; set; }

		public DateTime SentDate { get; set; }

		public string SendMessage { get; set; }

		public DateTime? ResponseDate { get; set; }

		public string? ResponseMessage { get; set; }
        public int ProjectId { get; set; }
		public Project Project { get; set; }


	}
}
