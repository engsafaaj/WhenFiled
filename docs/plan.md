# WhenFiled v1 plan

Ten small tasks, in order. Each one names the test that proves it is done. Tick a task only when its test passes and `dotnet build` has no warnings.

- "AC*n*" is acceptance criterion *n* in [spec.md](spec.md).
- "D*n*" is a decision in [decisions.md](decisions.md).
- For rules, write the test first and watch it fail (CLAUDE.md rule 3).
- No task may add anything from the spec's "Will NOT do in v1" list.

## Tasks

- [x] **T1 Solution and projects (D1, D4).** Create `WhenFiled.slnx`, `Directory.Build.props`, `src/WhenFiled.Core` (with `Rules/` and `Disk/`), `src/WhenFiled.App` and `tests/WhenFiled.Tests`.
  - Test:
    - One placeholder test passes.
    - `dotnet build` has 0 warnings.
    - Every command in CLAUDE.md runs.

- [x] **T2 MediaFilter and valid date range (D10).**
  - Test `MediaFilterTests`:
    - Accepted in any case: `.jpg .jpeg .heic .png .webp .mp4 .mov .3gp`.
    - Rejected: `notes.txt`, `Thumbs.db`, `x.pdf`, `photo.jpg.txt` and a zero-byte `empty.jpg` (AC7).
  - Test `ValidDateTests`, with today = 2026-11-09: 1989-12-31 no, 1990-01-01 yes, 2026-11-10 yes, 2026-11-11 no.

- [x] **T3 EXIF and video dates (D7, D9).**
  - Test `ExifDateTests`:
    - `2019:07:04 10:11:12` → 2019-07-04.
    - No date: `0000:00:00 00:00:00`, blank, null and `2019:02:30 10:00:00`.
    - A trailing `\0` is ignored.
  - Test `VideoDateTests`:
    - 2021-05-14 10:00 UTC → 2021-05-14.
    - 2021-05-31 22:30 UTC in UTC+2 → 2021-06-01.

- [x] **T4 Dates in file names (D8).**
  - Test `FileNameDateTests`:
    - Date found: `IMG-20160315-WA0020.jpg`, `Screenshot_20230102-101010.png`, `VID_20210514.mp4`, `PXL_20230102_101010123.jpg`, `2019-07-04 10.11.12.jpg`.
    - No date: `holiday.jpg`, `IMG_4821.jpg`, `20161332.jpg`, `20160230.jpg`, `1577836800000.jpg`.

- [x] **T5 DateResolver.** Picks the date and From in the spec's order.
  - Test `DateResolverTests`:
    - EXIF beats the video time and the file name (AC1).
    - The video time beats the file name (AC2).
    - A `0000:00:00` EXIF date falls through to the file name (AC3).
    - A future EXIF date falls through.
    - Nothing found → None (AC4).

- [ ] **T6 Folder rules and target path (D12, D13).**
  - Test `PathRulesTests`:
    - `D:\A` + `D:\A\B` → inside.
    - `D:\A` + `D:\A` → refused.
    - `D:\A` + `D:\AB` → fine.
    - `D:\A\` + `d:\a\b` → inside.
    - `D:\A` + `E:\B` → different drives.
  - Test `TargetPathTests`:
    - `D:\Sorted` + 2019-07-04 → `D:\Sorted\2019\07`, shown as `2019\07\`.
    - A target of 260 characters or more → "Path too long".

- [ ] **T7 Planner and summary (D14, D21).** Uses a fake `IDestinationView`.
  - Test `PlannerTests` (logic for AC4, AC6 and AC8):
    - A free name is kept. `写真.jpg` and `Café 😀.jpg` are kept exactly.
    - An identical file already there → "Already in destination".
    - A different file already there → `a (2).jpg`. If `(2)` is taken too → `a (3).jpg`.
    - `A.JPG` clashes with `a.jpg`.
    - Two sources named `a.jpg` for the same month → `a.jpg` and `a (2).jpg`, whatever the input order.
    - No date → "No date". The file stays and shows its own folder.
  - Test `PlanSummaryTests`:
    - 1,071 to move, 24 with no date and 7 others → "Sort 1,071 files" and the mock's banner text.
    - Nothing to move → Sort is off.

- [ ] **T8 Scanner (D5, D6, D11, D15).** Folder check, file list, MetadataExtractor and the same-content check.
  - Test `ScannerTests` (temp folder):
    - After Scan, every source file has the same SHA-256, modified time and attributes, and the destination has no new folders (AC5).
    - A destination inside the source → refused.
  - By hand: Scan a copy of real phone files, one of each type (JPEG, HEIC, PNG, WebP, MP4, MOV, 3GP). Each one shows the right date and From. If 3GP fails, add the small `mvhd` reader from D5.

- [ ] **T9 Mover: Sort and Undo (D16–D19).** Includes `MoveFailure.ReasonFor` and `UndoLog`.
  - Test `MoveFailureTests`:
    - Access denied → "Access denied".
    - Sharing or lock violation → "Still changing, scan again".
    - File already exists → "Destination changed, scan again".
  - Test `UndoLogTests`: moves come back in reverse order, and created folders deepest first.
  - Test `MoverTests` (temp folders):
    - **Never overwrite:** a target that appears after Scan → not moved, and both files keep their bytes.
    - **Read-only kept:** a read-only file is moved and is still read-only.
    - **Undo** (AC10):
      - Every file is back at its original path with the same SHA-256.
      - The Year/Month folders the Sort created are gone.
      - A created folder that now holds another file is kept.
    - **File held open** (AC9): it gets "Still changing, scan again", and the next file still moves.

- [ ] **T10 Window and acceptance run (D20–D24).** `MainWindow` with the mock's states, calling `Scanner` and `Mover`.
  - Test, by hand:
    - Each state matches the mock.
    - Run AC1–AC10 on a fresh copy of a test folder, then tick the boxes in [spec.md](spec.md).
    - For AC9, hold a file open in PowerShell with `$f = [IO.File]::Open('<path>', 'Open', 'ReadWrite', 'Read')`, then close it with `$f.Close()`.
