// Copyright © 2007 Triamec Motion AG

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Triamec.Tam.Samples {
	/// <summary>
	/// The state machine of the <see cref="DirectFeed"/>.
	/// </summary>
	public abstract class DirectFeedStateMachine {
		#region Fields
		static readonly IDictionary<DirectFeedState, IList<DirectFeedState>> Transitions =
			new Dictionary<DirectFeedState, IList<DirectFeedState>> {
				{ DirectFeedState.NoSystem, new[] {
					DirectFeedState.NoSystem,
					DirectFeedState.Unidentified }
				},
				{ DirectFeedState.Unidentified, new[] {
					DirectFeedState.NoSystem,
					DirectFeedState.Unidentified,
					DirectFeedState.Identifying }
				},
				{ DirectFeedState.Identifying, new[] {
					DirectFeedState.Unidentified,
					DirectFeedState.Identifying,
					DirectFeedState.Identified }
				},
				{ DirectFeedState.Identified, new[] {
					DirectFeedState.Identifying,
					DirectFeedState.Identified,
					DirectFeedState.PrepareFilling,
					DirectFeedState.NoSystem
				}},
				{ DirectFeedState.PrepareFilling, new[] {
					DirectFeedState.Identified,
					DirectFeedState.PrepareFilling,
					DirectFeedState.FillingPrepared
				}},
				{ DirectFeedState.FillingPrepared, new[] {
					DirectFeedState.FillingPrepared,
					DirectFeedState.Subscribing,
					DirectFeedState.PrepareFilling,
					DirectFeedState.NoSystem
				}},
				{ DirectFeedState.Subscribing, new[] {
					DirectFeedState.Unsubscribing,
					DirectFeedState.Subscribing,
					DirectFeedState.Subscribed
				}},
				{ DirectFeedState.Unsubscribing, new[] {
					DirectFeedState.Subscribed,
					DirectFeedState.Unsubscribing,
					DirectFeedState.FillingPrepared
				}},
				{ DirectFeedState.Subscribed, new[] {
					DirectFeedState.Unsubscribing,
					DirectFeedState.Subscribed,
					DirectFeedState.Enabling
				}},
				{ DirectFeedState.Enabling, new[] {
					DirectFeedState.Disabling,
					DirectFeedState.Enabling,
					DirectFeedState.Enabled
				}},
				{ DirectFeedState.Disabling, new[] {
					DirectFeedState.Enabled,
					DirectFeedState.Disabling,
					DirectFeedState.Subscribed
				}},
				{ DirectFeedState.Enabled, new[] {
					DirectFeedState.Disabling,
					DirectFeedState.Enabled,
					DirectFeedState.FillingFirst
				}},
				{ DirectFeedState.FillingFirst, new[] {
					DirectFeedState.Enabled,
					DirectFeedState.FillingFirst,
					DirectFeedState.FilledFirst
				}},
				{ DirectFeedState.FilledFirst, new[] {
					DirectFeedState.FilledFirst,
					DirectFeedState.Parking
				}},
				{ DirectFeedState.Repositioning, new[] {
					DirectFeedState.StopRepositioning,
					DirectFeedState.FilledFirst,
					DirectFeedState.Repositioning,
					DirectFeedState.Repositioned
				}},
				{ DirectFeedState.Parking, new[] {
					DirectFeedState.Parking,
					DirectFeedState.StopRepositioning,
					DirectFeedState.Enabled,
					DirectFeedState.Parked
				}},
				{ DirectFeedState.Parked, new[] {
					DirectFeedState.Parked,
					DirectFeedState.Parking,
					DirectFeedState.Disabling,
					DirectFeedState.Repositioning
				}},
				{ DirectFeedState.StopRepositioning, new[] {
					DirectFeedState.StopRepositioning,
					DirectFeedState.FilledFirst,
					DirectFeedState.Enabled,
					DirectFeedState.Disabling
				}},
				{ DirectFeedState.Repositioned, new[] {
					DirectFeedState.Repositioned,
					DirectFeedState.StartFeeding,
					DirectFeedState.Repositioning,
					DirectFeedState.Parking
				}},
				{ DirectFeedState.StartFeeding, new[] {
					DirectFeedState.StopFeeding,
					DirectFeedState.StartFeeding,
					DirectFeedState.FeedingFirst
				}},
				{ DirectFeedState.FeedingFirst, new[] {
					DirectFeedState.StopFeeding,
					DirectFeedState.FeedingFirst,
					DirectFeedState.Coupling
				}},
				{ DirectFeedState.StopFeeding, new[] {
					DirectFeedState.StopFeeding,
					DirectFeedState.FeedingStopped
				}},
				{ DirectFeedState.FeedingStopped, new[] {
					DirectFeedState.FeedingStopped,
					DirectFeedState.FillingFirst,
					DirectFeedState.Enabled
				}},
				{ DirectFeedState.Coupling, new[] {
					DirectFeedState.Stopping,
					DirectFeedState.Coupling,
					DirectFeedState.Coupled
				}},
				{ DirectFeedState.Stopping, new[] {
					DirectFeedState.Stopping,
					DirectFeedState.Stopped,
					DirectFeedState.StopFeeding
				}},
				{ DirectFeedState.ErrorStopping, new[] {
					DirectFeedState.ErrorStopping,
					DirectFeedState.Stopped,
					DirectFeedState.StopFeeding
				}},
				{ DirectFeedState.Stopped, new[] {
					DirectFeedState.Stopped,
					DirectFeedState.StopFeeding
				}},
				{ DirectFeedState.Coupled, new[] {
					DirectFeedState.Stopping,
					DirectFeedState.Coupled,
					DirectFeedState.FeedingMove
				}},
				{ DirectFeedState.FeedingMove, new[] {
					DirectFeedState.FeedingMove,
					DirectFeedState.FeedingAttached,
					DirectFeedState.FeedingLast,
					DirectFeedState.Stopping,
					DirectFeedState.ErrorStopping
				}},
				{ DirectFeedState.FeedingAttached, new[] {
					DirectFeedState.FeedingAttached,
					DirectFeedState.FeedingLast,
					DirectFeedState.Stopping,
					DirectFeedState.ErrorStopping
				}},
				{ DirectFeedState.FeedingLast, new[] {
					DirectFeedState.FeedingLast,
					DirectFeedState.Stopping,
					DirectFeedState.ErrorStopping
				}}
			};

		readonly object _syncLock = new object();
		#endregion Fields

		#region Delegates, Events

		/// <summary>
		/// Event raised whenever a state transition occurred in this <see cref="DirectFeedStateMachine"/>.
		/// </summary>
		public event EventHandler<TransitionEventArgs> Transition;

		#endregion Delegates, Events

		#region Constructor
		/// <summary>
		/// Initializes a new instance of the <see cref="DirectFeedStateMachine"/> class.
		/// </summary>
		protected DirectFeedStateMachine() : base() { }

		#endregion Constructor

		#region Public properties

		/// <summary>
		/// Gets the current state of the state machine.
		/// </summary>
		public DirectFeedState CurrentState { get; private set; } = DirectFeedState.NoSystem;

		/// <summary>
		/// Gets a value indicating whether this instance is 
		/// in a <see cref="CurrentState"/> 
		/// that is safe for smooth disposal.
		/// </summary>
		public virtual bool IsReadyForDisposal {
			get {
				switch (CurrentState) {
					case DirectFeedState.NoSystem:
					case DirectFeedState.Unidentified:
					case DirectFeedState.Identified:
					case DirectFeedState.FillingPrepared:
						return true;

					default:
						return false;
				}
			}
		}
		#endregion Public properties

		#region Protected methods
		/// <summary>
		/// Asserts that a state transition is legal.
		/// </summary>
		/// <param name="fromState">From state.</param>
		/// <param name="toState">To state.</param>
		/// <exception cref="InvalidOperationException">The asserted state transition is illegal.</exception>
		protected static void AssertTransition(DirectFeedState fromState, DirectFeedState toState) {
			IList<DirectFeedState> legalStates;

			if (Transitions.ContainsKey(fromState)) {
				legalStates = Transitions[fromState];

			} else {
				legalStates = Array.Empty<DirectFeedState>();
			}

			bool found = false;
			foreach (DirectFeedState expectedState in legalStates) {
				if (expectedState == toState) {
					found = true;
					break; // out of foreach loop
				}
			}

			if (!found) {
				string[] legalStateNames = new string[legalStates.Count];
				for (int legalStateIndex = 0; legalStateIndex < legalStates.Count; ++legalStateIndex) {
					legalStateNames[legalStateIndex] = @"""" + legalStates[legalStateIndex].ToString() + @"""";
				}

				throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture,
					@"Internal state assertion failure: current state ""{0}"" is none of {{{1}}}",
					toState, string.Join(", ", legalStateNames)));
			}
		}

		/// <summary>
		/// Raises a <see cref="Transition"/>.
		/// </summary>
		/// <param name="state">The new state to report in the <see cref="Transition"/>.</param>
		/// <param name="message">The message to include in the <see cref="Transition"/>.</param>
		protected virtual void OnTransition(DirectFeedState state, string message) =>
			Transition?.Invoke(this, new TransitionEventArgs(this, state, message));

		/// <summary>
		/// Transits the state machine to the specified state.
		/// </summary>
		/// <param name="toState">The new state of the state machine.</param>
		/// <param name="message">The message to associate with the state transition.</param>
		/// <exception cref="InvalidOperationException">The state transition is illegal.</exception>
		protected void Transit(DirectFeedState toState, string message) {
			lock (_syncLock) {
				AssertTransition(CurrentState, toState);
				CurrentState = toState;
			}

			// notify state transition listeners
			OnTransition(toState, message);
		}

		/// <summary>
		/// Issues a reflexive transition with the specified message.
		/// </summary>
		/// <param name="message">The message.</param>
		protected void Transit(string message) =>

			// notify state transition listeners
			OnTransition(CurrentState, message);

		/// <summary>
		/// Transits the state machine to the specified state.
		/// </summary>
		/// <param name="toState">The new state of the state machine.</param>
		/// <exception cref="InvalidOperationException">The state transition is illegal.</exception>
		protected void Transit(DirectFeedState toState) => Transit(toState, null);

		#endregion Protected methods

		#region Public methods

		/// <summary>
		/// Gets the states directly reachable from a specified state.
		/// </summary>
		/// <param name="fromState">The state for which to get the outgoing transitions.</param>
		/// <returns>Returns the list of target states of the transitions
		/// defined for <paramref name="fromState"/>.</returns>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="fromState"/> is a null reference.
		/// </exception>
		/// <exception cref="KeyNotFoundException">
		/// Internal error: the transitions for <paramref name="fromState"/> can't be retrieved.
		/// </exception>
		public static IList<DirectFeedState> GetTransitions(DirectFeedState fromState) => Transitions[fromState];

		/// <summary>
		/// Gets the states having a transition to the specified states.
		/// </summary>
		public static IList<DirectFeedState> GetTransitionsInverse(params DirectFeedState[] toStates) =>
			(from transition in Transitions.Keys
			 where ((toStates != null) && toStates.Any(toState => Transitions[transition].Contains(toState)))
			 select transition).ToArray();

		/// <summary>Indicates whether the machine is likely to be in a workpiece in the specified state.</summary>
		public static bool IsLikelyToBeInWorkPiece(DirectFeedState state) {
			switch (state) {
				case DirectFeedState.FeedingMove:
				case DirectFeedState.FeedingAttached:
					return true;

				default:
					return false;
			}
		}
		#endregion Public methods
	}
}
