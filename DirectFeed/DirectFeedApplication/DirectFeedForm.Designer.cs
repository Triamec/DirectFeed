// Copyright © 2007 Triamec Motion AG

using System;
using Triamec.Diagnostics;

namespace Triamec.Tam.Samples.UI {
	partial class DirectFeedForm {
		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose(bool disposing) {
			if (disposing) {
				try {
					DirectFeed directFeed = _directFeedControl.DirectFeedBusiness;
					if ((directFeed != null) && (directFeed.System != null)) {
						// withdraw the TamSystem from the business logic
						directFeed.System = null;
					}

					if ((_explorerForm != null) && (_explorerForm.Topology != null)) {
						// withdraw the TamTopology from the TamExplorerForm
						_explorerForm.Topology = null;
					}

				} catch (InvalidOperationException ex) {
					Log.Complain($"Could not correctly dispose {nameof(DirectFeedForm)}.", ex);
				}

				if (_explorerForm != null) {
					_explorerForm.Closing -= OnTamExplorerClosing;

					if (_explorerForm.Visible) this._explorerForm.Close();
					_explorerForm.Dispose();
				}
				if (_topology != null) _topology.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
            System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
            System.Windows.Forms.ToolStripMenuItem preferencesToolStripMenuItem;
            System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
            System.Windows.Forms.ToolStripMenuItem viewToolStripMenuItem;
            System.Windows.Forms.MenuStrip menuStrip;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DirectFeedForm));
            this._resetToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._systemExplorerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._drudge = new System.ComponentModel.BackgroundWorker();
            this._versionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._directFeedControl = new Triamec.Tam.Samples.UI.DirectFeedControl();
            fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            preferencesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            menuStrip = new System.Windows.Forms.MenuStrip();
            menuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // fileToolStripMenuItem
            // 
            fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            preferencesToolStripMenuItem,
            toolStripSeparator1,
            this._resetToolStripMenuItem,
            this._exitToolStripMenuItem});
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            fileToolStripMenuItem.Text = "&File";
            // 
            // preferencesToolStripMenuItem
            // 
            preferencesToolStripMenuItem.Name = "preferencesToolStripMenuItem";
            preferencesToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            preferencesToolStripMenuItem.Text = "&Preferences…";
            preferencesToolStripMenuItem.Click += new System.EventHandler(this.OnMenuItemPreferencesClick);
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new System.Drawing.Size(149, 6);
            // 
            // _resetToolStripMenuItem
            // 
            this._resetToolStripMenuItem.Name = "_resetToolStripMenuItem";
            this._resetToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this._resetToolStripMenuItem.Text = "&Reset";
            this._resetToolStripMenuItem.Click += new System.EventHandler(this.OnMenuItemFileResetClick);
            // 
            // _exitToolStripMenuItem
            // 
            this._exitToolStripMenuItem.Name = "_exitToolStripMenuItem";
            this._exitToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this._exitToolStripMenuItem.Text = "E&xit";
            this._exitToolStripMenuItem.Click += new System.EventHandler(this.OnMenuItemFileExitClick);
            // 
            // viewToolStripMenuItem
            // 
            viewToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._systemExplorerToolStripMenuItem});
            viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            viewToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            viewToolStripMenuItem.Text = "&View";
            // 
            // _systemExplorerToolStripMenuItem
            // 
            this._systemExplorerToolStripMenuItem.Name = "_systemExplorerToolStripMenuItem";
            this._systemExplorerToolStripMenuItem.Size = new System.Drawing.Size(157, 22);
            this._systemExplorerToolStripMenuItem.Text = "&System Explorer";
            this._systemExplorerToolStripMenuItem.Click += new System.EventHandler(this.OnMenuItemViewSystemExplorerClick);
            // 
            // _drudge
            // 
            this._drudge.DoWork += new System.ComponentModel.DoWorkEventHandler(this.OnDoWork);
            this._drudge.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.OnRunWorkerCompleted);
            // 
            // menuStrip
            // 
            menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            fileToolStripMenuItem,
            viewToolStripMenuItem,
            this._versionToolStripMenuItem});
            menuStrip.Location = new System.Drawing.Point(0, 0);
            menuStrip.Name = "menuStrip";
            menuStrip.Size = new System.Drawing.Size(356, 24);
            menuStrip.TabIndex = 1;
            // 
            // _versionToolStripMenuItem
            // 
            this._versionToolStripMenuItem.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this._versionToolStripMenuItem.Enabled = false;
            this._versionToolStripMenuItem.Name = "_versionToolStripMenuItem";
            this._versionToolStripMenuItem.Size = new System.Drawing.Size(57, 20);
            this._versionToolStripMenuItem.Text = "Version";
            // 
            // _directFeedControl
            // 
            this._directFeedControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this._directFeedControl.Location = new System.Drawing.Point(0, 24);
            this._directFeedControl.MinimumSize = new System.Drawing.Size(344, 344);
            this._directFeedControl.Name = "_directFeedControl";
            this._directFeedControl.DirectFeedBusiness = null;
            this._directFeedControl.Size = new System.Drawing.Size(356, 738);
            this._directFeedControl.TabIndex = 0;
            // 
            // DirectFeedForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(356, 762);
            this.Controls.Add(this._directFeedControl);
            this.Controls.Add(menuStrip);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = menuStrip;
            this.MinimumSize = new System.Drawing.Size(364, 696);
            this.Name = "DirectFeedForm";
            this.Text = "Feeding Packets";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OnFormClosing);
            menuStrip.ResumeLayout(false);
            menuStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion Windows Form Designer generated code

		private Triamec.Tam.Samples.UI.DirectFeedControl _directFeedControl;
		private System.ComponentModel.BackgroundWorker _drudge;
		private System.Windows.Forms.ToolStripMenuItem _versionToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem _systemExplorerToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem _resetToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem _exitToolStripMenuItem;
	}
}
