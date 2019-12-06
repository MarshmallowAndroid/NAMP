namespace NieRAutomataMusicTest
{
    partial class MainWindow
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.musicPath = new System.Windows.Forms.TextBox();
            this.playPosition = new System.Windows.Forms.TrackBar();
            this.PositionUpdate = new System.Windows.Forms.Timer(this.components);
            this.pauseButton = new System.Windows.Forms.Button();
            this.songList = new System.Windows.Forms.ListBox();
            this.trackList = new System.Windows.Forms.ListView();
            this.keyColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.valueColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.mainTracksPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.overlayTracksPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.playButton = new System.Windows.Forms.Button();
            this.fadeProgress = new System.Windows.Forms.ProgressBar();
            this.loopCheckBox = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.playPosition)).BeginInit();
            this.SuspendLayout();
            // 
            // musicPath
            // 
            this.musicPath.Location = new System.Drawing.Point(12, 12);
            this.musicPath.Name = "musicPath";
            this.musicPath.Size = new System.Drawing.Size(627, 20);
            this.musicPath.TabIndex = 0;
            this.musicPath.Text = "C:\\Users\\jacob\\Desktop\\test";
            // 
            // playPosition
            // 
            this.playPosition.Location = new System.Drawing.Point(12, 453);
            this.playPosition.Maximum = 1000;
            this.playPosition.Name = "playPosition";
            this.playPosition.Size = new System.Drawing.Size(627, 45);
            this.playPosition.TabIndex = 2;
            this.playPosition.TickFrequency = 0;
            this.playPosition.Scroll += new System.EventHandler(this.PlayPosition_Scroll);
            // 
            // PositionUpdate
            // 
            this.PositionUpdate.Tick += new System.EventHandler(this.PositionUpdate_Tick);
            // 
            // pauseButton
            // 
            this.pauseButton.Location = new System.Drawing.Point(288, 517);
            this.pauseButton.Name = "pauseButton";
            this.pauseButton.Size = new System.Drawing.Size(75, 23);
            this.pauseButton.TabIndex = 5;
            this.pauseButton.Text = "Pause";
            this.pauseButton.UseVisualStyleBackColor = true;
            this.pauseButton.Click += new System.EventHandler(this.PauseButton_Click);
            // 
            // songList
            // 
            this.songList.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.songList.FormattingEnabled = true;
            this.songList.ItemHeight = 18;
            this.songList.Location = new System.Drawing.Point(12, 91);
            this.songList.Name = "songList";
            this.songList.Size = new System.Drawing.Size(251, 148);
            this.songList.TabIndex = 6;
            this.songList.SelectedIndexChanged += new System.EventHandler(this.SongList_SelectedIndexChanged);
            // 
            // trackList
            // 
            this.trackList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.keyColumn,
            this.valueColumn});
            this.trackList.FullRowSelect = true;
            this.trackList.HideSelection = false;
            this.trackList.Location = new System.Drawing.Point(269, 91);
            this.trackList.Name = "trackList";
            this.trackList.Size = new System.Drawing.Size(251, 148);
            this.trackList.TabIndex = 7;
            this.trackList.UseCompatibleStateImageBehavior = false;
            this.trackList.View = System.Windows.Forms.View.Details;
            // 
            // keyColumn
            // 
            this.keyColumn.Text = "TrackName";
            this.keyColumn.Width = 68;
            // 
            // valueColumn
            // 
            this.valueColumn.Text = "File";
            this.valueColumn.Width = 179;
            // 
            // mainTracksPanel
            // 
            this.mainTracksPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.mainTracksPanel.Location = new System.Drawing.Point(13, 245);
            this.mainTracksPanel.Name = "mainTracksPanel";
            this.mainTracksPanel.Size = new System.Drawing.Size(250, 147);
            this.mainTracksPanel.TabIndex = 8;
            // 
            // overlayTracksPanel
            // 
            this.overlayTracksPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.overlayTracksPanel.Location = new System.Drawing.Point(269, 245);
            this.overlayTracksPanel.Name = "overlayTracksPanel";
            this.overlayTracksPanel.Size = new System.Drawing.Size(251, 147);
            this.overlayTracksPanel.TabIndex = 8;
            // 
            // playButton
            // 
            this.playButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.playButton.Location = new System.Drawing.Point(526, 91);
            this.playButton.Name = "playButton";
            this.playButton.Size = new System.Drawing.Size(113, 102);
            this.playButton.TabIndex = 5;
            this.playButton.Text = "Play";
            this.playButton.UseVisualStyleBackColor = true;
            this.playButton.Click += new System.EventHandler(this.PlayButton_Click);
            // 
            // fadeProgress
            // 
            this.fadeProgress.Location = new System.Drawing.Point(12, 398);
            this.fadeProgress.Name = "fadeProgress";
            this.fadeProgress.Size = new System.Drawing.Size(252, 23);
            this.fadeProgress.TabIndex = 9;
            this.fadeProgress.Visible = false;
            // 
            // loopCheckBox
            // 
            this.loopCheckBox.AutoSize = true;
            this.loopCheckBox.Checked = true;
            this.loopCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.loopCheckBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.loopCheckBox.Location = new System.Drawing.Point(526, 199);
            this.loopCheckBox.Name = "loopCheckBox";
            this.loopCheckBox.Size = new System.Drawing.Size(64, 24);
            this.loopCheckBox.TabIndex = 10;
            this.loopCheckBox.Text = "Loop";
            this.loopCheckBox.UseVisualStyleBackColor = true;
            this.loopCheckBox.CheckedChanged += new System.EventHandler(this.LoopCheckBox_CheckedChanged);
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(651, 584);
            this.Controls.Add(this.loopCheckBox);
            this.Controls.Add(this.fadeProgress);
            this.Controls.Add(this.overlayTracksPanel);
            this.Controls.Add(this.mainTracksPanel);
            this.Controls.Add(this.trackList);
            this.Controls.Add(this.songList);
            this.Controls.Add(this.playButton);
            this.Controls.Add(this.pauseButton);
            this.Controls.Add(this.playPosition);
            this.Controls.Add(this.musicPath);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "MainWindow";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "NieR:Automata Music Player";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainWindow_FormClosing);
            this.Load += new System.EventHandler(this.MainWindow_Load);
            ((System.ComponentModel.ISupportInitialize)(this.playPosition)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox musicPath;
        private System.Windows.Forms.TrackBar playPosition;
        private System.Windows.Forms.Timer PositionUpdate;
        private System.Windows.Forms.Button pauseButton;
        private System.Windows.Forms.ListBox songList;
        private System.Windows.Forms.ListView trackList;
        private System.Windows.Forms.ColumnHeader keyColumn;
        private System.Windows.Forms.ColumnHeader valueColumn;
        private System.Windows.Forms.FlowLayoutPanel mainTracksPanel;
        private System.Windows.Forms.FlowLayoutPanel overlayTracksPanel;
        private System.Windows.Forms.Button playButton;
        private System.Windows.Forms.ProgressBar fadeProgress;
        private System.Windows.Forms.CheckBox loopCheckBox;
    }
}

