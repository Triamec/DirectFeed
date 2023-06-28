// Copyright © 2007 Triamec Motion AG

using System;

namespace Triamec.Tam.Samples {
	/// <summary>
	/// The <see cref="EventArgs"/> of a <see cref="DirectFeedStateMachine.Transition"/>.
	/// </summary>
	public sealed class TransitionEventArgs : EventArgs {
		#region Constructor
		/// <summary>
		/// Initializes a new instance of the <see cref="TransitionEventArgs"/> class.
		/// </summary>
		/// <param name="stateMachine">The sender of the <see cref="DirectFeedStateMachine.Transition"/>.</param>
		/// <param name="state">The new state reached when the event was raised.</param>
		public TransitionEventArgs(DirectFeedStateMachine stateMachine, DirectFeedState state)
			: this(stateMachine, state, null) {
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="TransitionEventArgs"/> class.
		/// </summary>
		/// <param name="stateMachine">The sender of the <see cref="DirectFeedStateMachine.Transition"/>.</param>
		/// <param name="state">The new state reached when the event was raised.</param>
		/// <param name="message">An additional message to include into the event.</param>
		public TransitionEventArgs(DirectFeedStateMachine stateMachine, DirectFeedState state, string message)
			: base() {

			Sender = stateMachine;
			State = state;
			Message = message;
		}
		#endregion Constructor

		#region Public Properties

		/// <summary>
		/// Gets the sender of the event.
		/// </summary>
		public DirectFeedStateMachine Sender { get; }

		/// <summary>
		/// Gets the state at the time when the event was raised.
		/// </summary>
		public DirectFeedState State { get; }

		/// <summary>
		/// Gets the message of the state transition event.
		/// </summary>
		public string Message { get; }

		#endregion Public Properties
	}
}
