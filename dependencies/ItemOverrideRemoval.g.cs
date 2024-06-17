using Elements;
using System.Collections.Generic;
using System;
using System.Linq;

namespace Trolley
{
	/// <summary>
	/// Override metadata for ItemOverrideRemoval
	/// </summary>
	public partial class ItemOverrideRemoval : IOverride
	{
        public static string Name = "Item Removal";
        public static string Dependency = null;
        public static string Context = "[*discriminator=Elements.Item]";
		public static string Paradigm = "Edit";

        /// <summary>
        /// Get the override name for this override.
        /// </summary>
        public string GetName() {
			return Name;
		}

		public object GetIdentity() {

			return Identity;
		}

	}

}