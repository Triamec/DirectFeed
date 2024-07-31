// Copyright © 2007 Triamec Motion AG

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using Triamec.Tam.Periphery;
using Triamec.Tam.Registers;
using Triamec.Tam.Registers.Tags;
using Triamec.Tam.Requests;
using Triamec.Tam.Subscriptions;
using Triamec.TriaLink;

namespace Triamec.Tam.Samples {
	/// <summary>
	/// An axis of the <see cref="DirectFeed"/> application.
	/// </summary>
	internal sealed class DirectFeedAxis : IDirectFeedAxis {
		#region Fields

		/// <summary>
		/// Flag to indicate whether <see cref="ITamDevice.AddStateObserver"/>
		/// must be called before working with <see cref="TamRequest"/>s.
		/// </summary>
		bool _needsStateObserver = true;

		#endregion Fields

		#region Constructor
		/// <summary>
		/// Initializes a new instance of the <see cref="DirectFeedAxis"/> class.
		/// </summary>
		/// <param name="axis">The axis to create the <see cref="DirectFeedAxis"/> from.</param>
		/// <param name="table1">The first table feeding this axis.</param>
		/// <param name="table2">The second table feeding this axis, while the first table is refilled.</param>
		/// <exception cref="ArgumentNullException">
		/// 	<paramref name="axis"/>, <paramref name="table1"/> or <paramref name="table2"/> is
		/// <see langword="null"/>.
		/// </exception>
		/// <exception cref="TamException">
		/// 	<para>A communication timeout occurred.</para>
		/// 	<para>-or-</para>
		/// 	<para>A register is not present for the specified axis.</para>
		/// </exception>
		public DirectFeedAxis(TamAxis axis, PacketFeederTable table1, PacketFeederTable table2) {

			Axis = axis ?? throw new ArgumentNullException(nameof(axis));
			if (table1 == null) throw new ArgumentNullException(nameof(table1));
			if (table2 == null) throw new ArgumentNullException(nameof(table2));

			// Most drives get integrated into a real time control system. Accessing them via TAM API like we do here is considered
			// a secondary use case. Tell the axis that we're going to take control. Otherwise, the axis might reject our commands.
			// You should not do this, though, when this application is about to access the drive via the PCI interface.
			axis.ControlSystemTreatment.Override(enabled: true);

			// handle on another thread, as long running stuff should not be left here
			axis.Drive.AnyTransition += (s, e) => new EventHandler<StateTransition>(OnAnyTransition).BeginInvoke(s, e, null,
				null);

			#region Find subscription destinations

			IRegisterComponent axisRegister = axis.Register;

			ISubscribable timestampDestination;
			ISubscribable positionDestination;
			ISubscribable velocityDestination;
			ISubscribable accelerationDestination;

			timestampDestination = axisRegister.FindTaggedComponent(AxisCommandTags.PathPlannerCouplingTimestamp)
				as ISubscribable;

			positionDestination = axisRegister.FindTaggedComponent(AxisCommandTags.PathPlannerNewPosition)
				as ISubscribable;

			PositionSize = unchecked((int)(positionDestination.Size / RegisterComponent.BYTES_PER_REGISTER_WORD));

			velocityDestination = axisRegister.FindTaggedComponent(AxisCommandTags.PathPlannerNewVelocity)
				as ISubscribable;

			accelerationDestination = axisRegister.FindTaggedComponent(AxisCommandTags.PathPlannerNewAcceleration)
				as ISubscribable;

			Subscriber = new Subscriber(timestampDestination, false, positionDestination, velocityDestination,
				accelerationDestination);

			#endregion Find subscription destinations

			// create read only table view
			Tables = Array.AsReadOnly(new[] { table1, table2 });
		}
		#endregion Constructor

		#region Private methods
		/// <summary>
		/// Called when the axis had an error.
		/// </summary>
		[MethodImpl(MethodImplOptions.Synchronized)]
		void OnAnyTransition(object s, StateTransition e) {
			var handler = MotionError;
			if ((handler != null) &&
				(((e.AxisRequests[Axis.AxisIndex] != null) && (e.AxisRequests[Axis.AxisIndex].Termination == TamRequestResolution.AxisError)) ||
				 (e.Errors.DeviceError != DeviceErrorIdentification.None))) {

				handler(this, EventArgs.Empty);
			}
		}
		#endregion Private methods

		#region Public and private static methods
		/// <summary>
		/// Repositions all given axes simultaneously
		/// to their respective <see cref="StartPosition"/>.
		/// </summary>
		/// <param name="tamRequestTimeout">
		/// The time to wait for request completion, or <see cref="Timeout.InfiniteTimeSpan"/> to wait indefinitely.
		/// </param>
		/// <param name="feederAxes">The feeder axes to reposition.</param>
		/// <exception cref="ArgumentNullException">
		/// 	<para><paramref name="feederAxes"/> is <see langword="null"/>.</para>
		/// </exception>
		/// <exception cref="ArgumentException">
		/// 	<para>At least one element of <paramref name="feederAxes"/> is <see langword="null"/>.</para>
		/// </exception>
		/// <exception cref="TamException">
		/// 	<para>The moving failed.</para>
		/// 	<para>A TAM request timeout occurred.</para>
		/// </exception>
		public static void RepositionAll(TimeSpan tamRequestTimeout, params IDirectFeedAxis[] feederAxes) =>
			PositionAllSequential(tamRequestTimeout, true, feederAxes);
		//PositionAll(tamRequestTimeout, true, feederAxes);

		/// <summary>
		/// Repositions all given axes simultaneously
		/// to their respective <see cref="ParkPosition"/>.
		/// </summary>
		/// <param name="tamRequestTimeout">
		/// The time to wait for request completion, or <see cref="Timeout.InfiniteTimeSpan"/> to wait indefinitely.
		/// </param>
		/// <param name="feederAxes">The feeder axes to reposition.</param>
		/// <exception cref="ArgumentNullException">
		/// 	<para><paramref name="feederAxes"/> is <see langword="null"/>.</para>
		/// </exception>
		/// <exception cref="ArgumentException">
		/// 	<para>At least one element of <paramref name="feederAxes"/> is <see langword="null"/>.</para>
		/// </exception>
		/// <exception cref="TamException">
		/// 	<para>The moving failed.</para>
		/// 	<para>A TAM request timeout occurred.</para>
		/// </exception>
		public static void ParkAll(TimeSpan tamRequestTimeout, params IDirectFeedAxis[] feederAxes) =>
			PositionAllSequential(tamRequestTimeout, false, feederAxes);
		//  PositionAll(tamRequestTimeout, false, feederAxes);

		///// <summary>
		///// Positions all given axes simultaneously
		///// to their respective <see cref="StartPosition"/>.
		///// </summary>
		///// <param name="tamRequestTimeout">
		///// The time to wait for request completion, or <see cref="Timeout.InfiniteTimeSpan"/> to wait indefinitely.
		///// </param>
		///// <param name="startPosition">If set to <see langword="true"/>, uses <see cref="IPacketFeedAxis.StartPosition"/>;
		///// otherwise, uses <see cref="IPacketFeedAxis.ParkPosition"/>.</param>
		///// <param name="feederAxes">The feeder axes to reposition.</param>
		///// <exception cref="ArgumentNullException"><paramref name="feederAxes"/> is <see langword="null"/>.</exception>
		///// <exception cref="ArgumentException">
		///// At least one element of <paramref name="feederAxes"/> is <see langword="null"/>.
		///// </exception>
		///// <exception cref="TamException">
		///// 	<para>The moving failed.</para>
		///// 	<para>A TAM request timeout occurred.</para>
		///// </exception>
		//static void PositionAll(TimeSpan tamRequestTimeout, bool startPosition,
		//    params IPacketFeedAxis[] feederAxes) {

		//    #region sanity checks
		//    if (feederAxes == null) throw new ArgumentNullException("feederAxes");
		//    #endregion sanity checks

		//    int axesCount = feederAxes.Length;
		//    TamRequest[] requests = new TamRequest[axesCount];

		//    for (int axisIndex = 0; axisIndex < axesCount; ++axisIndex) {
		//        IPacketFeedAxis feederAxis = feederAxes[axisIndex];

		//        #region sanity checks
		//        if (feederAxis == null) throw new ArgumentException("Feeder axis " + axisIndex + " is null.", "feederAxes");
		//        #endregion sanity checks

		//        requests[axisIndex] = feederAxis.Axis.MoveAbsolute(
		//            startPosition ? feederAxis.StartPosition : feederAxis.ParkPosition, PathPlannerDirection.Shortest);
		//    }

		//    for (int axisIndex = 0; axisIndex < axesCount; ++axisIndex) {
		//        IPacketFeedAxis feederAxis = feederAxes[axisIndex];
		//        TamRequest request = requests[axisIndex];

		//        if (!request.WaitForSuccess(tamRequestTimeout)) {
		//            throw new TamException($"Could not move axis {feederAxis.Axis.ShortDescription}: timeout");
		//        }

		//        if (request.Termination != TamRequestResolution.Completed) {
		//            var position = startPosition ? feederAxis.StartPosition : feederAxis.ParkPosition;
		//            throw new TamException(
		//                $"Could not move {feederAxis.Axis.ShortDescription} to {position}: {request.Termination}");
		//        }
		//    }
		//}

		/// <summary>
		/// Positions all given axes sequential
		/// to their respective <see cref="StartPosition"/>.
		/// </summary>
		/// <param name="tamRequestTimeout">
		/// The time to wait for request completion, or <see cref="Timeout.InfiniteTimeSpan"/> to wait indefinitely.
		/// </param>
		/// <param name="startPosition">If set to <see langword="true"/>, uses <see cref="IDirectFeedAxis.StartPosition"/>;
		/// otherwise, uses <see cref="IDirectFeedAxis.ParkPosition"/>.</param>
		/// <param name="feederAxes">The feeder axes to reposition.</param>
		/// <exception cref="ArgumentNullException"><paramref name="feederAxes"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException">
		/// At least one element of <paramref name="feederAxes"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="TamException">
		/// 	<para>The moving failed.</para>
		/// 	<para>A TAM request timeout occurred.</para>
		/// </exception>
		static void PositionAllSequential(TimeSpan tamRequestTimeout, bool startPosition,
			params IDirectFeedAxis[] feederAxes) {

			#region sanity checks
			if (feederAxes == null) throw new ArgumentNullException(nameof(feederAxes));
			#endregion sanity checks

			int axesCount = feederAxes.Length;

			if (startPosition) {
				// move axis by axis (0,1,2,...) sequentially 
				for (int axisIndex = 0; axisIndex < axesCount; ++axisIndex) {
					IDirectFeedAxis feederAxis = feederAxes[axisIndex] ??
						throw new ArgumentException("Feeder axis " + axisIndex + " is null.", nameof(feederAxes));

					var position = feederAxis.StartPosition;
					feederAxis.Axis.MoveAbsolute(position, PathPlannerDirection.Shortest)
								   .WaitForSuccess(tamRequestTimeout);
				}
			} else {
				// move axis by axis (last, last-1, .. 0) sequentially 
				for (int axisIndex = axesCount - 1; axisIndex >= 0; --axisIndex) {
					IDirectFeedAxis feederAxis = feederAxes[axisIndex] ??
						throw new ArgumentException("Feeder axis " + axisIndex + " is null.", nameof(feederAxes));

					var position = feederAxis.ParkPosition;
					feederAxis.Axis.MoveAbsolute(position, PathPlannerDirection.Shortest)
								   .WaitForSuccess(tamRequestTimeout);
				}
			}
		}

		/// <summary>
		/// Stops all given axes simultaneously.
		/// </summary>
		/// <param name="tamRequestTimeout">
		/// The time to wait for request completion, or <see cref="Timeout.InfiniteTimeSpan"/> to wait indefinitely.
		/// </param>
		/// <param name="feederAxes">The feeder axes.</param>
		/// <exception cref="TamException">
		/// 	<para>The stopping failed.</para>
		/// 	<para>A TAM request timeout occurred.</para>
		/// </exception>
		public static void StopAll(TimeSpan tamRequestTimeout, params IDirectFeedAxis[] feederAxes) {
			int axesCount = feederAxes.Length;
			var requests = new TamRequest[axesCount];

			for (int axisIndex = 0; axisIndex < axesCount; ++axisIndex) {
				IDirectFeedAxis feederAxis = feederAxes[axisIndex];
				requests[axisIndex] = feederAxis.Axis.Stop();
			}

			for (int axisIndex = 0; axisIndex < axesCount; ++axisIndex) {
				requests[axisIndex].WaitForSuccess(tamRequestTimeout);
			}
		}
		#endregion Public and private static methods

		#region IDirectFeedAxis members

		// <inheritdoc/>
		public event EventHandler MotionError;

		// <inheritdoc/>
		public TamAxis Axis { get; }

		// <inheritdoc/>
		public ISubscriber Subscriber { get; }

		// <inheritdoc/>
		public float StartPosition { get; set; }

		// <inheritdoc/>
		public float ParkPosition { get; set; }

		// <inheritdoc/>
		public EncoderSource EncoderSource { get; }

		// <inheritdoc/>
		public IList<PacketFeederTable> Tables { get; }

		// <inheritdoc/>
		public int PositionSize { get; }

		// <inheritdoc/>
		public void Enable(TimeSpan tamRequestTimeout) {
			if (_needsStateObserver) {
				Axis.Drive.AddStateObserver(this);
				_needsStateObserver = false;
			}

			Axis.Drive.SetOperational();
			Axis.Control(AxisControlCommands.ResetErrorAndEnable).WaitForSuccess(tamRequestTimeout);

			//Axis.Drive.TamaManager.IsochronousVM.EnableAndVerify();
		}

		// <inheritdoc/>
		public void Disable(TimeSpan tamRequestTimeout) {
			Axis.AbortTamRequests();

			if ((Axis.Drive.ReadDeviceState() == DeviceState.Operational) &&
				(Axis.ReadAxisState() != AxisState.Disabled)) {

				Axis.Control(AxisControlCommands.Disable).WaitForSuccess(tamRequestTimeout);
			}

			//Axis.Drive.TamaManager.IsochronousVM.DisableAndVerify();

			Axis.Drive.SwitchOff().WaitForSuccess(tamRequestTimeout);
		}

		// <inheritdoc/>
		public void Couple() {
			TamRequest request = Axis.CoupleIn(true);

			if (!request.WaitForExecuting(500)) {
				throw new TamException($"Could not couple {Axis.ShortDescription}: timeout");
			}
			if (request.Termination != TamRequestResolution.None) {
				throw new TamException($"Could not couple {Axis.ShortDescription}: {request.Termination}");
			}
		}

		// <inheritdoc/>
		public void StopMotion(TimeSpan tamRequestTimeout) => Axis.Stop().WaitForSuccess(tamRequestTimeout);

		#endregion IDirectFeedAxis members
	}
}
