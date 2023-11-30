namespace FFC.Payment.Integrations.Function.Models
{
	/// <summary>
	/// Incoming message
	/// </summary>
	public class IncomingQueueMessage
	{
		/// <summary>
		/// SBI
		/// </summary>
		public string Sbi { get; set; }

		/// <summary>
		/// FRN - customer reference number
		/// </summary>
		public string Frn { get; set; }

		/// <summary>
		/// Api link which contains filename
		/// </summary>
		public string ApiLink { get; set; }

		/// <summary>
		/// Document type
		/// </summary>
		public string DocumentType { get; set; }

        /// <summary>
        /// Scheme
        /// </summary>
        public string Scheme { get; set; }
    }
}

