using System.Diagnostics.CodeAnalysis;

namespace FFC.Payment.Integrations.Function.Models
{
	/// <summary>
	/// Model for generic GET reponse from CRM
	/// </summary>
	[ExcludeFromCodeCoverage]
	public class CrmGenericGetResponse<T> where T : CrmBaseType 
	{
		/// <summary>
		/// value
		/// </summary>
		public T[] value { get; set; }
    }
}

