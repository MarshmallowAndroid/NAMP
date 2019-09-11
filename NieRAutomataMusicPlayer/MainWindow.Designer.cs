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
            this.mainTracks = new System.Windows.Forms.FlowLayoutPanel();
            this.overlayTracks = new System.Windows.Forms.FlowLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)(this.playPosition)).BeginInit();
            this.SuspendLayout();
            // 
            // musicPath
            // 
            this.musicPath.Location = new System.Drawing.Point(12, 12);
            this.musicPath.Name = "musicPath";
            this.musicPath.Size = new System.Drawing.Size(366, 20);
            this.musicPath.TabIndex = 0;
            this.musicPath.Text = "F:\\NieRAutomata Modding\\NME2-0.5-alpha-x64\\x64\\separated";
            // 
            // playPosition
            // 
            this.playPosition.Location = new System.Drawing.Point(12, 446);
            this.playPosition.Maximum = 1000;
            this.playPosition.Name = "playPosition";
            this.playPosition.Size = new System.Drawing.Size(693, 45);
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
            this.pauseButton.Location = new System.Drawing.Point(321, 542);
            this.pauseButton.Name = "pauseButton";
            this.pauseButton.Size = new System.Drawing.Size(75, 23);
            this.pauseButton.TabIndex = 5;
            this.pauseButton.Text = "Pause";
            this.pauseButton.UseVisualStyleBackColor = true;
            this.pauseButton.Click += new System.EventHandler(this.PauseButton_Click);
            // 
            // songList
            // 
            this.songList.FormattingEnabled = true;
            this.songList.Location = new System.Drawing.Point(12, 91);
            this.songList.Name = "songList";
            this.songList.Size = new System.Drawing.Size(251, 147);
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
            this.trackList.Size = new System.Drawing.Size(251, 147);
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
            // mainTracks
            // 
            this.mainTracks.Location = new System.Drawing.Point(12, 244);
            this.mainTracks.Name = "mainTracks";
            this.mainTracks.Size = new System.Drawing.Size(251, 147);
            this.mainTracks.TabIndex = 8;
            // 
            // overlayTracks
            // 
            this.overlayTracks.Location = new System.Drawing.Point(269, 244);
            this.overlayTracks.Name = "overlayTracks";
            this.overlayTracks.Size = new System.Drawing.Size(251, 147);
            this.overlayTracks.TabIndex = 8;
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(717, 654);
            this.Controls.Add(this.overlayTracks);
            this.Controls.Add(this.mainTracks);
            this.Controls.Add(this.trackList);
            this.Controls.Add(this.songList);
            this.Controls.Add(this.pauseButton);
            this.Controls.Add(this.playPosition);
            this.Controls.Add(this.musicPath);
            this.Name = "MainWindow";
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
        private System.Windows.Forms.FlowLayoutPanel mainTracks;
        private System.Windows.Forms.FlowLayoutPanel overlayTracks;
    }
}

