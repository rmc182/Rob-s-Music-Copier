using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

[assembly: System.Reflection.AssemblyTitle("Rob's Music Copier")]
[assembly: System.Reflection.AssemblyDescription("Copies music files referenced by M3U playlists")]
[assembly: System.Reflection.AssemblyCompany("Rob Cannell")]
[assembly: System.Reflection.AssemblyProduct("Rob's Music Copier")]
[assembly: System.Reflection.AssemblyCopyright("Copyright (c) 2026 Rob Cannell")]
[assembly: System.Reflection.AssemblyVersion("1.0.5.0")]
[assembly: System.Reflection.AssemblyFileVersion("1.0.5.0")]
[assembly: System.Reflection.AssemblyInformationalVersion("1.05")]

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new MainForm());
    }
}

internal sealed class MainForm : Form
{
    private const string AppVersion = "1.05";
    private const string PayPalUrl = "https://www.paypal.com/ncp/payment/XDAPV78QYRSXN";

    private readonly TextBox playlistBox = new TextBox();
    private readonly TextBox destinationBox = new TextBox();
    private readonly TextBox sourceRootBox = new TextBox();
    private readonly TextBox logBox = new TextBox();
    private readonly RadioButton preserveButton = new RadioButton();
    private readonly RadioButton flattenButton = new RadioButton();
    private readonly CheckBox numberTracksBox = new CheckBox();
    private readonly Button playlistBrowseButton = new Button();
    private readonly Button destinationBrowseButton = new Button();
    private readonly Button sourceRootBrowseButton = new Button();
    private readonly Button startButton = new Button();
    private readonly Button openDestinationButton = new Button();
    private readonly Button clearCopyButton = new Button();
    private readonly Label sourceRootLabel = new Label();
    private readonly Label statusLabel = new Label();
    private readonly ProgressBar progressBar = new ProgressBar();

    private readonly TextBox editPlaylistBox = new TextBox();
    private readonly TextBox oldLocationBox = new TextBox();
    private readonly TextBox newLocationBox = new TextBox();
    private readonly TextBox editDestinationBox = new TextBox();
    private readonly TextBox editLogBox = new TextBox();
    private readonly Label editOutputLabel = new Label();
    private readonly Button editDestinationBrowseButton = new Button();
    private readonly Button openCreatedPlaylistButton = new Button();
    private readonly Button createTextListButton = new Button();
    private readonly Button clearEditButton = new Button();
    private string lastCreatedPlaylistPath = String.Empty;
    private string lastCreatedTextPath = String.Empty;

    internal MainForm()
    {
        Text = "Rob's Music Copier v" + AppVersion;
        Icon = LoadApplicationIcon();
        ClientSize = new Size(744, 735);
        MinimumSize = new Size(760, 775);
        StartPosition = FormStartPosition.CenterScreen;
        Font = new Font("Segoe UI", 10F);

        TabControl tabs = new TabControl();
        tabs.SetBounds(10, 10, 724, 665);
        tabs.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        Controls.Add(tabs);

        TabPage copyTab = new TabPage("Copy Music");
        TabPage editTab = new TabPage("Edit Playlist Paths");
        tabs.TabPages.Add(copyTab);
        tabs.TabPages.Add(editTab);

        BuildCopyTab(copyTab);
        BuildEditTab(editTab);

        Button supportButton = new Button();
        supportButton.Text = "Support This Project";
        supportButton.SetBounds(20, 685, 205, 36);
        supportButton.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
        supportButton.Image = LoadPayPalLogo();
        supportButton.ImageAlign = ContentAlignment.MiddleLeft;
        supportButton.TextImageRelation = TextImageRelation.ImageBeforeText;
        supportButton.TextAlign = ContentAlignment.MiddleCenter;
        supportButton.Click += OpenPayPal;
        Controls.Add(supportButton);

        Label supportLabel = new Label();
        supportLabel.Text = "Opens PayPal in your default browser.";
        supportLabel.AutoSize = true;
        supportLabel.Location = new Point(240, 693);
        supportLabel.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
        Controls.Add(supportLabel);

        Button aboutButton = new Button();
        aboutButton.Text = "About";
        aboutButton.SetBounds(535, 685, 90, 36);
        aboutButton.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
        aboutButton.Click += ShowAbout;
        Controls.Add(aboutButton);

        Label versionLabel = new Label();
        versionLabel.Text = "Version " + AppVersion;
        versionLabel.AutoSize = true;
        versionLabel.Location = new Point(645, 693);
        versionLabel.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
        Controls.Add(versionLabel);
    }

    private void BuildCopyTab(TabPage page)
    {
        GroupBox instructions = new GroupBox();
        instructions.Text = "How to use";
        instructions.SetBounds(10, 10, 685, 145);
        page.Controls.Add(instructions);

        Label instructionText = new Label();
        instructionText.Text =
            "Copies the music files referenced by an M3U playlist to a folder of your choice.\r\n" +
            "1. Select the M3U playlist.\r\n" +
            "2. Select the destination folder.\r\n" +
            "3. Choose Preserve Folder Hierarchy or Flatten Into One Folder.\r\n" +
            "4. If flattening, choose whether to number tracks in playlist order.\r\n" +
            "5. Click Start Copy. Use Open Destination to view the copied files.";
        instructionText.AutoSize = true;
        instructionText.MaximumSize = new Size(545, 0);
        instructionText.Location = new Point(15, 25);
        instructions.Controls.Add(instructionText);

        AddBrandLogo(instructions);

        AddLabel(page, "Playlist (.m3u / .m3u8)", 10, 165);
        playlistBox.SetBounds(10, 188, 580, 28);
        page.Controls.Add(playlistBox);
        playlistBrowseButton.Text = "Browse...";
        playlistBrowseButton.SetBounds(600, 186, 95, 30);
        playlistBrowseButton.Click += BrowsePlaylist;
        page.Controls.Add(playlistBrowseButton);
        playlistBox.Leave += delegate { AutoDetectRoot(); };

        AddLabel(page, "Destination folder", 10, 230);
        destinationBox.SetBounds(10, 253, 580, 28);
        page.Controls.Add(destinationBox);
        destinationBrowseButton.Text = "Browse...";
        destinationBrowseButton.SetBounds(600, 251, 95, 30);
        destinationBrowseButton.Click += BrowseDestination;
        page.Controls.Add(destinationBrowseButton);

        GroupBox modeGroup = new GroupBox();
        modeGroup.Text = "Copy mode";
        modeGroup.SetBounds(10, 298, 685, 135);
        page.Controls.Add(modeGroup);

        preserveButton.Text = "Preserve folder hierarchy";
        preserveButton.SetBounds(18, 28, 250, 25);
        preserveButton.Checked = true;
        preserveButton.CheckedChanged += ToggleMode;
        modeGroup.Controls.Add(preserveButton);

        flattenButton.Text = "Flatten into one folder";
        flattenButton.SetBounds(18, 58, 250, 25);
        flattenButton.CheckedChanged += ToggleMode;
        modeGroup.Controls.Add(flattenButton);

        numberTracksBox.Text = "Prefix flattened files with playlist-order numbers (001, 002, ...)";
        numberTracksBox.SetBounds(42, 88, 550, 25);
        numberTracksBox.Checked = true;
        numberTracksBox.Enabled = false;
        modeGroup.Controls.Add(numberTracksBox);

        sourceRootLabel.Text = "Source music root (auto-detected)";
        sourceRootLabel.AutoSize = true;
        sourceRootLabel.Location = new Point(310, 30);
        modeGroup.Controls.Add(sourceRootLabel);

        sourceRootBox.SetBounds(310, 55, 250, 28);
        modeGroup.Controls.Add(sourceRootBox);
        sourceRootBrowseButton.Text = "Browse...";
        sourceRootBrowseButton.SetBounds(570, 53, 95, 30);
        sourceRootBrowseButton.Click += BrowseSourceRoot;
        modeGroup.Controls.Add(sourceRootBrowseButton);

        startButton.Text = "Start Copy";
        startButton.SetBounds(10, 445, 120, 36);
        startButton.Click += StartCopy;
        page.Controls.Add(startButton);

        openDestinationButton.Text = "Open Destination";
        openDestinationButton.SetBounds(145, 445, 145, 36);
        openDestinationButton.Click += OpenDestination;
        page.Controls.Add(openDestinationButton);

        clearCopyButton.Text = "Clear All";
        clearCopyButton.SetBounds(305, 445, 120, 36);
        clearCopyButton.Click += ClearCopyTab;
        page.Controls.Add(clearCopyButton);

        progressBar.SetBounds(440, 450, 255, 25);
        progressBar.Minimum = 0;
        progressBar.Maximum = 100;
        page.Controls.Add(progressBar);

        statusLabel.Text = "Ready.";
        statusLabel.AutoSize = true;
        statusLabel.Location = new Point(10, 492);
        page.Controls.Add(statusLabel);

        logBox.SetBounds(10, 518, 685, 105);
        logBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        logBox.Multiline = true;
        logBox.ScrollBars = ScrollBars.Vertical;
        logBox.ReadOnly = true;
        page.Controls.Add(logBox);
    }

    private void BuildEditTab(TabPage page)
    {
        GroupBox instructions = new GroupBox();
        instructions.Text = "How to use";
        instructions.SetBounds(10, 10, 685, 155);
        page.Controls.Add(instructions);

        Label instructionText = new Label();
        instructionText.Text =
            "Creates a new copy of an M3U/M3U8 playlist with its file locations changed.\r\n" +
            "1. Select the existing playlist.\r\n" +
            "2. Enter the old location and new location (example: T:\\ to I:\\).\r\n" +
            "3. Select an output folder, then click Create Updated Playlist.\r\n" +
            "4. Click Create TXT List for a numbered artist/title list in play order.\r\n" +
            "The original playlist is never changed.";
        instructionText.AutoSize = true;
        instructionText.MaximumSize = new Size(545, 0);
        instructionText.Location = new Point(15, 25);
        instructions.Controls.Add(instructionText);

        AddBrandLogo(instructions);

        AddLabel(page, "Playlist (.m3u / .m3u8)", 10, 175);
        editPlaylistBox.SetBounds(10, 198, 580, 28);
        page.Controls.Add(editPlaylistBox);

        Button browse = new Button();
        browse.Text = "Browse...";
        browse.SetBounds(600, 196, 95, 30);
        browse.Click += BrowseEditPlaylist;
        page.Controls.Add(browse);

        AddLabel(page, "Old location", 10, 240);
        oldLocationBox.SetBounds(10, 263, 315, 28);
        oldLocationBox.Text = @"T:\";
        page.Controls.Add(oldLocationBox);

        AddLabel(page, "New location", 370, 240);
        newLocationBox.SetBounds(370, 263, 325, 28);
        newLocationBox.Text = @"I:\";
        newLocationBox.TextChanged += delegate { UpdateEditOutputPreview(); };
        page.Controls.Add(newLocationBox);

        AddLabel(page, "Destination folder for updated playlist and TXT list", 10, 305);
        editDestinationBox.SetBounds(10, 328, 580, 28);
        editDestinationBox.TextChanged += delegate { UpdateEditOutputPreview(); };
        page.Controls.Add(editDestinationBox);

        editDestinationBrowseButton.Text = "Browse...";
        editDestinationBrowseButton.SetBounds(600, 326, 95, 30);
        editDestinationBrowseButton.Click += BrowseEditDestination;
        page.Controls.Add(editDestinationBrowseButton);

        editOutputLabel.Text = "Output folder: not selected";
        editOutputLabel.AutoSize = true;
        editOutputLabel.Location = new Point(10, 370);
        page.Controls.Add(editOutputLabel);

        Button createButton = new Button();
        createButton.Text = "Create Updated Playlist";
        createButton.SetBounds(10, 395, 195, 38);
        createButton.Click += CreateUpdatedPlaylist;
        page.Controls.Add(createButton);

        openCreatedPlaylistButton.Text = "Open Created Playlist";
        openCreatedPlaylistButton.SetBounds(215, 395, 170, 38);
        openCreatedPlaylistButton.Enabled = false;
        openCreatedPlaylistButton.Click += OpenCreatedPlaylist;
        page.Controls.Add(openCreatedPlaylistButton);

        createTextListButton.Text = "Create TXT List";
        createTextListButton.SetBounds(395, 395, 140, 38);
        createTextListButton.Enabled = false;
        createTextListButton.Click += CreateTextList;
        page.Controls.Add(createTextListButton);

        clearEditButton.Text = "Clear All";
        clearEditButton.SetBounds(545, 395, 150, 38);
        clearEditButton.Click += ClearEditTab;
        page.Controls.Add(clearEditButton);

        editLogBox.SetBounds(10, 445, 685, 178);
        editLogBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        editLogBox.Multiline = true;
        editLogBox.ScrollBars = ScrollBars.Vertical;
        editLogBox.ReadOnly = true;
        page.Controls.Add(editLogBox);
    }

    private static Image LoadPayPalLogo()
    {
        using (Stream logoStream = typeof(Program).Assembly.GetManifestResourceStream("PayPalLogo.png"))
        {
            if (logoStream == null) return null;

            using (Image logoSource = Image.FromStream(logoStream))
            {
                return new Bitmap(logoSource, new Size(24, 29));
            }
        }
    }

    private static Image LoadBrandLogo()
    {
        using (Stream logoStream = typeof(Program).Assembly.GetManifestResourceStream("RobsMusicCopierLogo.png"))
        {
            if (logoStream == null) return null;
            using (Image logoSource = Image.FromStream(logoStream))
                return new Bitmap(logoSource);
        }
    }

    private static void AddBrandLogo(Control parent)
    {
        PictureBox logo = new PictureBox();
        logo.Image = LoadBrandLogo();
        logo.SizeMode = PictureBoxSizeMode.Zoom;
        logo.BackColor = Color.Transparent;
        logo.SetBounds(575, 22, 95, 112);
        parent.Controls.Add(logo);
    }

    private static Icon LoadApplicationIcon()
    {
        using (Stream iconStream = typeof(Program).Assembly.GetManifestResourceStream("RobsMusicCopier.ico"))
        {
            if (iconStream == null) return SystemIcons.Application;

            using (Icon iconSource = new Icon(iconStream))
            {
                return (Icon)iconSource.Clone();
            }
        }
    }

    private void AddLabel(Control parent, string text, int x, int y)
    {
        Label label = new Label();
        label.Text = text;
        label.AutoSize = true;
        label.Location = new Point(x, y);
        parent.Controls.Add(label);
    }

    private static string CleanPath(string value)
    {
        if (value == null) return String.Empty;
        return value.Trim().Trim('"');
    }

    private void ShowError(string message)
    {
        MessageBox.Show(this, message, "Rob's Music Copier", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    private void ShowInfo(string message)
    {
        MessageBox.Show(this, message, "Rob's Music Copier", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void BrowseEditPlaylist(object sender, EventArgs e)
    {
        using (OpenFileDialog dialog = new OpenFileDialog())
        {
            dialog.Filter = "M3U Playlists (*.m3u;*.m3u8)|*.m3u;*.m3u8|All files (*.*)|*.*";
            dialog.Title = "Select playlist to update";
            if (dialog.ShowDialog(this) == DialogResult.OK)
                editPlaylistBox.Text = dialog.FileName;
        }
    }

    private static string GetLocationName(string newLocation)
    {
        string value = CleanPath(newLocation).TrimEnd('\\', '/');
        if (value.Length == 2 && value[1] == ':')
            return Char.ToUpperInvariant(value[0]) + " Drive";

        string name = Path.GetFileName(value);
        if (String.IsNullOrWhiteSpace(name))
            name = value.Replace("\\", " ").Replace("/", " ").Replace(":", "").Trim();
        return String.IsNullOrWhiteSpace(name) ? "New Location" : name;
    }

    private void UpdateEditOutputPreview()
    {
        string destination = CleanPath(editDestinationBox.Text);
        editOutputLabel.Text = destination.Length > 0
            ? "Output folder: " + destination
            : "Output folder: not selected";
    }

    private void BrowseEditDestination(object sender, EventArgs e)
    {
        using (FolderBrowserDialog dialog = new FolderBrowserDialog())
        {
            dialog.Description = "Select folder for the updated playlist and TXT list";
            if (dialog.ShowDialog(this) == DialogResult.OK)
                editDestinationBox.Text = dialog.SelectedPath;
        }
    }

    private void CreateUpdatedPlaylist(object sender, EventArgs e)
    {
        try
        {
            editLogBox.Clear();

            string playlist = CleanPath(editPlaylistBox.Text);
            string oldLocation = CleanPath(oldLocationBox.Text);
            string newLocation = CleanPath(newLocationBox.Text);

            if (!File.Exists(playlist))
                throw new FileNotFoundException("Playlist not found.\r\n" + playlist);
            if (oldLocation.Length == 0)
                throw new InvalidOperationException("Please enter the old location.");
            if (newLocation.Length == 0)
                throw new InvalidOperationException("Please enter the new location.");
            if (String.Equals(oldLocation, newLocation, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Old location and new location are the same.");

            string outputFolder = CleanPath(editDestinationBox.Text);
            if (outputFolder.Length == 0)
                throw new InvalidOperationException("Please select a destination folder on the Edit Playlist Paths tab.");
            Directory.CreateDirectory(outputFolder);

            string outputFile = Path.Combine(outputFolder, Path.GetFileName(playlist));
            if (String.Equals(Path.GetFullPath(outputFile), Path.GetFullPath(playlist), StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Output file would overwrite the original playlist.");

            string[] lines = File.ReadAllLines(playlist);
            int changed = 0;
            int mediaEntries = 0;

            for (int i = 0; i < lines.Length; i++)
            {
                string trimmed = lines[i].Trim();
                if (trimmed.Length == 0 || trimmed.StartsWith("#", StringComparison.Ordinal))
                    continue;

                mediaEntries++;
                int position = lines[i].IndexOf(oldLocation, StringComparison.OrdinalIgnoreCase);
                if (position >= 0)
                {
                    lines[i] = lines[i].Substring(0, position) +
                               newLocation +
                               lines[i].Substring(position + oldLocation.Length);
                    changed++;
                }
            }

            if (changed == 0)
                throw new InvalidOperationException(
                    "No playlist paths contained the old location:\r\n" + oldLocation +
                    "\r\n\r\nNo output playlist was created.");

            File.WriteAllLines(outputFile, lines, new UTF8Encoding(false));
            lastCreatedPlaylistPath = outputFile;
            lastCreatedTextPath = String.Empty;
            openCreatedPlaylistButton.Enabled = true;
            createTextListButton.Enabled = true;

            editLogBox.AppendText("Source: " + playlist + "\r\n");
            editLogBox.AppendText("Old location: " + oldLocation + "\r\n");
            editLogBox.AppendText("New location: " + newLocation + "\r\n");
            editLogBox.AppendText("Media entries: " + mediaEntries + "\r\n");
            editLogBox.AppendText("Paths changed: " + changed + "\r\n");
            editLogBox.AppendText("Saved: " + outputFile + "\r\n");

            ShowInfo(
                "Updated playlist created.\r\n\r\n" +
                "Paths changed: " + changed + "\r\n" +
                "Saved in:\r\n" + outputFolder);
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);
        }
    }

    private void OpenCreatedPlaylist(object sender, EventArgs e)
    {
        try
        {
            if (String.IsNullOrEmpty(lastCreatedPlaylistPath) || !File.Exists(lastCreatedPlaylistPath))
                throw new FileNotFoundException("No created playlist is available to open.");

            Process.Start(new ProcessStartInfo(lastCreatedPlaylistPath) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);
        }
    }

    private void CreateTextList(object sender, EventArgs e)
    {
        try
        {
            if (String.IsNullOrEmpty(lastCreatedPlaylistPath) || !File.Exists(lastCreatedPlaylistPath))
                throw new FileNotFoundException("No created playlist is available for a TXT list.");

            List<string> entries = new List<string>();
            string pendingTitle = String.Empty;

            foreach (string rawLine in File.ReadAllLines(lastCreatedPlaylistPath))
            {
                string line = rawLine.Trim();
                if (line.Length == 0)
                    continue;

                if (line.StartsWith("#EXTINF:", StringComparison.OrdinalIgnoreCase))
                {
                    int comma = line.IndexOf(',');
                    pendingTitle = comma >= 0 && comma + 1 < line.Length
                        ? line.Substring(comma + 1).Trim()
                        : String.Empty;
                    continue;
                }

                if (line.StartsWith("#", StringComparison.Ordinal))
                    continue;

                string display = pendingTitle;
                if (display.Length == 0)
                {
                    string cleanEntry = CleanPath(line);
                    try { display = Path.GetFileNameWithoutExtension(cleanEntry); }
                    catch { display = cleanEntry; }
                }

                entries.Add(String.IsNullOrWhiteSpace(display) ? line : display);
                pendingTitle = String.Empty;
            }

            if (entries.Count == 0)
                throw new InvalidOperationException("The created playlist contains no media entries.");

            int digits = Math.Max(3, entries.Count.ToString().Length);
            string[] outputLines = new string[entries.Count];
            for (int i = 0; i < entries.Count; i++)
                outputLines[i] = (i + 1).ToString("D" + digits) + ". " + entries[i];

            string outputFolder = Path.GetDirectoryName(lastCreatedPlaylistPath);
            string outputName = Path.GetFileNameWithoutExtension(lastCreatedPlaylistPath) + ".txt";
            lastCreatedTextPath = Path.Combine(outputFolder, outputName);
            File.WriteAllLines(lastCreatedTextPath, outputLines, new UTF8Encoding(false));

            editLogBox.AppendText("TXT entries: " + entries.Count + "\r\n");
            editLogBox.AppendText("TXT saved: " + lastCreatedTextPath + "\r\n");

            ShowInfo(
                "TXT playlist list created.\r\n\r\n" +
                "Entries: " + entries.Count + "\r\n" +
                "Saved as:\r\n" + lastCreatedTextPath);
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);
        }
    }

    private void BrowsePlaylist(object sender, EventArgs e)
    {
        using (OpenFileDialog dialog = new OpenFileDialog())
        {
            dialog.Filter = "M3U Playlists (*.m3u;*.m3u8)|*.m3u;*.m3u8|All files (*.*)|*.*";
            dialog.Title = "Select playlist";
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                playlistBox.Text = dialog.FileName;
                AutoDetectRoot();
            }
        }
    }

    private void ClearCopyTab(object sender, EventArgs e)
    {
        playlistBox.Clear();
        destinationBox.Clear();
        sourceRootBox.Clear();
        logBox.Clear();
        preserveButton.Checked = true;
        numberTracksBox.Checked = true;
        progressBar.Value = 0;
        statusLabel.Text = "Ready.";
    }

    private void ClearEditTab(object sender, EventArgs e)
    {
        editPlaylistBox.Clear();
        oldLocationBox.Clear();
        newLocationBox.Clear();
        editDestinationBox.Clear();
        editLogBox.Clear();
        lastCreatedPlaylistPath = String.Empty;
        lastCreatedTextPath = String.Empty;
        openCreatedPlaylistButton.Enabled = false;
        createTextListButton.Enabled = false;
        UpdateEditOutputPreview();
    }

    private void BrowseDestination(object sender, EventArgs e)
    {
        using (FolderBrowserDialog dialog = new FolderBrowserDialog())
        {
            dialog.Description = "Select destination folder";
            if (dialog.ShowDialog(this) == DialogResult.OK)
                destinationBox.Text = dialog.SelectedPath;
        }
    }

    private void BrowseSourceRoot(object sender, EventArgs e)
    {
        using (FolderBrowserDialog dialog = new FolderBrowserDialog())
        {
            dialog.Description = "Select common source music root";
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                sourceRootBox.Text = dialog.SelectedPath;
                statusLabel.Text = "Source root set manually.";
            }
        }
    }

    private void ToggleMode(object sender, EventArgs e)
    {
        bool preserve = preserveButton.Checked;
        sourceRootBox.Enabled = preserve;
        sourceRootBrowseButton.Enabled = preserve;
        sourceRootLabel.Enabled = preserve;
        numberTracksBox.Enabled = !preserve;
    }

    private void AutoDetectRoot()
    {
        string playlist = CleanPath(playlistBox.Text);
        if (!File.Exists(playlist)) return;

        try
        {
            List<string> tracks = GetPlaylistTracks(playlist);
            if (tracks.Count == 0)
            {
                sourceRootBox.Text = String.Empty;
                statusLabel.Text = "Playlist selected, but no track paths were found.";
                return;
            }

            string root = GetCommonDirectory(tracks);
            sourceRootBox.Text = root;
            statusLabel.Text = root.Length > 0
                ? "Source root detected automatically."
                : "Could not determine a common source root.";
        }
        catch
        {
            sourceRootBox.Text = String.Empty;
            statusLabel.Text = "Could not detect source root.";
        }
    }

    private static List<string> GetPlaylistTracks(string playlistPath)
    {
        List<string> tracks = new List<string>();
        string playlistDirectory = Path.GetDirectoryName(Path.GetFullPath(playlistPath));

        foreach (string rawLine in File.ReadAllLines(playlistPath))
        {
            string line = rawLine.Trim();
            if (line.Length == 0 || line.StartsWith("#", StringComparison.Ordinal)) continue;

            if (!Path.IsPathRooted(line))
                line = Path.Combine(playlistDirectory, line);

            try { tracks.Add(Path.GetFullPath(line)); }
            catch { tracks.Add(line); }
        }

        return tracks;
    }

    private static string GetCommonDirectory(List<string> filePaths)
    {
        if (filePaths.Count == 0) return String.Empty;

        List<string> directories = new List<string>();
        foreach (string filePath in filePaths)
        {
            try
            {
                string directory = Path.GetDirectoryName(Path.GetFullPath(filePath));
                if (!String.IsNullOrEmpty(directory)) directories.Add(directory);
            }
            catch { }
        }

        if (directories.Count == 0) return String.Empty;
        string candidate = directories[0];

        while (!String.IsNullOrEmpty(candidate))
        {
            string prefix = EnsureTrailingSeparator(candidate);
            bool allMatch = true;
            foreach (string directory in directories)
            {
                string fullDirectory = EnsureTrailingSeparator(Path.GetFullPath(directory));
                if (!fullDirectory.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    allMatch = false;
                    break;
                }
            }

            if (allMatch) return candidate.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            DirectoryInfo parent = Directory.GetParent(candidate);
            if (parent == null) break;
            candidate = parent.FullName;
        }

        return String.Empty;
    }

    private static string EnsureTrailingSeparator(string path)
    {
        return path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
    }

    private void OpenPayPal(object sender, EventArgs e)
    {
        try
        {
            Process.Start(new ProcessStartInfo(PayPalUrl) { UseShellExecute = true });
        }
        catch
        {
            ShowError("Could not open the PayPal support page.");
        }
    }

    private void ShowAbout(object sender, EventArgs e)
    {
        using (Form about = new Form())
        {
            about.Text = "About Rob's Music Copier";
            about.Icon = LoadApplicationIcon();
            about.ClientSize = new Size(500, 360);
            about.FormBorderStyle = FormBorderStyle.FixedDialog;
            about.MaximizeBox = false;
            about.MinimizeBox = false;
            about.ShowInTaskbar = false;
            about.StartPosition = FormStartPosition.CenterParent;
            about.Font = new Font("Segoe UI", 10F);

            PictureBox brandLogo = new PictureBox();
            brandLogo.Image = LoadBrandLogo();
            brandLogo.SizeMode = PictureBoxSizeMode.Zoom;
            brandLogo.BackColor = Color.Transparent;
            brandLogo.SetBounds(20, 18, 105, 125);
            about.Controls.Add(brandLogo);

            Label heading = new Label();
            heading.Text = "Rob's Music Copier v" + AppVersion;
            heading.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
            heading.AutoSize = true;
            heading.Location = new Point(140, 25);
            about.Controls.Add(heading);

            Label details = new Label();
            details.Text =
                "Copies music referenced by M3U and M3U8 playlists, preserves or " +
                "flattens folder layouts, updates playlist paths, and creates a " +
                "numbered text list in playlist order.\r\n\r\n" +
                "Copyright (c) 2026 Rob Cannell\r\n\r\n" +
                "Licensed under the MIT License. See LICENSE.txt included with " +
                "the source package. Donations are optional and are not required " +
                "to use the program.";
            details.SetBounds(140, 68, 335, 205);
            about.Controls.Add(details);

            PictureBox paypalLogo = new PictureBox();
            paypalLogo.Image = LoadPayPalLogo();
            paypalLogo.SizeMode = PictureBoxSizeMode.Zoom;
            paypalLogo.SetBounds(22, 292, 40, 40);
            about.Controls.Add(paypalLogo);

            Button support = new Button();
            support.Text = "Support This Project";
            support.SetBounds(72, 294, 190, 36);
            support.Click += OpenPayPal;
            about.Controls.Add(support);

            Button close = new Button();
            close.Text = "Close";
            close.DialogResult = DialogResult.OK;
            close.SetBounds(375, 294, 100, 36);
            about.Controls.Add(close);
            about.AcceptButton = close;
            about.CancelButton = close;

            about.ShowDialog(this);
        }
    }

    private void OpenDestination(object sender, EventArgs e)
    {
        try
        {
            string destination = CleanPath(destinationBox.Text);
            if (destination.Length == 0) throw new InvalidOperationException("Please select a destination folder first.");
            if (!Directory.Exists(destination)) throw new DirectoryNotFoundException("Destination folder does not exist.\r\n" + destination);
            Process.Start(new ProcessStartInfo(destination) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);
        }
    }

    private void SetCopyControls(bool enabled)
    {
        startButton.Enabled = enabled;
        playlistBrowseButton.Enabled = enabled;
        destinationBrowseButton.Enabled = enabled;
        openDestinationButton.Enabled = enabled;
        sourceRootBrowseButton.Enabled = enabled && preserveButton.Checked;
    }

    private void StartCopy(object sender, EventArgs e)
    {
        try
        {
            logBox.Clear();
            progressBar.Value = 0;
            statusLabel.Text = "Validating...";

            string playlist = CleanPath(playlistBox.Text);
            string destinationRoot = CleanPath(destinationBox.Text);
            if (!File.Exists(playlist)) throw new FileNotFoundException("Playlist not found.\r\n" + playlist);
            if (destinationRoot.Length == 0) throw new InvalidOperationException("Please select a destination folder.");
            Directory.CreateDirectory(destinationRoot);

            bool preserve = preserveButton.Checked;
            string normalizedRoot = String.Empty;
            if (preserve)
            {
                if (CleanPath(sourceRootBox.Text).Length == 0) AutoDetectRoot();
                string sourceRoot = CleanPath(sourceRootBox.Text);
                if (!Directory.Exists(sourceRoot)) throw new DirectoryNotFoundException("Source music root not found.\r\n" + sourceRoot);
                normalizedRoot = Path.GetFullPath(sourceRoot).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            }

            List<string> tracks = GetPlaylistTracks(playlist);
            if (tracks.Count == 0) throw new InvalidOperationException("No media file entries were found in the playlist.");

            SetCopyControls(false);
            int copied = 0;
            int missing = 0;
            int skipped = 0;
            int collisions = 0;
            List<string> missingFiles = new List<string>();
            HashSet<string> seenDestinations = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            int digits = Math.Max(3, tracks.Count.ToString().Length);

            for (int index = 0; index < tracks.Count; index++)
            {
                string sourceFile = tracks[index];
                progressBar.Value = Math.Min(100, (int)((index / (double)Math.Max(1, tracks.Count)) * 100));
                statusLabel.Text = "Processing " + (index + 1) + " of " + tracks.Count + "...";
                Application.DoEvents();

                if (!File.Exists(sourceFile))
                {
                    logBox.AppendText("NOT FOUND: " + sourceFile + "\r\n");
                    missing++;
                    missingFiles.Add(sourceFile);
                    continue;
                }

                string destinationFile;
                if (preserve)
                {
                    string normalizedFile = Path.GetFullPath(sourceFile);
                    string rootPrefix = EnsureTrailingSeparator(normalizedRoot);
                    if (!normalizedFile.StartsWith(rootPrefix, StringComparison.OrdinalIgnoreCase))
                    {
                        logBox.AppendText("SKIPPED (outside source root): " + sourceFile + "\r\n");
                        skipped++;
                        continue;
                    }

                    string relativePath = normalizedFile.Substring(rootPrefix.Length);
                    destinationFile = Path.Combine(destinationRoot, relativePath);
                }
                else
                {
                    string originalName = Path.GetFileName(sourceFile);
                    string destinationName = numberTracksBox.Checked
                        ? (index + 1).ToString("D" + digits) + " - " + originalName
                        : originalName;
                    destinationFile = Path.Combine(destinationRoot, destinationName);

                    if (seenDestinations.Contains(destinationFile) || File.Exists(destinationFile))
                    {
                        string baseName = Path.GetFileNameWithoutExtension(destinationName);
                        string extension = Path.GetExtension(destinationName);
                        int number = 2;
                        do
                        {
                            destinationFile = Path.Combine(destinationRoot, baseName + " (" + number + ")" + extension);
                            number++;
                        }
                        while (seenDestinations.Contains(destinationFile) || File.Exists(destinationFile));
                        collisions++;
                    }
                }

                string destinationFolder = Path.GetDirectoryName(destinationFile);
                if (!String.IsNullOrEmpty(destinationFolder)) Directory.CreateDirectory(destinationFolder);
                File.Copy(sourceFile, destinationFile, true);
                seenDestinations.Add(destinationFile);
                copied++;
                logBox.AppendText("Copied: " + destinationFile + "\r\n");
            }

            progressBar.Value = 100;
            statusLabel.Text = "Complete.";
            if (missingFiles.Count > 0)
                File.WriteAllLines(Path.Combine(destinationRoot, "MissingFiles.txt"), missingFiles.ToArray(), Encoding.UTF8);

            string summary =
                "Playlist entries: " + tracks.Count + "\r\n" +
                "Files copied:     " + copied + "\r\n" +
                "Files not found:  " + missing + "\r\n" +
                "Files skipped:    " + skipped + "\r\n" +
                "Name collisions:  " + collisions;
            logBox.AppendText("\r\n=== COMPLETE ===\r\n" + summary + "\r\n");
            ShowInfo(summary);
        }
        catch (Exception ex)
        {
            statusLabel.Text = "Error.";
            ShowError(ex.Message);
        }
        finally
        {
            SetCopyControls(true);
        }
    }
}
