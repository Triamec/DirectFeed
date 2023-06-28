// Copyright © 2007 Triamec Motion AG

using System.ComponentModel;
using System.Xml.Serialization;
using Triamec.Tam.Periphery;

namespace Triamec.Tam.Samples {
	/// <summary>
	/// Information to find and configure an <see cref="IDirectFeedAxis"/>.
	/// </summary>
	[DefaultProperty("DriveName")]
	public sealed class AxisConfiguration {
		/// <summary>
		/// Gets or sets the name of the <see cref="Tam.ITamDrive"/>.
		/// </summary>
		[XmlAttribute(AttributeName = "Drive")]
		[Category("Axis")]
		[DisplayName("Station Name")]
		public string DriveName { get; set; }

		/// <summary>
		/// Gets or sets the index of the axis to use.
		/// </summary>
		[XmlAttribute(AttributeName = "Axis")]
		[DefaultValue(0)]
		[Category("Axis")]
		[DisplayName("Index of the Axis")]
		public int AxisIndex { get; set; }

		/// <summary>
		/// Gets or sets the <see cref="EncoderSource"/> to configure.
		/// </summary>
		[XmlAttribute(AttributeName = "Encoder")]
		[DefaultValue(EncoderSource.Analog)]
		[Category("Encoder")]
		[DisplayName("Encoder Type")]
		public EncoderSource EncoderSource { get; set; }
	}
}
