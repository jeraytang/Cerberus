using System.Collections.Generic;
using System.Threading.Tasks;
using IdentityServer4.Models;

namespace IdentityServer4.Storage.MySql.TokenCleanup
{
	/// <summary>
	/// Interface to model notifications from the TokenCleanup feature.
	/// </summary>
	public interface IOperationalStoreNotification
	{
		/// <summary>
		/// Notification for persisted grants being removed.
		/// </summary>
		/// <param name="persistedGrants"></param>
		/// <returns></returns>
		Task PersistedGrantsRemovedAsync(IEnumerable<PersistedGrant> persistedGrants);
	}
}
