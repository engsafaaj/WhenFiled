using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using Microsoft.Win32;
using WhenFiled.Core.Disk;
using WhenFiled.Core.Rules;

namespace WhenFiled.App;

public enum UiState { Empty, Scanning, Preview, Sorting, Done, Error }

/// <summary>The one window (D20). It shows what Scanner, Mover and PlanSummary give it.</summary>
public partial class MainWindow : Window
{
    private readonly Scanner _scanner = new();
    private readonly Mover _mover = new();
    private string? _source;
    private string? _dest;
    private IReadOnlyList<PlanItem> _plan = [];
    private PlanSummary _summary = new(0, 0, 0);

    public MainWindow()
    {
        InitializeComponent();
        BrowseSource.Click += (_, _) => Browse(isSource: true);
        BrowseDest.Click += (_, _) => Browse(isSource: false);
        ScanButton.Click += async (_, _) => await RunScan();
        SortButton.Click += async (_, _) => await RunSort();
        UndoButton.Click += async (_, _) => await RunUndo();
        StartOverButton.Click += (_, _) => StartOver();
        ShowState(UiState.Empty);
    }

    private static SolidColorBrush Color(string hex) => (SolidColorBrush)new BrushConverter().ConvertFromString(hex)!;

    private static Visibility Show(bool on) => on ? Visibility.Visible : Visibility.Collapsed;

    private static string Files(int n) => $"{PlanSummary.Format(n)} {(n == 1 ? "file" : "files")}";

    /// <summary>Turns the parts of the window on and off for one state (the mock's five states, D21).</summary>
    private void ShowState(UiState s)
    {
        var scanning = s == UiState.Scanning;
        var sorting = s == UiState.Sorting;
        var preview = s is UiState.Preview or UiState.Sorting;
        var idle = s is UiState.Empty or UiState.Error or UiState.Preview;
        var bothChosen = _source is not null && _dest is not null;

        Heading.Text = s switch
        {
            UiState.Scanning => "Scanning…",
            UiState.Preview => "Preview",
            UiState.Sorting => "Sorting…",
            UiState.Done => "Done",
            _ => "Sort photos by the date they were taken",
        };

        EmptyCard.Visibility = Show(s is UiState.Empty or UiState.Error);
        ScanCard.Visibility = Show(scanning);
        PreviewList.Visibility = Show(preview);
        DonePanel.Visibility = Show(s == UiState.Done);
        if (s != UiState.Error)
            EmptyText.Text = "Pick both folders, then press Scan. Scan only looks – it never moves or changes a file.";
        if (s is UiState.Empty or UiState.Scanning)
            Banner.Visibility = Banner2.Visibility = Visibility.Collapsed;
        if (s != UiState.Done)
            Banner2.Visibility = Visibility.Collapsed;
        DestBox.BorderBrush = s == UiState.Error ? Color("#C42B1C") : Color("#E0E0E0");

        BrowseSource.IsEnabled = BrowseDest.IsEnabled = idle;
        ScanButton.Visibility = Show(s != UiState.Done);
        ScanButton.Content = preview ? "Scan again" : "Scan";
        ScanButton.IsEnabled = idle && bothChosen;
        SortButton.Visibility = Show(preview);
        SortButton.Content = _summary.SortButton;
        SortButton.IsEnabled = s == UiState.Preview && _summary.CanSort;
        StartOverButton.Visibility = Show(preview || s == UiState.Done);
        StartOverButton.Content = s == UiState.Done ? "Sort another folder" : "Start over";
        StartOverButton.IsEnabled = !sorting;
        Hint.Visibility = Show(preview);

        // D23: Undo shows whenever the log has moves.
        UndoButton.Visibility = Show(_mover.CanUndo && !scanning && !sorting);
        if (s == UiState.Done) UndoButton.Style = (Style)FindResource("Primary");
        else UndoButton.ClearValue(StyleProperty);
    }

    private void ShowFolders()
    {
        SourceText.Text = _source ?? "Choose the folder with your photos (subfolders included)";
        DestText.Text = _dest ?? @"Choose where the Year\Month folders go";
        SourceText.Foreground = _source is null ? Color("#5D5D5D") : Color("#1B1B1B");
        DestText.Foreground = _dest is null ? Color("#5D5D5D") : Color("#1B1B1B");
    }

    private void Browse(bool isSource)
    {
        var dialog = new OpenFolderDialog
        {
            Title = isSource ? "Choose the folder with your photos" : @"Choose where the Year\Month folders go",
        };
        if (dialog.ShowDialog(this) != true)
            return;
        if (isSource) _source = dialog.FolderName;
        else _dest = dialog.FolderName;
        ShowFolders();
        _plan = [];
        _summary = new PlanSummary(0, 0, 0);
        ShowState(UiState.Empty); // the old preview no longer matches the folders
    }

    private void StartOver()
    {
        _source = _dest = null;
        _plan = [];
        _summary = new PlanSummary(0, 0, 0);
        ShowFolders();
        ShowState(UiState.Empty);
    }

    private void ShowBanner(string icon, string title, string detail, string background, string iconColor)
    {
        Banner.Background = Color(background);
        BannerIcon.Text = icon;
        BannerIcon.Foreground = Color(iconColor);
        BannerTitle.Text = title;
        BannerDetail.Text = detail;
        BannerDetail.Visibility = Show(detail.Length > 0);
        Banner.Visibility = Visibility.Visible;
    }

    private async Task RunScan()
    {
        var source = _source!;
        var dest = _dest!;
        ScanCounts.Text = "";
        ScanPath.Text = "";
        ShowState(UiState.Scanning);

        var progress = new Progress<ScanProgress>(p =>
        {
            ScanCounts.Text = $"{PlanSummary.Format(p.FilesChecked)} files checked · " +
                              $"{PlanSummary.Format(p.MediaFound)} photos and videos found";
            ScanPath.Text = p.CurrentPath;
        });
        var today = DateOnly.FromDateTime(DateTime.Now); // the clock and zone live here, not in the rules
        var zone = TimeZoneInfo.Local;

        ScanResult result;
        try
        {
            result = await Task.Run(() => _scanner.Scan(source, dest, today, zone, progress));
        }
        catch (Exception e)
        {
            result = new ScanResult(e.Message, []);
        }

        if (result.Refusal is { } refusal)
        {
            EmptyText.Text = "Fix the folders, then press Scan.";
            ShowBanner("✕", "Scan didn’t start", refusal + "\nNothing was scanned or moved.", "#FDE7E9", "#C42B1C");
            ShowState(UiState.Error);
            return;
        }

        _plan = result.Items;
        _summary = PlanSummary.From(_plan);
        var rows = RowBuilder.Preview(_plan, _summary);
        var view = new ListCollectionView(rows);
        view.GroupDescriptions.Add(new PropertyGroupDescription(nameof(PreviewRow.Group)));
        PreviewList.ItemsSource = view;
        ShowBanner("✓", _summary.BannerTitle, _summary.BannerDetail, "#DFF6DD", "#0F7B0F");
        ShowState(UiState.Preview);
    }

    private async Task RunSort()
    {
        ShowState(UiState.Sorting); // the preview stays on screen, every button off
        var plan = _plan;
        var result = await Task.Run(() => _mover.Sort(plan));

        ShowBanner("✓", $"{Files(result.Moved)} moved to {_dest}",
            "Changed your mind? Undo puts every file back where it was.", "#DFF6DD", "#0F7B0F");

        var failed = result.NotMoved;
        Banner2.Visibility = Show(failed.Count > 0);
        Banner2Text.Inlines.Clear();
        Banner2Text.Inlines.Add(new System.Windows.Documents.Run(
            $"{Files(failed.Count)} {(failed.Count == 1 ? "was" : "were")} not moved")
        { FontWeight = FontWeights.SemiBold });
        Banner2Text.Inlines.Add(" – they are still in the source folder");
        FailedList.ItemsSource = failed.Select(f => new FailedRow(System.IO.Path.GetFileName(f.Path), f.Reason)).ToList();
        FailedList.Visibility = Show(failed.Count > 0);

        DoneNote.Text = $"The {Files(_summary.NoDate)} with no date and {PlanSummary.Format(_summary.Other)} " +
                        "others skipped at Scan were not part of this sort.";
        if (_mover.CanUndo)
            DoneNote.Text += "\nUndo is only available until the next sort or until you close WhenFiled.";
        ShowState(UiState.Done);
    }

    private async Task RunUndo()
    {
        ShowState(UiState.Sorting); // buttons off while files go back
        Heading.Text = "Undoing…";
        var undo = await Task.Run(() => _mover.Undo());

        if (undo.NotRestored.Count > 0)
        {
            var lines = undo.NotRestored.Take(10).Select(n => $"{System.IO.Path.GetFileName(n.Path)}: {n.Reason}");
            MessageBox.Show(this,
                $"{Files(undo.NotRestored.Count)} could not be put back:\n\n" + string.Join("\n", lines),
                "Undo last sort", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        // D24: scan again so the preview matches the disk.
        if (_source is not null && _dest is not null) await RunScan();
        else ShowState(UiState.Empty);
    }
}