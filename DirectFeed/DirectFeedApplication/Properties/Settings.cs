// Copyright © 2013 Triamec Motion AG

using System.Collections.Generic;
using Triamec.Configuration;
using Triamec.Tam.Samples.UI.Properties;

[assembly: Preferences(typeof(Settings))]

namespace Triamec.Tam.Samples.UI.Properties {
	partial class Settings : IPreferences {
		private Settings() {
			this.RegisterForAdditionalServices();
		}

		#region IPreferences members
		IEnumerable<SettingsPreferenceDescriptor> IPreferences.Preferences =>
			new List<SettingsPreferenceDescriptor> {
				new Preference(this, nameof(Axes), "DirectFeed"),
				new Preference(this, nameof(Repeat), "DirectFeed")
			};

		#endregion IPreferences members
	}
}
