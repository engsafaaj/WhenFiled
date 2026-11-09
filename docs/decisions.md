# WhenFiled v1 decisions

Each entry is one decision and the reason for it. Add new entries at the end. When a decision changes, mark the old entry "Replaced by D*n*" instead of deleting it.

Written 2026-11-09, before any app code. "Spec" is [spec.md](spec.md), "mock" is [../design/mock.html](../design/mock.html), and tasks T1–T10 are in [plan.md](plan.md).

## v1 at a glance

```text
WhenFiled.App     the WPF window: picks folders, shows the plan, calls Scanner and Mover
WhenFiled.Core
  Rules/          pure code: MediaFilter, date parsers, DateResolver, PathRules,
                  Planner, MoveFailure, UndoLog, PlanSummary
  Disk/           the only disk code: Scanner (reads only), Mover (moves and undo)
WhenFiled.Tests   unit tests for Rules, plus a few temp-folder tests for Scanner and Mover
```

1. **Scan** (`Scanner`) checks the folders, lists the files and reads their raw dates. The rules pick each date and target. Scan returns a `Plan` and changes nothing.
2. **Sort** (`Mover`) re-checks each file, creates its folder if needed, moves it, and records it in the `UndoLog`. A failure skips only that file.
3. **Undo** (`Mover`) replays the `UndoLog` backwards, then removes the empty folders the Sort created.

## Structure

### D1. Three projects
- `WhenFiled.Core` (net10.0): the rules as pure code in `Rules/`, plus `Scanner` and `Mover` in `Disk/`.
- `WhenFiled.App` (net10.0-windows): the WPF window.
- `WhenFiled.Tests` (net10.0): unit tests for the rules, and a few real temp-folder tests.

References: App → Core and Tests → Core.

**Reason:** this is a small teaching example. Three projects keep the rules testable without a project per layer. `Scanner` sits next to `Mover` because Scan has to read files, and the window should stay thin.

### D2. The rules are pure code
Everything in `Core/Rules/` takes plain values: names, sizes, raw date strings, `today` and a `TimeZoneInfo`. It never uses `File`, `Directory`, `FileStream`, `DateTime.Now` or `TimeZoneInfo.Local`.

The rules see the destination only through a small read-only interface, `IDestinationView`, which `Scanner` implements. It answers "which file names are in this folder?" and "is this file the same as that one?".

This is kept by convention and review. There is no analyzer.

**Reason:** the rules can be unit tested fast, with the same result on every day and every PC. An analyzer costs more setup than it saves at this size.

### D3. Only Scanner and Mover touch the disk
`Scanner` only reads. `Mover` only moves files and creates folders; in Undo it also removes the empty folders it created. Neither one deletes, copies, replaces or writes a file, or changes a file's attributes or times. Temp-folder tests check "never overwrite", "read-only kept" and Undo.

**Reason:** with every disk call in two small classes, "never delete a photo" and "never overwrite a file" are easy to review.

### D4. Solution basics
`WhenFiled.slnx`, and a `Directory.Build.props` with nullable on, implicit usings and warnings as errors. No DI container and no logging package.

**Reason:** fewest moving parts for a small teaching example.

## Dates

### D5. MetadataExtractor reads EXIF and video times
The `MetadataExtractor` NuGet package, used only by `Scanner`. It is Core's only package.

**Reason:** it is plain .NET code. It reads EXIF from JPEG, HEIC, PNG and WebP, and the creation time from MP4 and MOV, with nothing for the user to install. Windows' own readers (WIC and Shell properties) need the HEIF extension from the Store for HEIC, which breaks the spec's "no extra tools to install".

**Risk:** 3GP support isn't confirmed. T8 checks it with a real 3GP file. If it fails, `Scanner` gets a tiny reader for the video's `mvhd` box.

### D6. Scanner returns raw values and the rules decide
`Scanner` hands the rules `RawDates(string? ExifDateTimeOriginal, DateTime? VideoCreatedUtc)`. The rules parse them, check the range and pick the source.

**Reason:** every date rule sits in one tested place.

### D7. Video times are converted from UTC to the PC's time zone
MP4 and MOV store the creation time in UTC. The rules convert it with the time zone they are given (the PC's zone when the app runs), then take the date. EXIF `DateTimeOriginal` is already local time, so it is used as it is.

**Reason:** without this, a video taken at 00:30 on 1 June in Paris (UTC+2) is 31 May in UTC and lands in May.

**Known limit:** some Android phones write local time instead of UTC. Near midnight, their videos can land on the wrong day. Accepted for v1.

### D8. Dates in file names
A file-name date is either 8 digits (`YYYYMMDD`), or `YYYY-MM-DD` or `YYYY_MM_DD` with the same separator twice. It must not touch other digits, must be a real calendar date, and must be in the valid range. The first match that passes wins.

**Reason:** this covers WhatsApp (`IMG-20160315-WA0020`), screenshots (`Screenshot_20230102-101010`), `IMG_`, `VID_` and `PXL_` names, and `2019-07-04 10.11.12`. Requiring a real date with no digits next to it rejects counters like `IMG_4821` and long numbers. Names that are Unix timestamps (`1577836800000.jpg`) are not read in v1.

### D9. Dates are `DateOnly`
Once a date is picked, its time is dropped.

**Reason:** only the year and month are needed, the preview shows only the date, and file-name dates may have no time.

### D10. The valid range includes both ends
From 1990-01-01 up to and including tomorrow. `today` is the PC's local date, passed in by the caller. A date outside the range falls through to the next source.

**Reason:** the spec sets the range. Allowing tomorrow absorbs time zone differences.

### D11. Read problems during Scan
- File locked: "Still changing, scan again".
- Windows refuses to let it be read: "Access denied".
- Metadata can't be parsed (corrupt or unknown): treated as no metadata, so a date in the file name still counts.

**Reason:** a problem with one file never stops the run (spec). A broken header shouldn't hide a good file-name date.

## Destinations and names

### D12. Folder checks before Scan starts
Scan refuses to start when:
- the destination is the source, or is inside it
- the two folders are on different drives
- either folder doesn't exist (checked by `Scanner`)

The rules compare full paths, ignoring case, with a trailing `\`, so `D:\A` and `D:\AB` don't clash. Junctions and `subst` drives are not resolved.

**Reason:** the spec forbids a destination inside the source and supports one drive only. On different drives, `File.Move` copies and then deletes, which is not the safe rename v1 relies on.

### D13. Target path and long paths
The target is `Destination\YYYY\MM\name`. If the full target path is 260 characters or longer, the file isn't moved: "Path too long".

**Reason:** the spec puts long paths out of scope. Refusing is safer than half-working.

### D14. Name clashes
- Names are compared ignoring case, so `A.JPG` clashes with `a.jpg`. There is no Unicode normalization, and names are kept exactly as they are.
- When a file already in the destination has the name, `Scanner` compares content. The same size and the same SHA-256 means identical: "Already in destination", not moved.
- Otherwise the file gets the first free ` (n)` name, starting at ` (2)`: `a (2).jpg`, then `a (3).jpg`.
- When two source files would get the same name in the same plan, the second gets ` (2)` with no content check.
- Files are planned in source path order, so the numbers come out the same on every scan.

**Reason:** this matches how Windows treats names, and no file is ever lost or overwritten. Two source files with the same name are rare, and both survive. Hashing only on a clash keeps Scan fast.

### D15. Scan skips hidden, system and reparse-point items
This applies to files and folders.

**Reason:** it skips files like `Thumbs.db`, avoids junction loops, and doesn't make OneDrive download cloud-only files. A source folder inside OneDrive therefore finds nothing, which is fine because OneDrive is out of scope.

## Sort and Undo

### D16. A move is a rename on the same drive
`Mover` moves files only with `File.Move(source, target)`, the 2-argument form. It throws if the target exists. On the same drive it is a rename, so the file's bytes, times, attributes and read-only flag stay as they were.

**Reason:** the spec says move, not copy, and files are never changed. A read-only file stays read-only.

### D17. Each file is re-checked just before it moves
1. Its size still equals the size seen at Scan. If not: "Still changing, scan again".
2. It opens for reading with `FileShare.None`, then closes. If not: "Still changing, scan again".
3. Its target name is still free. If not: "Destination changed, scan again".

Then the run moves on to the next file.

**Reason:** this is the spec's rule for locked files and size changes, and a problem with one file never stops the run. Not moving a file is always safe.

### D18. The rules map exceptions to reasons
`MoveFailure.ReasonFor(exception)` looks at the exception's type and HResult:
- `UnauthorizedAccessException`: "Access denied"
- sharing or lock violation: "Still changing, scan again"
- file already exists: "Destination changed, scan again"
- anything else: "Couldn't move: " and the error message

**Reason:** the mapping is pure, so it can be unit tested. `Mover` only catches the exception and asks.

### D19. The undo log
It lives in memory only and holds one level. Each Sort starts a fresh log and records every move (from, to) and every folder the Sort created.

Undo:
1. Moves files back in reverse order with the same 2-argument `File.Move`, and re-creates the original folder if it is gone. It skips a file whose original path is now taken ("Original place is taken") or that is no longer in the destination ("No longer in destination").
2. Removes the folders the Sort created, deepest first, with the 1-argument `Directory.Delete`. That only removes an empty folder, so a folder that isn't empty stays.
3. Empties the log.

**Reason:** the spec asks for one level of undo that lasts until the next Sort or until the app closes, and that also removes the empty Year/Month folders. No step in Undo can overwrite or delete a file.

## Window

### D20. One window with code-behind
`MainWindow` uses code-behind, with no MVVM framework. Counts and texts come from `PlanSummary`. The window only shows them and turns buttons on and off.

**Reason:** there is one window and very little UI logic, so a framework would add more code than it saves. Revisit this if a second window appears.

### D21. The UI follows the mock
- The states are Empty, Scanning, Preview, Done and Error. While sorting, the preview stays on screen with the heading "Sorting…" and every button off, because the spec has no progress bar or cancel for Sort.
- Scan and Sort run on `Task.Run`. Scan reports progress through `IProgress` about every 50 files.
- The preview is a ListView grouped by status, with virtualization kept on while grouping.
- Folders are picked with WPF's built-in `OpenFolderDialog`.
- The theme is light Fluent (`ThemeMode="Light"`). If it is still marked experimental, suppress WPF0001 in the App project only.
- Numbers have thousands separators ("1,071"), and the wording comes from the mock.

**Reason:** this matches the mock and keeps the window responsive with thousands of files.

### D22. From labels
The code uses the spec's names: `Exif`, `Video`, `FileName` and `None`. The window shows the mock's labels: "Photo info", "Video", "File name" and "None".

**Reason:** "Photo info" is clearer to a non-technical user, and the mock is the user-facing wording.

### D23. Undo stays available until the next Sort
"Undo last sort" shows whenever the log has moves, including after "Sort another folder".

**Reason:** the spec keeps undo until the next Sort or until the app closes. Hiding it after leaving the Done screen would cut that short.

### D24. Scan runs again after Undo
**Reason:** the preview then matches the disk, and the mock goes back to Preview after Undo.

## Tests

### D25. One test project, xUnit v3, no assertion library
**Reason:** one project is enough at this size. The template comes with the SDK, and the built-in `Assert` is enough.

### D26. A few real-disk tests, and no test-file builders
The rules get unit tests. `Mover` gets a few temp-folder tests (never overwrite, read-only kept, Undo, a file held open), and `Scanner` gets one (Scan changes nothing). These tests use plain files with made-up bytes. Reading real EXIF and video dates is checked by hand with real phone files of every type, in T8 and T10.

**Reason:** building valid JPEG, MP4 or HEIC files in code would cost more than the app is worth, and MetadataExtractor has its own tests.

### D27. No UI automation in v1
The window is checked by hand against the mock.

**Reason:** the window has little logic, and the rules and moves are covered by tests.

### D28. Window details the mock leaves open
- Choosing a new folder in Preview or Error goes back to Empty with both folders kept, because the old preview no longer matches them. "Start over" and "Sort another folder" clear both folders.
- Scan checks the folder pair (same, inside, different drives) before it checks that the destination exists, so a missing destination inside the source still gets the "inside" message.
- If Undo can't put some files back, a message box lists up to 10 of them. The scan after Undo then runs as in D24.

**Reason:** these are the smallest behaviours that keep the screen true to the disk without adding states.
