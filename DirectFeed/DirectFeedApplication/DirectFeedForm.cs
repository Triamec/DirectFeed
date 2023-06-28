// Copyright © 2007 Triamec Motion AG

using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using Triamec.Configuration.UI;
using Triamec.Tam.Configuration;
using Triamec.Tam.Samples.UI.Properties;
using Triamec.Tam.UI;

namespace Triamec.Tam.Samples.UI {
	/// <summary>
	///	The main form of the DirectFeed sample.
	/// </summary>
	public sealed partial class DirectFeedForm : Form {
		#region Constants

		const string ErrorMessageBoxCaption = "Table Feeder Error";

		const string Continuation = "…";

		#endregion Constants

		#region Fields
		readonly string _path;
		TamTopology _topology;
		TamExplorerForm _explorerForm;
		#endregion Fields

		#region Constructor
		/// <summary>
		/// Initializes a new instance of the <see cref="DirectFeedForm"/> class.
		/// </summary>
		/// <param name="path">The file to load the paths from.</param>
		public DirectFeedForm(string path) {
			_path = path;

			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			_resetToolStripMenuItem.Enabled = OwnsTopology;

			var versionAttribute = typeof(TamTopology).Assembly.GetCustomAttributes(
				false).OfType<AssemblyFileVersionAttribute>().SingleOrDefault();
			_versionToolStripMenuItem.Text = "Version: " + Application.ProductVersion + " using TAM Software " +
				(versionAttribute == null ? "DEBUG" : versionAttribute.Version);

			_directFeedControl.DirectFeedBusiness = new DirectFeed();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DirectFeedForm"/> class.
		/// </summary>
		public DirectFeedForm() : this(null) { }

		#endregion Constructor

		#region Private properties
		TamSystem TamSystem => _topology[0];
		bool OwnsTopology => _path == null;

		#endregion Private properties

		#region Public properties
		/// <summary>
		/// Gets the business logic of this user interface.
		/// </summary>
		public DirectFeed DirectFeedBusiness {
			get { return _directFeedControl.DirectFeedBusiness; }
			set { _directFeedControl.DirectFeedBusiness = value; }
		}
		#endregion Public properties

		#region Private methods
		#region Private GUI event handlers
		/// <summary>
		/// Called when menu item File/Exit is chosen.
		/// </summary>
		void OnMenuItemFileExitClick(object sender, EventArgs e) => Close();

		/// <summary>
		/// Called when user tries to close the form.
		/// </summary>
		void OnFormClosing(object sender, FormClosingEventArgs e) {
			if (OwnsTopology) e.Cancel = !TeardownTopology(true);
		}

		/// <summary>
		/// Called when the preferences menu is chosen.
		/// </summary>
		void OnMenuItemPreferencesClick(object sender, EventArgs e) {
			using (var dialog = new PreferencesDialog()) {
				dialog.ShowDialog();
			}
		}

		/// <summary>
		/// Called when menu item File/Reset is chosen.
		/// </summary>
		void OnMenuItemFileResetClick(object sender, EventArgs e) {
			UseWaitCursor = true;
			try {
				_drudge.RunWorkerAsync(sender);
			} catch (InvalidOperationException) {
				MessageBox.Show("Application is busy.",
					ErrorMessageBoxCaption,
					MessageBoxButtons.OK, MessageBoxIcon.Error,
					MessageBoxDefaultButton.Button1, 0);

			}
		}

		/// <summary>
		/// Called when menu item View/System explorer is chosen.
		/// </summary>
		void OnMenuItemViewSystemExplorerClick(object sender, EventArgs e) {
			UseWaitCursor = true;

			// create the explorer form if necessary
			if ((_explorerForm == null) || _explorerForm.IsDisposed) {
				_explorerForm = new TamExplorerForm() {
					AutoLoadTamConfiguration = false,
					Topology = _topology
				};
				_explorerForm.Closing += OnTamExplorerClosing;
			}

			// show the explorer form
			_explorerForm.Show();

			UseWaitCursor = false;
		}

		/// <summary>
		/// Called when the TAM system explorer form was closed by the user.
		/// </summary>
		void OnTamExplorerClosing(object sender, CancelEventArgs e) => _explorerForm = null;

		#endregion Private GUI event handlers

		#region Private background worker methods

		void OnDoWork(object sender, DoWorkEventArgs e) {
			bool success = false;

			// expect e.Argument to be one of
			// this, menuItemFileReset
			if (e.Argument == this) {
				Thread.Sleep(100); // let the controls build up first

				if (OwnsTopology) {
					success = ResetTopology();
				} else {

					// disable view explorer because we cannot pass the topology in this mode
					_systemExplorerToolStripMenuItem.Enabled = false;

					DirectFeedBusiness.Execute(_path, Settings.Default.Repeat, Settings.Default.Axes);
				}
			} else if (e.Argument == _resetToolStripMenuItem) {
				success = ResetTopology();
			}

			e.Result = e.Argument;
			e.Cancel = !success;
		}

		void OnRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
			UseWaitCursor = false;

			if (!e.Cancelled && (e.Result == _exitToolStripMenuItem)) {
				Close();
			}
		}

		/// <summary>
		/// Disposes the TAM topology.
		/// </summary>
		void DisposeTopology() {
			if (_topology != null) {
				if (_explorerForm != null) {
					_explorerForm.Topology = null;
				}
				_topology.Dispose();
			}
			_topology = null;
		}

		DialogResult ShowErrorMessageBox(string message) =>
			ShowErrorMessageBox(message, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);

		DialogResult ShowErrorMessageBox(string message, MessageBoxButtons buttons,
			MessageBoxIcon icon, MessageBoxDefaultButton defaultButton) {

			DialogResult dialogResult = DialogResult.OK;

			if (InvokeRequired) {
				dialogResult = (DialogResult)Invoke(
					new Func<string, MessageBoxButtons, MessageBoxIcon, MessageBoxDefaultButton, DialogResult>(
						ShowErrorMessageBox), message, buttons, icon, defaultButton);

			} else {
				dialogResult = MessageBox.Show(this, message, ErrorMessageBoxCaption,
					buttons, icon, defaultButton, 0);
			}

			return dialogResult;
		}

		#endregion Private background worker methods
		#endregion Private methods

		#region Public methods
		/// <summary>
		/// Disposes the <see cref="TamTopology"/>.
		/// </summary>
		/// <param name="allowEnforce">If set to <see langword="true"/>, allows the user
		/// to enforce the teardown even if not in an appropriate state.</param>
		/// <param name="consolePrefix">The string to prefix to console outputs.</param>
		/// <returns>
		/// Returns <see langword="true"/> on success;
		/// otherwise, returns <see langword="false"/>.
		/// </returns>
		public bool TeardownTopology(bool allowEnforce, string consolePrefix) {
			_directFeedControl.AppendLine(consolePrefix + "Teardown of the current topology");

			// Continuation flag
			bool success = true;
			bool forceTeardown = false;
			DirectFeed directFeed = _directFeedControl.DirectFeedBusiness;

			#region Ready to tear down?

			bool isReadyForDisposal = (directFeed != null) && directFeed.IsReadyForDisposal;
			if (!isReadyForDisposal) {
				string errorMessage = "The current state of the exhibit does not allow to tear down the topology";
				if (allowEnforce) {
					string dialogMessage = errorMessage + Environment.NewLine +
						"Do you want to tear down anyway?";

					success = forceTeardown = (DialogResult.OK == ShowErrorMessageBox(dialogMessage,
						MessageBoxButtons.OKCancel, MessageBoxIcon.Warning,
						MessageBoxDefaultButton.Button2));

				} else {
					success = false;
					ShowErrorMessageBox(errorMessage,
						 MessageBoxButtons.OK, MessageBoxIcon.Warning,
						 MessageBoxDefaultButton.Button1);
				}

				if (!success) {
					_directFeedControl.AppendLine(consolePrefix + "Teardown failed: ",
						DirectFeedControl.ErrorColor, errorMessage);
				}
			}

			#endregion Ready to tear down?

			#region Teardown of the current topology
			if (success) {
				try {
					if ((directFeed != null) && (directFeed.System != null)) {
						// withdraw the TamSystem from the business logic
						directFeed.System = null;
					}

					if ((_explorerForm != null) && (_explorerForm.Topology != null)) {
						// withdraw the TamTopology from the TamExplorerForm
						_explorerForm.Topology = null;
					}

					DisposeTopology();

				} catch (InvalidOperationException ex) {
					success = forceTeardown;

					_directFeedControl.AppendLine(consolePrefix + "Teardown failed: ",
						DirectFeedControl.ErrorColor, ex.Message);

					if (isReadyForDisposal) {
						ShowErrorMessageBox("Failed to tear down the current topology:\n" + ex.Message);
					}
				}
			}
			#endregion Teardown of the current topology

			return success;
		}

		/// <summary>
		/// Disposes the <see cref="TamTopology"/>.
		/// </summary>
		/// <param name="allowEnforce">If set to <see langword="true"/>, allows the user
		/// to enforce the teardown even if not in an appropriate state.</param>
		/// <returns>
		/// Returns <see langword="true"/> on success;
		/// otherwise, returns <see langword="false"/>.
		/// </returns>
		public bool TeardownTopology(bool allowEnforce) => TeardownTopology(allowEnforce, string.Empty);

		/// <summary>
		/// Resets the application
		/// by disposing and re-creating its <see cref="TamTopology"/>.
		/// </summary>
		/// <returns>
		/// Returns <see langword="true"/> on success;
		/// otherwise, returns <see langword="false"/>.
		/// </returns>
		public bool ResetTopology() {
			_directFeedControl.AppendLine("Reset topology" + Continuation);

			// Continuation flag
			bool success = true;
			DirectFeed directFeed = _directFeedControl.DirectFeedBusiness;

			#region Teardown of the current topology
			success &= TeardownTopology(false, Continuation);
			#endregion Teardown of the current topology

			#region Create and boot the new topology
			if (success) {
				Exception exception = null;
				_directFeedControl.AppendLine(Continuation + "Create and boot the new topology");
				try {
					_topology = new TamTopology();
					_topology.AddLocalSystem();
					TamSystem.Initialize();

				} catch (ArgumentException ex) {
					exception = ex;
				} catch (TamException ex) {
					exception = ex;
				} catch (InvalidOperationException ex) {
					exception = ex;
				}

				if (exception != null) {
					success = false;

					_directFeedControl.AppendLine(Continuation + "Reset topology failed: ",
						DirectFeedControl.ErrorColor, exception.GetBaseException().Message);

					ShowErrorMessageBox("Failed to create and boot the topology:\n" + exception.Message);

				}
			}
			#endregion Create and boot the new topology

			#region Configure the new topology
			if (success) {
				_directFeedControl.AppendLine(Continuation + "Configure the new topology");
				// get TAM configuration path from app.config settings
				string tamConfigurationPath = TamTopologyConfiguration.TamConfigurationPath;
				if (string.IsNullOrEmpty(tamConfigurationPath)) {
					//ShowErrorMessageBox("TAM configuration not specified. Please set the Startup | TAM configuration " +
					//	" file preference and restart the application.");
				} else {
					// load the TAM configuration
					try {
						LoadSurveyor.Load(tamConfigurationPath, _topology, autoStart: true, mainForm: this);
					} catch (TamException exception) {
						success = false;

						_directFeedControl.AppendLine(Continuation + "Reset topology failed: ",
							DirectFeedControl.ErrorColor, exception.Message);

						ShowErrorMessageBox("Failed to configure the topology:\n" + exception.Message);
					}
				}
			}
			#endregion Configure the new topology

			#region Assign the new topology and system
			if (success) {
				_directFeedControl.AppendLine(Continuation + "Assign the new topology and system");
				try {
					if (directFeed != null) {
						// assign the TamSystem to the business logic
						directFeed.System = TamSystem;
					}

					if (_explorerForm != null) {
						// assign the TamTopology to the TamExplorerForm
						_explorerForm.Topology = _topology;
					}

				} catch (TamException ex) {
					success = false;

					_directFeedControl.AppendLine(Continuation + "Reset topology failed: ",
						DirectFeedControl.ErrorColor, ex.Message);

					ShowErrorMessageBox("Failed to assign the topology:\n" + ex.Message);
				}
			}
			#endregion Assign the new topology and system

			#region Identify the new system and select the encoder source
			if (success) {
				_directFeedControl.AppendLine(Continuation + "Identify the new system" + Continuation);
				success = DirectFeedBusiness.Identify(Settings.Default.Axes);

				if (!success) {
					_directFeedControl.AppendLine(Continuation + "Failed to identify the expected system.");
				}
			}
			#endregion Identify the new system and select the encoder source

			if (success) {
				_directFeedControl.AppendLine("Reset topology done.");
			} else {
				_directFeedControl.AppendLine(string.Empty, DirectFeedControl.ErrorColor, "Reset topology failed.");
			}
			return success;
		}

		#endregion Public methods

		#region Form overrides
		/// <summary>Schedules a TAM system reset if applicable.</summary>
		protected override void OnShown(EventArgs e) {
			if (!DesignMode) {
				UseWaitCursor = OwnsTopology;

				// schedule for reset
				_drudge.RunWorkerAsync(this);
			}

			base.OnShown(e);
		}
		#endregion Form overrides
	}
}
