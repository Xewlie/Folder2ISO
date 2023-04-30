using System.ComponentModel;
using System.Diagnostics;
using Export;

namespace Folder2ISO;

public class Form1 : Form
{
    private readonly IContainer components = null!;
    private readonly Folder2ISO m_creator;

    private Button buttonBrowse = null!;
    private Button buttonClose = null!;
    private Button buttonCreate = null!;
    private Button buttonSaveas = null!;

    private GroupBox groupBox = null!;

    private Label label1 = null!;
    private Label label2 = null!;
    private Label label3 = null!;
    private Label labelStatus = null!;

    private CancellationTokenSource? m_cancellationTokenSource;

    private Task? m_task;

    private ProgressBar progressBar = null!;

    private TextBox textBoxISOFile;
    private TextBox textBoxSourceFolder;
    private TextBox textBoxVolumeName;

    private delegate void SetLabelDelegate(string text);
    private delegate void SetNumericValueDelegate(int value);

    public Form1()
    {
        InitializeComponent();
        textBoxSourceFolder!.Text = "";
        textBoxISOFile!.Text = "";
        textBoxVolumeName!.Text = "";
        m_creator = new Folder2ISO();
        m_creator.Progress += creator_Progress;
        m_creator.Finish += creator_Finished;
        m_creator.Abort += creator_Abort;
    }

    private void SetLabelStatus(string? text)
    {
        labelStatus.Text = text;
        labelStatus.Refresh();
    }

    private void SetProgressValue(int value)
    {
        progressBar.Value = value;
    }

    private void SetProgressMaximum(int maximum)
    {
        progressBar.Maximum = maximum;
    }

    private void buttonCreate_Click(object? sender, EventArgs e)
    {
        if (m_task is not { IsCompleted: false, IsCanceled: false, IsFaulted: false })
        {
            textBoxSourceFolder.Text = textBoxSourceFolder.Text.Trim();
            textBoxVolumeName.Text = textBoxVolumeName.Text.Trim();
            textBoxISOFile.Text = textBoxISOFile.Text.Trim();
            if (textBoxSourceFolder.Text == "")
            {
                MessageBox.Show("Please input a Folder", "Information", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
            else if (textBoxVolumeName.Text == "")
            {
                MessageBox.Show("Please input a Name", "Information", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
            else if (textBoxISOFile.Text == "")
            {
                MessageBox.Show("Please input a ISO File Name", "Information", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
            else if (!Directory.Exists(textBoxSourceFolder.Text))
            {
                MessageBox.Show("The Folder \"" + textBoxSourceFolder.Text + "\" don't exists.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
            else if (!File.Exists(textBoxISOFile.Text) ||
                     MessageBox.Show(
                         "The ISO File \"" + textBoxISOFile.Text +
                         "\" already exists. Do you want to overwrite it?", "Information", MessageBoxButtons.YesNo,
                         MessageBoxIcon.Question) == DialogResult.Yes)
            {
                m_cancellationTokenSource = new CancellationTokenSource();
                var token = m_cancellationTokenSource.Token;
                m_task = Task.Run(() => m_creator.Folder2Iso(new Folder2ISO.Folder2ISOArgs(textBoxSourceFolder.Text, textBoxISOFile.Text, textBoxVolumeName.Text), token), token);

                labelStatus.Text = "";
                buttonCreate.Text = "Abort";
            }
        }
        else if (MessageBox.Show("Are you sure you want to abort the process?", "Abort", MessageBoxButtons.YesNo,
                     MessageBoxIcon.Question) == DialogResult.Yes)
        {
            m_cancellationTokenSource?.Cancel();
            MessageBox.Show("The ISO creating process has been stopped.", "Abort", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            buttonCreate.Enabled = true;
            buttonCreate.Text = "Create";
            progressBar.Value = 0;
            progressBar.Maximum = 0;
            labelStatus.Text = "Process not started";
            Refresh();
        }
    }

    private void creator_Abort(object sender, AbortEventArgs e) { }

    private void creator_Finished(object sender, FinishEventArgs e)
    {
        MessageBox.Show(e.Message, "Finish", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
        buttonCreate.Text = "Create";
        labelStatus.Text = "Finish";
        buttonCreate.Enabled = true;
        buttonCreate.Refresh();
        Refresh();
    }

    private void creator_Progress(object sender, ProgressEventArgs e)
    {
        if (e.Action != null)
        {
            if (!InvokeRequired)
            {
                SetLabelStatus(e.Action);
            }
            else
            {
                Invoke(new SetLabelDelegate(SetLabelStatus), e.Action);
            }
        }

        if (e.Maximum != -1)
        {
            if (!InvokeRequired)
            {
                SetProgressMaximum(e.Maximum);
            }
            else
            {
                Invoke(new SetNumericValueDelegate(SetProgressMaximum), e.Maximum);
            }
        }

        if (!InvokeRequired)
        {
            progressBar.Value = e.Current <= progressBar.Maximum ? e.Current : progressBar.Maximum;
            return;
        }

        var num = e.Current <= progressBar.Maximum ? e.Current : progressBar.Maximum;
        Invoke(new SetNumericValueDelegate(SetProgressValue), num);
    }

    private void buttonBrowse_Click(object? sender, EventArgs e)
    {
        var folderBrowserDialog = new FolderBrowserDialog();
        if (folderBrowserDialog.ShowDialog(this) == DialogResult.OK)
        {
            textBoxSourceFolder.Text = folderBrowserDialog.SelectedPath;
            textBoxVolumeName.Text = Path.GetFileName(folderBrowserDialog.SelectedPath);
        }
    }

    private void buttonSaveas_Click(object? sender, EventArgs e)
    {
        var saveFileDialog = new SaveFileDialog();
        saveFileDialog.Filter = "ISO Image Files (*.iso)|*.iso";
        if (saveFileDialog.ShowDialog(this) == DialogResult.OK) textBoxISOFile.Text = saveFileDialog.FileName;
    }

    private void buttonClose_Click(object? sender, EventArgs e)
    {
        Close();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            components?.Dispose();
        }
        base.Dispose(disposing);
    }


    private void InitializeComponent()
    {
        ComponentResourceManager resources = new ComponentResourceManager(typeof(Form1));
        label1 = new Label();
        textBoxSourceFolder = new TextBox();
        buttonBrowse = new Button();
        label2 = new Label();
        textBoxISOFile = new TextBox();
        buttonSaveas = new Button();
        textBoxVolumeName = new TextBox();
        label3 = new Label();
        buttonCreate = new Button();
        groupBox = new GroupBox();
        labelStatus = new Label();
        progressBar = new ProgressBar();
        buttonClose = new Button();
        groupBox.SuspendLayout();
        SuspendLayout();
        // 
        // label1
        // 
        label1.AutoSize = true;
        label1.Location = new Point(46, 64);
        label1.Margin = new Padding(6, 0, 6, 0);
        label1.Name = "label1";
        label1.Size = new Size(86, 32);
        label1.TabIndex = 0;
        label1.Text = "Folder:";
        label1.Click += label1_Click;
        // 
        // textBoxSourceFolder
        // 
        textBoxSourceFolder.Location = new Point(155, 57);
        textBoxSourceFolder.Margin = new Padding(6, 8, 6, 8);
        textBoxSourceFolder.Name = "textBoxSourceFolder";
        textBoxSourceFolder.Size = new Size(472, 39);
        textBoxSourceFolder.TabIndex = 1;
        // 
        // buttonBrowse
        // 
        buttonBrowse.Location = new Point(639, 46);
        buttonBrowse.Margin = new Padding(6, 8, 6, 8);
        buttonBrowse.Name = "buttonBrowse";
        buttonBrowse.Size = new Size(136, 61);
        buttonBrowse.TabIndex = 2;
        buttonBrowse.Text = "Browse";
        buttonBrowse.UseVisualStyleBackColor = true;
        buttonBrowse.Click += buttonBrowse_Click;
        // 
        // label2
        // 
        label2.AutoSize = true;
        label2.Location = new Point(43, 150);
        label2.Margin = new Padding(6, 0, 6, 0);
        label2.Name = "label2";
        label2.Size = new Size(100, 32);
        label2.TabIndex = 3;
        label2.Text = "ISO File:";
        // 
        // textBoxISOFile
        // 
        textBoxISOFile.Location = new Point(155, 143);
        textBoxISOFile.Margin = new Padding(6, 8, 6, 8);
        textBoxISOFile.Name = "textBoxISOFile";
        textBoxISOFile.Size = new Size(472, 39);
        textBoxISOFile.TabIndex = 4;
        // 
        // buttonSaveas
        // 
        buttonSaveas.Location = new Point(639, 132);
        buttonSaveas.Margin = new Padding(6, 8, 6, 8);
        buttonSaveas.Name = "buttonSaveas";
        buttonSaveas.Size = new Size(133, 61);
        buttonSaveas.TabIndex = 5;
        buttonSaveas.Text = "Save as";
        buttonSaveas.UseVisualStyleBackColor = true;
        buttonSaveas.Click += buttonSaveas_Click;
        // 
        // textBoxVolumeName
        // 
        textBoxVolumeName.Location = new Point(155, 221);
        textBoxVolumeName.Margin = new Padding(6, 8, 6, 8);
        textBoxVolumeName.Name = "textBoxVolumeName";
        textBoxVolumeName.Size = new Size(472, 39);
        textBoxVolumeName.TabIndex = 3;
        // 
        // label3
        // 
        label3.AutoSize = true;
        label3.Location = new Point(46, 228);
        label3.Margin = new Padding(6, 0, 6, 0);
        label3.Name = "label3";
        label3.Size = new Size(83, 32);
        label3.TabIndex = 6;
        label3.Text = "Name:";
        // 
        // buttonCreate
        // 
        buttonCreate.Location = new Point(194, 605);
        buttonCreate.Margin = new Padding(6, 8, 6, 8);
        buttonCreate.Name = "buttonCreate";
        buttonCreate.Size = new Size(162, 61);
        buttonCreate.TabIndex = 8;
        buttonCreate.Text = "Create";
        buttonCreate.UseVisualStyleBackColor = true;
        buttonCreate.Click += buttonCreate_Click;
        // 
        // groupBox
        // 
        groupBox.Controls.Add(labelStatus);
        groupBox.Controls.Add(progressBar);
        groupBox.Location = new Point(50, 355);
        groupBox.Margin = new Padding(6, 8, 6, 8);
        groupBox.Name = "groupBox";
        groupBox.Padding = new Padding(6, 8, 6, 8);
        groupBox.Size = new Size(722, 219);
        groupBox.TabIndex = 6;
        groupBox.TabStop = false;
        groupBox.Text = "Progress";
        // 
        // labelStatus
        // 
        labelStatus.Location = new Point(39, 147);
        labelStatus.Margin = new Padding(6, 0, 6, 0);
        labelStatus.Name = "labelStatus";
        labelStatus.Size = new Size(656, 45);
        labelStatus.TabIndex = 10;
        // 
        // progressBar
        // 
        progressBar.Location = new Point(43, 59);
        progressBar.Margin = new Padding(6, 8, 6, 8);
        progressBar.Name = "progressBar";
        progressBar.Size = new Size(652, 56);
        progressBar.TabIndex = 7;
        // 
        // buttonClose
        // 
        buttonClose.Location = new Point(439, 605);
        buttonClose.Margin = new Padding(6, 8, 6, 8);
        buttonClose.Name = "buttonClose";
        buttonClose.Size = new Size(162, 61);
        buttonClose.TabIndex = 9;
        buttonClose.Text = "Close";
        buttonClose.UseVisualStyleBackColor = true;
        buttonClose.Click += buttonClose_Click;
        // 
        // Form1
        // 
        AutoScaleDimensions = new SizeF(13F, 32F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(792, 692);
        Controls.Add(buttonClose);
        Controls.Add(groupBox);
        Controls.Add(buttonCreate);
        Controls.Add(textBoxVolumeName);
        Controls.Add(label3);
        Controls.Add(buttonSaveas);
        Controls.Add(textBoxISOFile);
        Controls.Add(label2);
        Controls.Add(buttonBrowse);
        Controls.Add(textBoxSourceFolder);
        Controls.Add(label1);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        Icon = (Icon)resources.GetObject("$this.Icon");
        Margin = new Padding(6, 8, 6, 8);
        MaximizeBox = false;
        MinimizeBox = false;
        Name = "Form1";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Folder2ISO";
        groupBox.ResumeLayout(false);
        ResumeLayout(false);
        PerformLayout();
    }

    private void label1_Click(object sender, EventArgs e)
    {

    }
}