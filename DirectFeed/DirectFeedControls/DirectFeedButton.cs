// Copyright © 2007 Triamec Motion AG

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;

namespace Triamec.Tam.Samples.UI {
	/// <summary>
	/// Enhanced button supporting background work and state machines.
	/// </summary>
	[DefaultProperty("ToState")]
	internal class DirectFeedButton : Button {
		#region Constants
		/// <summary>
		/// The state specifying that no transition is defined.
		/// <para>The value is <c><see cref="DirectFeedState.NoSystem"/></c>.</para>
		/// </summary>
		public const DirectFeedState NoTransition = DirectFeedState.NoSystem;
		#endregion Constants

		#region Fields
		/// <summary>The states used to compute <see cref="_enabledStates"/>.</summary>
		DirectFeedState[] _toStates;

		/// <summary>The states where this button must be <see cref="Control.Enabled"/>.</summary>
		ICollection<DirectFeedState> _enabledStates;
		BackgroundWorker _backgroundWorker;
		#endregion Fields

		#region Events
		/// <summary>
		/// Occurs when the button was clicked, on a secondary <see cref="System.Threading.Thread"/>.
		/// </summary>
		[Category("Action")]
		[DisplayName("Click (Background)")]
		[Description("Occurs when the component was clicked and the background worker is invoked.")]
		public event EventHandler Click2;

		#endregion Events

		#region Constructor
		public DirectFeedButton() {
			InitializeComponent();
		}
		#endregion Constructor

		#region Properties
		/// <summary>
		/// Gets or sets the state to which the transition issued by this button leads to.
		/// </summary>
		/// <value>
		/// To state or <see cref="NoTransition"/> in order to not specify any such states.
		/// </value>
		[Category("DirectFeed")]
		[Description("The state to which this button is designed to transit.")]
		[DefaultValue(NoTransition)]
		public DirectFeedState ToState {
			get { return ((_toStates == null) || (_toStates.Length > 1)) ? NoTransition : _toStates[0]; }
			set {
				if (value == NoTransition) {
					_toStates = null;
				} else {
					_toStates = new[] { value };
				}
			}
		}

		[Category("DirectFeed")]
		[Description("The states to which this button is designed to transit to.")]
		[DefaultValue(null)]
		public DirectFeedState[] ToStates {
			get { return _toStates != null && _toStates.Length == 1 ? null : _toStates; }
			set {
				if (value != null && value.Length == 1 && value[0] == NoTransition) {
					_toStates = null;
				} else {
					_toStates = value;
				}
			}
		}

		[Category("DirectFeed")]
		[Description("Whether the button needs to be disabled when automatically looping.")]
		[DefaultValue(false)]
		public bool IsLoopLocked { get; set; }

		#endregion Properties

		#region Private methods
		void OnBackgroundWorkerDoWork(object sender, DoWorkEventArgs e) =>
			Click2?.Invoke(this, EventArgs.Empty);

		void InitializeComponent() {
			_backgroundWorker = new BackgroundWorker();
			SuspendLayout();
			// 
			// backgroundWorker
			// 
			_backgroundWorker.DoWork += OnBackgroundWorkerDoWork;
			ResumeLayout(false);

		}
		#endregion Private methods

		#region Public methods
		/// <summary>
		/// Updates the <see cref="Control.Enabled"/> state of this <see cref="Button"/> based on a specified state
		/// and whether looping is enabled.
		/// </summary>
		/// <param name="state">The business model state.</param>
		/// <param name="isLoop">if set to <see langword="true"/>, the application is looping.</param>
		public virtual void Update(DirectFeedState state, bool isLoop) {
			if (_enabledStates == null) {
				_enabledStates = ToState == NoTransition ?
					DirectFeedStateMachine.GetTransitionsInverse(ToStates) :
					DirectFeedStateMachine.GetTransitionsInverse(ToState);
			}

			Enabled = !(isLoop && IsLoopLocked) && _enabledStates.Contains(state);
		}
		#endregion Public methods

		#region Button overrides
		/// <summary>
		/// Issues the click on a secondary thread.
		/// </summary>
		protected override void OnClick(EventArgs e) {
			if ((Click2 != null) && !_backgroundWorker.IsBusy) {
				_backgroundWorker.RunWorkerAsync();
			}
			base.OnClick(e);
		}
		#endregion Button overrides
	}
}
