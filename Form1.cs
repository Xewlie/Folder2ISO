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
                MessageBox.Show("Please input a Source Folder", "Information", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
            else if (textBoxVolumeName.Text == "")
            {
                MessageBox.Show("Please input a Volume Name", "Information", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
            else if (textBoxISOFile.Text == "")
            {
                MessageBox.Show("Please input a ISO File Name", "Information", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
            else if (!Directory.Exists(textBoxSourceFolder.Text))
            {
                MessageBox.Show("The Source Folder \"" + textBoxSourceFolder.Text + "\" don't exists.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
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
        label1.AutoSize = true;
        label1.Location = new Point(21, 24);
        label1.Name = "label1";
        label1.Size = new Size(89, 12);
        label1.TabIndex = 0;
        label1.Text = "Source Folder:";
        textBoxSourceFolder.Location = new Point(116, 21);
        textBoxSourceFolder.Name = "textBoxSourceFolder";
        textBoxSourceFolder.Size = new Size(220, 21);
        textBoxSourceFolder.TabIndex = 1;
        buttonBrowse.Location = new Point(347, 19);
        buttonBrowse.Name = "buttonBrowse";
        buttonBrowse.Size = new Size(75, 23);
        buttonBrowse.TabIndex = 2;
        buttonBrowse.Text = "Browse";
        buttonBrowse.UseVisualStyleBackColor = true;
        buttonBrowse.Click += buttonBrowse_Click;
        label2.AutoSize = true;
        label2.Location = new Point(21, 97);
        label2.Name = "label2";
        label2.Size = new Size(59, 12);
        label2.TabIndex = 3;
        label2.Text = "ISO File:";
        textBoxISOFile.Location = new Point(116, 94);
        textBoxISOFile.Name = "textBoxISOFile";
        textBoxISOFile.Size = new Size(220, 21);
        textBoxISOFile.TabIndex = 4;
        buttonSaveas.Location = new Point(347, 92);
        buttonSaveas.Name = "buttonSaveas";
        buttonSaveas.Size = new Size(75, 23);
        buttonSaveas.TabIndex = 5;
        buttonSaveas.Text = "Save as";
        buttonSaveas.UseVisualStyleBackColor = true;
        buttonSaveas.Click += buttonSaveas_Click;
        textBoxVolumeName.Location = new Point(116, 57);
        textBoxVolumeName.Name = "textBoxVolumeName";
        textBoxVolumeName.Size = new Size(140, 21);
        textBoxVolumeName.TabIndex = 3;
        label3.AutoSize = true;
        label3.Location = new Point(21, 60);
        label3.Name = "label3";
        label3.Size = new Size(77, 12);
        label3.TabIndex = 6;
        label3.Text = "Volume Name:";
        buttonCreate.Location = new Point(261, 227);
        buttonCreate.Name = "buttonCreate";
        buttonCreate.Size = new Size(75, 23);
        buttonCreate.TabIndex = 8;
        buttonCreate.Text = "Create";
        buttonCreate.UseVisualStyleBackColor = true;
        buttonCreate.Click += buttonCreate_Click;
        groupBox.Controls.Add(labelStatus);
        groupBox.Controls.Add(progressBar);
        groupBox.Location = new Point(23, 133);
        groupBox.Name = "groupBox";
        groupBox.Size = new Size(399, 82);
        groupBox.TabIndex = 6;
        groupBox.TabStop = false;
        groupBox.Text = "Progress";
        labelStatus.Location = new Point(18, 55);
        labelStatus.Name = "labelStatus";
        labelStatus.Size = new Size(359, 17);
        labelStatus.TabIndex = 10;
        progressBar.Location = new Point(20, 22);
        progressBar.Name = "progressBar";
        progressBar.Size = new Size(357, 21);
        progressBar.TabIndex = 7;
        buttonClose.Location = new Point(347, 227);
        buttonClose.Name = "buttonClose";
        buttonClose.Size = new Size(75, 23);
        buttonClose.TabIndex = 9;
        buttonClose.Text = "Close";
        buttonClose.UseVisualStyleBackColor = true;
        buttonClose.Click += buttonClose_Click;
        AutoScaleDimensions = new SizeF(6f, 12f);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(444, 262);
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
        MaximizeBox = false;
        MinimizeBox = false;
        Name = "Form1";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Folder2ISO";
        groupBox.ResumeLayout(false);
        ResumeLayout(false);
        PerformLayout();
    }
}