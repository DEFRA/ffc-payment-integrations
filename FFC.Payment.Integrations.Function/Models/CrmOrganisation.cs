namespace FFC.Payment.Integrations.Function.Models
{
	/// <summary>
	/// Organisation details from CRM
	/// </summary>
	public class CrmOrganisation : CrmBaseType
	{
		/// <summary>
		/// Constructor for CrmOrganisation
		/// </summary>
		/// <param name="name"></param>
		/// <param name="accountId"></param>
		public CrmOrganisation(string name, string accountId)
		{
			Name = name;
			AccountId = accountId;
		}

		/// <summary>
		/// Name of organisation
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// AccountId of organisation
		/// </summary>
		public string AccountId { get; }
	}
}

