// Copyright © 2007 Triamec Motion AG

#if DEBUG
using System.Diagnostics;
#endif
#if TEST_PACKETFEEDER
using System.IO;
#endif
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using Triamec.Tam.Configuration;
using Triamec.Tam.Periphery;
using Triamec.Tam.Requests;
using Triamec.Tam.Samples.Properties;
using Triamec.Tam.Subscriptions;
using Triamec.TamMath;
using Triamec.TriaLink;

namespace Triamec.Tam.Samples {
	using static FormattableString;

	/// <summary>
	/// Business logic for the direct feed sample application.
	/// </summary>
	public sealed class DirectFeed : DirectFeedStateMachine, IDisposable {
		#region Constants
		static readonly TimeSpan StopTimeout = TimeSpan.FromSeconds(5);
		static readonly TimeSpan DisableTimeout = TimeSpan.FromSeconds(2);
		#endregion Constants

		#region Fields

		/// <summary>Reference to the method importing all rows.</summary>
		readonly Func<uint> _rowImporter;

		/// <summary>The signal for the feeder task to continue to fill into tables.</summary>
		readonly EventWaitHandle _firstFillContinuation = new ManualResetEvent(false);

		/// <summary>The signal for the <see cref="FillFirst"/> method to return.</summary>
		readonly EventWaitHandle _filledFirst = new ManualResetEvent(false);

		/// <summary>The signal set when the table feeder finished moving.</summary>
		readonly EventWaitHandle _moveCompleted = new ManualResetEvent(false);

		/// <summary>Lock object for synchronization of certain commands.</summary>
		readonly object _syncPoint = new object();

		/// <summary>
		/// The TAM system to work with.
		/// </summary>
		TamSystem _system;

		/// <summary>
		/// The packet feeder to work with.
		/// </summary>
		PacketFeeder _feeder;

		/// <summary>
		/// The axes to work with.
		/// </summary>
		IDirectFeedAxis[] _feederAxes;

		/// <summary>
		/// The table feeding subscriptions.
		/// </summary>
		ISubscription[] _subscriptions;

		/// <summary>
		/// The length of the tables, in unit of <see cref="TriaLink.Packets.Packet"/>s.
		/// </summary>
		uint _length;

		/// <summary>
		/// The index to the tables currently being prepared.
		/// </summary>
		int _currentTableIndex;

		/// <summary>
		/// The tables currently prepared.
		/// </summary>
		PacketFeederTable[] _currentTables;

		/// <summary>
		/// The offset between the currently imported row and the first row of the table currently being prepared.
		/// </summary>
		uint _rowOffset;

		/// <summary>
		/// The index of the row currently filled into the table currently being prepared.
		/// </summary>
		uint _tableRowIndex;

		/// <summary>
		/// Flag to indicates if this instance is disposed.
		/// </summary>
		bool _disposed = false;

		#endregion Fields

		#region Constructors
		/// <summary>
		/// Initializes a new instance of the <see cref="DirectFeed"/> class.
		/// </summary>
		public DirectFeed() {

			Importer = new TableImporter {

				// Prototype for an import of one row
				RowImportCallback = ImportRow
			};

			// Prototype for an import of a whole file
			_rowImporter = Importer.Import;
		}
		#endregion Constructors

		#region IDisposable members
		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting
		/// unmanaged resources.
		/// </summary>
		public void Dispose() {
			if (!_disposed) {
				_disposed = true;
				_firstFillContinuation.Close();
				_filledFirst.Close();
				_moveCompleted.Close();
				if (_subscriptions != null) {
					foreach (var subscription in _subscriptions) {
						try {
							if (subscription.Enabled) {
								subscription.Disable();
							}
							subscription.Unsubscribe();
						} catch (ObjectDisposedException) {
						} catch (SubscriptionException) {
						} finally {
							subscription.Dispose();
						}
					}
					_subscriptions = null;
				}
			}
		}
		#endregion IDisposable members

		#region Private properties
		bool IsFeeding =>
			(CurrentState == DirectFeedState.FeedingMove) || (CurrentState == DirectFeedState.FeedingAttached);

		#endregion Private properties

		#region Public Properties
		/// <summary>
		/// Gets or sets the TAM system to work with.
		/// </summary>
		/// <remarks>
		/// The TAM system should be <see cref="TamSystem.Boot"/>ed
		/// before calling any further methods of the <see cref="DirectFeed"/>.
		/// </remarks>
		[Browsable(false)]
		public TamSystem System {
			get { return _system; }

			set {
				Transit(DirectFeedState.NoSystem);
				_system = value;
				if (_feederAxes != null) Array.ForEach(_feederAxes, axis => {
					if (axis != null) axis.MotionError -= OnAxisError;
				});
				_feederAxes = null;
				if (value != null) Transit(DirectFeedState.Unidentified);
			}
		}

		/// <summary>
		/// Gets or sets the <see cref="TableImporter"/>
		/// used by this <see cref="DirectFeed"/>.
		/// </summary>
		public TableImporter Importer { get; }

		/// <summary>
		/// Gets the number of axis currently steered.
		/// </summary>
		public int AxisCount => _feederAxes.Length;

		/// <summary>
		/// Gets the number of values transmitted to each axis.
		/// </summary>
		public int PositionDimensionality { get; private set; }

		#endregion Public Properties

		#region Private methods
		/// <summary>
		/// Chooses the tables to currently prepare.
		/// </summary>
		/// <param name="index"><c>0</c> or <c>1</c>.</param>
		void ChooseTables(int index) {
			_currentTableIndex = index;
			_currentTables = _feederAxes.Select(axis => axis.Tables[index]).ToArray();
		}

		/// <summary>
		/// Callback for use by <see cref="TableImporter.Import"/>.
		/// </summary>
		/// <param name="lineIndex">The index of the processed line in the table file.</param>
		/// <param name="rowIndex">The index of the table row to import.
		/// This value increments in subsequent calls of this callback method.</param>
		/// <param name="values">The column values of the table row to import.</param>
		/// <returns>Whether to cancel importing rows.</returns>
		bool ImportRow(uint lineIndex, uint rowIndex, params float[] values) {
			#region Save Park position initially
			if (rowIndex == 0) {
				for (int axisIndex = 0; axisIndex < _feederAxes.Length; ++axisIndex) {
					// set park positions to values found in first row
					_feederAxes[axisIndex].ParkPosition = values[axisIndex * PositionDimensionality];
				}
				return false;
			}
			#endregion Save Park position initially

			#region Save Start position initially
			if (rowIndex == 1) {
				_rowOffset = 1;
				for (int axisIndex = 0; axisIndex < _feederAxes.Length; ++axisIndex) {
					// set start positions to values found in first row 
					_feederAxes[axisIndex].StartPosition = values[axisIndex * PositionDimensionality];
				}
			}
			#endregion Save Start position initially

			// translate to current table row index
			_tableRowIndex = rowIndex - _rowOffset;

			if (_tableRowIndex >= _length) {
				#region Table swapping
				// data does not fit in the current table -> prepare a new table

				// inform the state machine that filling is done
				if (CurrentState == DirectFeedState.FillingFirst) {
					Transit(DirectFeedState.FilledFirst);
				}

				// wait until moving is permitted
				_firstFillContinuation.WaitOne();
				if (!IsFeeding) return true;

#if DEBUG
				var watch = Stopwatch.StartNew();
#endif

				// need to attach more data
				try {
					if (CurrentState == DirectFeedState.FeedingAttached) {

						// register the next table while the feeder is already moving
						// waits until the current move is done
						// cancel when interrupted
						if (!_feeder.FeedAttachMove(_length, _currentTables)) return true;
					} else {
						// cancel when interrupted
						if (!_feeder.FeedMove()) return true;
					}
				} catch (TamException ex) {
					Transit(ex.FullMessage());
					return true;
				}

				// swap tables
				ChooseTables(1 - _currentTableIndex);
				_rowOffset = rowIndex;
				_tableRowIndex = 0;

#if DEBUG
				Transit(DirectFeedState.FeedingAttached, "Filling table " + _currentTableIndex +
					". Reserve time: " + watch.Elapsed);
#else
				// do this also when already in state FeedingAttached in order to signal swapped tables
				Transit(DirectFeedState.FeedingAttached);
#endif
				#endregion Table swapping
			}

			#region Table filling
			for (int tableIndex = 0; tableIndex < _currentTables.Length; tableIndex++) {
				var axisValues = new TamValue32[_feeder.ColumnCount];
				int offset = tableIndex * PositionDimensionality;
				var positionSize = _feederAxes[tableIndex].PositionSize;
				var value = values[offset];
				if (positionSize == 1) {

					// float
					axisValues[0] = value;
				} else {
					TamValue32Pair pair;
					if (_feederAxes[tableIndex].Subscriber[0].ValueType == typeof(double)) {

						// double
						pair = Float64.ConvertToTamValue32Pair(value);
					} else {

						// Float 40
						pair = Float40.ConvertToTamValue32Pair(value);
					}
					axisValues[0] = pair.Value0;
					axisValues[1] = pair.Value1;
				}

				for (int i = positionSize; i < axisValues.Length; i++) {
					axisValues[i] = values[offset + i - positionSize + 1];
				}

#if !TEST_TABLEFEEDER
				_currentTables[tableIndex].Fill(_subscriptions[tableIndex], _tableRowIndex, axisValues);
#else
				// sending straight lines only
				currentTables[tableIndex].Fill(subscriptions[tableIndex], tableRowIndex, (float)rowIndex,
					(float)rowIndex, (float)rowIndex);
#endif
			}
			#endregion Table filling

			return (CurrentState != DirectFeedState.FillingFirst) && !IsFeeding;
		}

		void ImportCompleted(IAsyncResult result) {
			#region Check import result
			try {
				_rowImporter.EndInvoke(result);
			} catch (TamException ex) {

				// thrown in ImportRow
				// show message from failure
				Transit(CurrentState == DirectFeedState.FillingFirst ? DirectFeedState.Enabled : CurrentState,
					ex.FullMessage());
				return;
			} catch (IndexOutOfRangeException) {

				// thrown in ImportRow
				Transit(CurrentState == DirectFeedState.FillingFirst ? DirectFeedState.Enabled : CurrentState,
					"Not enough rows specified in import.");
			} catch (InvalidOperationException ex) {
				Transit(CurrentState == DirectFeedState.FillingFirst ? DirectFeedState.Enabled : CurrentState,
					ex.FullMessage());
				return;
			}
			#endregion Check import result

			#region Fill further

			// the file import may be completed, but the feeder needs not to be moving already
			switch (CurrentState) {
				case DirectFeedState.FillingFirst:
					Transit(DirectFeedState.FilledFirst);

					_firstFillContinuation.WaitOne();
					if (!IsFeeding) return;

					goto case DirectFeedState.FeedingMove;

				case DirectFeedState.FeedingMove:
					try {

						// simple move
						_feeder.FeedMove();

						// wait until move done, break if interrupted
						if (!_feeder.FeedLast()) break;
					} catch (TamException ex) {
						Transit(ex.FullMessage());
						break;
					}

					// only transit if not interrupted via decoupling
					if (IsFeeding) Transit(DirectFeedState.FeedingLast);
					break;

				case DirectFeedState.FeedingAttached:
					// attach last table
					try {
						// wait until attach move done, break if interrupted
						if (!_feeder.FeedAttachMove(_tableRowIndex + 1, _currentTables)) break;

						// wait until move done, break if interrupted
						if (!_feeder.FeedLast()) break;
					} catch (TamException ex) {
						Transit(ex.FullMessage());
						break;
					}

					// only transit if not interrupted via decoupling
					if (IsFeeding) Transit(DirectFeedState.FeedingLast);
					break;
			}
			#endregion Fill further
		}

		/// <summary>
		/// Stops the system in case of axis errors.
		/// </summary>
		void OnAxisError(object sender, EventArgs e) {
#if !TEST_TABLEFEEDER
			var axis = (DirectFeedAxis)sender;
			var axisError = axis.Axis.ReadAxisError();
			var deviceError = axis.Axis.Drive.ReadDeviceError();
			var errorToShow = deviceError == DeviceErrorIdentification.None ? axisError.ToString() : deviceError.ToString();
			OnTransition(CurrentState, axis.Axis.Drive.Name + " has error " + errorToShow + ".");

			if (GetTransitionsInverse(DirectFeedState.ErrorStopping).Contains(CurrentState)) {
				Transit(DirectFeedState.ErrorStopping);
				try {
					// only stop other axes, simultaneously
					var requests = new TamRequest[_feederAxes.Length];
					for (int i = 0; i < _feederAxes.Length; i++) {
						var feederAxis = _feederAxes[i];
						if (feederAxis != axis) {
							requests[i] = feederAxis.Axis.Stop(true);
						}

					}
					for (int i = 0; i < _feederAxes.Length; i++) {
						requests[i]?.WaitForSuccess(StopTimeout);
					}
					StopFeeding(null);
					Transit(DirectFeedState.Enabled);
					Disable(null, DisableTimeout);
				} catch (TamException ex) {
					Transit(ex.FullMessage());
				}
			}
#endif
		}
		#endregion Private methods

		#region Public Methods
		/// <summary>
		/// Identifies the stations of the application.
		/// </summary>
		/// <param name="axisNames">The configurations for one or multiple axes to use.</param>
		/// <returns>
		/// 	<see langword="true"/> if the expected system was found;
		/// otherwise, <see langword="false"/>.
		/// </returns>
		public bool Identify(params string[] axisNames) {

			// Don't continue when no axes where configured
			if ((axisNames == null) || (axisNames.Length == 0)) {
				Transit("No axes configured.");
				return false;
			}

			Transit(DirectFeedState.Identifying);

			// reset state
			_feeder = null;
			_feederAxes = new IDirectFeedAxis[axisNames.Length];

			TamAdapter adapterToUse = null;

			var messageBuilder = new StringBuilder();

			for (int axisIndex = 0; axisIndex < axisNames.Length; axisIndex++) {
				var axisName = axisNames[axisIndex];

				// loop over all adapters of the TAM system (however, lock to one adapter)
				foreach (TamAdapter adapter in System.Adapters.Where(adapter =>
					adapterToUse == null || adapter == adapterToUse)) {

					// loop over all axes within the adapter
					foreach (var axis in adapter.AsDepthFirstLeaves<TamAxis>()) {
						if (axis.Name == axisName) {

							// lock adapter
							adapterToUse = adapter;

							if (_feeder == null) {
								#region Get table feeder
								var periphery = (adapter as IPeripheryLayoutOwner)?.Periphery;
								if (periphery?.Contains(PeripheryDeviceIdentification.PacketFeeders) ?? false) {
									#region Sanity checks
									if (!(periphery.GetDevice(
										PeripheryDeviceIdentification.PacketFeeders) is PacketFeedersDevice tableFeedersDevice)) {
										messageBuilder.AppendLine("No table feeders device found.");
										break;
									}
									#endregion Sanity checks

									// use the whole SDRAM, equally portioned for all tables
									uint packetCapacity = tableFeedersDevice.PacketCapacity;
									_length = packetCapacity / (2 * (uint)axisNames.Length);

									_feeder = tableFeedersDevice[0];
								}
								#endregion Get table feeder
							}

							try {
								#region Create the tables to work with

								PacketFeederTable table1 = _feeder[2 * axisIndex];
								table1.WriteTableAddress(2 * (uint)axisIndex * _length);

								PacketFeederTable table2 = _feeder[2 * axisIndex + 1];
								table2.WriteTableAddress((2 * (uint)axisIndex + 1) * _length);

								// note: column counts are written at a later stage
								#endregion Create the tables to work with

								_feederAxes[axisIndex] = new DirectFeedAxis(axis, table1, table2);
							} catch (TamException ex) {
								messageBuilder.Append(ex.FullMessage());
							} catch (NotSupportedException ex) {
								messageBuilder.Append(ex.FullMessage());
							}
							_feederAxes[axisIndex].MotionError += OnAxisError;
						}
					}
				}
				if (_feederAxes[axisIndex] == null) {
					messageBuilder.AppendLine(Invariant($"Axis {axisName} not found.{1}"));
				}
			}

			if (messageBuilder.Length == 0) {
				Transit(DirectFeedState.Identified);
				return true;
			} else {
				Transit(DirectFeedState.Unidentified, messageBuilder.ToString());
				return false;
			}
		}

		/// <summary>
		/// Creates the DirectFeed subscription.
		/// </summary>
		public void Subscribe() {

			// mutual exclusion with Unsubscribe
			lock (_syncPoint) {
				Transit(DirectFeedState.Subscribing);

				try {
					// create the subscriptions
					var subscriptionManager = _feeder.Link.SubscriptionManager;
					_subscriptions = new ISubscription[_feederAxes.Length];
					for (int axisIndex = 0; axisIndex < _subscriptions.Length; axisIndex++) {

#if !TEST_PACKETFEEDER
						_subscriptions[axisIndex] = subscriptionManager.Subscribe(_feeder,
							_feederAxes[axisIndex].Subscriber);
#else
					// test back to PC
					var s = subscriptionManager.Subscribe(feeder);
					subscriptions[axisIndex] = s;
					if (axisIndex == subscriptions.Length - 1) {
						s.PacketSender.PacketsAvailable += (o, a) => {
							var packets = s.PacketSender.Dequeue();
							foreach (var packet in packets) {
								testWriter.WriteLine(packet[3].AsSingle);
							}
						};
					}
					s.Enable();
#endif
					}

					Transit(DirectFeedState.Subscribed);
				} catch (KeyNotFoundException ex) {
					Transit(ex.FullMessage());
				} catch (ArgumentException ex) {
					Transit(ex.FullMessage());
				} catch (SubscriptionException ex) {
					Transit(ex.FullMessage());
				}
			}
		}

		/// <summary>
		/// Unsubscribes and disposes the DirectFeed subscriptions.
		/// </summary>
		/// <param name="message">The message to associate with the unsubscribing transition,
		/// or <see langword="null"/> if there is no message.</param>
		public void Unsubscribe(string message) {

			// mutual exclusion with Subscribe
			lock (_syncPoint) {
				Transit(DirectFeedState.Unsubscribing, message);

				if (_subscriptions == null) {
					// nothing to unsubscribe
					Transit(DirectFeedState.Identified);

				} else {
					// assume we are got here from the PacketFeedState.Subscribed state
					try {
						foreach (var subscription in _subscriptions) {
							if (subscription != null) {
								if (subscription.Enabled) {
									subscription.Disable();
								}
								subscription.Unsubscribe();
								subscription.Dispose();
							}
						}
						_subscriptions = null;

						// unsubscribing was successful
						Transit(DirectFeedState.FillingPrepared);

					} catch (ObjectDisposedException ex) {
						// remain partially unsubscribed
						Transit(ex.FullMessage());

					} catch (SubscriptionException ex) {
						// remain partially unsubscribed
						Transit(ex.FullMessage());
					}
				}
			}
		}

		/// <summary>
		/// Prepares the filling.
		/// </summary>
		public void PrepareFilling() {
			Transit(DirectFeedState.PrepareFilling);

			// update caches
			PositionDimensionality = Settings.Default.PositionDimensionality;

			// if at least one position is double precision, the column count needs to be one more than dimensionality.
			var positionSizeMax = _feederAxes.Select(axis => axis.PositionSize).Max();

			// reset feeder
			_feeder.Stop();

			_feeder.ColumnCount = PositionDimensionality + positionSizeMax - 1;
		}

		/// <summary>
		/// Completes the filling preparation.
		/// </summary>
		/// <param name="message">The error message, or <see langword="null"/> in case of success.</param>
		public void EndPrepareFilling(string message) {
			if (message == null) {
				var tickTime = _feeder.Link.TickPeriod.TotalSeconds;

				// Set downsampling to the ratio between stations and links isochronous base periods,
				// assuming all stations have the same isochronous base period.
				var device = _feederAxes[0].Axis.Drive;
				int deviceIsoPeriod = (int)Math.Round(device.IsochronousBasePeriod.TotalSeconds / tickTime);
				var downsampling = deviceIsoPeriod / _feeder.Link.IsochronousBasePeriod;

				// set handshake to the duration to send one table, plus one second extra
				_feeder.HandshakeTimeout = 1000 + 1000 * (int)(_length * downsampling *
					_feeder.Link.IsochronousBasePeriod * tickTime);

				try {
					_feeder.DownsamplingControl.WriteValue(downsampling);
					Transit(DirectFeedState.FillingPrepared);
				} catch (TamException ex) {
					Transit(DirectFeedState.Identified, ex.FullMessage());
				}
			} else {
				Transit(DirectFeedState.Identified, message);
			}
		}

		/// <summary>
		/// Enables the axes of the application.
		/// </summary>
		/// <param name="tamRequestTimeout">
		/// The time to wait for request completion, or <see cref="Timeout.InfiniteTimeSpan"/> to wait indefinitely.
		/// </param>
		public void Enable(TimeSpan tamRequestTimeout) {

			// sync with Disable
			lock (_syncPoint) {
				Transit(DirectFeedState.Enabling);

				try {
					for (int axisIndex = 0; axisIndex < _feederAxes.Length; ++axisIndex) {
						_feederAxes[axisIndex].Enable(tamRequestTimeout);
					}

					Transit(DirectFeedState.Enabled);

				} catch (TamException ex) {

					// remain partially enabled
					Transit("Enabling failed: " + ex.FullMessage());
				}
			}
		}

		/// <summary>
		/// Disables the axes of the application.
		/// </summary>
		/// <param name="message">The message to associate with the disabling transition,
		/// or <see langword="null"/> if there is no message.</param>
		/// <param name="tamRequestTimeout">
		/// The time to wait for request completion, or <see cref="Timeout.InfiniteTimeSpan"/> to wait indefinitely.
		/// </param>
		public void Disable(string message, TimeSpan tamRequestTimeout) {

			// sync with Enable
			lock (_syncPoint) {
				Transit(DirectFeedState.Disabling, message);

				try {
					for (int axisIndex = 0; axisIndex < _feederAxes.Length; ++axisIndex) {
						_feederAxes[axisIndex].Disable(tamRequestTimeout);
					}

					Transit(DirectFeedState.Subscribed);

				} catch (TamException ex) {
					// remain partially disabled
					Transit("Disabling failed: " + ex.FullMessage());
				}
			}
		}

		/// <summary>
		/// Fills the packet feeder by importing table data from a file.
		/// </summary>
		public void FillFirst() {
			Transit(DirectFeedState.FillingFirst);
			ChooseTables(0);

			// begin to import the table data on another thread				
			_rowImporter.BeginInvoke(ImportCompleted, null);

			// wait until the first table is filled
			_filledFirst.WaitOne();
		}

		/// <summary>
		/// Moves the axes to their respective <see cref="IDirectFeedAxis.StartPosition"/>s.
		/// </summary>
		/// <param name="tamRequestTimeout">
		/// The time to wait for request completion, or <see cref="Timeout.InfiniteTimeSpan"/> to wait indefinitely.
		/// </param>
		/// <remarks>The start position is retrieved from the first column in the table file.</remarks>
		public void Reposition(TimeSpan tamRequestTimeout) {
			Transit(DirectFeedState.Repositioning);

			try {
				DirectFeedAxis.RepositionAll(tamRequestTimeout, _feederAxes);

				Transit(DirectFeedState.Repositioned);

			} catch (TamException ex) {
				Transit("Repositioning failed: " + ex.FullMessage());
			}
		}

		/// <summary>
		/// Starts sending the first packet of the table repeatedly.
		/// </summary>
		public void Send() {
			Transit(DirectFeedState.StartFeeding);

			try {
				_feeder.FeedFirst(_tableRowIndex, _currentTables);
				Transit(DirectFeedState.FeedingFirst);
			} catch (TamException ex) {
				StopFeeding(ex.FullMessage());
			}
		}

		/// <summary>
		/// Stops the sending of table packets.
		/// </summary>
		/// <param name="message">The message to associate with the disabling transition,
		/// or <see langword="null"/> if there is no message.</param>
		public void StopFeeding(string message) {
			Transit(DirectFeedState.StopFeeding, message);
			try {
				_feeder.Stop();
				Transit(DirectFeedState.FeedingStopped);
			} catch (TamException ex) {
				Transit("Could not stop the feeder: " + ex.FullMessage());
			}
		}

		/// <summary>
		/// Couples the axes of the application with DirectFeed.
		/// </summary>
		public void Couple() {
			Transit(DirectFeedState.Coupling);

			try {
				for (int axisIndex = 0; axisIndex < _feederAxes.Length; ++axisIndex) {
					_feederAxes[axisIndex].Couple();
				}

				Transit(DirectFeedState.Coupled);

			} catch (TamException ex) {
				Transit("Coupling failed: " + ex.FullMessage());
			}
		}

		/// <summary>
		/// Stops the axes of the application, which implies decoupling from DirectFeed.
		/// </summary>
		/// <param name="tamRequestTimeout">
		/// The time to wait for request completion, or <see cref="Timeout.InfiniteTimeSpan"/> to wait indefinitely.
		/// </param>
		public void Stop(TimeSpan tamRequestTimeout) {
			DirectFeedState stoppingState;
			DirectFeedState successState;

			switch (CurrentState) {
				case DirectFeedState.Parking:
					stoppingState = DirectFeedState.StopRepositioning;
					successState = DirectFeedState.Enabled;
					break;

				case DirectFeedState.Repositioning:
					stoppingState = DirectFeedState.StopRepositioning;
					successState = DirectFeedState.FilledFirst;
					break;

				default:
					stoppingState = DirectFeedState.Stopping;
					successState = DirectFeedState.Stopped;
					break;
			}

			Transit(stoppingState);

			try {
				DirectFeedAxis.StopAll(tamRequestTimeout, _feederAxes);

				Transit(successState);

			} catch (TamException ex) {
				// remain stopping
				Transit("Stopping failed: " + ex.FullMessage());
			}
		}

		/// <summary>
		/// Sends subsequent packets of the table.
		/// </summary>
		public void StartMove() => Transit(DirectFeedState.FeedingMove);

		/// <summary>
		/// Sends subsequent packets of the table and waits until all packets are sent.
		/// </summary>
		/// <param name="timeout">
		/// The time to wait for completion, or <see cref="Timeout.InfiniteTimeSpan"/> to wait indefinitely.
		/// </param>
		public void Move(TimeSpan timeout) {
			StartMove();
			_moveCompleted.WaitOne(timeout);
		}

		/// <summary>
		/// Moves the axes to their respective <see cref="IDirectFeedAxis.ParkPosition"/>s.
		/// </summary>
		/// <param name="tamRequestTimeout">
		/// The time to wait for request completion, or <see cref="Timeout.InfiniteTimeSpan"/> to wait indefinitely.
		/// </param>
		public void Park(TimeSpan tamRequestTimeout) {
			Transit(DirectFeedState.Parking);

			try {
				DirectFeedAxis.ParkAll(tamRequestTimeout, _feederAxes);

				Transit(DirectFeedState.Parked);

			} catch (TamException ex) {
				Transit(DirectFeedState.Enabled, "Parking failed: " + ex.FullMessage());
			}
		}

		/// <summary>
		/// Applies the path in the specified file to the specified devices.
		/// </summary>
		/// <param name="path">The path.</param>
		/// <param name="topologyMonitor">If set, called with the topology instance created in this method.</param>
		/// <param name="timeout">
		/// The timeout or <see cref="Timeout.InfiniteTimeSpan"/> to wait indefinitely. Used for all operations
		/// potentially timing out.
		/// </param>
		/// <param name="repeat">Whether to repeat feeding the configured paths.</param>
		/// <param name="axes">The names of the axes to use.</param>
		/// <exception cref="TamException">Exception occurred in the TAM layer.</exception>
		/// <remarks>
		/// 	<para>Loads the <see cref="TamTopologyConfiguration.TamConfigurationPath"/>, if specified.</para>
		/// 	<para>Assumes the first line contains headers, and all others positional data.</para>
		/// 	<para>Ignores the first row, and expects the other rows being sorted by axes, then by positions,
		/// compliant to <see cref="AxisCount"/> and <see cref="PositionDimensionality"/>.</para>
		/// </remarks>
		public void Execute(string path, Action<TamTopology> topologyMonitor, TimeSpan timeout, bool repeat,
			params string[] axes) {

			using (var topology = new TamTopology()) {
				topologyMonitor?.Invoke(topology);

				// give some feedback what's going on
#if DEBUG
				Transition += (s, e) => Console.WriteLine("{0} {1}", e.State, e.Message);
#endif

				// setup system
				System = topology.AddLocalSystem();
				System.Initialize();
				var config = TamTopologyConfiguration.TamConfigurationPath;
				if (!string.IsNullOrEmpty(config)) topology.Load(config);

				// setup feeder
				Identify(axes);
				PrepareFilling();
				Importer.FileName = path;
				Importer.RowRanges.ReplaceParsed("10-", NumberFormatInfo.InvariantInfo);
				Importer.ColumnRanges.ReplaceParsed("2-" + (AxisCount * PositionDimensionality + 1), NumberFormatInfo.InvariantInfo);
				EndPrepareFilling(null);
				Subscribe();
				Enable(timeout);

				do {
					FillFirst();
					Park(timeout);
					Reposition(timeout);
					Send();
					Couple();

					// move
					Move(timeout);

					// tear down
					if (CurrentState == DirectFeedState.FeedingLast) {
						Stop(timeout);
						StopFeeding(null);
						if (!repeat) {
							FillFirst();
							Park(timeout);
							Disable(null, timeout);
						}
					} else {
						repeat = false;

						// the error handler is active, or control was taken
						while (CurrentState != DirectFeedState.Subscribed) Thread.Sleep(100);
					}
				} while (repeat);

				Unsubscribe(null);

				System = null;
			}
		}

		/// <inheritdoc cref="Execute(string, Action{TamTopology}, TimeSpan, bool, string[])"/>
		public void Execute(string path, Action<TamTopology> topologyMonitor, bool repeat, params string[] axes) =>
			Execute(path, topologyMonitor, Timeout.InfiniteTimeSpan, repeat, axes);

		#endregion Public Methods

		#region PacketFeedStateMachine overrides
		/// <inheritdoc/>
		protected override void OnTransition(DirectFeedState state, string message) {

			// set/reset signals based on states
			switch (state) {
				case DirectFeedState.FillingFirst:
					_firstFillContinuation.Reset();
					_filledFirst.Reset();
					break;

				case DirectFeedState.FilledFirst:
					_filledFirst.Set();
					break;

				case DirectFeedState.Coupling:
					_moveCompleted.Reset();
					break;

				case DirectFeedState.Disabling:
				case DirectFeedState.FeedingStopped:
				case DirectFeedState.FeedingMove:
					_firstFillContinuation.Set();
					_filledFirst.Set();
					break;

				case DirectFeedState.FeedingLast:
				case DirectFeedState.Stopping:
					_firstFillContinuation.Set();
					_filledFirst.Set();
					_moveCompleted.Set();
					break;
			}

			base.OnTransition(state, message);
		}
		#endregion PacketFeedStateMachine overrides
	}
}
