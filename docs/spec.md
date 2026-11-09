# WhenFiled v1.0 spec

Working name from [research.md](research.md) (web search clear, trademark not checked). v1 is a single Windows window. Written 2026-11-09.

## The user and the job

**User:** one Windows 11 user with a folder of photos copied from their phone: camera photos, WhatsApp images, screenshots and videos.

**Job:** move them into `Year\Month` folders by the date each was taken, see where each date came from, and undo it if it looks wrong.

## What v1 does

1. The user picks a **source folder** (subfolders included) and a **destination folder**.
2. **Scan** builds a preview and never changes anything. Each file's date comes from the first of these that works:
   1. EXIF `DateTimeOriginal`
   2. Video creation time
   3. A date in the file name, such as `IMG-20160315-WA0020` or `Screenshot_20230102-101010`
   4. Otherwise **No date**, and the file stays where it is
3. The **preview** lists each file with its date, **From** (EXIF / Video / File name / None) and target folder.
4. **Sort** moves the files to `Destination\YYYY\MM\` and lists any file it didn't move, with the reason.
5. **Undo last sort** moves every file back to where it was.

## Assumptions

- **Move, not copy**, with source and destination on the same drive.
- **A valid date** falls between 1990-01-01 and tomorrow. Any other date (such as EXIF `0000:00:00`) falls through to the next source.
- **File system dates** (Created, Modified) are never used. They are wrong after copying from a phone.
- **Files are never changed.** The only possible change is a ` (2)` added to a name to avoid a clash.
- **The destination can't be inside the source.** Scan refuses to start.
- **Undo** is held in memory until the next sort or until the app closes.
- **Offline**, English interface, no extra tools to install.

## Will NOT do in v1

- Moves across drives, or a copy mode
- Undo that survives a crash or closing the app; more than one level of undo
- Paths longer than 260 characters
- Cancel during a sort, or a progress bar
- A speed target (such as 5,000 files in 60 s)
- Day folders, custom templates, renaming, or writing dates into files
- File system dates, Google Takeout JSON, RAW formats, editing dates by hand
- Importing from a phone over USB, OneDrive or Google Photos
- Network drives, Windows 10, macOS, a command line, background running

## Bad inputs it must handle safely

A problem with one file never stops the run. That file is left where it is, and the results say why.

| Input | What v1 does |
|---|---|
| Photo with no date | Not moved. Shown as "No date". |
| Duplicate file name | Never overwrites. Identical content already there: not moved, "Already in destination". Different content: arrives as `name (2).jpg`. |
| Non-English file name | Any Unicode name is shown correctly and kept exactly. |
| Read-only file | Moved, still read-only. If Windows refuses the move: left in place, "Access denied". |
| Not a photo or video | Only `.jpg .jpeg .heic .png .webp .mp4 .mov .3gp` are considered. Everything else, and zero-byte files, are skipped. |
| File still being copied | If it is locked, or its size changed since Scan: not moved, "Still changing, scan again". |

## Acceptance criteria

Run each test on a fresh copy of the test folder. "Same file" means the same SHA-256 hash, modified time and attributes as before.

1. [ ] A JPEG with EXIF date `2019:07:04` and a 2024 modified date goes to `2019\07`. From = EXIF.
2. [ ] An MP4 with a 2021-05 creation time goes to `2021\05`. From = Video.
3. [ ] `IMG-20160315-WA0020.jpg` with no EXIF date goes to `2016\03`. From = File name.
4. [ ] `holiday.jpg` with no EXIF date is not moved and shows "No date".
5. [ ] After Scan with no Sort, every file is the same file and no folders exist in the destination.
6. [ ] The destination already has an identical `a.jpg`: the source `a.jpg` is not moved. A different `a.jpg`: the source arrives as `a (2).jpg`, and the existing file is untouched.
7. [ ] `notes.txt`, `Thumbs.db`, a zero-byte `empty.jpg` and a `.pdf` are not moved.
8. [ ] `写真.jpg` and `Café 😀.jpg` (both with EXIF dates) are moved with their names unchanged.
9. [ ] A file held open for writing by a test script is not moved and shows "Still changing".
10. [ ] Undo returns every file to its original path as the same file and removes the empty Year/Month folders the sort created.
