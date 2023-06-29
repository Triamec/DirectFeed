// Copyright © 2007 Triamec Motion AG

namespace Triamec.Tam.Samples.UI {
	partial class DirectFeedControl {

		#region Component Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
            System.Windows.Forms.GroupBox consoleGroupBox;
            System.Windows.Forms.PictureBox triamecLogo;
            Triamec.Tam.Samples.UI.DirectFeedButton parkButton;
            Triamec.Tam.Samples.UI.DirectFeedButton autoLoopButton;
            Triamec.Tam.Samples.UI.DirectFeedButton fillButton;
            Triamec.Tam.Samples.UI.DirectFeedButton repositionButton;
            Triamec.Tam.Samples.UI.DirectFeedButton stopFeedingButton;
            Triamec.Tam.Samples.UI.DirectFeedButton sendButton;
            Triamec.Tam.Samples.UI.DirectFeedButton moveButton;
            Triamec.Tam.Samples.UI.DirectFeedButton coupleButton;
            Triamec.Tam.Samples.UI.DirectFeedButton unsubscribeButton;
            Triamec.Tam.Samples.UI.DirectFeedButton subscribeButton;
            Triamec.Tam.Samples.UI.DirectFeedButton enableButton;
            Triamec.Tam.Samples.UI.DirectFeedButton disableButton;
            Triamec.Tam.Samples.UI.DirectFeedButton importButton;
            Triamec.Tam.Samples.UI.DirectFeedButton stopButton;
            System.Windows.Forms.CheckBox workingOnPartCheckBox;
            this._console = new System.Windows.Forms.RichTextBox();
            this._decoupleButton = new Triamec.Tam.Samples.UI.PartDisabledButton();
            this._drudge = new System.ComponentModel.BackgroundWorker();
            this._feederLoopGroupBox = new System.Windows.Forms.GroupBox();
            this._loopCheckBox = new System.Windows.Forms.CheckBox();
            consoleGroupBox = new System.Windows.Forms.GroupBox();
            triamecLogo = new System.Windows.Forms.PictureBox();
            parkButton = new Triamec.Tam.Samples.UI.DirectFeedButton();
            autoLoopButton = new Triamec.Tam.Samples.UI.DirectFeedButton();
            fillButton = new Triamec.Tam.Samples.UI.DirectFeedButton();
            repositionButton = new Triamec.Tam.Samples.UI.DirectFeedButton();
            stopFeedingButton = new Triamec.Tam.Samples.UI.DirectFeedButton();
            sendButton = new Triamec.Tam.Samples.UI.DirectFeedButton();
            moveButton = new Triamec.Tam.Samples.UI.DirectFeedButton();
            coupleButton = new Triamec.Tam.Samples.UI.DirectFeedButton();
            unsubscribeButton = new Triamec.Tam.Samples.UI.DirectFeedButton();
            subscribeButton = new Triamec.Tam.Samples.UI.DirectFeedButton();
            enableButton = new Triamec.Tam.Samples.UI.DirectFeedButton();
            disableButton = new Triamec.Tam.Samples.UI.DirectFeedButton();
            importButton = new Triamec.Tam.Samples.UI.DirectFeedButton();
            stopButton = new Triamec.Tam.Samples.UI.DirectFeedButton();
            workingOnPartCheckBox = new System.Windows.Forms.CheckBox();
            consoleGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(triamecLogo)).BeginInit();
            this._feederLoopGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // consoleGroupBox
            // 
            consoleGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            consoleGroupBox.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            consoleGroupBox.Controls.Add(this._console);
            consoleGroupBox.Location = new System.Drawing.Point(3, 618);
            consoleGroupBox.Name = "consoleGroupBox";
            consoleGroupBox.Size = new System.Drawing.Size(350, 119);
            consoleGroupBox.TabIndex = 21;
            consoleGroupBox.TabStop = false;
            consoleGroupBox.Text = "Log";
            // 
            // _console
            // 
            this._console.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this._console.CausesValidation = false;
            this._console.DetectUrls = false;
            this._console.Dock = System.Windows.Forms.DockStyle.Fill;
            this._console.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._console.Location = new System.Drawing.Point(3, 16);
            this._console.Name = "_console";
            this._console.ReadOnly = true;
            this._console.Size = new System.Drawing.Size(344, 100);
            this._console.TabIndex = 20;
            this._console.TabStop = false;
            this._console.Text = "";
            this._console.WordWrap = false;
            // 
            // triamecLogo
            // 
            triamecLogo.Image = global::Triamec.Tam.Samples.Properties.Resources.Triamec_Logo_47x48;
            triamecLogo.Location = new System.Drawing.Point(293, 5);
            triamecLogo.Name = "triamecLogo";
            triamecLogo.Size = new System.Drawing.Size(47, 48);
            triamecLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            triamecLogo.TabIndex = 6;
            triamecLogo.TabStop = false;
            // 
            // parkButton
            // 
            parkButton.Enabled = false;
            parkButton.Font = new System.Drawing.Font("Verdana", 14.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            parkButton.IsLoopLocked = true;
            parkButton.Location = new System.Drawing.Point(13, 79);
            parkButton.Margin = new System.Windows.Forms.Padding(10, 5, 10, 5);
            parkButton.Name = "parkButton";
            parkButton.Size = new System.Drawing.Size(152, 48);
            parkButton.TabIndex = 5;
            parkButton.Text = "&Park";
            parkButton.ToState = Triamec.Tam.Samples.DirectFeedState.Parking;
            parkButton.UseVisualStyleBackColor = true;
            parkButton.Click2 += new System.EventHandler(this.OnParkButtonClick);
            // 
            // autoLoopButton
            // 
            autoLoopButton.Enabled = false;
            autoLoopButton.Font = new System.Drawing.Font("Verdana", 14.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            autoLoopButton.IsLoopLocked = true;
            autoLoopButton.Location = new System.Drawing.Point(16, 179);
            autoLoopButton.Margin = new System.Windows.Forms.Padding(10, 5, 10, 5);
            autoLoopButton.Name = "autoLoopButton";
            autoLoopButton.Size = new System.Drawing.Size(152, 48);
            autoLoopButton.TabIndex = 4;
            autoLoopButton.Text = "&Auto Loop";
            autoLoopButton.ToState = Triamec.Tam.Samples.DirectFeedState.FillingFirst;
            autoLoopButton.UseVisualStyleBackColor = true;
            autoLoopButton.Click2 += new System.EventHandler(this.OnAutoLoopButtonClick);
            // 
            // fillButton
            // 
            fillButton.Enabled = false;
            fillButton.Font = new System.Drawing.Font("Verdana", 14.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            fillButton.IsLoopLocked = true;
            fillButton.Location = new System.Drawing.Point(13, 21);
            fillButton.Margin = new System.Windows.Forms.Padding(10, 5, 10, 5);
            fillButton.Name = "fillButton";
            fillButton.Size = new System.Drawing.Size(152, 48);
            fillButton.TabIndex = 0;
            fillButton.Text = "&Fill";
            fillButton.ToState = Triamec.Tam.Samples.DirectFeedState.FillingFirst;
            fillButton.UseVisualStyleBackColor = true;
            fillButton.Click2 += new System.EventHandler(this.OnFillButtonClick);
            // 
            // repositionButton
            // 
            repositionButton.Enabled = false;
            repositionButton.Font = new System.Drawing.Font("Verdana", 14.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            repositionButton.IsLoopLocked = true;
            repositionButton.Location = new System.Drawing.Point(13, 137);
            repositionButton.Margin = new System.Windows.Forms.Padding(10, 5, 10, 5);
            repositionButton.Name = "repositionButton";
            repositionButton.Size = new System.Drawing.Size(152, 48);
            repositionButton.TabIndex = 1;
            repositionButton.Text = "&Reposition";
            repositionButton.ToState = Triamec.Tam.Samples.DirectFeedState.Repositioning;
            repositionButton.UseVisualStyleBackColor = true;
            repositionButton.Click2 += new System.EventHandler(this.OnRepositionButtonClick);
            // 
            // stopFeedingButton
            // 
            stopFeedingButton.Enabled = false;
            stopFeedingButton.Font = new System.Drawing.Font("Verdana", 14.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            stopFeedingButton.IsLoopLocked = true;
            stopFeedingButton.Location = new System.Drawing.Point(185, 195);
            stopFeedingButton.Margin = new System.Windows.Forms.Padding(10, 5, 10, 5);
            stopFeedingButton.Name = "stopFeedingButton";
            stopFeedingButton.Size = new System.Drawing.Size(152, 48);
            stopFeedingButton.TabIndex = 7;
            stopFeedingButton.Text = "S&top Feed";
            stopFeedingButton.ToState = Triamec.Tam.Samples.DirectFeedState.StopFeeding;
            stopFeedingButton.UseVisualStyleBackColor = true;
            stopFeedingButton.Click2 += new System.EventHandler(this.OnStopFeedingButtonClick);
            // 
            // sendButton
            // 
            sendButton.Enabled = false;
            sendButton.Font = new System.Drawing.Font("Verdana", 14.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            sendButton.IsLoopLocked = true;
            sendButton.Location = new System.Drawing.Point(13, 195);
            sendButton.Margin = new System.Windows.Forms.Padding(10, 5, 10, 5);
            sendButton.Name = "sendButton";
            sendButton.Size = new System.Drawing.Size(152, 48);
            sendButton.TabIndex = 3;
            sendButton.Text = "S&end";
            sendButton.ToState = Triamec.Tam.Samples.DirectFeedState.StartFeeding;
            sendButton.UseVisualStyleBackColor = true;
            sendButton.Click2 += new System.EventHandler(this.OnSendButtonClick);
            // 
            // moveButton
            // 
            moveButton.Enabled = false;
            moveButton.Font = new System.Drawing.Font("Verdana", 14.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            moveButton.IsLoopLocked = true;
            moveButton.Location = new System.Drawing.Point(13, 311);
            moveButton.Margin = new System.Windows.Forms.Padding(10, 5, 10, 5);
            moveButton.Name = "moveButton";
            moveButton.Size = new System.Drawing.Size(152, 48);
            moveButton.TabIndex = 5;
            moveButton.Text = "&Move";
            moveButton.ToState = Triamec.Tam.Samples.DirectFeedState.FeedingMove;
            moveButton.UseVisualStyleBackColor = true;
            moveButton.Click2 += new System.EventHandler(this.OnMoveButtonClick);
            // 
            // coupleButton
            // 
            coupleButton.Enabled = false;
            coupleButton.Font = new System.Drawing.Font("Verdana", 14.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            coupleButton.IsLoopLocked = true;
            coupleButton.Location = new System.Drawing.Point(13, 253);
            coupleButton.Margin = new System.Windows.Forms.Padding(10, 5, 10, 5);
            coupleButton.Name = "coupleButton";
            coupleButton.Size = new System.Drawing.Size(152, 48);
            coupleButton.TabIndex = 4;
            coupleButton.Text = "&Couple";
            coupleButton.ToState = Triamec.Tam.Samples.DirectFeedState.Coupling;
            coupleButton.UseVisualStyleBackColor = true;
            coupleButton.Click2 += new System.EventHandler(this.OnCoupleButonClick);
            // 
            // unsubscribeButton
            // 
            unsubscribeButton.BackColor = System.Drawing.SystemColors.Control;
            unsubscribeButton.Enabled = false;
            unsubscribeButton.Font = new System.Drawing.Font("Verdana", 14.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            unsubscribeButton.Location = new System.Drawing.Point(188, 63);
            unsubscribeButton.Margin = new System.Windows.Forms.Padding(10, 5, 10, 5);
            unsubscribeButton.Name = "unsubscribeButton";
            unsubscribeButton.Size = new System.Drawing.Size(152, 48);
            unsubscribeButton.TabIndex = 7;
            unsubscribeButton.Tag = "";
            unsubscribeButton.Text = "&Unsubscribe";
            unsubscribeButton.ToState = Triamec.Tam.Samples.DirectFeedState.Unsubscribing;
            unsubscribeButton.UseVisualStyleBackColor = true;
            unsubscribeButton.Click2 += new System.EventHandler(this.OnUnsubscribeButtonClick);
            // 
            // subscribeButton
            // 
            subscribeButton.Enabled = false;
            subscribeButton.Font = new System.Drawing.Font("Verdana", 14.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            subscribeButton.Location = new System.Drawing.Point(16, 63);
            subscribeButton.Margin = new System.Windows.Forms.Padding(10, 5, 10, 5);
            subscribeButton.Name = "subscribeButton";
            subscribeButton.Size = new System.Drawing.Size(152, 48);
            subscribeButton.TabIndex = 1;
            subscribeButton.Tag = "";
            subscribeButton.Text = "Su&bscribe";
            subscribeButton.ToState = Triamec.Tam.Samples.DirectFeedState.Subscribing;
            subscribeButton.UseVisualStyleBackColor = true;
            subscribeButton.Click2 += new System.EventHandler(this.OnSubscribeButtonClick);
            // 
            // enableButton
            // 
            enableButton.Enabled = false;
            enableButton.Font = new System.Drawing.Font("Verdana", 14.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            enableButton.Location = new System.Drawing.Point(16, 121);
            enableButton.Margin = new System.Windows.Forms.Padding(10, 5, 10, 5);
            enableButton.Name = "enableButton";
            enableButton.Size = new System.Drawing.Size(152, 48);
            enableButton.TabIndex = 2;
            enableButton.Tag = "";
            enableButton.Text = "&Enable";
            enableButton.ToState = Triamec.Tam.Samples.DirectFeedState.Enabling;
            enableButton.UseVisualStyleBackColor = true;
            enableButton.Click2 += new System.EventHandler(this.OnEnableButtonClick);
            // 
            // disableButton
            // 
            disableButton.BackColor = System.Drawing.SystemColors.Control;
            disableButton.Enabled = false;
            disableButton.Font = new System.Drawing.Font("Verdana", 14.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            disableButton.IsLoopLocked = true;
            disableButton.Location = new System.Drawing.Point(188, 121);
            disableButton.Margin = new System.Windows.Forms.Padding(10, 5, 10, 5);
            disableButton.Name = "disableButton";
            disableButton.Size = new System.Drawing.Size(152, 48);
            disableButton.TabIndex = 6;
            disableButton.Tag = "";
            disableButton.Text = "&Disable";
            disableButton.ToState = Triamec.Tam.Samples.DirectFeedState.Disabling;
            disableButton.UseVisualStyleBackColor = true;
            disableButton.Click2 += new System.EventHandler(this.OnDisableButtonClick);
            // 
            // importButton
            // 
            importButton.Enabled = false;
            importButton.Font = new System.Drawing.Font("Verdana", 14.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            importButton.Location = new System.Drawing.Point(16, 5);
            importButton.Margin = new System.Windows.Forms.Padding(10, 5, 10, 5);
            importButton.Name = "importButton";
            importButton.Size = new System.Drawing.Size(152, 48);
            importButton.TabIndex = 0;
            importButton.Tag = "";
            importButton.Text = "&Import…";
            importButton.ToState = Triamec.Tam.Samples.DirectFeedState.PrepareFilling;
            importButton.UseVisualStyleBackColor = true;
            importButton.Click += new System.EventHandler(this.OnImportButtonClick);
            // 
            // stopButton
            // 
            stopButton.Enabled = false;
            stopButton.Font = new System.Drawing.Font("Verdana", 14.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            stopButton.Location = new System.Drawing.Point(185, 137);
            stopButton.Margin = new System.Windows.Forms.Padding(10, 5, 10, 5);
            stopButton.Name = "stopButton";
            stopButton.Size = new System.Drawing.Size(152, 48);
            stopButton.TabIndex = 2;
            stopButton.Text = "&Stop";
            stopButton.ToState = Triamec.Tam.Samples.DirectFeedState.StopRepositioning;
            stopButton.UseVisualStyleBackColor = true;
            stopButton.Click2 += new System.EventHandler(this.OnStopButtonClick);
            // 
            // workingOnPartCheckBox
            // 
            workingOnPartCheckBox.AutoSize = true;
            workingOnPartCheckBox.Checked = true;
            workingOnPartCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            workingOnPartCheckBox.Location = new System.Drawing.Point(185, 331);
            workingOnPartCheckBox.Name = "workingOnPartCheckBox";
            workingOnPartCheckBox.Size = new System.Drawing.Size(102, 17);
            workingOnPartCheckBox.TabIndex = 31;
            workingOnPartCheckBox.Text = "Working on part";
            workingOnPartCheckBox.UseVisualStyleBackColor = true;
            workingOnPartCheckBox.CheckedChanged += new System.EventHandler(this.OnWorkingOnPartCheckedChanged);
            // 
            // _decoupleButton
            // 
            this._decoupleButton.Enabled = false;
            this._decoupleButton.Font = new System.Drawing.Font("Verdana", 14.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._decoupleButton.Location = new System.Drawing.Point(185, 253);
            this._decoupleButton.Margin = new System.Windows.Forms.Padding(10, 5, 10, 5);
            this._decoupleButton.Name = "_decoupleButton";
            this._decoupleButton.Size = new System.Drawing.Size(152, 48);
            this._decoupleButton.TabIndex = 6;
            this._decoupleButton.Text = "Dec&ouple";
            this._decoupleButton.ToState = Triamec.Tam.Samples.DirectFeedState.Stopping;
            this._decoupleButton.UseVisualStyleBackColor = true;
            this._decoupleButton.WorkingOnPartIndicator = workingOnPartCheckBox;
            this._decoupleButton.Click2 += new System.EventHandler(this.OnStopButtonClick);
            // 
            // _drudge
            // 
            this._drudge.DoWork += new System.ComponentModel.DoWorkEventHandler(this.OnDoWork);
            this._drudge.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.OnWorkCompleted);
            // 
            // _feederLoopGroupBox
            // 
            this._feederLoopGroupBox.Controls.Add(workingOnPartCheckBox);
            this._feederLoopGroupBox.Controls.Add(parkButton);
            this._feederLoopGroupBox.Controls.Add(this._loopCheckBox);
            this._feederLoopGroupBox.Controls.Add(fillButton);
            this._feederLoopGroupBox.Controls.Add(repositionButton);
            this._feederLoopGroupBox.Controls.Add(stopFeedingButton);
            this._feederLoopGroupBox.Controls.Add(sendButton);
            this._feederLoopGroupBox.Controls.Add(moveButton);
            this._feederLoopGroupBox.Controls.Add(coupleButton);
            this._feederLoopGroupBox.Controls.Add(stopButton);
            this._feederLoopGroupBox.Controls.Add(this._decoupleButton);
            this._feederLoopGroupBox.Location = new System.Drawing.Point(3, 235);
            this._feederLoopGroupBox.Name = "_feederLoopGroupBox";
            this._feederLoopGroupBox.Size = new System.Drawing.Size(350, 377);
            this._feederLoopGroupBox.TabIndex = 3;
            this._feederLoopGroupBox.TabStop = false;
            this._feederLoopGroupBox.Text = "Feeder Loop";
            // 
            // _loopCheckBox
            // 
            this._loopCheckBox.AutoSize = true;
            this._loopCheckBox.Enabled = false;
            this._loopCheckBox.Font = new System.Drawing.Font("Verdana", 14.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))));
            this._loopCheckBox.Location = new System.Drawing.Point(257, 33);
            this._loopCheckBox.Name = "_loopCheckBox";
            this._loopCheckBox.Size = new System.Drawing.Size(80, 27);
            this._loopCheckBox.TabIndex = 8;
            this._loopCheckBox.Text = "L&oop";
            this._loopCheckBox.UseVisualStyleBackColor = true;
            // 
            // DirectFeedControl
            // 
            this.Controls.Add(autoLoopButton);
            this.Controls.Add(this._feederLoopGroupBox);
            this.Controls.Add(unsubscribeButton);
            this.Controls.Add(subscribeButton);
            this.Controls.Add(consoleGroupBox);
            this.Controls.Add(enableButton);
            this.Controls.Add(disableButton);
            this.Controls.Add(triamecLogo);
            this.Controls.Add(importButton);
            this.MinimumSize = new System.Drawing.Size(356, 691);
            this.Name = "DirectFeedControl";
            this.Size = new System.Drawing.Size(356, 740);
            consoleGroupBox.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(triamecLogo)).EndInit();
            this._feederLoopGroupBox.ResumeLayout(false);
            this._feederLoopGroupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion Component Designer generated code

		private System.Windows.Forms.RichTextBox _console;
		private System.ComponentModel.BackgroundWorker _drudge;
		private System.Windows.Forms.CheckBox _loopCheckBox;
		private System.Windows.Forms.GroupBox _feederLoopGroupBox;
		private PartDisabledButton _decoupleButton;

	}
}
