// Copyright © 2016 Triamec Motion AG

using System.ComponentModel;
using System.Windows.Forms;

namespace Triamec.Tam.Samples.UI {
	/// <summary>
	/// <see cref="DirectFeedButton"/> which takes into account whether the machine is working on a workpiece.
	/// </summary>
	internal class PartDisabledButton : DirectFeedButton {
		/// <summary>
		/// Gets or sets the <see cref="CheckBox" /> enabling the operator to tell the application whether the
		/// machine is working on a part.
		/// </summary>
		/// <remarks>The button doesn't listen to check changes.</remarks>
		[Category("DirectFeed")]
		[Description("A check box enabling the operator to tell the application whether the machine is working on a part.")]
		public CheckBox WorkingOnPartIndicator { get; set; }

		/// <inheritdoc/>
		public override void Update(DirectFeedState state, bool isLoop) {
			CheckBox workingOnPartIndicator = WorkingOnPartIndicator;

			// Be conservative when there is no indicator check box
			if (DirectFeedStateMachine.IsLikelyToBeInWorkPiece(state) &&
				((workingOnPartIndicator == null) || workingOnPartIndicator.Checked)) {

				Enabled = false;
			} else {
				base.Update(state, isLoop);
			}
		}
	}
}
