// Copyright © 2007 Triamec Motion AG

using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Triamec.Diagnostics;
using Triamec.Windows.Forms;
using Triamec.Tam.Samples.Properties;

namespace Triamec.Tam.Samples.UI {
	/// <summary>
	/// The control for the DirectFeed sample.
	/// </summary>
	public sealed partial class DirectFeedControl : UserControl {
		#region Constants
		/// <summary>
		/// The timeout of TAM requests.
		/// </summary>
		static readonly TimeSpan TamRequestTimeout = Settings.Default.TamRequestTimeout;

		/// <summary>
		/// The maximum number of lines to display in the console.
		/// </summary>
		const int MaxConsoleLineCount = 40;

		/// <summary>
		/// The error color to use in the console output.
		/// </summary>
		public static readonly Color ErrorColor = Color.Red;

		#endregion Constants

		#region Fields

		DirectFeed _directFeed;
		TableImportForm _tableImportForm;
		bool _finishLoop;

		#endregion Fields

		#region Constructor
		/// <summary>
		///     Initializes a new instance of the <see cref="DirectFeedControl"/> class.
		/// </summary>
		public DirectFeedControl() {
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();
		}
		#endregion Constructor

		#region Public Properties
		/// <summary>
		/// Gets the business logic of this user interface.
		/// </summary>
		public DirectFeed DirectFeedBusiness {
			get { return _directFeed; }

			set {
				if (_directFeed != null) {
					_directFeed.Transition -= OnTransition;
				}

				_directFeed = value;

				if (_directFeed != null) {
					AppendLine("Start");
					_directFeed.Transition += OnTransition;
				}
			}
		}
		#endregion Public Properties

		#region Private methods
		#region Private business logic event handlers
		/// <summary>
		/// Callback for <see cref="DirectFeedStateMachine.Transition"/>s.
		/// </summary>
		/// <param name="sender">The state machine that raised the event.</param>
		/// <param name="transition">The <see cref="TransitionEventArgs"/> instance containing the event data.</param>
		void OnTransition(object sender, TransitionEventArgs transition) {
			if (InvokeRequired) {
				// recursive call of this method on the correct thread
				BeginInvoke(new EventHandler<TransitionEventArgs>(OnTransition), sender, transition);

			} else {
				// Update the GUI to reflect the new state

				#region Transition message

				if (string.IsNullOrEmpty(transition.Message)) {
					AppendLine(string.Format(CultureInfo.InvariantCulture,
						"-> {0}", transition.State));
				} else {
					Log.Complain(transition.Message);
					AppendLine(string.Format(CultureInfo.InvariantCulture,
						"-> {0}: ", transition.State), ErrorColor, transition.Message);
				}

				#endregion Transition message

				#region Update button enabling

				foreach (var button in Controls.OfType<DirectFeedButton>()) {
					button.Update(transition.State, _finishLoop);
				}
				foreach (var button in _feederLoopGroupBox.Controls.OfType<DirectFeedButton>()) {
					button.Update(transition.State, _finishLoop);
				}

				#endregion Update button enabling

				#region Auto loop handling

				// auto start handling
				if (transition.State == DirectFeedState.FillingPrepared) {
					_loopCheckBox.Checked = _finishLoop;
				}
				#endregion Auto loop handling
			}
		}

		#endregion Private business logic event handlers

		#region Private GUI event handlers

		void OnImportButtonClick(object sender, EventArgs e) {
			DirectFeedBusiness.PrepareFilling();

			if (_tableImportForm == null) {
				_tableImportForm = new TableImportForm();

				// initially preset values appropriate for this application
				DirectFeedBusiness.Importer.FileName = "data.txt";
				DirectFeedBusiness.Importer.RowRanges.ReplaceParsed("10-", NumberFormatInfo.InvariantInfo);
				DirectFeedBusiness.Importer.ColumnRanges.ReplaceParsed(
					"2-" + (DirectFeedBusiness.AxisCount * DirectFeedBusiness.PositionDimensionality + 1),
					NumberFormatInfo.InvariantInfo);
			}

			// assign the importer each time; 
			// DirectFeedBusiness may have changed since last call
			_tableImportForm.Importer = DirectFeedBusiness.Importer;

			DialogResult dialogResult = _tableImportForm.ShowDialog(this);

			if (dialogResult == DialogResult.OK) {
				DirectFeedBusiness.EndPrepareFilling(null);
			} else {
				DirectFeedBusiness.EndPrepareFilling("Canceled import.");
			}
		}

		void OnSubscribeButtonClick(object sender, EventArgs args) => DirectFeedBusiness.Subscribe();
		void OnUnsubscribeButtonClick(object sender, EventArgs args) => DirectFeedBusiness.Unsubscribe(null);
		void OnEnableButtonClick(object sender, EventArgs e) => DirectFeedBusiness.Enable(TamRequestTimeout);
		void OnDisableButtonClick(object sender, EventArgs e) =>
			DirectFeedBusiness.Disable(null, TamRequestTimeout);
		void OnFillButtonClick(object sender, EventArgs e) => DirectFeedBusiness.FillFirst();
		void OnRepositionButtonClick(object sender, EventArgs e) =>
			DirectFeedBusiness.Reposition(TamRequestTimeout);
		void OnParkButtonClick(object sender, EventArgs e) => DirectFeedBusiness.Park(TamRequestTimeout);
		void OnSendButtonClick(object sender, EventArgs e) => DirectFeedBusiness.Send();
		void OnStopFeedingButtonClick(object sender, EventArgs e) => DirectFeedBusiness.StopFeeding(null);
		void OnCoupleButonClick(object sender, EventArgs e) => DirectFeedBusiness.Couple();
		void OnStopButtonClick(object sender, EventArgs e) {
			_finishLoop = false;
			DisableLoopCheckBox();
			DirectFeedBusiness.Stop(TamRequestTimeout);
		}
		void OnMoveButtonClick(object sender, EventArgs e) => DirectFeedBusiness.StartMove();
		void OnAutoLoopButtonClick(object sender, EventArgs e) {
			_finishLoop = true;
			EnableLoopCheckBox();
			_drudge.RunWorkerAsync();
		}

		void OnWorkingOnPartCheckedChanged(object sender, EventArgs e) =>
			_decoupleButton.Update(DirectFeedBusiness.CurrentState, _loopCheckBox.Enabled);

		#endregion Private GUI event handlers

		#region Private background worker methods

		void OnDoWork(object sender, DoWorkEventArgs e) {
			#region Dispatch work depending on sender
			if (_finishLoop) {
				NextAutoLoopStep(TamRequestTimeout);
			}
			#endregion Dispatch work depending on sender
		}

		/// <summary>
		/// Executes the next command in the auto loop.
		/// </summary>
		/// <param name="tamRequestTimeout">
		/// The time to wait for request completion, or <see cref="System.Threading.Timeout.InfiniteTimeSpan"/> to wait
		/// indefinitely.
		/// </param>
		void NextAutoLoopStep(TimeSpan tamRequestTimeout) {
			switch (DirectFeedBusiness.CurrentState) {
				case DirectFeedState.FillingPrepared:
					DirectFeedBusiness.Subscribe();
					break;

				case DirectFeedState.Subscribed:
					DirectFeedBusiness.Enable(tamRequestTimeout);
					break;

				case DirectFeedState.Enabled:
				case DirectFeedState.FeedingStopped:
					if (_loopCheckBox.Checked) {
						// start a new loop
						DirectFeedBusiness.FillFirst();
					} else {
						// don't start a new loop
						DisableLoopCheckBox();
						_finishLoop = false;
						// enforce an update of the button enable states
						var transitionEventArgs = new TransitionEventArgs(
							DirectFeedBusiness, DirectFeedBusiness.CurrentState);

						OnTransition(transitionEventArgs.Sender, transitionEventArgs);
					}
					break;

				case DirectFeedState.FilledFirst:
					DirectFeedBusiness.Park(tamRequestTimeout);
					break;

				case DirectFeedState.Parked:
					DirectFeedBusiness.Reposition(tamRequestTimeout);
					break;

				case DirectFeedState.Repositioned:
					DirectFeedBusiness.Send();
					break;

				case DirectFeedState.FeedingFirst:
					DirectFeedBusiness.Couple();
					break;

				case DirectFeedState.Coupled:
					DirectFeedBusiness.StartMove();
					break;

				case DirectFeedState.FeedingLast:
					DirectFeedBusiness.Stop(tamRequestTimeout);
					break;

				case DirectFeedState.Stopped:
					DirectFeedBusiness.StopFeeding(null);
					break;

			}
		}

		void OnWorkCompleted(object sender, RunWorkerCompletedEventArgs e) {
			
			// Force to run this on the GUI thread in order to not overwhelm it.
			if (InvokeRequired) {
				BeginInvoke(new RunWorkerCompletedEventHandler(OnWorkCompleted), sender, e);
			} else if (_finishLoop) {
				_drudge.RunWorkerAsync();
			}
		}

		/// <summary>
		/// Enables the "loop" CheckBox, using the GUI thread.
		/// </summary>
		void EnableLoopCheckBox() {
			if (InvokeRequired) {
				IAsyncResult asyncResult = BeginInvoke(new MethodInvoker(EnableLoopCheckBox));
				// wait for the GUI to be done updating the check box
				asyncResult.AsyncWaitHandle.WaitOne();

			} else {
				_loopCheckBox.Checked = true;
				_loopCheckBox.Enabled = true;
			}
		}

		/// <summary>
		/// Disables the "loop" CheckBox, using the GUI thread.
		/// </summary>
		void DisableLoopCheckBox() {
			if (InvokeRequired) {
				IAsyncResult asyncResult = BeginInvoke(new MethodInvoker(DisableLoopCheckBox));
				// wait for the GUI to be done updating the check box
				asyncResult.AsyncWaitHandle.WaitOne();

			} else {
				_loopCheckBox.Enabled = false;
				_loopCheckBox.Checked = false;
			}
		}

		#endregion Private background worker methods
		#endregion Private methods

		#region Public console methods
		/// <summary>
		/// Appends an empty line to the <see cref="_console"/> control.
		/// Removes lines at the beginning of the <see cref="_console"/>
		/// when there are more than <see cref="MaxConsoleLineCount"/> lines.
		/// </summary>
		public void AppendLine() => AppendLine(null, Color.Black, null);

		/// <summary>
		/// Appends a line of <see cref="Color.Black"/> text to the <see cref="_console"/> control.
		/// Removes lines at the beginning of the <see cref="_console"/>
		/// when there are more than <see cref="MaxConsoleLineCount"/> lines.
		/// </summary>
		/// <param name="text">The text to write in <see cref="Color.Black"/>.</param>
		public void AppendLine(string text) => AppendLine(text, Color.Black, null);

		/// <summary>
		/// Appends a line of <see cref="Color.Black"/> and colored text to the <see cref="_console"/> control.
		/// Removes lines at the beginning of the <see cref="_console"/>
		/// when there are more than <see cref="MaxConsoleLineCount"/> lines.
		/// </summary>
		/// <param name="text">The text to write in <see cref="Color.Black"/>.</param>
		/// <param name="color">The color for <paramref name="colorText"/>.</param>
		/// <param name="colorText">The text to write in <paramref name="color"/>.</param>
		public void AppendLine(string text, Color color, string colorText) {
			if (InvokeRequired) {
				BeginInvoke(new Action<string, Color, string>(AppendLine), text, color, colorText);

			} else {
				#region Remove lines at the beginning when too long

				int removeLineCount = 1 /* the new line to be added */
					+ _console.Lines.Length - MaxConsoleLineCount;
				int removeCharCount = 0;
				for (int removeLineIndex = 0; removeLineIndex < removeLineCount; ++removeLineIndex) {
					string line = _console.Lines[removeLineIndex];
					removeCharCount += line.Length + 1; // line separator length is 1, not Environment.NewLine.Length
				}
				if (removeCharCount > 0) {

					// remove the beginning lines of the text box
					_console.ReadOnly = false;
					_console.Select(0, removeCharCount);
					_console.SelectedText = string.Empty;
					_console.ReadOnly = true;
				}

				#endregion Remove lines at the beginning when too long

				#region Append black text

				if (!string.IsNullOrEmpty(text)) {
					_console.SelectionColor = Color.Black;
					_console.AppendText(text);
				}

				#endregion Append black text

				#region Append color text

				if (!string.IsNullOrEmpty(colorText)) {
					_console.SelectionColor = color;
					_console.AppendText(colorText);
					_console.SelectionColor = Color.Black;
				}

				#endregion Append color text

				_console.AppendText(Environment.NewLine);
				_console.ScrollToCaret();
			}
		}

		#endregion Public console methods
	}
}
