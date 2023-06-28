// Copyright © 2007 Triamec Motion AG

using System;

namespace Triamec.Tam.Samples {
	/// <summary>
	/// The states of the application's state machine.
	/// </summary>
	public enum DirectFeedState {
		/// <summary>None</summary>
		None,

		/// <summary>NoSystem</summary>
		NoSystem,

		/// <summary>Unidentified</summary>
		Unidentified,

		/// <summary>Identifying</summary>
		Identifying,

		/// <summary>Identified</summary>
		Identified,

		/// <summary>PrepareFilling</summary>
		PrepareFilling,

		/// <summary>FillingPrepared</summary>
		FillingPrepared,

		/// <summary>Subscribing</summary>
		Subscribing,

		/// <summary>Unsubscribing</summary>
		Unsubscribing,

		/// <summary>Subscribed</summary>
		Subscribed,

		/// <summary>Enabling</summary>
		Enabling,

		/// <summary>Disabling</summary>
		Disabling,

		/// <summary>Enabled</summary>
		Enabled,

		/// <summary>Filling the first table.</summary>
		FillingFirst,

		/// <summary>First table is filled.</summary>
		FilledFirst,

		/// <summary>Repositioning</summary>
		Repositioning,

		/// <summary>Parking</summary>
		Parking,

		/// <summary>Parked</summary>
		Parked,

		/// <summary>StopRepositioning</summary>
		StopRepositioning,

		/// <summary>Repositioned</summary>
		Repositioned,

		/// <summary>StartFeeding</summary>
		StartFeeding,

		/// <summary>Stopping</summary>
		StopFeeding,

		/// <summary>FeedingStopped</summary>
		FeedingStopped,

		/// <summary>FeedingFirst</summary>
		FeedingFirst,

		/// <summary>Coupling</summary>
		Coupling,

		/// <summary>Stopping</summary>
		Stopping,

		/// <summary>ErrorStopping</summary>
		ErrorStopping,

		/// <summary>Stopped</summary>
		Stopped,

		/// <summary>Coupled</summary>
		Coupled,

		/// <summary>FeedingMove</summary>
		FeedingMove,

		/// <summary>Feeding move with more data to come.</summary>
		FeedingAttached,

		/// <summary>FeedingLast</summary>
		FeedingLast,
	}
}
