// Copyright © 2007 Triamec Motion AG

using System;
using System.Collections.Generic;
using System.Threading;
using Triamec.Tam.Periphery;
using Triamec.Tam.Subscriptions;

namespace Triamec.Tam.Samples {
	/// <summary>
	/// A drive of the <see cref="DirectFeed"/> application.
	/// </summary>
	public interface IDirectFeedAxis {
		#region Events
		/// <summary>
		/// Occurs when an error occurs or is pending on the axis.
		/// </summary>
		event EventHandler MotionError;
		#endregion Events

		#region Properties

		/// <summary>
		/// Gets the <see cref="TamAxis"/> where this <see cref="IDirectFeedAxis"/> is associated to.
		/// </summary>
		TamAxis Axis { get; }

		/// <summary>
		/// Provides the subscriber of this <see cref="IDirectFeedAxis"/>
		/// to participate in the DirectFeed subscription.
		/// </summary>
		ISubscriber Subscriber { get; }

		/// <summary>
		/// Gets or sets the start position of this axis.
		/// </summary>
		float StartPosition {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the park position of this axis.
		/// </summary>
		float ParkPosition {
			get;
			set;
		}

		/// <summary>
		/// Gets the encoder source.
		/// </summary>
		EncoderSource EncoderSource {
			get;
		}

		/// <summary>
		/// Gets the two tables feeding this axis.
		/// </summary>
		IList<PacketFeederTable> Tables { get; }

		/// <summary>Gets the number of buckets in the table a position targeting this axis occupies.</summary>
		int PositionSize { get; }

		#endregion Properties

		#region Methods
		/// <summary>
		/// Enables the axis and its isochronous Tama virtual machine.
		/// </summary>
		/// <param name="tamRequestTimeout">
		/// The time to wait for request completion, or <see cref="Timeout.InfiniteTimeSpan"/> to wait indefinitely.
		/// </param>
		/// <exception cref="TamException">
		/// <para>The enabling failed.</para>
		/// <para>A TAM request timeout occurred.</para>
		/// </exception>
		void Enable(TimeSpan tamRequestTimeout);

		/// <summary>
		/// Disables the axis and its isochronous Tama virtual machine.
		/// </summary>
		/// <param name="tamRequestTimeout">
		/// The time to wait for request completion, or <see cref="Timeout.InfiniteTimeSpan"/> to wait indefinitely.
		/// </param>
		/// <exception cref="TamException">
		/// <para>The disabling failed.</para>
		/// <para>A TAM request timeout occurred.</para>
		/// </exception>
		void Disable(TimeSpan tamRequestTimeout);

		/// <summary>
		/// Couples the axis with DirectFeed values.
		/// </summary>
		/// <exception cref="TamException">
		/// 	<para>The coupling failed.</para>
		/// 	<para>A TAM request timeout occurred.</para>
		/// </exception>
		void Couple();

		/// <summary>
		/// Sends a stop command to the axis, which implies decoupling the axis from DirectFeed.
		/// </summary>
		/// <param name="tamRequestTimeout">
		/// The time to wait for request completion, or <see cref="Timeout.InfiniteTimeSpan"/> to wait indefinitely.
		/// </param>
		/// <exception cref="TamException">
		/// <para>The stopping failed.</para>
		/// <para>A TAM request timeout occurred.</para>
		/// </exception>
		void StopMotion(TimeSpan tamRequestTimeout);

		#endregion Methods
	}
}
