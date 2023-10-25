namespace FFC.Payment.Integrations.Function.Models
{
	/// <summary>
	/// Model for auth token for CRM
	/// </summary>
	public class CrmAuthToken
	{
		/// <summary>
		/// access_token
		/// </summary>
		public string access_token { get; set; }

		/// <summary>
		/// number of seconds the token expires in
		/// </summary>
		public string expires_in { get; set; }

		/// <summary>
		/// date/time token expires
		/// </summary>
        public string expires_on { get; set; }

		/// <summary>
		/// number of seconds token expires in
		/// </summary>
        public string ext_expires_in { get; set; }

		/// <summary>
		/// date/time token is not valid before
		/// </summary>
        public string not_before { get; set; }

		/// <summary>
		/// resource url that the token allows access to
		/// </summary>
        public string resource { get; set; }

		/// <summary>
		/// type of token
		/// </summary>
        public string token_type { get; set; }
    }
}

